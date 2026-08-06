using Content.Shared.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Content.Shared.Whitelist;
using Content.Shared._WL.Languages.Components;
using Content.Shared._WL.Languages;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.Server.Traits;

public sealed partial class TraitSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Check if player's job allows to apply traits
        if (args.JobId == null ||
            !ProtoMan.Resolve<JobPrototype>(args.JobId, out var protoJob) ||
            !protoJob.ApplyTraits)
        {
            return;
        }

        //WL-Changes-Languages-Start
        // Aggregate language modifications from all selected traits before applying any ModifyLanguagesComponent
        var modifyComps = new List<ModifyLanguagesComponent>();
        //WL-Changes-Languages-End

        foreach (var traitId in args.Profile.TraitPreferences)
        {
            if (!ProtoMan.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Log.Error($"No trait found with ID {traitId}!");
                return;
            }

            if (_whitelistSystem.IsWhitelistFail(traitPrototype.Whitelist, args.Mob) ||
                _whitelistSystem.IsWhitelistPass(traitPrototype.Blacklist, args.Mob))
                continue;

            // Add all components required by the prototype
            // Aggregate language modifications across all selected traits to avoid races with LanguagesSystem
            // (LanguagesSystem processes ModifyLanguagesComponent on ComponentInit and may remove it immediately).

            if (traitPrototype.Components.Count > 0)
            {
                foreach (var componentEntry in traitPrototype.Components)
                {
                    if (componentEntry.Value.Component is ModifyLanguagesComponent modifyLanguagesComponent)
                    {
                        //WL-Changes-Languages-Start
                        // Collect ModifyLanguagesComponent instances to aggregate later.
                        modifyComps.Add(modifyLanguagesComponent);

                        // Don't immediately add this trait's ModifyLanguagesComponent
                        continue;
                        //WL-Changes-Languages-End
                    }

                    EntityManager.AddComponent(args.Mob, componentEntry.Value, false);
                }
            }
 
            // Add all JobSpecials required by the prototype
            foreach (var special in traitPrototype.Specials)
            {
                special.AfterEquip(args.Mob);
            }

            // Add item required by the trait
            if (traitPrototype.TraitGear == null)
                continue;

            if (!TryComp(args.Mob, out HandsComponent? handsComponent))
                continue;

            var coords = Transform(args.Mob).Coordinates;
            var inhandEntity = Spawn(traitPrototype.TraitGear, coords);
            _sharedHandsSystem.TryPickup(args.Mob,
                inhandEntity,
                checkActionBlocker: false,
                handsComp: handsComponent);
        }

        //WL-Changes-Languages-Start
        // After processing all traits, apply aggregated ModifyLanguagesComponent once
        var aggregated = ModifyLanguagesAggregator.Aggregate(modifyComps);

        if ((aggregated.Languages.Count > 0) || aggregated.ToRemove || aggregated.ToUnderstood || aggregated.ToSpeaking || aggregated.SpecieLanguage)
        {
            AddComp(args.Mob, aggregated, false);
        }
        //WL-Changes-Languages-End
    }
}
