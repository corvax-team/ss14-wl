namespace Content.Shared._WL.Records;

public static class RecordSpeciesClassification
{
    public static bool IsManufactured(string species) => species is "Ipc" or "Android";

    public static bool IsOrganic(string species) => species is not ("Ipc" or "Android" or "Golem");
}
