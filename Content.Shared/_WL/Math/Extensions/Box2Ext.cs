using Robust.Shared.Utility;
using static Robust.Shared.Maths.MathHelper;

namespace Content.Shared._WL.Mathemathics.Extensions // Don't name Math this again
{
    public static class Box2Ext
    {
        public static List<Box2> Subtract(this Box2 box, Box2 other, float tolerance = .0000001f)
        {
            var intersected_percentage = other.IntersectPercentage(box);

            if (CloseTo(intersected_percentage, 1f, tolerance))
                return new();

            if (other.IsEmpty() || CloseTo(intersected_percentage, 0f, tolerance))
                return new() { box };

            var intersected = other.Intersect(box);

            var list = new List<Box2>();

            // Left
            if (!CloseTo(intersected.Left, box.Left, tolerance))
            {
                var box_left = new Box2(box.Left, box.Bottom, intersected.Left, box.Top);
                list.Add(box_left);
            }

            // Right
            if (!CloseTo(intersected.Right, box.Right, tolerance))
            {
                var box_right = new Box2(intersected.Right, box.Bottom, box.Right, box.Top);
                list.Add(box_right);
            }

            // Top
            if (!CloseTo(intersected.Top, box.Top, tolerance))
            {
                var box_top = new Box2(intersected.Left, intersected.Top, intersected.Right, box.Top);
                list.Add(box_top);
            }

            // Bottom
            if (!CloseTo(intersected.Bottom, box.Bottom, tolerance))
            {
                var box_bottom = new Box2(intersected.Left, box.Bottom, intersected.Right, intersected.Bottom);
                list.Add(box_bottom);
            }

            DebugTools.Assert(list.Count != 0);

            return list;
        }
    }
}
