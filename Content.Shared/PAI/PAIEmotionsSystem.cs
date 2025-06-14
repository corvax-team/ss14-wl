using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.PAI;
using Content.Shared.Appearance;
using Robust.Shared.Timing;

namespace Content.Shared.PAI;

/// <summary>
/// Система управления эмоциями ПИИ
/// </summary>
public sealed class PAIEmotionsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PAIEmotionsComponent, PAIEmotionActionEvent>(OnEmotionAction);
        SubscribeLocalEvent<PAIEmotionsComponent, PAIEmotionChangeDoAfterEvent>(OnEmotionChangeDoAfter);
        SubscribeLocalEvent<PAIEmotionsComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<PAIEmotionsComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnEmotionAction(EntityUid uid, PAIEmotionsComponent component, PAIEmotionActionEvent args)
    {
        if (args.Handled)
            return;

        TryChangeEmotion(uid, args.Emotion, component);
        args.Handled = true;
    }

    /// <summary>
    /// Попытка сменить эмоцию ПИИ
    /// </summary>
    public bool TryChangeEmotion(EntityUid uid, PAIEmotion newEmotion, PAIEmotionsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        // Проверяем кулдаун
        var currentTime = _timing.CurTime;
        if (currentTime - component.LastEmotionChange < TimeSpan.FromSeconds(component.EmotionCooldown))
            return false;

        // Если эмоция уже установлена, не меняем
        if (component.CurrentEmotion == newEmotion)
            return false;

        // Запускаем DoAfter для смены эмоции
        var doAfterArgs = new DoAfterArgs(EntityManager, uid, 1.0f, new PAIEmotionChangeDoAfterEvent(newEmotion), uid)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false
        };

        return _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEmotionChangeDoAfter(EntityUid uid, PAIEmotionsComponent component, PAIEmotionChangeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        component.CurrentEmotion = args.NewEmotion;
        component.LastEmotionChange = _timing.CurTime;

        Dirty(uid, component);
        args.Handled = true;

        // Обновляем внешний вид
        UpdateAppearance(uid, component);
    }

    private void UpdateAppearance(EntityUid uid, PAIEmotionsComponent component)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        _appearance.SetData(uid, PAIEmotionVisuals.Emotion, component.CurrentEmotion, appearance);
    }

    private void OnGetState(EntityUid uid, PAIEmotionsComponent component, ref ComponentGetState args)
    {
        args.State = new PAIEmotionsComponentState
        {
            CurrentEmotion = component.CurrentEmotion,
            LastEmotionChange = component.LastEmotionChange
        };
    }

    private void OnHandleState(EntityUid uid, PAIEmotionsComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not PAIEmotionsComponentState state)
            return;

        component.CurrentEmotion = state.CurrentEmotion;
        component.LastEmotionChange = state.LastEmotionChange;

        UpdateAppearance(uid, component);
    }
}

/// <summary>
/// Ключи для визуального отображения эмоций ПИИ
/// </summary>
[Serializable, NetSerializable]
public enum PAIEmotionVisuals : byte
{
    Emotion
}

/// <summary>
/// Состояние компонента эмоций ПИИ для сетевой синхронизации
/// </summary>
[Serializable, NetSerializable]
public sealed class PAIEmotionsComponentState : ComponentState
{
    public PAIEmotion CurrentEmotion;
    public TimeSpan LastEmotionChange;
}
