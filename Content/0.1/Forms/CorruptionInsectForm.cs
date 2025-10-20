using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature.Forms;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class CorruptionInsectForm : InsectForm {
    protected override Vector2 GetLightingPos(Player player) => player.Center + new Vector2(20f * player.direction, -4f);
    //protected override _color LightingColor => new(8, 150, 100);

    protected override ushort InsectProjectileType => (ushort)ModContent.ProjectileType<CorruptionInsect>();

    protected override void SafeSetDefaults2() {
        MountData.spawnDust = 107;
    }
}