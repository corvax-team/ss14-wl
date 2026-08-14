using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Records;

[DataDefinition]
public sealed partial class SpecialtySection
{
    [DataField(required: true)]
    public string Id { get; private set; } = string.Empty;

    [DataField(required: true)]
    public List<string> Groups { get; private set; } = new();
}

[Prototype]
public sealed partial class SpecialtyGroupCatalogPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public List<SpecialtySection> Sections { get; private set; } = new();

    [DataField(required: true)]
    public Dictionary<string, int> SubgroupCounts { get; private set; } = new();

    [DataField]
    public Dictionary<string, int> LegacySubgroupCounts { get; private set; } = new();
}

/// <summary>
/// Large and small specialty groups from the WL education article.
/// Specific specialties remain free-form.
/// </summary>
public static class SpecialtyGroupCatalog
{
    public static readonly ProtoId<SpecialtyGroupCatalogPrototype> DefaultCatalog = "WLRecordsSpecialties";

    public static IReadOnlyList<SpecialtySection> GetSections(IPrototypeManager prototypes) =>
        prototypes.Index(DefaultCatalog).Sections;

    public static IReadOnlyList<string> GetGroups(IPrototypeManager prototypes) =>
        GetSections(prototypes).SelectMany(section => section.Groups).ToArray();

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> GetSubgroups(IPrototypeManager prototypes) =>
        prototypes.Index(DefaultCatalog).SubgroupCounts.ToDictionary(
            entry => entry.Key,
            entry => (IReadOnlyList<string>) CreateSubgroups(entry.Key, entry.Value));

    public static IReadOnlyList<string> GetSubgroups(IPrototypeManager prototypes, string group)
    {
        var catalog = prototypes.Index(DefaultCatalog);
        return catalog.SubgroupCounts.TryGetValue(group, out var count)
            ? CreateSubgroups(group, count)
            : Array.Empty<string>();
    }

    public static bool ContainsSubgroup(IPrototypeManager prototypes, string subgroup)
    {
        foreach (var subgroups in GetSubgroups(prototypes).Values)
        {
            foreach (var candidate in subgroups)
            {
                if (candidate == subgroup)
                    return true;
            }
        }

        return CreateLegacySubgroups(prototypes).Contains(subgroup);
    }

    public static bool ContainsGroup(IPrototypeManager prototypes, string group)
    {
        foreach (var candidate in GetGroups(prototypes))
        {
            if (candidate == group)
                return true;
        }

        return prototypes.Index(DefaultCatalog).LegacySubgroupCounts.ContainsKey(group);
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

    private static IReadOnlySet<string> CreateLegacySubgroups(IPrototypeManager prototypes)
    {
        var subgroups = new HashSet<string>();
        foreach (var (group, count) in prototypes.Index(DefaultCatalog).LegacySubgroupCounts)
        {
            for (var i = 1; i <= count; i++)
            {
                subgroups.Add($"{group}-{i}");
            }
        }

        return subgroups;
    }
}
