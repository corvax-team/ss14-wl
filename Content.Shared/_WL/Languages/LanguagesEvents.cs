using Content.Shared._WL.Languages.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;


namespace Content.Shared._WL.Languages;

/// <summary>
/// Проверка на окружающее давление
/// </summary>
[ByRefEvent]
public record struct PressureLanguageCheckEvent(ProtoId<LanguagePrototype> language, string message)
{
    public ProtoId<LanguagePrototype> Language = language;
    public string Message = message;
    public bool Cancelled = false;
    public bool ForceWhisper = false;
}

/// <summary>
/// Проверка на то, можно ли на языке говорить по радио
/// </summary>
[ByRefEvent]
public record struct RadioLanguageCheckEvent(string Message, ProtoId<LanguagePrototype> langId)
{
    public string Message = Message;
    public ProtoId<LanguagePrototype> Language = langId;
    public bool Cancelled = false;
}

[Serializable, NetSerializable]
public sealed class LanguagesChangedEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class SetLanguageEvent(NetEntity netEntity, ProtoId<LanguagePrototype> language) : EntityEventArgs
{
    public NetEntity Entity { get; } = netEntity;
    public ProtoId<LanguagePrototype> Language { get; } = language;
}

[Serializable, NetSerializable]
public sealed class LanguageSoundEvent : EntityEventArgs
{
    public ProtoId<LanguagePrototype> Language { get; }
    public NetEntity? SourceUid { get; }
    public bool IsWhisper { get; }

    public LanguageSoundEvent(ProtoId<LanguagePrototype> language, NetEntity? sourceUid = null, bool isWhisper = false)
    {
        Language = language;
        SourceUid = sourceUid;
        IsWhisper = isWhisper;
    }
}
