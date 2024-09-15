using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class MiracleMintDust : ModDust {
    public override void OnSpawn(Dust dust) {
        UpdateType = DustID.Demonite;

        dust.velocity *= 0.25f;
    }

    public override bool Update(Dust dust) {
        dust.velocity *= 0.9f;

        return base.Update(dust);
    }

    public override bool PreDraw(Dust dust) {
        Lighting.AddLight(dust.position, new Microsoft.Xna.Framework.Vector3(0.3f, 0.6f, 1.2f) * 0.5f);

        return base.PreDraw(dust);
    }
}