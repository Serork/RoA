using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class BackwoodsDust : ModDust {
    public override void OnSpawn(Dust dust) {
        UpdateType = DustID.Demonite;

        dust.velocity *= 0.35f;
    }

    public override bool Update(Dust dust) {
        dust.velocity *= 0.9f;

        return base.Update(dust);
    }
}