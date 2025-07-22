using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Content.Server._WL.ChatGpt.Elements.OpenAi.Request;
using static Content.Server._WL.ChatGpt.Elements.OpenAi.Response.GptChoice.Constants;

namespace Content.Server._WL.ChatGpt.Elements.OpenAi.Response
{
    /// <summary>
    /// Класс, характеризующий один из вариантов ответа текстовой модели OpenAi.
    /// </summary>
    public sealed class GptChoice
    {
        /// <summary>
        /// Каждое завершение ответа на запрос пользователя сопровождается кодом.
        /// В этом коде описана причина завершения.
        /// Смотреть <see cref="FinishType"/> и <see cref="FromFinishString()"/>.
        /// </summary>
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        /// <summary>
        /// Идентификатор выбора в массиве выборов.
        /// </summary>
        [JsonPropertyName("index")]
        public required int Index { get; set; }

        /// <summary>
        /// Сгенерированное моделью сообщение в ответ на запрос пользователя.
        /// </summary>
        [JsonPropertyName("message")]
        public required ChoiceMessage Message { get; set; }

        /// <summary>
        /// Более подробная информация о текущем выборе.
        /// </summary>
        [JsonPropertyName("logprobs")]
        public LogProbabilities? LogProbs { get; set; }

        #region Methods api
        /// <summary>
        /// Для объекта класса.
        /// Также есть статическая версия <see cref="FromFinishString(in string)"/>.
        /// </summary>
        public FinishType FromFinishString()
        {
            return FromFinishString(FinishReason);
        }

        /// <summary>
        /// Статическая версия метода <see cref="FromFinishString()"/>.
        /// </summary>
        /// <param name="finish_string">Строка-код завершения ответа модели. <see cref="FinishType"/>.</param>
        /// <returns>Если входной параметр не подошёл ни под один тип завершения, то возвращается <see cref="FinishType.Null"/>.</returns>
        public static FinishType FromFinishString(in string? finish_string)
        {
            return finish_string switch
            {
                StopFinishString => FinishType.Stop,
                LengthFinishString => FinishType.Length,
                FunctionFinishString => FinishType.FunctionCall,
                FilterFinishString => FinishType.ContentFilter,
                ToolFinishString => FinishType.ToolCall,
                _ => FinishType.Null
            };
        }
        #endregion

        #region Utility classes and other

        #region Choice message
        /// <summary>
        /// Содержит информацию об ответе текстовой модели на запрос.
        /// </summary>
        public sealed class ChoiceMessage
        {
            /// <summary>
            /// Собственно... Ответ модели.
            /// </summary>
            [JsonPropertyName("content")]
            public string? Content { get; set; }

            /// <summary>
            /// Если <see cref="FinishReason"/> имеет тип <see cref="FinishType.ContentFilter"/>,
            /// То модель ответит тем, что не может ответить на заданный запрос.
            /// В этой переменной и содержится этот ответ.
            /// </summary>
            [JsonPropertyName("refusal")]
            public string? RefusalMessage { get; set; }

            /// <summary>
            /// Роль сообщения. Смотреть <see cref="ModelRole"/>.
            /// </summary>
            [JsonPropertyName("role")]
            public required string Role { get; set; }

            /// <summary>
            /// Список вызванных функций.
            /// Может быть NULL, если на выбор модели не было предоставлено каких-либо функций.
            /// </summary>
            [JsonPropertyName("tool_calls")]
            public ResponseToolCall[]? Tools { get; set; }

            /// <summary>
            /// Превращает ответ модели в объект <see cref="GptChatMessage"/>.
            /// </summary>
            /// <returns></returns>
            public GptChatMessage ToChatMessage(string? name = null)
            {
                var role = ModelRole.FromString(Role);
                var content = Content ?? RefusalMessage ?? string.Empty;

                return role switch
                {
                    ModelRole.ModelRoleType.User => new GptChatMessage.User(content)
                    {
                        Name = name
                    },
                    ModelRole.ModelRoleType.System => new GptChatMessage.System(content)
                    {
                        Name = name
                    },
                    ModelRole.ModelRoleType.Assistant => new GptChatMessage.Assistant(content)
                    {
                        Refusal = RefusalMessage,
                        Name = name,
                        Tools = Tools
                    }
                };
            }

            /// <summary>
            /// Класс, содержащий информацию о вызванных моделью функциях.
            /// </summary>
            public sealed class ResponseToolCall
            {
                /// <summary>
                /// ID ответа.
                /// Абсолютно случайное и ни к чему не привязано.
                /// </summary>
                [JsonPropertyName("id")]
                public required string ID { get; set; }

                /// <summary>
                /// Тип "утилиты".
                /// На данный момент есть только функция.
                /// Смотреть в <see cref="ModelTool.ModelToolType"/>.
                /// </summary>
                [JsonPropertyName("type")]
                public required string Type { get; set; }

                /// <summary>
                /// Сама функция.
                /// </summary>
                [JsonPropertyName("function")]
                public required FunctionResponseCall Function { get; set; }

