using Robust.Shared.Random;
using System.Numerics;

namespace Content.Shared._WL.Random.Extensions;

public static class RobustRandomExt
{
    public static Vector2 Next(this IRobustRandom rand, Box2 box)
    {
        return rand.NextVector2Box(box.Left, box.Bottom, box.Right, box.Top);
    }
}
