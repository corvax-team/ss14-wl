using Content.Server.Radio.Components;
using Content.Shared._WL.PulseDemon;
using Content.Shared.Radio.Components;
using Robust.Shared.Random;

namespace Content.Server._WL.PulseDemon.TamperActions;

public sealed partial class EncryptionKeyHolderAction : ElectromagneticTamperAction
{
    public override bool Action(ElectromagneticTamperActionArgs args)
    {
        var _entityManager = args.EntityManager;
        var _random = IoCManager.Resolve<IRobustRandom>();

        if (!_entityManager.TryGetComponent<EncryptionKeyHolderComponent>(args.TargetUid, out var encryptKeyHolder))
            return false;

        if (!_entityManager.TryGetComponent<ActiveRadioComponent>(args.DemonUid, out var activeRadioComp))
            activeRadioComp = _entityManager.AddComponent<ActiveRadioComponent>(args.DemonUid);

        var channel = _random.Pick/*AndTake*/(encryptKeyHolder.Channels);

        activeRadioComp.Channels.Add(channel);

        return true;
    }
}
