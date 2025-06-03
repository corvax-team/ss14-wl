#pragma warning disable IDE0290

using Content.Server._WL.ChatGpt.Elements.OpenAi.Response;
using Robust.Shared.Utility;
using System.Text.Json.Serialization;

namespace Content.Server._WL.ChatGpt.Elements.OpenAi.Request
{
    /// <summary>
    /// Класс, используемый для отправки запросов к модели.
    /// </summary>
    public abstract class GptChatMessage
    {
        /// <summary>
        /// Роль сообщения. Смотреть <see cref="ModelRole.ModelRoleType"/>.
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; init; }

        /// <summary>
        /// Содержание запроса.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        #region ctor
        protected GptChatMessage(string role, string content)
        {
            DebugTools.Assert(ModelRole.IsStringValid(role));

            Role = role;
            Content = content;
        }

        protected GptChatMessage(ModelRole.ModelRoleType roleType, string content)
            : this(ModelRole.FromModelRoleType(roleType), content) { }
        #endregion

        #region Message types
        /// <summary>
        /// Сообщение от пользователя.
        /// </summary>
        public sealed class User : GptChatMessage
        {
            /// <summary>
            /// Имя, от которого будет отправлено пользовательское сообщение.
            /// Нужно для различия разных пользователей, если используется "память".
            /// </summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            public User(string content)
                : base(ModelRole.ModelRoleType.User, content)
            {
            }
        }

        /// <summary>
        /// Сообщение от системы.
        /// </summary>
        public sealed class System(string content) : GptChatMessage(ModelRole.ModelRoleType.System, content)
        {
            /// <summary>
            /// Имя системы(не виндовс. кхм, бля, я хз).
            /// </summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        /// <summary>
        /// Инструмент.
        /// Используется для оповещения модель о том, что она выбрала какую-то функцию.
        /// </summary>
        public sealed class Tool : GptChatMessage
        {
            private static readonly string FallbackContent = "";

            public Tool(string? content) : base(ModelRole.ModelRoleType.Tool, content ?? FallbackContent)
            {

            }

            /// <summary>
            /// ID выбранной функции.
            /// Смотреть <see cref="Response.GptChoice.ChoiceMessage.ResponseToolCall.ID"/>.
            /// </summary>
            [JsonPropertyName("tool_call_id")]
            public required string ToolId { get; set; }
        }

        /// <summary>
        /// Ассистент, т.е. модель, чат-гпт.
        /// </summary>
        /// <param name="content"></param>
        public sealed class Assistant(string content) : GptChatMessage(ModelRole.ModelRoleType.Assistant, content)
        {
            /// <summary>
            /// Сообщение, которое будет высвечено при блокировке запроса фильтрами модели.
            /// </summary>
            [JsonPropertyName("refusal")]
            public string? Refusal { get; set; }

            /// <summary>
            /// Имя, от которого будет системное сообщение.
            /// Н-р: CHAT GPT OPEN AI SIKIBIDI DOP DOP.
            /// Кхм.
            /// </summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            /// <summary>
            /// Инструменты, которые вызвала модель.
            /// </summary>
            [JsonPropertyName("tool_calls")]
            public GptChoice.ChoiceMessage.ResponseToolCall[]? Tools { get; set; }
        }
        #endregion
    }
}
