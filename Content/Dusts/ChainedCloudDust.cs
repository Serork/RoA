using RoA.Common.Dusts;
using RoA.Core;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ChainedCloudDust : ModDust, IDrawDustPreProjectiles {
    public override void SetStaticDefaults() => UpdateType = DustID.Cloud;

    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        dust.QuickDraw(Texture2D.Value, Lighting.GetColor(dust.position.ToTileCoordinates()) * 0.9f);
    }

    public override bool PreDraw(Dust dust) => false;
}

sealed class ChainedCloudDust2 : ModDust {
    public override string Texture => ResourceManager.DustTextures + nameof(ChainedCloudDust);

    public override void SetStaticDefaults() => UpdateType = DustID.Cloud;

    public override bool PreDraw(Dust dust) {
        dust.QuickDraw(Texture2D.Value, Lighting.GetColor(dust.position.ToTileCoordinates()) * 0.9f);

        return false;
    }
}
