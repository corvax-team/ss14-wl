using Content.Server._WL.ChatGpt.Elements.OpenAi;
using Content.Server._WL.ChatGpt.Elements.OpenAi.Request;
using Content.Server._WL.ChatGpt.Elements.OpenAi.Response;
using Content.Server._WL.CVars;
using Content.Shared._WL.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._WL.ChatGpt.Managers
{
    public sealed partial class ChatGptManager : IChatGptManager, IPostInjectInit
    {
        [Dependency] private readonly ILogManager _logMan = default!;
        [Dependency] private readonly IConfigurationManager _confMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;

        private ISawmill _sawmill = default!;
        public const string SawmillName = "chat.gpt.mngr";

        private const string AIDisabledDeclineMessage = "Использование API нейросетей на данный момент выключено.";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        [ViewVariables(VVAccess.ReadOnly)]
        private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(15);

        private HttpClient _httpClient = default!;

        private string _apiKey = default!;
        private string _endpoint = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        private bool _enabled = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        private string _chatModel = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        private int _maxResponseTokens = default!;

        private Uri _balanceMap = default!;

        #region Init stuff
        public void Initialize()
        {
            _httpClient = new HttpClient()
            {
                Timeout = QueryTimeout
            };

            SetupCVars();

            var endpoint = new Uri(_endpoint, UriKind.Absolute);

            _httpClient.BaseAddress = endpoint;
        }

        private void SetupCVars()
        {
            // Api key
            _apiKey = _confMan.GetCVar(ServerWLCVars.GptApiKey);
            _confMan.OnValueChanged(ServerWLCVars.GptApiKey, (value) => _apiKey = value, true);

            // Endpoint
            _endpoint = _confMan.GetCVar(WLCVars.GptQueriesEndpoint);
            _confMan.OnValueChanged(WLCVars.GptQueriesEndpoint, (value) =>
            {
                _endpoint = value;
            }, true);

            // Chat model
            _chatModel = _confMan.GetCVar(WLCVars.GptChatModel);
            _confMan.OnValueChanged(WLCVars.GptChatModel, (new_model) =>
            {
                _chatModel = new_model;
            }, true);

            //Max tokens
            _enabled = _confMan.GetCVar(WLCVars.IsGptEnabled);
            _confMan.OnValueChanged(WLCVars.IsGptEnabled, (enable) => _enabled = enable, true);

            //Max tokens
            _maxResponseTokens = _confMan.GetCVar(WLCVars.GptMaxTokens);
            _confMan.OnValueChanged(WLCVars.GptMaxTokens, (max) => _maxResponseTokens = max, true);

            //Balance
            _balanceMap = new(_confMan.GetCVar(WLCVars.GptBalanceMap));
            _confMan.OnValueChanged(WLCVars.GptBalanceMap, (map) => _balanceMap = new(map), true);
        }

        public void PostInject()
        {
            _sawmill = _logMan.GetSawmill(SawmillName);
        }
        #endregion

        #region Public api
        /// <summary>
        /// Показывает можно ли сейчас отправлять запросы к ИИ.
        /// </summary>
        public bool IsEnabled()
        {
            return _enabled;
        }

        /// <summary>
        /// <see cref="IsEnabled()" />.
        /// </summary>
        /// <param name="reason">Сообщение, когда ИИ выключен. <see cref="AIDisabledDeclineMessage" />.</param>
        public bool IsEnabled([NotNullWhen(false)] out string? reason)
        {
            reason = null;

            if (!_enabled)
                reason = AIDisabledDeclineMessage;

            return _enabled;
        }

        /// <summary>
        /// Получает оставшееся количество рублей на счёте(((
        /// </summary>
        /// <returns></returns>
        public async Task<decimal?> GetBalanceAsync(CancellationToken cancel = default)
        {
            using var http = new HttpRequestMessage(HttpMethod.Get, _balanceMap);

            AddDefaultsHeaders(http);

            try
            {
                var resp = await _httpClient.SendAsync(http, cancel).ConfigureAwait(false);

                resp.EnsureSuccessStatusCode();

                var balance_obj = await resp.Content.ReadFromJsonAsync<AccountBalance>(cancel).ConfigureAwait(false) ??
                    throw new JsonException($"Неудачная сериализация в объект {nameof(AccountBalance)}!");

                return balance_obj.Balance;
            }
            catch (Exception ex)
            {
                if (!_enabled)
                {
                    _sawmill.Info(AIDisabledDeclineMessage);
                    return null;
                }
                else
                {
                    _sawmill.Warning("Ошибка при получении баланса аккаунта ProxyAi!" + " " + "Bearer " + _apiKey);
                    _sawmill.Warning(ex.ToStringBetter());
                    return null;
                }
            }
        }

        /// <summary>.
        /// Отправляет запрос к выбранной модели.
        /// </summary>
        /// <returns>МОЖЕТ вернуть null, если использование нейросетей в КВарах выключено.</returns>
        public async Task<GptChatResponse> SendChatQueryAsync(
            GptChatRequest gpt_request,
            IEnumerable<ToolFunctionModel>? methods = null,
            CancellationToken cancel = default)
        {
            using var http_request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress);

            try
            {
                gpt_request.Model = _chatModel;
                gpt_request.MaxTokens = _maxResponseTokens;

                if (methods != null)
                {
                    var list = methods.Select(m => m.GetToolFunction());
                    gpt_request.Tools = list.ToArray();
                }

                AddDefaultsHeaders(http_request);

                var rs = RStopwatch.StartNew();

                var body = JsonSerializer.Serialize(gpt_request, SerializerOptions);

                http_request.Content = new StringContent(body, null, "application/json");

                using var resp = await _httpClient.SendAsync(http_request, cancel);

                var resp_string = await resp.Content.ReadAsStringAsync(cancel);

                try
                {
                    var gpt_resp = JsonSerializer.Deserialize<GptChatResponse>(resp_string, SerializerOptions);
                    if (gpt_resp == null)
                    {
                        _sawmill.Error("При десериализации ответа от модели произошла ошибка! Десериализованное значение равнялось NULL!");
                        throw new HttpRequestException(resp_string, null, resp.StatusCode);
                    }

                    LogInf(gpt_resp);

                    return gpt_resp;
                }
                catch (Exception ex)
                {
                    _sawmill.Error($"Ошибка при отправке запроса! Полученный ответ: {resp_string}");
                    _sawmill.Error(ex.ToStringBetter());
                    throw;
                }

                void LogInf(GptChatResponse resp)
                {
                    var elapsed = rs.Elapsed.Milliseconds;
                    _sawmill.Info($"Запрос {resp.ID} был обработан моделью {resp.Model} за {elapsed}мс. Количество входных токенов: {resp.Usage.InputTokens}. " +
                        $"Количество выходных токенов: {resp.Usage.OutputTokens}. " +
                        $"Общее количество токенов: {resp.Usage.TotalTokens}.");
                }
            }
            catch (Exception ex)
            {
                _sawmill.Error(ex.ToStringBetter());
                throw;
            }
        }

        /// <summary>
        /// Более простая перегрузка метода <see cref="SendChatQueryAsync(GptChatRequest, CancellationToken)"/>.
        /// </summary>
        /// <param name="prompt">Текстовый промт.</param>
        /// <returns>Возвращает только ответ ИИ. Никакой памяти. Никаких списков. Всё просто.</returns>
        public async Task<string?> SendChatQuery(string prompt)
        {
            var msg = new GptChatMessage.User(prompt);

            var req = new GptChatRequest()
            {
                Messages = [msg]
            };

            var resp = await SendChatQueryAsync(req);

            return resp.GetRawStringResponse();
        }


        #endregion

        #region Private Utility
        /// <summary>
        /// Добавляет заголовки по-умолчанию для каждого запроса.
        /// </summary>
        private void AddDefaultsHeaders(HttpRequestMessage msg)
        {
            msg.Headers.Authorization = new("Bearer", _apiKey);
        }
        #endregion
    }
}
