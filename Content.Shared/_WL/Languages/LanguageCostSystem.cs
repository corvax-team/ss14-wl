using System;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Languages;

/// <summary>
/// Single source of truth for language pricing.
/// </summary>
public static class LanguageCostSystem
{
    public static bool TryGetLanguagePrototype(
        TraitPrototype trait,
        IPrototypeManager prototypeManager,
        [NotNullWhen(true)] out LanguagePrototype? language)
    {
        foreach (var entry in trait.Components.Values)
        {
            if (entry.Component is not ModifyLanguagesComponent modifyLanguages ||
                modifyLanguages.SpecieLanguage ||
                modifyLanguages.Languages.Count == 0)
            {
                continue;
            }

            if (prototypeManager.TryIndex(modifyLanguages.Languages[0], out language))
                return true;
        }

        language = null;
        return false;
    }

    public static int? GetLevelCost(
        LanguagePrototype language,
        int level,
        string speciesId,
        string confederationId)
    {
        if (level <= 0 || level >= language.LevelCosts.Count)
            return null;

        var cost = language.LevelCosts[level];

        if (!string.IsNullOrEmpty(speciesId) &&
            language.SpeciesModifiers.TryGetValue(speciesId, out var speciesModifier) &&
            level < speciesModifier.LevelModifiers.Count)
        {
            cost += speciesModifier.LevelModifiers[level];
        }

        if (!string.IsNullOrEmpty(confederationId) &&
            language.ConfederationModifiers.TryGetValue(confederationId, out var confederationModifier) &&
            level < confederationModifier.LevelModifiers.Count)
        {
            cost += confederationModifier.LevelModifiers[level];
        }

        if (cost >= 999)
            return null;

        return Math.Max(0, cost);
    }

    public static bool TryGetTotalCostForLevel(
        LanguagePrototype language,
        int targetLevel,
        string speciesId,
        string confederationId,
        out int totalCost)
    {
        totalCost = 0;

        if (targetLevel < 0)
            return false;

        for (var level = 1; level <= targetLevel; level++)
        {
            var levelCost = GetLevelCost(language, level, speciesId, confederationId);
            if (!levelCost.HasValue)
            {
                totalCost = 0;
                return false;
            }

            totalCost += levelCost.Value;
        }

        return true;
    }

    public static bool TryGetUpgradeCost(
        LanguagePrototype language,
        int currentLevel,
        int targetLevel,
        string speciesId,
        string confederationId,
        out int upgradeCost)
    {
        upgradeCost = 0;

        if (currentLevel < 0 || targetLevel < currentLevel)
            return false;

        if (!TryGetTotalCostForLevel(language, currentLevel, speciesId, confederationId, out var currentCost) ||
            !TryGetTotalCostForLevel(language, targetLevel, speciesId, confederationId, out var targetCost))
        {
            return false;
        }

        upgradeCost = targetCost - currentCost;
        return true;
    }

    public static bool IsLevelConfigured(LanguagePrototype language, int level)
    {
        return level > 0 && level < language.LevelCosts.Count;
    }
}
