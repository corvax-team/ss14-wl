using Content.Server._WL.ChatGpt.Elements.OpenAi.Request;
using Content.Server._WL.ChatGpt.Elements.OpenAi.Response;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;

namespace Content.Server._WL.ChatGpt.Elements.OpenAi
{
    public abstract class ToolFunctionModel
    {
        /// <summary>
        /// Имя метода.
        /// </summary>
        public abstract LocId Name { get; }

        /// <summary>
        /// Описание метода.
        /// </summary>
        public abstract LocId Description { get; }

        /// <summary>
        /// Сообщение, использующееся для указания ИИ его действие.
        /// </summary>
        public virtual LocId FallbackMessage { get; }

        /// <summary>
        /// Аргументы метода.
        /// </summary>
        public abstract IReadOnlyDictionary<string, Parameter<object>> Parameters { get; }

        /// <summary>
        /// Метод вызова... метода, кхм.
        /// </summary>
        /// <param name="arguments">Аргументы, переданные в эту функцию.</param>
        /// <returns>Строку-статус, который будет передан текстовой модели для понимания того, что она сделала.</returns>
        public virtual string? Invoke(Arguments? arguments) => null;

        /// <summary>
        /// Тип объекта, возвращаемый функцией.
        /// </summary>
        public abstract JsonSchemeType ReturnType { get; }

        /// <summary>
        /// Превращает объект <see cref="ToolFunctionModel"/> в <see cref="GptChatTool"/>.
        /// </summary>
        /// <returns></returns>
        public GptChatTool GetToolFunction()
        {
            var required = Parameters
                //.Where(x => x.Value.Required)
                .Select(x => x.Key)
                .ToArray();

            var properties = Parameters
                .ToDictionary(k => k.Key, v =>
                {
                    var desc = v.Value.Description;

                    var string_type = v.Value.Type.ToString();

                    var type = (object)(v.Value.Required
                        ? string_type
                        : new List<string>() { string_type, "null" });

                    var property = new GptChatTool.Tool.Function.FunctionArgumentsScheme.Property()
                    {
                        Description = desc == null ? null : Loc.GetString(desc),
                        Enum = v.Value.Enum?.ToArray(),
                        Type = type
                    };

                    return property;
                });

            var function = new GptChatTool.Tool.Function()
            {
                Name = Loc.GetString(Name),
                Description = Loc.GetString(Description),
                Strict = true,
                Parameters = new()
                {
                    ReturnType = ReturnType.ToFormatString(),
                    Required = required,
                    Properties = properties
                }
            };

            var tool = new GptChatTool()
            {
                Type = ModelTool.Constants.FunctionToolString,
                UsingTool = function
            };

            return tool;
        }

        /// <summary>
        /// Статический метод, позволяющий сгруппировать класс инструмента функции и ответ с инструментом от модели.
        /// </summary>
        /// <param name="response">Список ответов модели.</param>
        /// <param name="models">Список переданных функций.</param>
        /// <returns></returns>
        public static List<(GptChoice.ChoiceMessage.ResponseToolCall Response, ToolFunctionModel Model)> GiveChosenModels(
            IEnumerable<GptChoice.ChoiceMessage.ResponseToolCall> response,
            IEnumerable<ToolFunctionModel> models)
        {
            var list = new List<(GptChoice.ChoiceMessage.ResponseToolCall Response, ToolFunctionModel Model)>();

            foreach (var tool_call in response)
            {
                foreach (var model in models)
                {
                    if (tool_call.Function.Name == Loc.GetString(model.Name))
                    {
                        list.Add((tool_call, model));
                        break;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Класс, описывающий параметр метода.
        /// </summary>
        public sealed class Parameter<T> where T : notnull
        {
            /// <summary>
            /// Используется для сохранения типа параметра после приведения <see cref="T"/> к <see langword="object"/>.
            /// </summary>
            private Type _type;

            /// <summary>
            /// Строковое представление типа в формате JSON scheme.
            /// </summary>
            public string Type => _type.ToJsonSchemeString();

            /// <summary>
            /// Описание параметра.
            /// </summary>
            public LocId? Description { get; set; }

            /// <summary>
            /// Набор константных значений параметра.
            /// </summary>
            public HashSet<object?>? Enum { get; set; }

            /// <summary>
            /// Обязателен ли этот параметра?
            /// </summary>
            public bool Required { get; set; } = true;

            public static implicit operator Parameter<object>(Parameter<T> parameter)
            {
                return new()
                {
                    Enum = parameter.Enum?
                        .Select(x => (object?)x)
                        .ToHashSet(),
                    Description = parameter.Description,
                    Required = parameter.Required,
                    _type = parameter._type
                };
            }

            public Parameter()
            {
                _type = typeof(T);
            }
        }

        /// <summary>
        /// Класс, описывающий аргументы метода.
        /// </summary>
        public sealed class Arguments
        {
            private readonly JsonNode _node;

            public T? Caste<T>(string element)
            {
                TryCaste<T>(element, out var parsed);

                return parsed;
            }

            /// <summary>
            /// Достаёт из словаря аргумент и приводит к выбранному типу.
            /// При неудаче возвращает <see langword="false"/>.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="element"></param>
            /// <param name="parsed"></param>
            /// <returns></returns>
            public bool TryCaste<T>(string element, [NotNullWhen(true)] out T? parsed)
            {
                parsed = default;

                var node = _node[element];
                if (node == null)
                    return false;

                var parsed_t = node.GetValue<T>();
                if (parsed_t == null)
                    return false;

                parsed = parsed_t;

                return true;
            }

            public string Json()
            {
                return _node.ToJsonString();
            }

            [return: NotNullIfNotNull(nameof(node))]
            public static Arguments? FromNode(JsonNode? node)
            {
                if (node == null)
                    return null;

                return new Arguments(node);
            }

            private Arguments(JsonNode node)
            {
                _node = node;
            }
        }
    }
}
