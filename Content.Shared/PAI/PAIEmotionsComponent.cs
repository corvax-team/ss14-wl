using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.PAI;

/// <summary>
/// Компонент для управления эмоциями ПИИ
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PAIEmotionsComponent : Component
{
    /// <summary>
    /// Текущая эмоция ПИИ
    /// </summary>
    [DataField("emotion")]
    public PAIEmotion CurrentEmotion = PAIEmotion.Neutral;

    /// <summary>
    /// Время последней смены эмоции
    /// </summary>
    [DataField("lastEmotionChange")]
    public TimeSpan LastEmotionChange = TimeSpan.Zero;

    /// <summary>
    /// Минимальное время между сменами эмоций (в секундах)
    /// </summary>
    [DataField("emotionCooldown")]
    public float EmotionCooldown = 3.0f;
}

/// <summary>
/// Типы эмоций для ПИИ
/// </summary>
[Serializable, NetSerializable]
public enum PAIEmotion : byte
{
    Neutral = 0,
    Happy = 1,
    Sad = 2,
    Angry = 3
}

/// <summary>
/// Событие смены эмоции ПИИ через DoAfter
/// </summary>
[Serializable, NetSerializable]
public sealed partial class PAIEmotionChangeDoAfterEvent : SimpleDoAfterEvent
{
    public PAIEmotion NewEmotion;

    public PAIEmotionChangeDoAfterEvent(PAIEmotion newEmotion)
    {
        NewEmotion = newEmotion;
    }
