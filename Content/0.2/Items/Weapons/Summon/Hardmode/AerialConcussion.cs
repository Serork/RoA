using Microsoft.Xna.Framework;

using RoA.Common.Items;
using RoA.Core.Defaults;

namespace RoA.Content.Items.Weapons.Summon.Hardmode;

sealed class AerialConcussion : WhipBase {
    protected override int TagDamage => 10;
    protected override int SegmentCount => 15;
    protected override float RangeMultiplier => 1f;
    protected override Rectangle TailClip => new(14, 0, 10, 24);
    protected override Rectangle Body1Clip => new(14, 34, 14, 18);
    protected override Rectangle Body2Clip => new(14, 62, 14, 18);
    protected override Rectangle Body3Clip => new(14, 90, 14, 18);
    protected override Rectangle TipClip => new(12, 112, 20, 26);

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 34);
    }
}