using Content.Shared._WL.Passports.Components;
using Content.Shared._WL.Passports.Events;
using Robust.Client.GameObjects;


namespace Content.Client._WL.Passports.Systems;

public sealed partial class PassportSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, ComponentStartup>(OnPassportStartup);
        SubscribeLocalEvent<PassportComponent, PassportToggleEvent>(OnPassportToggled);
    }

    private void OnPassportToggled(Entity<PassportComponent> passport, ref PassportToggleEvent evt)
    {
        if (evt.Handled || !TryComp<SpriteComponent>(passport, out var sprite))
            return;

        if (!_sprite.TryGetLayer((passport.Owner, sprite), 0, out var currentLayer, true))
            return;

        if (currentLayer.State.Name is not { } currentName)
            return;

        evt.Handled = true;

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
            _sprite.LayerSetRsiState((passport.Owner, sprite), 0, desiredStateName);
    }

    private void OnPassportStartup(Entity<PassportComponent> passport, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(passport, out var sprite))
            return;

        if (!_sprite.TryGetLayer((passport.Owner, sprite), 0, out var currentLayer, true))
            return;

        if (currentLayer.State.Name is not { } currentName)
            return;

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
            _sprite.LayerSetRsiState((passport.Owner, sprite), 0, desiredStateName);

        Dirty(passport);
    }
}
