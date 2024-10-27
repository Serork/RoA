using Microsoft.Xna.Framework;

using System;

namespace RoA.Core;

static class Ease {
    public delegate float Easer(float value);

    public static Easer SineIn => value => -(float)Math.Cos(MathHelper.PiOver2 * value) + 1;
    public static Easer SineOut => value => (float)Math.Sin(MathHelper.PiOver2 * value);
    public static Easer SineInOut => value => -(float)Math.Cos(MathHelper.Pi * value) / 2f + 0.5f;

    public static Easer ExpoIn => value => (float)Math.Pow(2.0, 10.0 * ((double)value - 1.0));
    public static Easer ExpoOut => Invert(ExpoIn);
    public static Easer ExpoInOut => Follow(ExpoIn, ExpoOut);

    public static Easer ExpoInSinOut => Follow(ExpoIn, SineOut);
    public static Easer SineInExpoOut => Follow(SineIn, ExpoOut);

    public static Easer CubeIn => value => value * value * value;
    public static Easer CubeOut => Invert(CubeIn);
    public static Easer CubeInOut => Follow(CubeIn, CubeOut);

    public static Easer CubeInExpoOut => Follow(CubeIn, ExpoOut);

    public static Easer ExpoInCubeOut => Follow(ExpoIn, CubeOut);

    public static Easer QuadIn => value => value * value;
    public static Easer QuadOut => Invert(QuadIn);
    public static Easer QuadInOut => Follow(QuadIn, QuadOut);

    public static Easer QuartIn => value => value * value * value * value;
    public static Easer QuartOut => value => 1f - (float)Math.Pow(1.0 - (double)value, 4);
    public static Easer QuartInOut => Follow(QuartIn, QuartIn);

    public static Easer QuintIn => value => value * value * value * value * value;
    public static Easer QuintOut => value => 1f - (float)Math.Pow(1.0 - (double)value, 5);

    public static Easer TestIn => value => value * value * value * value * value * value * value;

    public static Easer CircOut => value => (float)Math.Sqrt(1 - Math.Pow(value - 1.0, 2));

    private const float B1 = 1f / 2.75f;
    private const float B2 = 2f / 2.75f;
    private const float B3 = 1.5f / 2.75f;
    private const float B4 = 2.5f / 2.75f;
    private const float B5 = 2.25f / 2.75f;
    private const float B6 = 2.625f / 2.75f;

    public static readonly Easer BounceIn = (float value) => {
        value = 1 - value;
        if (value < B1) {
            return 1 - 7.5625f * value * value;
        }
        if (value < B2) {
            return 1 - (7.5625f * (value - B3) * (value - B3) + .75f);
        }
        if (value < B4) {
            return 1 - (7.5625f * (value - B5) * (value - B5) + .9375f);
        }
        return 1 - (7.5625f * (value - B6) * (value - B6) + .984375f);
    };

    public static readonly Easer BounceOut = (float value) => {
        if (value < B1)
            return 7.5625f * value * value;
        if (value < B2)
            return 7.5625f * (value - B3) * (value - B3) + .75f;
        if (value < B4)
            return 7.5625f * (value - B5) * (value - B5) + .9375f;
        return 7.5625f * (value - B6) * (value - B6) + .984375f;
    };

    public static readonly Easer BounceInOut = (float value) => {
        if (value < .5f) {
            value = 1 - value * 2;
            if (value < B1) {
                return (1 - 7.5625f * value * value) / 2;
            }
            if (value < B2) {
                return (1 - (7.5625f * (value - B3) * (value - B3) + .75f)) / 2;
            }
            if (value < B4) {
                return (1 - (7.5625f * (value - B5) * (value - B5) + .9375f)) / 2;
            }
            return (1 - (7.5625f * (value - B6) * (value - B6) + .984375f)) / 2;
        }
        value = value * 2 - 1;
        if (value < B1) {
            return (7.5625f * value * value) / 2 + .5f;
        }
        if (value < B2) {
            return (7.5625f * (value - B3) * (value - B3) + .75f) / 2 + .5f;
        }
        if (value < B4) {
            return (7.5625f * (value - B5) * (value - B5) + .9375f) / 2 + .5f;
        }
        return (7.5625f * (value - B6) * (value - B6) + .984375f) / 2 + .5f;
    };

    public static Easer Invert(Easer easer) => value => 1f - easer(1f - value);

    public static Easer Follow(Easer first, Easer second) => value => value > 0.5f ? second(value * 2f - 1f) / 2f + 0.5f : first(value * 2f) / 2f;
}
