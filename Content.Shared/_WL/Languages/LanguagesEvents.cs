using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;


namespace Content.Shared._WL.Languages;

/// <summary>
/// Проверка на окружающее давление
/// </summary>
[ByRefEvent]
public record struct PressureLanguageCheckEvent(string Message, EntityUid Source)
{
    public string Message = Message;
    public readonly EntityUid Source = Source;
    public bool Cancelled = false;
    public bool ForceWhisper = false;
}

/// <summary>
/// Проверка на то, можно ли на языке говорить по радио
/// </summary>
[ByRefEvent]
public record struct RadioLanguageCheckEvent(string Message, EntityUid RadioSource)
{
    public string Message = Message;
    public readonly EntityUid RadioSource = RadioSource;
    public bool Cancelled = false;
}

[Serializable, NetSerializable]
public sealed partial class LanguageChangeEvent(NetEntity entity, ProtoId<LanguagePrototype> protoId) : EntityEventArgs
{
    public NetEntity Entity { get; } = entity;

    public ProtoId<LanguagePrototype> Language { get; } = protoId;
}

[Serializable, NetSerializable]
public sealed partial class AfterLanguageChangeEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class LanguagesSyncEvent(
        NetEntity entity,
        List<ProtoId<LanguagePrototype>> speaking,
        List<ProtoId<LanguagePrototype>> understood) : EntityEventArgs
{
    public NetEntity Entity { get; } = entity;

    public List<ProtoId<LanguagePrototype>> Speaking { get; } = speaking;

    public List<ProtoId<LanguagePrototype>> Understood { get; } = understood;
}

[Serializable, NetSerializable]
public sealed partial class LanguageSyncRequestEvent(
        NetEntity entity,
        List<ProtoId<LanguagePrototype>> speaking,
        List<ProtoId<LanguagePrototype>> understood) : EntityEventArgs
{
    public NetEntity Entity { get; } = entity;

    public List<ProtoId<LanguagePrototype>> Speaking { get; } = speaking;

    public List<ProtoId<LanguagePrototype>> Understood { get; } = understood;
}

[Serializable, NetSerializable]
public sealed class LanguageSoundEvent(
        ProtoId<LanguagePrototype> language,
        NetEntity? sourceUid = null,
        bool isWhisper = false) : EntityEventArgs
{
    public ProtoId<LanguagePrototype> Language { get; } = language;
    public NetEntity? SourceUid { get; } = sourceUid;
    public bool IsWhisper { get; } = isWhisper;
}
