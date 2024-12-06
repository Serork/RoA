using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Content.Forms;

sealed class CorruptionInsectForm : InsectForm {
    protected override Vector2 GetLightingPos(Player player) => player.Center + Vector2.UnitX * 15f * player.direction;
    protected override Color LightingColor => new(8, 150, 100);

    protected override void SafeSetDefaults2() {
        MountData.spawnDust = 107;
    }
}