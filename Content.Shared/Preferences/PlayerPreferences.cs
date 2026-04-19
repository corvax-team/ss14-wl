using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences
{
    /// <summary>
    ///     Contains all player characters and the index of the currently selected character.
    ///     Serialized both over the network and to disk.
    /// </summary>
    [Serializable]
    [NetSerializable]
    public sealed class PlayerPreferences
    {
        private Dictionary<int, HumanoidCharacterProfile> _characters;

        public PlayerPreferences(IEnumerable<KeyValuePair<int, HumanoidCharacterProfile>> characters, int selectedCharacterIndex, Color adminOOCColor, List<ProtoId<ConstructionPrototype>> constructionFavorites, /*WL-Changes: Sponsor*/Color sponsorColor/*WL-Changes: Sponsor*/)
        {
            _characters = new Dictionary<int, HumanoidCharacterProfile>(characters);
            SelectedCharacterIndex = selectedCharacterIndex;
            AdminOOCColor = adminOOCColor;
            ConstructionFavorites = constructionFavorites;
            //WL-Changes: Sponsor start
            SponsorColor = sponsorColor;
            //WL-Changes: Sponsor end
        }

        /// <summary>
        ///     All player characters.
        /// </summary>
        public IReadOnlyDictionary<int, HumanoidCharacterProfile> Characters => _characters;

        public HumanoidCharacterProfile GetProfile(int index)
        {
            return _characters[index];
        }

        /// <summary>
        ///     Index of the currently selected character.
        /// </summary>
        public int SelectedCharacterIndex { get; }

        /// <summary>
        ///     The currently selected character.
        /// </summary>
        public HumanoidCharacterProfile SelectedCharacter => Characters[SelectedCharacterIndex];

        public Color AdminOOCColor { get; set; }

        //WL-Changes: Sponsor start
        public Color SponsorColor { get; set; }
        //WL-Changes: Sponsor end

        /// <summary>
        ///    List of favorite items in the construction menu.
        /// </summary>
        public List<ProtoId<ConstructionPrototype>> ConstructionFavorites { get; set; } = [];

        public int IndexOfCharacter(HumanoidCharacterProfile profile)
        {
            return _characters.FirstOrNull(p => p.Value == profile)?.Key ?? -1;
        }

        public bool TryIndexOfCharacter(HumanoidCharacterProfile profile, out int index)
        {
            return (index = IndexOfCharacter(profile)) != -1;
        }
    }
}
