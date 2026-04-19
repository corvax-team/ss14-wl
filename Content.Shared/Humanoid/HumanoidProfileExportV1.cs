using System.Numerics;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid;

/// <summary>
/// Holds all of the data for importing / exporting character profiles.
/// </summary>
[DataDefinition]
public sealed partial class HumanoidProfileExportV1
{
    [DataField]
    public string ForkId;

    [DataField]
    public int Version = 1;

    [DataField(required: true)]
    public HumanoidCharacterProfileV1 Profile = default!;

    public HumanoidProfileExportV2 ToV2()
    {
        return new()
        {
            ForkId = ForkId,
            Version = 2,
            Profile = Profile.ToV2()
        };
    }
}

[DataDefinition, Serializable]
public sealed partial class HumanoidCharacterProfileV1
{
    [DataField("_jobPriorities")]
    public Dictionary<ProtoId<JobPrototype>, JobPriority> JobPriorities = new();

    [DataField("_antagPreferences")]
    public HashSet<ProtoId<AntagPrototype>> AntagPreferences = new();

    [DataField("_traitPreferences")]
    public HashSet<ProtoId<TraitPrototype>> TraitPreferences = new();

    [DataField("_loadouts")]
    public Dictionary<string, RoleLoadout> Loadouts = new();

    [DataField("_jobSubnames")]
    public Dictionary<string, string> JobSubnames = new();

    [DataField("_jobUnblockings")]
    public Dictionary<string, bool> JobUnblockings = new();

    [DataField]
    public string Name;

    [DataField]
    public string FlavorText;

    [DataField]
    public string OocText; //WL-Changes: OOC text

    [DataField]
    public ProtoId<SpeciesPrototype> Species;

    [DataField] //Corvax-TTS
    public string Voice = HumanoidProfileSystem.DefaultVoice;

    [DataField]
    public int Age;

    [DataField]
    public int Height; //WL-Changes: Height

    [DataField]
    public Sex Sex;

    [DataField]
    public Gender Gender;

    [DataField]
    public HumanoidCharacterAppearanceV1 Appearance;

    [DataField]
    public SpawnPriorityPreference SpawnPriority;

    [DataField]
    public PreferenceUnavailableMode PreferenceUnavailable;

    //WL-Changes: Records start
    [DataField]
    public string MedicalRecord;

    [DataField]
    public string SecurityRecord;

    [DataField]
    public string EmploymentRecord;

    [DataField]
    public string FullName;

    [DataField]
    public string DateOfBirth;

    [DataField]
    public string Confederation;

    [DataField]
    public string Country;

    [DataField("skills")]
    public Dictionary<string, Dictionary<byte, int>> Skills = new();

    //WL-Changes: Records end
    public HumanoidCharacterProfile ToV2()
    {
        return new(Name, FlavorText, OocText, Species, Voice, Age, Height, Sex, Gender, Appearance.ToV2(Species), SpawnPriority, JobPriorities, PreferenceUnavailable, JobSubnames, AntagPreferences, TraitPreferences, Loadouts, JobUnblockings, MedicalRecord, SecurityRecord, EmploymentRecord, FullName, DateOfBirth, Confederation, Country, Skills);
    }
}


[DataDefinition, Serializable]
public sealed partial class HumanoidCharacterAppearanceV1
{
    [DataField("hair")]
    public string HairStyleId;

    [DataField]
    public Color HairColor;

    [DataField("facialHair")]
    public string FacialHairStyleId;

    [DataField]
    public Color FacialHairColor;

    [DataField]
    public Color EyeColor;

    [DataField]
    public Color SkinColor;

    [DataField]
    public List<Marking> Markings = new();

    public HumanoidCharacterAppearance ToV2(ProtoId<SpeciesPrototype> species)
    {
        var markingManager = IoCManager.Resolve<MarkingManager>();

        var incomingMarkings = Markings.ShallowClone();
        if (HairStyleId != string.Empty)
            incomingMarkings.Add(new(HairStyleId, new List<Color>() { HairColor }));
        if (FacialHairStyleId != string.Empty)
            incomingMarkings.Add(new(FacialHairStyleId, new List<Color>() { FacialHairColor }));

        return new HumanoidCharacterAppearance(EyeColor, SkinColor, markingManager.ConvertMarkings(incomingMarkings, species));
    }
}
