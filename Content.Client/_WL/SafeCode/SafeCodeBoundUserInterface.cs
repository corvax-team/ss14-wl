using Content.Shared._WL.SafeCode;
using Robust.Client.UserInterface;

namespace Content.Client._WL.SafeCode;


public sealed partial class SafeCodeBoundUserInterface : BoundUserInterface
{
    private SafeCodeWindow? _window;

    public SafeCodeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SafeCodeWindow>();
        _window.OpenCentered();

        _window.OnSubmit += code =>
        {
            SendMessage(new SafeCodeRequestMessage(code));
        };

        _window.OnLock += () =>
        {
            SendMessage(new SafeCodeLockRequestMessage());
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SafeCodeBoundUserInterfaceState safeState)
            return;

        _window?.UpdateState(safeState.CodeLength, safeState.Locked, safeState.LastAttemptCorrect);
    }

}
