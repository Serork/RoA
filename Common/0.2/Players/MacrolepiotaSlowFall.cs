using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class MacrolepiotaSlowFall : ModPlayer {
    public override void PostUpdateEquips() {
        ApplyMacrolepiotaSlowFalling();
    }

    private void ApplyMacrolepiotaSlowFalling() {
        if (!Player.HasProjectile<Macrolepiota_HeldProjectile>()) {
            return;
        }

        Player.fallStart = (int)(Player.position.Y / 16f);
        Player.jumpSpeedBoost += 1.6f;
        if (!Player.controlDown) {
            Player.gravity *= 0.9f;
        }
        float maxYSpeed = 3f;
        if (Player.gravDir == -1f) {
            if (Player.velocity.Y < -maxYSpeed && !Player.controlDown)
                Player.velocity.Y = -maxYSpeed;
        }
        else if (Player.velocity.Y > maxYSpeed && !Player.controlDown) {
            Player.velocity.Y = maxYSpeed;
        }
    }
}
