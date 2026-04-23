using System.Numerics;
using Content.Shared.Corvax.TTS;
using Content.Shared.Examine;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Preferences;
using Content.Shared.Sprite;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid;

public sealed class HumanoidProfileSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly GrammarSystem _grammar = default!;
    [Dependency] private readonly SharedScaleVisualsSystem _scale = default!; // WL-Changes

    // Corvax-TTS-Start
    public const string DefaultVoice = "Garithos";
    public static readonly Dictionary<Sex, string> DefaultSexVoice = new()
    {
        {Sex.Male, "Garithos"},
        {Sex.Female, "Maiev"},
        {Sex.Unsexed, "Myron"},
    };
    // Corvax-TTS-End
    // WL-Height-Start
    public const float MaxHeightScale = 100f;
    public const float MinHeightScale = 0.1f;
    // WL-Height-End

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidProfileComponent, ExaminedEvent>(OnExamined);
    }

    public void ApplyProfileTo(Entity<HumanoidProfileComponent?> ent, HumanoidCharacterProfile profile)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.Gender = profile.Gender;
        ent.Comp.Age = profile.Age;
        ent.Comp.Species = profile.Species;
        ent.Comp.Sex = profile.Sex;
        // Corvax-TTS-start
        ent.Comp.Voice = profile.Voice;
        if (TryComp<TTSComponent>(ent, out var _TTSComponent) && _TTSComponent.VoicePrototypeId == "Taskmaster")
        {
            _TTSComponent.VoicePrototypeId = profile.Voice;
        }
        // Corvax-TTS-end
        //Wl-Changes: Height start
        ent.Comp.Height = profile.Height;
        ApplyHeight(ent);
        //Wl-Changes: Height end
        Dirty(ent);

        var sexChanged = new SexChangedEvent(ent.Comp.Sex, profile.Sex);
        RaiseLocalEvent(ent, ref sexChanged);

        if (TryComp<GrammarComponent>(ent, out var grammar))
        {
            _grammar.SetGender((ent, grammar), profile.Gender);
        }
    }

    private void OnExamined(Entity<HumanoidProfileComponent> ent, ref ExaminedEvent args)
    {
        var identity = Identity.Entity(ent, EntityManager);
        var species = GetSpeciesRepresentation(ent.Comp.Species).ToLower();
        var age = GetAgeRepresentation(ent.Comp.Species, ent.Comp.Age);
        var height = GetHeightRepresentation(ent.Comp.Height); // WL-Height

        args.PushText(Loc.GetString("humanoid-appearance-component-examine", ("user", identity), ("age", age), ("height", height), ("species", species)));
    }

    // WL-Height-Start
    public void ApplyHeight(Entity<HumanoidProfileComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        var scale = System.Math.Min(System.Math.Max(ent.Comp.Height / 165f, MinHeightScale), MaxHeightScale);
        _scale.SetSpriteScale(ent, new Vector2(scale));
    }

    public void SetHeight(Entity<HumanoidProfileComponent?> ent, int height)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.Height = height;
        Dirty(ent, ent.Comp);
        ApplyHeight(ent);
    }
    // WL-Height-End

    /// <summary>
    /// Takes ID of the species prototype, returns UI-friendly name of the species.
    /// </summary>
    public string GetSpeciesRepresentation(ProtoId<SpeciesPrototype> species)
    {
        if (_prototype.TryIndex(species, out var speciesPrototype))
            return Loc.GetString(speciesPrototype.Name);

        Log.Error("Tried to get representation of unknown species: {speciesId}");
        return Loc.GetString("humanoid-appearance-component-unknown-species");
    }

    /// <summary>
    /// Takes ID of the species prototype and an age, returns an approximate description
    /// </summary>
    public string GetAgeRepresentation(ProtoId<SpeciesPrototype> species, int age)
    {
        if (!_prototype.TryIndex(species, out var speciesPrototype))
        {
            Log.Error("Tried to get age representation of species that couldn't be indexed: " + species);
            return Loc.GetString("identity-age-young");
        }

        if (age < speciesPrototype.YoungAge)
        {
            return Loc.GetString("identity-age-young");
        }

        if (age < speciesPrototype.OldAge)
        {
            return Loc.GetString("identity-age-middle-aged");
        }

        return Loc.GetString("identity-age-old");
    }

    // WL-Height-Start
    public string GetHeightRepresentation(int height)
    {
        return Loc.GetString("identity-height", ("height", MathF.Round(height / 10f) * 10));
    }
    // WL-Height-End
}
