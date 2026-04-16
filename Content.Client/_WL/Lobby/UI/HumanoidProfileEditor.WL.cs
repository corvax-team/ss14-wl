using System.Linq;
using Content.Shared._WL.Records; // WL-Records
using Content.Shared._WL.Skills; // WL-Skills
using Content.Shared.Roles;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Corvax.CCCVars;
using Content.Client.Corvax.TTS;
using Content.Client._WL.Skills.Ui; // WL-Skills
using Content.Client._WL.Records; // WL-Records
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private SkillsWindow? _skillsWindow;

    private RecordsTab? _recordsTab; // WL-Records
    private TextEdit? _medicalRecordEdit; // WL-Records
    private TextEdit? _securityRecordEdit; // WL-Records
    private TextEdit? _employmentRecordEdit; // WL-Records

    private LineEdit? _generalRecordNameEdit; // WL-Records
    private LineEdit? _generalRecordAgeEdit; // WL-Records
    private LineEdit? _generalRecordCountryEdit; // WL-Records

    private OptionButton? _confederationButton; // WL-Records

    private LineEdit _heightEdit => CHeightEdit; // WL-Height

    private TextEdit _oocTextEdit = null!; // WL-OOCText

    private List<ConfederationRecordsPrototype> _confederations = new(); // WL-Recordss

    public void RefreshSkills()
    {
        _skillsWindow?.Dispose();

        if (Profile == null)
            return;

        var skillsSystem = _entManager.System<SharedSkillsSystem>();
        foreach (var (jobId, skills) in Profile.Skills.ToList())
        {
            if (!_prototypeManager.TryIndex<JobPrototype>(jobId, out var jobProto))
                continue;

            var currentSkills = skills.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var defaultSkills = jobProto.DefaultSkills.ToDictionary(
                kvp => (byte)kvp.Key,
                kvp => kvp.Value
            );

            var bonusPoints = jobProto.BonusSkillPoints;
            var racialBonus = CalculateRacialBonus(Profile.Species.Id, Profile.Age);
            var totalPoints = bonusPoints + racialBonus;

            var spentPoints = CalculateSpentPoints(skillsSystem, currentSkills, defaultSkills);

            if (spentPoints > totalPoints)
            {
                foreach (var (skillKey, defaultValue) in defaultSkills)
                {
                    Profile = Profile.WithSkill(jobId, skillKey, defaultValue);
                }

                var skillsToReset = currentSkills.Keys.Except(defaultSkills.Keys).ToList();
                foreach (var skillKey in skillsToReset)
                {
                    Profile = Profile.WithSkill(jobId, skillKey, 1);
                }

                SetDirty();
            }
        }
    }

    private int CalculateSpentPoints(SharedSkillsSystem skillsSystem, Dictionary<byte, int> currentSkills, Dictionary<byte, int> defaultSkills)
    {
        var spentPoints = 0;
        foreach (var (skillKey, currentLevel) in currentSkills)
        {
            if (!Enum.IsDefined(typeof(SkillType), (SkillType)skillKey))
                continue;

            var skillType = (SkillType)skillKey;
            var defaultLevel = defaultSkills.GetValueOrDefault(skillKey, 1);

            if (currentLevel > defaultLevel)
            {
                var currentCost = skillsSystem.GetSkillTotalCost(skillType, currentLevel);
                var defaultCost = skillsSystem.GetSkillTotalCost(skillType, defaultLevel);
                spentPoints += currentCost - defaultCost;
            }
        }

        return spentPoints;
    }

    public void RefreshRecords()
    {
        if (_recordsTab != null)
            return;

        _recordsTab = new RecordsTab();
        TabContainer.AddChild(_recordsTab);
        TabContainer.SetTabTitle(TabContainer.ChildCount - 1, Loc.GetString("humanoid-profile-editor-records-tab"));

        _medicalRecordEdit = _recordsTab.MedicalRecordInput;
        _securityRecordEdit = _recordsTab.SecurityRecordInput;
        _employmentRecordEdit = _recordsTab.EmploymentRecordInput;

        _generalRecordNameEdit = _recordsTab.NameEdit;
        _generalRecordAgeEdit = _recordsTab.AgeEdit;
        _generalRecordCountryEdit = _recordsTab.CountryEdit;

        _confederationButton = _recordsTab.ConfederationButton;

        _recordsTab.OnMedicalRecordChanged += OnMedicalRecordChange;
        _recordsTab.OnSecurityRecordChanged += OnSecurityRecordChange;
        _recordsTab.OnEmploymentRecordChanged += OnEmploymentRecordChange;

        _recordsTab.OnGeneralRecordNameChanged += OnGeneralRecordNameChanged;
        _recordsTab.OnGeneralRecordAgeChanged += OnGeneralRecordDateOfBirthChanged;
        _recordsTab.OnGeneralRecordCountryChanged += OnGeneralRecordCountryChanged;

        _recordsTab.OnGeneralRecordConfederationChanged += SetConfederation;

        _confederations.AddRange(_prototypeManager.EnumeratePrototypes<ConfederationRecordsPrototype>());

        _confederations.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));

        for (var i = 0; i < _confederations.Count; i++)
        {
            var name = Loc.GetString(_confederations[i].Name);

            _recordsTab.ConfederationButton.AddItem(name, i);

            if (_confederations[i].ID == "NoConfederation")
            {
                _recordsTab.ConfederationButton.SelectId(i);
            }
        }
    }

    private void OnMedicalRecordChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithMedicalRecord(content);
        SetDirty();
    }

    private void OnSecurityRecordChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithSecurityRecord(content);
        SetDirty();
    }

    private void OnEmploymentRecordChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithEmploymentRecord(content);
        SetDirty();
    }

    private void OnGeneralRecordNameChanged(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithFullName(content);
        SetDirty();
    }

    private void OnGeneralRecordDateOfBirthChanged(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithDateOfBirth(content);
        SetDirty();
    }

    private void OnGeneralRecordCountryChanged(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithCountry(content);
        SetDirty();
    }

    private void SetConfederation(OptionButton.ItemSelectedEventArgs args)
    {
        if (_confederationButton is null)
            return;

        if (Profile is null)
            return;

        _confederationButton.SelectId(args.Id);
        Profile = Profile.WithConfederation(_confederations[args.Id].ID);
        SetDirty();
    }

    private void UpdateRecordsEdit()
    {
        if (_medicalRecordEdit != null)
            _medicalRecordEdit.TextRope = new Rope.Leaf(Profile?.MedicalRecord ?? "");

        if (_securityRecordEdit != null)
            _securityRecordEdit.TextRope = new Rope.Leaf(Profile?.SecurityRecord ?? "");

        if (_employmentRecordEdit != null)
            _employmentRecordEdit.TextRope = new Rope.Leaf(Profile?.EmploymentRecord ?? "");

        if (_generalRecordNameEdit != null)
            _generalRecordNameEdit.Text = Profile?.FullName ?? "";

        if (_generalRecordAgeEdit != null)
            _generalRecordAgeEdit.Text = Profile?.DateOfBirth ?? "";

        if (_generalRecordCountryEdit != null)
            _generalRecordCountryEdit.Text = Profile?.Country ?? "";

        if (_confederationButton != null)
            for (var i = 0; i < _confederations.Count; i++)
            {
                if (Profile?.Confederation.Equals(_confederations[i].ID) == true)
                {
                    _confederationButton.SelectId(i);
                }
            }
    }

    private void UpdateHeightEdit()
    {
        _heightEdit.Text = Profile?.Height.ToString() ?? "";
    }

    private void UpdateOocTextEdit()
    {
        if(_oocTextEdit != null)
        {
            _oocTextEdit.TextRope = new Rope.Leaf(Profile?.OocText ?? "");
        }
    }

    private void UpdateJobSubnameControls()
    {
        if (Profile == null)
            return;

        foreach (var jobSelector in _jobPriorities)
        {
            var jobId = jobSelector.Item1; //WL-Changes
            if (!Profile.JobSubnames.TryGetValue(jobId, out var subname))
                continue;

            jobSelector.Item2.SelectItem(subname, true); //WL-Changes
        }
    }

    private void OpenSkills(JobPrototype? jobProto)
    {
        _skillsWindow?.Dispose();
        _skillsWindow = null;

        if (jobProto == null || Profile == null)
            return;

        JobOverride = jobProto;

        var currentSkills = Profile.Skills.GetValueOrDefault(jobProto.ID, new Dictionary<byte, int>());
        var defaultSkills = jobProto.DefaultSkills.ToDictionary(
            kvp => (byte)kvp.Key,
            kvp => kvp.Value
        );

        var bonusPoints = jobProto.BonusSkillPoints;
        var racialBonus = CalculateRacialBonus(Profile.Species, Profile.Age);
        var totalPoints = bonusPoints + racialBonus;

        _skillsWindow = new SkillsWindow(jobProto.ID, currentSkills, defaultSkills, totalPoints);
        _skillsWindow.OnSkillChanged += (jobId, skillKey, newLevel) =>
        {
            Profile = Profile.WithSkill(jobId, skillKey, newLevel);
            SetDirty();
        };

        _skillsWindow.OnClose += () =>
        {
            JobOverride = null;
            ReloadPreview();
        };

        _skillsWindow.OpenCenteredLeft();
        JobOverride = jobProto;
        ReloadPreview();
    }

    private int CalculateRacialBonus(string species, int age)
    {
        var bonus = 0;
        foreach (var racialBonusProto in _prototypeManager.EnumeratePrototypes<RacialSkillBonusPrototype>())
        {
            if (racialBonusProto.Species != species)
                continue;

            bonus = racialBonusProto.GetBonusForAge(age);
            break;
        }

        return bonus;
    }

    private void OnOocTextChange(string content)
    {
        if (Profile is null)
            return;

        Profile = Profile.WithOocText(content);
        SetDirty();
    }

    private void SetCharHeight(int newHeight)
    {
        Profile = Profile?.WithHeight(newHeight);
        IsDirty = true;
    }
}
