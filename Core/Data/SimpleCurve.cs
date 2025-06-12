using Microsoft.Xna.Framework;

namespace RoA.Core.Data;

struct SimpleCurve(Vector2 begin, Vector2 end, Vector2 control) {
    public Vector2 Begin = begin, End = end, Control = control;

    public readonly Vector2 GetPoint(float percent) {
        float num = 1f - percent;
        return num * num * Begin + 2f * num * percent * Control + percent * percent * End;
    }
}
