using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.UserInterface.Controls;
using System.Linq;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMachineMenu? _menu;

        [ViewVariables]
        private List<VendingMachineInventoryEntry> _cachedInventory = new();

        [ViewVariables]
        private List<int> _cachedFilteredIndex = new();

        private readonly VendingMachineSystem _vending = default!;
        private readonly VendingMachineComponent _vendingMachineComponent = default!;

        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            _vending = EntMan.System<VendingMachineSystem>();

            _vendingMachineComponent = EntMan.GetComponent<VendingMachineComponent>(Owner);
        }

        protected override void Open()
        {
            base.Open();

            _cachedInventory = _vending.GetAllInventory(Owner);

            _menu = new VendingMachineMenu { Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName };

            _menu.OnClose += Close;
            _menu.OnItemSelected += OnItemSelected;
            _menu.OnSearchChanged += OnSearchChanged;

            _menu.Populate(_cachedInventory, out _cachedFilteredIndex, _vending.GetBalance("Credit", Owner, _vendingMachineComponent));

            _menu.OpenCenteredLeft();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not VendingMachineInterfaceState newState)
                return;

            _cachedInventory = newState.Inventory;

            _menu?.Populate(_cachedInventory, out _cachedFilteredIndex, _vending.GetBalance("Credit", Owner, _vendingMachineComponent), _menu.SearchBar.Text);
        }

        private void OnItemSelected(ItemList.ItemListSelectedEventArgs args)
        {
            if (_cachedInventory.Count == 0)
                return;

            var selectedItem = _cachedInventory.ElementAtOrDefault(_cachedFilteredIndex.ElementAtOrDefault(args.ItemIndex));

            if (selectedItem == null)
                return;

            SendMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_menu == null)
                return;

            _menu.OnItemSelected -= OnItemSelected;
            _menu.OnClose -= Close;
            _menu.Dispose();
        }

        private void OnSearchChanged(string? filter)
        {
            _menu?.Populate(_cachedInventory, out _cachedFilteredIndex, _vending.GetBalance("Credit", Owner, _vendingMachineComponent), filter);
        }
    }
}
