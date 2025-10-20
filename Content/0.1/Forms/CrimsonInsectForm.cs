using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature.Forms;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class CrimsonInsectForm : InsectForm {
    protected override Vector2 GetLightingPos(Player player) => player.Center + new Vector2(20f * player.direction, -4f);
    //protected override _color LightingColor => new(190, 130, 2);

    protected override ushort InsectProjectileType => (ushort)ModContent.ProjectileType<CrimsonInsect>();

    protected override void SafeSetDefaults2() {
        MountData.spawnDust = ModContent.DustType<CrimsonInsectDust>();
    }
}