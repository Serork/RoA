using Microsoft.Xna.Framework;

using System;

namespace RoA.Common.GlowMasks;

[AttributeUsage(AttributeTargets.Class)]
sealed class AutoloadGlowMaskAttribute(byte r = 255, byte g = 255, byte b = 255, byte a = 255, string requirement = "_Glow", bool shouldApplyItemAlpha = false) : Attribute {
    public readonly Color GlowColor = new(r, g, b, a);
    public readonly string Requirement = requirement;
    public readonly bool ShouldApplyItemAlpha = shouldApplyItemAlpha;
}

[AttributeUsage(AttributeTargets.Class)]
sealed class AutoloadGlowMask2Attribute : Attribute {
    public readonly string[] CustomGlowmasks;
    public readonly bool AutoAssignItemID;

    public AutoloadGlowMask2Attribute() {
        AutoAssignItemID = true;
        CustomGlowmasks = null;
    }

    public AutoloadGlowMask2Attribute(params string[] glowmasks) {
        AutoAssignItemID = false;
        CustomGlowmasks = glowmasks;
    }
}