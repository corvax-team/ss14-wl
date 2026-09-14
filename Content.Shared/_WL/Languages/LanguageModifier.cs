using System.Collections.Generic;
namespace Content.Shared._WL.Languages;

[DataDefinition]
public sealed partial class LanguageModifier
{
    /// <summary>
    /// Модификаторы стоимости уровней.
    /// Индекс = номер уровня.
    /// </summary>
    [DataField]
    public List<int> LevelModifiers { get; private set; } = new();

    /// <summary>
    /// Возвращает модификатор для уровня.
    /// </summary>
    public int GetModifier(int level)
    {
        if (level < 0 || level >= LevelModifiers.Count)
            return 0;
        return LevelModifiers[level];
    }
}
