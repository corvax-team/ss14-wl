using Robust.Shared.Audio;

namespace Content.Server._WL.Economics.Components
{
    [RegisterComponent]
    public sealed partial class PokerCardComponent : Component
    {
        [DataField]
        public SoundSpecifier FlipSound = new SoundPathSpecifier(@"/Audio/_WL/Economics/flip.ogg");

        [DataField]
        public string FlippedCardName = "перевёрнутая карта";

        [DataField]
        public bool FlipPopup = true;

        public string OriginalName = "";
    }
}
