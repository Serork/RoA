using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class TylerMessageHandler : ModPlayer {
    public override void UpdateEquips() {
        if (!IsTylerSet()) {
            return;
        }

        UpdateMessageCooldowns();

        if (!Main.rand.NextBool(100)) {
            return;
        }

        TryPlayingIdleMessage();
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (Player.statLife > 0 && Player.statLife < Player.statLifeMax2 * 0.25f && Main.rand.NextBool(10)) {
            Create(MessageSource.AlmostDeath, Player.Top, _messageVelocity);
        }
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
        if (Player.statLife > 0 && Player.statLife < Player.statLifeMax2 * 0.25f && Main.rand.NextBool(10)) {
            Create(MessageSource.AlmostDeath, Player.Top, _messageVelocity);
        }
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
        if (Main.rand.NextBool(5)) {
            Create(MessageSource.Death, Player.Top, _messageVelocity);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.lifeMax <= 5 && Main.rand.NextBool(20)) {
            Create(MessageSource.KilledBunny, Player.Top, _messageVelocity);
        }
    }

    public override void OnHitAnything(float x, float y, Entity victim) {
        if (victim is Player player) {
            if (player.statLife <= 0 && Main.rand.NextBool(10)) {
                Create(MessageSource.KilledBunny, Player.Top, _messageVelocity);
            }
            if (player.statLife <= 0 && (Player.name.Equals("Serork", StringComparison.CurrentCultureIgnoreCase) || Player.name.Equals("peege.on", StringComparison.CurrentCultureIgnoreCase) || Player.name.Equals("has2r", StringComparison.CurrentCultureIgnoreCase))) {
                Create(MessageSource.Special, Player.Top, _messageVelocity);
            }
        }
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
        if (Main.rand.NextBool(20)) {
            Create(MessageSource.OnBuy, Player.Top, _messageVelocity);
        }
    }

    public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
        if (Main.rand.NextBool(20)) {
            Create(MessageSource.OnSell, Player.Top, _messageVelocity);
        }
    }
}