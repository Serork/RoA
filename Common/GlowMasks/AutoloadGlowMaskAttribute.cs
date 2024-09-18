using System;

namespace RoA.Common.GlowMasks;

[AttributeUsage(AttributeTargets.Class)]
sealed class AutoloadGlowMaskAttribute : Attribute {
    public readonly string[] CustomGlowMasks;
    public readonly bool AutoAssignItemID;

    public AutoloadGlowMaskAttribute() {
        AutoAssignItemID = true;
        CustomGlowMasks = null;
    }

    public AutoloadGlowMaskAttribute(params string[] glowMasks) {
        AutoAssignItemID = false;
        CustomGlowMasks = glowMasks;
    }
}