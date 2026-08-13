using System.Linq;

namespace Content.Shared._WL.Records;

public readonly record struct SpecialtySection(string Id, IReadOnlyList<string> Groups);

/// <summary>
/// Large and small specialty groups from the WL education article.
/// Specific specialties remain free-form.
/// </summary>
public static class SpecialtyGroupCatalog
{
    // Keep recognizing the catalog shipped with the initial records rework so
    // already-saved education entries retain their original display names.
    private static readonly IReadOnlyDictionary<string, int> LegacySubgroupCounts =
        new Dictionary<string, int>
        {
            ["mathematics-and-mechanics"] = 3,
            ["computer-science"] = 2,
            ["physical-sciences"] = 4,
            ["chemical-sciences"] = 2,
            ["biological-sciences"] = 4,
            ["planetary-and-environmental-sciences"] = 4,
            ["construction-and-architecture"] = 4,
            ["electronics-and-communications"] = 4,
            ["information-technology"] = 3,
            ["energy"] = 3,
            ["mechanical-engineering"] = 4,
            ["materials-and-chemical-technology"] = 3,
            ["resource-use-and-transport"] = 3,
            ["technosphere-safety"] = 1,
            ["clinical-medicine"] = 7,
            ["preventive-medicine"] = 2,
            ["medical-biological-sciences"] = 3,
            ["pharmaceutical-sciences"] = 2,
            ["agronomy-and-crop-production"] = 3,
            ["forestry-and-water-management"] = 2,
            ["animal-husbandry-and-veterinary"] = 3,
            ["agricultural-engineering-and-food-technology"] = 3,
            ["law-and-politics"] = 3,
            ["economics-and-management"] = 3,
            ["psychology-and-sociology"] = 4,
            ["history-and-philosophy"] = 4,
            ["pedagogy-and-philology"] = 3,
            ["arts-and-cognitive-sciences"] = 10,
            ["military-training-and-education"] = 3,
            ["strategy-and-operational-art"] = 3,
            ["security-and-law-enforcement"] = 2,
        };

    private static readonly IReadOnlySet<string> LegacySubgroups = CreateLegacySubgroups();

    public static readonly IReadOnlyList<SpecialtySection> Sections =
    [
        new("natural-sciences",
        [
            "mathematics-and-mechanics",
            "computer-and-information-sciences",
            "physics-and-astronomy",
            "chemistry",
            "earth-sciences",
            "biological-sciences",
        ]),
        new("technical-sciences",
        [
            "architecture",
            "construction-engineering-and-technology",
            "computer-science-and-engineering",
            "information-security",
            "electronics-radio-engineering-and-communications",
            "photonics-instrumentation-optical-and-biotechnical-systems",
            "electrical-and-thermal-power-engineering",
            "nuclear-power-and-technology",
            "mechanical-engineering",
            "physical-and-technical-sciences-and-technologies",
            "chemical-technologies",
            "technosphere-safety-and-environmental-engineering",
            "applied-geology-mining-oil-and-gas-and-geodesy",
            "materials-technology",
            "ground-transport-engineering-and-technology",
            "aviation-and-aerospace-engineering",
            "air-navigation-and-aerospace-operation",
            "shipbuilding-and-water-transport-engineering",
            "control-in-technical-systems",
            "nanotechnology-and-nanomaterials",
            "light-industry-technologies",
        ]),
        new("medical-sciences",
        [
            "clinical-medicine",
            "preventive-medicine",
            "medical-biological-sciences",
            "pharmaceutical-sciences",
        ]),
        new("military-and-security-sciences",
        [
            "weapons-and-armament-systems",
            "military-training-and-education",
            "strategy-and-operational-art",
            "security-and-law-enforcement",
        ]),
        new("agricultural-sciences",
        [
            "industrial-ecology-and-biotechnology",
            "agriculture-forestry-and-fisheries",
            "veterinary-and-animal-science",
        ]),
        new("social-and-humanities",
        [
            "economics-and-management",
            "sociology-and-social-work",
            "jurisprudence",
            "political-sciences",
            "mass-media-and-library-science",
            "service-and-tourism",
            "education-and-pedagogical-sciences",
            "linguistics-and-literary-studies",
            "history-and-archaeology",
            "philosophy-ethics-and-religious-studies",
            "physical-education-and-sports",
            "art-history",
            "cultural-studies-and-sociocultural-projects",
            "performing-arts-and-literary-creation",
            "musical-art",
            "visual-and-applied-arts",
            "screen-arts",
        ]),
    ];