                /// <summary>
                /// Информация о вызове функции моделью.
                /// </summary>
                public sealed class FunctionResponseCall
                {
                    /// <summary>
                    /// Имя функции.
                    /// </summary>
                    [JsonPropertyName("name")]
                    public required string Name { get; set; }

                    /// <summary>
                    /// Переданные в функцию параметры.
                    /// </summary>
                    [JsonPropertyName("arguments")]
                    public string? Arguments { get; set; }

                    /// <summary>
                    /// Вытаскивает словарь названий и значений выбранных аргументов.
                    /// Не вызывайте туеву тучу раз!!
                    /// Вызвали, кешировали, профит.
                    /// </summary>
                    /// <returns></returns>
                    public JsonObject? ParseArguments()
                    {
                        if (Arguments == null)
                            return null;

                        var node = JsonNode.Parse(Arguments);

                        return node?.AsObject();
                    }
                }
            }
        }
        #endregion

        #region Log probs
        /// <summary>
        /// Класс, содержащий подробную информацию о выборе модели при ответе на запрос.
        /// По большей части тут содержится информация о каждом токене в запросе к/ответе модели.
        /// </summary>
        public sealed class LogProbabilities
        {
            /// <summary>
            /// Информация о каждом токене ответа/запроса.
            /// </summary>
            [JsonPropertyName("content")]
            public Content[]? LogContent { get; set; }

            /// <summary>
            /// Информация о каждом из отклонённых(?, я честно хз за что отвечает это поле. Перепишите кто-нибудь.) токенов ответа/запроса.
            /// </summary>
            [JsonPropertyName("refusal")]
            public Content[]? LogRefusalContent { get; set; }

            /// <summary>
            /// В этом классе содержится информация об токене: вероятность выбора, байтовое представление.
            /// </summary>
            [Virtual]
            public class Content
            {
                /// <summary>
                /// Сам токен.
                /// </summary>
                [JsonPropertyName("token")]
                public required string Token { get; set; }

                /// <summary>
                /// Вероятность выбора токена.
                /// </summary>
                [JsonPropertyName("logprob")]
                public required float LogProb { get; set; }

                /// <summary>
                /// Байтовое представление токена.
                /// Кодировка - UTF8.
                /// Может иметь NULL, если токен не имеет байтового представления.
                /// </summary>
                [JsonPropertyName("bytes")]
                public byte[]? Bytes { get; set; }

                /// <summary>
                /// <see cref="TopContent"/>.
                /// </summary>
                [JsonPropertyName("top_logprobs")]
                public required TopContent[] TopLogProb { get; set; }
            }

            /// <summary>
            /// List of the most likely tokens and their log probability, at this token position.
            /// In rare cases, there may be fewer than the number of requested top_logprobs returned.
            /// </summary>
            public sealed class TopContent : Content;
        }
        #endregion

        #region Finish type
        /// <summary>
        /// Перечисление, которое содержит строковые коды окончания ответа модели.
        /// </summary>
        public enum FinishType : byte
        {
            /// <summary>
            /// Модель полностью ответила на запрос, либо сообщение остановлено одной из последовательностей остановки.
            /// </summary>
            Stop,

            /// <summary>
            /// Неполный вывод модели из-за параметра <see cref="GptChatRequest.MaxTokens"/> или из-за лимита токенов.
            /// </summary>
            Length,

            /// <summary>
            /// Модель вызвала функцию.
            /// </summary>
            [Obsolete($"Сейчас чаще используется методика ToolCalls")]
            FunctionCall,

            /// <summary>
            /// Модель не смогла ответить на запрос из-за фильтров содержимого(NSFW).
            /// </summary>
            ContentFilter,

            /// <summary>
            /// Модель вызвала функцию.
            /// </summary>
            ToolCall,

            /// <summary>
            /// Ответ всё еще выполняется, или он неполный.
            /// Либо ответ не подошёл под другие коды, в последнем случае просмотрите логи.
            /// </summary>
            Null
        }
        #endregion

        #region Constants
        /// <summary>
        /// Содержит константы для класса <see cref="GptChoice"/>.
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// Строковое представление типа окончания диалога с кодом "STOP".
            /// </summary>
            public const string StopFinishString = "stop";

            /// <summary>
            /// Строковое представление типа окончания диалога с кодом "LENGTH".
            /// </summary>
            public const string LengthFinishString = "length";

            /// <summary>
            /// Строковое представление типа окончания диалога с кодом "FUNCTION_CALL".
            /// </summary>
            public const string FunctionFinishString = "function_call";

            /// <summary>
            /// Строковое представление типа окончания диалога с кодом "CONTENT_FILTER".
            /// </summary>
            public const string FilterFinishString = "content_filter";

            /// <summary>
            /// Строковое представление типа окончания диалога с кодом "TOOL_CALLS".
            /// </summary>
            public const string ToolFinishString = "tool_calls";
        }
        #endregion

        #endregion
    }
}
