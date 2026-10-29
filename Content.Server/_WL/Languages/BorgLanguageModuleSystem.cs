using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Traits;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Languages;

public sealed partial class BorgLanguageModuleSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private LanguagesSystem _languages = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BorgLanguageModuleComponent, BoundUIOpenedEvent>(OnOpened);
        SubscribeLocalEvent<BorgLanguageModuleComponent, BorgLanguageModuleConfirmMessage>(OnConfirm);
        SubscribeLocalEvent<BorgLanguageModuleComponent, BorgModuleInsertAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<BorgLanguageModuleComponent, BorgModuleInstalledEvent>(OnInstalled);
        SubscribeLocalEvent<BorgLanguageModuleComponent, BorgModuleUninstalledEvent>(OnUninstalled);
    }

    public List<ProtoId<LanguagePrototype>> GetAvailableLanguages()
    {
        return _prototypes.EnumeratePrototypes<TraitPrototype>()
            .Where(trait => trait.Category?.Id == "Languages")
            .Select(trait => LanguageCostSystem.TryGetLanguagePrototype(trait, _prototypes, out var language) ? language.ID : null)
            .Where(id => id != null && id is not ("Wingdings" or "RootSpeak" or "Ternary" or "Unknown"))
            .Select(id => new ProtoId<LanguagePrototype>(id!)).Distinct().ToList();
    }

    private void OnOpened(Entity<BorgLanguageModuleComponent> ent, ref BoundUIOpenedEvent args) => UpdateUi(ent);

    private void UpdateUi(Entity<BorgLanguageModuleComponent> ent)
    {
        _ui.SetUiState(ent.Owner, BorgLanguageModuleUiKey.Key,
            new BorgLanguageModuleState(GetAvailableLanguages(), ent.Comp.Languages.ToList(), ent.Comp.Configured));
    }

    private void OnConfirm(Entity<BorgLanguageModuleComponent> ent, ref BorgLanguageModuleConfirmMessage args)
    {
        TryConfigure(ent, args.Languages);
        UpdateUi(ent);
    }

    public bool TryConfigure(Entity<BorgLanguageModuleComponent> ent, List<ProtoId<LanguagePrototype>> languages)
    {
        if (ent.Comp.Configured || languages.Count is < 1 or > BorgLanguageModuleComponent.MaxLanguages ||
            languages.Distinct().Count() != languages.Count ||
            !languages.All(GetAvailableLanguages().Contains) ||
            TryComp<BorgModuleComponent>(ent, out var module) && module.Installed)
            return false;

        ent.Comp.Languages = languages.ToList();
        ent.Comp.Configured = true;
        Dirty(ent);
        return true;
    }

    private void OnInsertAttempt(Entity<BorgLanguageModuleComponent> ent, ref BorgModuleInsertAttemptEvent args)
    {
        if (ent.Comp.Configured)
            return;
        args.Cancelled = true;
        args.Reason = Loc.GetString("borg-language-module-unconfigured");
    }

    private void OnInstalled(Entity<BorgLanguageModuleComponent> ent, ref BorgModuleInstalledEvent args) => Refresh(args.ChassisEnt);
    private void OnUninstalled(Entity<BorgLanguageModuleComponent> ent, ref BorgModuleUninstalledEvent args) => Refresh(args.ChassisEnt);

    private void Refresh(EntityUid borg)
    {
        if (!TryComp<BorgChassisComponent>(borg, out var chassis) || !TryComp<LanguagesComponent>(borg, out var knowledge))
            return;

        knowledge.ModuleLanguages.Clear();
        foreach (var module in chassis.ModuleContainer.ContainedEntities)
        {
            if (!TryComp<BorgLanguageModuleComponent>(module, out var card) || !card.Configured ||
                !TryComp<BorgModuleComponent>(module, out var installed) || installed.InstalledEntity != borg)
                continue;

            foreach (var language in card.Languages)
                knowledge.ModuleLanguages[language] = LanguageLevel.Full;
        }
        _languages.RefreshLanguages(borg);
    }

}
