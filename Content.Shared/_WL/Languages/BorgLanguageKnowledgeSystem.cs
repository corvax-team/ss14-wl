using Content.Shared.Silicons.Borgs.Components;

namespace Content.Shared._WL.Languages;

public sealed partial class BorgLanguageKnowledgeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BorgChassisComponent, GetKnownLanguagesEvent>(OnGetKnownLanguages);
    }

    private void OnGetKnownLanguages(Entity<BorgChassisComponent> ent, ref GetKnownLanguagesEvent args)
    {
        if (ent.Comp.ModuleContainer == null)
            return;
        foreach (var module in ent.Comp.ModuleContainer.ContainedEntities)
        {
            if (!TryComp<BorgLanguageModuleComponent>(module, out var card) || !card.Configured ||
                !TryComp<BorgModuleComponent>(module, out var installed) || installed.InstalledEntity != ent.Owner)
                continue;

            foreach (var (language, level) in card.Languages)
                args.AddLanguage(language, level);
        }
    }
}
