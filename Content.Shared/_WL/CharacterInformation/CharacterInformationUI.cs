using Robust.Shared.Serialization;

namespace Content.Shared._WL.CharacterInformation;

[Serializable, NetSerializable]
public enum CharacterInformationUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class CharacterInformationBuiState(
        NetEntity uid,
        string characterName,
        string flavorText,
        string? oocText) : BoundUserInterfaceState
{
    public NetEntity Uid = uid;
    public string CharacterName = characterName;
    public string FlavorText = flavorText;
    public string? OocText = oocText;
}
