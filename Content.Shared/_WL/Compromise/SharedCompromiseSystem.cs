using Content.Shared.IdentityManagement;
using Content.Shared.Security;
using Content.Shared.Security.Components;
using Content.Shared.Station;
using Content.Shared.Station.Systems;
using Content.Shared.StationRecords;
using Content.Shared.StationRecords.Systems;

namespace Content.Shared._WL.Compromise;

public abstract partial class SharedCompromiseSystem : EntitySystem
{
    public string BuildCompromiseText(CompromiseRecord record)
    {
        var message = string.Empty;

        if (!record.Valid)
            return message;

        foreach (var compromiseId in record.CompromiseData)
        {
            if (!ProtoMan.TryIndex<CompromisePrototype>(compromiseId, out var compromise) ||
                    !ProtoMan.TryIndex<CompromiseCategoryPrototype>(compromise.Category, out var category))
                continue;

            message += $"{category.LocalizedName}: {compromise.LocalizedName}. {compromise.LocalizedDesc}\n\n";
        }

        message += record.CompromiseText;

        return (message);
    }
}
