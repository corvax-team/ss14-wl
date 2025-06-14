using Content.Shared.PAI;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Client.PAI;

/// <summary>
/// Система для визуального отображения эмоций ПИИ на клиенте
/// </summary>
public sealed class PAIAppearanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PAIEmotionsComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, PAIEmotionsComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!args.Data.TryGetValue(PAIEmotionVisuals.Emotion, out var emotionObj))
            return;

        if (emotionObj is not PAIEmotion emotion)
            return;

        // Обновляем спрайт в зависимости от эмоции
        UpdateSpriteForEmotion(sprite, emotion);
    }

    private void UpdateSpriteForEmotion(SpriteComponent sprite, PAIEmotion emotion)
    {
        // Базовый слой спрайта ПИИ
        const string baseLayer = "base";

        // Определяем состояние спрайта для каждой эмоции
        var spriteState = emotion switch
        {
            PAIEmotion.Happy => "pai-happy",
            PAIEmotion.Sad => "pai_-ad",
            PAIEmotion.Angry => "pai-angry",
            _ => "pai-on-overlay"
        };

        // Устанавливаем состояние спрайта
        if (sprite.LayerMapTryGet(baseLayer, out var layerIndex))
        {
            sprite.LayerSetState(layerIndex, spriteState);
        }
    }
}
