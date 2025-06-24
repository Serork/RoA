using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons;

sealed class Macrolepiota_PlayerSlowFallHandler : ModPlayer {
    public override void PostUpdateEquips() {
        ApplyMacrolepiotaSlowFalling();
    }

    private void ApplyMacrolepiotaSlowFalling() {
        if (!Player.Holding<Macrolepiota>()) {
            return;
        }

        Player.fallStart = (int)(Player.position.Y / 16f);
        Player.jumpSpeedBoost += 1.6f;
        Player.gravity *= 0.9f;
        if (Player.gravDir == -1f) {
            if (Player.velocity.Y < -4f && !Player.controlDown)
                Player.velocity.Y = -4f;
        }
        else if (Player.velocity.Y > 4f && !Player.controlDown) {
            Player.velocity.Y = 4f;
        }
    }
}
