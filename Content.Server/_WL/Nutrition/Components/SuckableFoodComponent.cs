using Content.Server._WL.Nutrition.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Nutrition.Components;

[RegisterComponent]
public sealed partial class SuckableFoodComponent : Component
{
    [DataField]
    public string Solution { get; set; } = "food";

    /// <summary>
    /// Количество поглощаемой из контейнера жидкости в секунду.
    /// </summary>
    [DataField]
    public FixedPoint2 DissolveAmount { get; set; } = FixedPoint2.New(0.05f);

    /// <summary>
    /// Не указывайте сущности в прототипе, у которых есть <see cref="SuckableFoodComponent"/>, иначе будет runtime-ошибочка.
    /// </summary>
    [DataField("entityOnDissolve")]
    public EntProtoId<ClothingComponent>? EquippedEntityOnDissolve { get; set; }

    [DataField]
    public ComponentRegistry? ComponentsOverride { get; set; }

    [DataField]
    public bool CanSuck { get; set; } = true;

    [DataField]
    public bool DeleteOnEmpty { get; set; } = true;

    public bool IsSucking => SuckingEntity != null && CanSuck;

    [Access(typeof(SuckableFoodSystem))]
    public EntityUid? SuckingEntity;
}
