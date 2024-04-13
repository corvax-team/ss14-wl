using Content.Server.Chat.Systems;
using Content.Shared._WL.Slimes;
using Content.Shared.Chat;
using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._WL.Slimes.Commands;

public sealed partial class HelloSlimeCommand : SlimeCommand
{
    [DataField("dataset", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<DatasetPrototype>))]
    public string Dataset;

    public override bool Command(SlimeCommandArgs args)
    {
        var _entMan = args.EntityManager;
        var _chat = _entMan.System<ChatSystem>();
        var _protoMan = IoCManager.Resolve<IPrototypeManager>();
        var _random = IoCManager.Resolve<IRobustRandom>();

        var sourceMetaData = _entMan.GetComponent<MetaDataComponent>(args.Source);

        var locMessage = _random.Pick(_protoMan.Index<DatasetPrototype>(Dataset));
        var message = Loc.GetString(locMessage, ("target", sourceMetaData.EntityName));

        _chat.TrySendInGameICMessage(args.Slime, message, InGameICChatType.Speak, false, true, checkRadioPrefix: false);

        return true;
    }
}
