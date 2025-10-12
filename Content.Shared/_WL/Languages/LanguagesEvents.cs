using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages;

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
