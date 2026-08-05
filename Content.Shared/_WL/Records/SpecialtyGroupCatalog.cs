namespace Content.Shared._WL.Records;

/// <summary>
/// Large and small specialty groups from the WL education article.
/// Specific specialties remain free-form.
/// </summary>
public static class SpecialtyGroupCatalog
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Subgroups =
        new Dictionary<string, IReadOnlyList<string>>
        {
        ["mathematics-and-mechanics"] =
        [
            "mathematics-and-mechanics-1",
            "mathematics-and-mechanics-2",
            "mathematics-and-mechanics-3",
        ],
        ["computer-science"] =
        [
            "computer-science-1",
            "computer-science-2",
        ],
        ["physical-sciences"] =
        [
            "physical-sciences-1",
            "physical-sciences-2",
            "physical-sciences-3",
            "physical-sciences-4",
        ],
        ["chemical-sciences"] =
        [
            "chemical-sciences-1",
            "chemical-sciences-2",
        ],
        ["biological-sciences"] =
        [
            "biological-sciences-1",
            "biological-sciences-2",
            "biological-sciences-3",
            "biological-sciences-4",
        ],
        ["planetary-and-environmental-sciences"] =
        [
            "planetary-and-environmental-sciences-1",
            "planetary-and-environmental-sciences-2",
            "planetary-and-environmental-sciences-3",
            "planetary-and-environmental-sciences-4",
        ],
        ["construction-and-architecture"] =
        [
            "construction-and-architecture-1",
            "construction-and-architecture-2",
            "construction-and-architecture-3",
            "construction-and-architecture-4",
        ],
        ["electronics-and-communications"] =
        [
            "electronics-and-communications-1",
            "electronics-and-communications-2",
            "electronics-and-communications-3",
            "electronics-and-communications-4",
        ],
        ["information-technology"] =
        [
            "information-technology-1",
            "information-technology-2",
            "information-technology-3",
        ],
        ["energy"] =
        [
            "energy-1",
            "energy-2",
            "energy-3",
        ],
        ["mechanical-engineering"] =
        [
            "mechanical-engineering-1",
            "mechanical-engineering-2",
            "mechanical-engineering-3",
            "mechanical-engineering-4",
        ],
        ["materials-and-chemical-technology"] =
        [
            "materials-and-chemical-technology-1",
            "materials-and-chemical-technology-2",
            "materials-and-chemical-technology-3",
        ],
        ["resource-use-and-transport"] =
        [
            "resource-use-and-transport-1",
            "resource-use-and-transport-2",
            "resource-use-and-transport-3",
        ],
        ["technosphere-safety"] =
        [
            "technosphere-safety-1",
        ],
        ["clinical-medicine"] =
        [
            "clinical-medicine-1",
            "clinical-medicine-2",
            "clinical-medicine-3",
            "clinical-medicine-4",
            "clinical-medicine-5",
            "clinical-medicine-6",
            "clinical-medicine-7",
        ],
        ["preventive-medicine"] =
        [
            "preventive-medicine-1",
            "preventive-medicine-2",
        ],
        ["medical-biological-sciences"] =
        [
            "medical-biological-sciences-1",
            "medical-biological-sciences-2",
            "medical-biological-sciences-3",
        ],
        ["pharmaceutical-sciences"] =
        [
            "pharmaceutical-sciences-1",
            "pharmaceutical-sciences-2",
        ],
        ["agronomy-and-crop-production"] =
        [
            "agronomy-and-crop-production-1",
            "agronomy-and-crop-production-2",
            "agronomy-and-crop-production-3",
        ],
        ["forestry-and-water-management"] =
        [
            "forestry-and-water-management-1",
            "forestry-and-water-management-2",
        ],
        ["animal-husbandry-and-veterinary"] =
        [
            "animal-husbandry-and-veterinary-1",
            "animal-husbandry-and-veterinary-2",
            "animal-husbandry-and-veterinary-3",
        ],
        ["agricultural-engineering-and-food-technology"] =
        [
            "agricultural-engineering-and-food-technology-1",
            "agricultural-engineering-and-food-technology-2",
            "agricultural-engineering-and-food-technology-3",
        ],
        ["law-and-politics"] =
        [
            "law-and-politics-1",
            "law-and-politics-2",
            "law-and-politics-3",
        ],
        ["economics-and-management"] =
        [
            "economics-and-management-1",
            "economics-and-management-2",
            "economics-and-management-3",
        ],
        ["psychology-and-sociology"] =
        [
            "psychology-and-sociology-1",
            "psychology-and-sociology-2",
            "psychology-and-sociology-3",
            "psychology-and-sociology-4",
        ],
        ["history-and-philosophy"] =
        [
            "history-and-philosophy-1",
            "history-and-philosophy-2",
            "history-and-philosophy-3",
            "history-and-philosophy-4",
        ],
        ["pedagogy-and-philology"] =
        [
            "pedagogy-and-philology-1",
            "pedagogy-and-philology-2",
            "pedagogy-and-philology-3",
        ],
        ["arts-and-cognitive-sciences"] =
        [
            "arts-and-cognitive-sciences-1",
            "arts-and-cognitive-sciences-2",
            "arts-and-cognitive-sciences-3",
            "arts-and-cognitive-sciences-4",
            "arts-and-cognitive-sciences-5",
            "arts-and-cognitive-sciences-6",
            "arts-and-cognitive-sciences-7",
            "arts-and-cognitive-sciences-8",
            "arts-and-cognitive-sciences-9",
            "arts-and-cognitive-sciences-10",
        ],
        ["military-training-and-education"] =
        [
            "military-training-and-education-1",
            "military-training-and-education-2",
            "military-training-and-education-3",
        ],
        ["strategy-and-operational-art"] =
        [
            "strategy-and-operational-art-1",
            "strategy-and-operational-art-2",
            "strategy-and-operational-art-3",
        ],
        ["security-and-law-enforcement"] =
        [
            "security-and-law-enforcement-1",
            "security-and-law-enforcement-2",
        ],
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

        return false;
    }

    public static string GetSubgroupLocalizationKey(string subgroup) =>
        $"records-specialty-subgroup-value-{subgroup}";
}