    public static readonly IReadOnlyList<string> Groups =
        Sections.SelectMany(section => section.Groups).ToArray();

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Subgroups =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["mathematics-and-mechanics"] = CreateSubgroups("mathematics-and-mechanics", 6),
            ["computer-and-information-sciences"] = CreateSubgroups("computer-and-information-sciences", 4),
            ["physics-and-astronomy"] = CreateSubgroups("physics-and-astronomy", 6),
            ["chemistry"] = CreateSubgroups("chemistry", 3),
            ["earth-sciences"] = CreateSubgroups("earth-sciences", 6),
            ["biological-sciences"] = CreateSubgroups("biological-sciences", 4),
            ["architecture"] = CreateSubgroups("architecture", 8),
            ["construction-engineering-and-technology"] = CreateSubgroups("construction-engineering-and-technology", 3),
            ["computer-science-and-engineering"] = CreateSubgroups("computer-science-and-engineering", 4),
            ["information-security"] = CreateSubgroups("information-security", 6),
            ["electronics-radio-engineering-and-communications"] = CreateSubgroups("electronics-radio-engineering-and-communications", 6),
            ["photonics-instrumentation-optical-and-biotechnical-systems"] = CreateSubgroups("photonics-instrumentation-optical-and-biotechnical-systems", 6),
            ["electrical-and-thermal-power-engineering"] = CreateSubgroups("electrical-and-thermal-power-engineering", 3),
            ["nuclear-power-and-technology"] = CreateSubgroups("nuclear-power-and-technology", 5),
            ["mechanical-engineering"] = CreateSubgroups("mechanical-engineering", 7),
            ["physical-and-technical-sciences-and-technologies"] = CreateSubgroups("physical-and-technical-sciences-and-technologies", 4),
            ["chemical-technologies"] = CreateSubgroups("chemical-technologies", 4),
            ["technosphere-safety-and-environmental-engineering"] = CreateSubgroups("technosphere-safety-and-environmental-engineering", 3),
            ["applied-geology-mining-oil-and-gas-and-geodesy"] = CreateSubgroups("applied-geology-mining-oil-and-gas-and-geodesy", 9),
            ["materials-technology"] = CreateSubgroups("materials-technology", 2),
            ["ground-transport-engineering-and-technology"] = CreateSubgroups("ground-transport-engineering-and-technology", 9),
            ["aviation-and-aerospace-engineering"] = CreateSubgroups("aviation-and-aerospace-engineering", 12),
            ["air-navigation-and-aerospace-operation"] = CreateSubgroups("air-navigation-and-aerospace-operation", 9),
            ["shipbuilding-and-water-transport-engineering"] = CreateSubgroups("shipbuilding-and-water-transport-engineering", 11),
            ["control-in-technical-systems"] = CreateSubgroups("control-in-technical-systems", 6),
            ["nanotechnology-and-nanomaterials"] = CreateSubgroups("nanotechnology-and-nanomaterials", 3),
            ["light-industry-technologies"] = CreateSubgroups("light-industry-technologies", 5),
            ["clinical-medicine"] = CreateSubgroups("clinical-medicine", 7),
            ["preventive-medicine"] = CreateSubgroups("preventive-medicine", 2),
            ["medical-biological-sciences"] = CreateSubgroups("medical-biological-sciences", 3),
            ["pharmaceutical-sciences"] = CreateSubgroups("pharmaceutical-sciences", 2),
            ["weapons-and-armament-systems"] = CreateSubgroups("weapons-and-armament-systems", 4),
            ["military-training-and-education"] = CreateSubgroups("military-training-and-education", 3),
            ["strategy-and-operational-art"] = CreateSubgroups("strategy-and-operational-art", 3),
            ["security-and-law-enforcement"] = CreateSubgroups("security-and-law-enforcement", 2),
            ["industrial-ecology-and-biotechnology"] = CreateSubgroups("industrial-ecology-and-biotechnology", 4),
            ["agriculture-forestry-and-fisheries"] = CreateSubgroups("agriculture-forestry-and-fisheries", 11),
            ["veterinary-and-animal-science"] = CreateSubgroups("veterinary-and-animal-science", 3),
            ["economics-and-management"] = CreateSubgroups("economics-and-management", 10),
            ["sociology-and-social-work"] = CreateSubgroups("sociology-and-social-work", 4),
            ["jurisprudence"] = CreateSubgroups("jurisprudence", 5),
            ["political-sciences"] = CreateSubgroups("political-sciences", 3),
            ["mass-media-and-library-science"] = CreateSubgroups("mass-media-and-library-science", 5),
            ["service-and-tourism"] = CreateSubgroups("service-and-tourism", 3),
            ["education-and-pedagogical-sciences"] = CreateSubgroups("education-and-pedagogical-sciences", 7),
            ["linguistics-and-literary-studies"] = CreateSubgroups("linguistics-and-literary-studies", 5),
            ["history-and-archaeology"] = CreateSubgroups("history-and-archaeology", 5),
            ["philosophy-ethics-and-religious-studies"] = CreateSubgroups("philosophy-ethics-and-religious-studies", 4),
            ["physical-education-and-sports"] = CreateSubgroups("physical-education-and-sports", 4),
            ["art-history"] = CreateSubgroups("art-history", 4),
            ["cultural-studies-and-sociocultural-projects"] = CreateSubgroups("cultural-studies-and-sociocultural-projects", 7),
            ["performing-arts-and-literary-creation"] = CreateSubgroups("performing-arts-and-literary-creation", 11),
            ["musical-art"] = CreateSubgroups("musical-art", 13),
            ["visual-and-applied-arts"] = CreateSubgroups("visual-and-applied-arts", 10),
            ["screen-arts"] = CreateSubgroups("screen-arts", 5),
        };

    public static IReadOnlyList<string> GetSubgroups(string group)
    {
        return Subgroups.TryGetValue(group, out var subgroups)
            ? subgroups
            : Array.Empty<string>();
    }

    public static bool ContainsSubgroup(string subgroup)
    {
        foreach (var subgroups in Subgroups.Values)
        {
            foreach (var candidate in subgroups)
            {
                if (candidate == subgroup)
                    return true;
            }
        }

        return LegacySubgroups.Contains(subgroup);
    }

    public static bool ContainsGroup(string group)
    {
        foreach (var candidate in Groups)
        {
            if (candidate == group)
                return true;
        }

        return LegacySubgroupCounts.ContainsKey(group);
    }

    public static string GetSubgroupLocalizationKey(string subgroup) =>
        $"records-specialty-subgroup-value-{subgroup}";

    private static IReadOnlyList<string> CreateSubgroups(string group, int count)
    {
        var subgroups = new string[count];
        for (var i = 0; i < count; i++)
        {
            subgroups[i] = $"{group}-2026-{i + 1}";
        }

        return subgroups;
    }

    private static IReadOnlySet<string> CreateLegacySubgroups()
    {
        var subgroups = new HashSet<string>();
        foreach (var (group, count) in LegacySubgroupCounts)
        {
            for (var i = 1; i <= count; i++)
            {
                subgroups.Add($"{group}-{i}");
            }
        }

        return subgroups;
    }
}
