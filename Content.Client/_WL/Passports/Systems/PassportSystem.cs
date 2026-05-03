using Content.Shared._WL.Passports.Components;
using Content.Shared._WL.Passports.Systems;
using Robust.Client.GameObjects;


namespace Content.Client._WL.Passports.Systems;

public sealed class PassportSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, ComponentStartup>(OnPassportStartup);
        SubscribeLocalEvent<PassportComponent, SharedPassportSystem.PassportToggleEvent>(OnPassportToggled);
    }

    private void OnPassportToggled(Entity<PassportComponent> passport, ref SharedPassportSystem.PassportToggleEvent evt)
    {
        if (evt.Handled || !TryComp<SpriteComponent>(passport, out var sprite))
            return;

        var currentState = sprite.LayerGetState(0);

        if (currentState.Name == null)
            return;

        evt.Handled = true;

        var currentName = currentState.Name;
        var prefix = currentName;

        if (currentName.EndsWith("_open", StringComparison.Ordinal))
            prefix = currentName[..^"_open".Length];
        else if (currentName.EndsWith("_closed", StringComparison.Ordinal))
            prefix = currentName[..^"_closed".Length];

        var desiredStateName = prefix + (passport.Comp.IsClosed ? "_closed" : "_open");

        if (desiredStateName == currentName)
            return;

        if (prefix == currentName && desiredStateName.Contains("_open") && desiredStateName.Contains("_closed"))
        {
            var from = passport.Comp.IsClosed ? "_open" : "_closed";
            var to = passport.Comp.IsClosed ? "_closed" : "_open";
            desiredStateName = currentName.Replace(from, to, StringComparison.Ordinal);
        }

        if (desiredStateName != currentName)
            sprite.LayerSetState(0, desiredStateName);
    }

    private void OnPassportStartup(Entity<PassportComponent> passport, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(passport, out var sprite))
            return;

        var currentState = sprite.LayerGetState(0);
        if (currentState.Name == null)
            return;

        var currentName = currentState.Name;
        var prefix = currentName;

        if (currentName.EndsWith("_open", StringComparison.Ordinal))
            prefix = currentName[..^"_open".Length];
        else if (currentName.EndsWith("_closed", StringComparison.Ordinal))
            prefix = currentName[..^"_closed".Length];

        var desiredStateName = prefix + (passport.Comp.IsClosed ? "_closed" : "_open");

        if (desiredStateName == currentName)
            return;

        if (prefix == currentName && desiredStateName.Contains("_open") && desiredStateName.Contains("_closed"))
        {
            var from = passport.Comp.IsClosed ? "_open" : "_closed";
            var to = passport.Comp.IsClosed ? "_closed" : "_open";
            desiredStateName = currentName.Replace(from, to, StringComparison.Ordinal);
        }

        if (desiredStateName != currentName)
            sprite.LayerSetState(0, desiredStateName);

        Dirty(passport);
    }
}
