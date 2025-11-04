using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class FeatherinABottleGore32 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore22 : FeatherinABottleGore1 { }
class FeatherinABottleGore12 : FeatherinABottleGore1 {
    protected override Color LightColor => new(248, 119, 119);
}

sealed class FeatherinABottleGore31 : FeatherinABottleGore11 { }
sealed class FeatherinABottleGore21 : FeatherinABottleGore11 { }
class FeatherinABottleGore11 : FeatherinABottleGore1 {
    protected override Color LightColor => new(251, 234, 94);
}

sealed class FeatherinABottleGore3 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore2 : FeatherinABottleGore1 { }
class FeatherinABottleGore1 : ModGore {
    protected virtual Color LightColor { get; } = new(170, 252, 134);

    public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White * 0.9f;

    public override void OnSpawn(Gore gore, IEntitySource source) {
        UpdateType = 11;
    }

    public override bool Update(Gore gore) {
        Lighting.AddLight(gore.position, LightColor.ToVector3() * gore.scale * 0.5f);

        return base.Update(gore);
    }
}