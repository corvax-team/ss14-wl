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
public sealed partial class LanguageChangeEvent : EntityEventArgs
{
    public NetEntity Entity { get; }

    public ProtoId<LanguagePrototype> Language { get; }

    public LanguageChangeEvent(NetEntity entity, ProtoId<LanguagePrototype> protoId)
    {
        Entity = entity;
        Language = protoId;
    }
}

[Serializable, NetSerializable]
public sealed partial class AfterLanguageChangeEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class LanguagesSyncEvent : EntityEventArgs
{
    public NetEntity Entity { get; }

    public List<ProtoId<LanguagePrototype>> Speaking { get; }

    public List<ProtoId<LanguagePrototype>> Understood { get; }

    public LanguagesSyncEvent(NetEntity entity, List<ProtoId<LanguagePrototype>> speaking, List<ProtoId<LanguagePrototype>> understood)
    {
        Entity = entity;
        Speaking = speaking;
        Understood = understood;
    }
}

[Serializable, NetSerializable]
public sealed partial class LanguageSyncRequestEvent : EntityEventArgs
{
    public NetEntity Entity { get; }

    public List<ProtoId<LanguagePrototype>> Speaking { get; }

    public List<ProtoId<LanguagePrototype>> Understood { get; }

    public LanguageSyncRequestEvent(NetEntity entity, List<ProtoId<LanguagePrototype>> speaking, List<ProtoId<LanguagePrototype>> understood)
    {
        Entity = entity;
        Speaking = speaking;
        Understood = understood;
    }
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
