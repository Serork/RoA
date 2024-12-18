using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class TylerMessageHandler : ModPlayer {
    public override void UpdateEquips() {
        if (!Main.rand.NextBool(100)) {
            return;
        }

        TryPlayingIdleMessage();
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
        if (Player.statLife <= hurtInfo.Damage && Main.rand.NextBool(4)) {
            Create(MessageSource.AlmostDeath, Player.Top, _messageVelocity);
        }
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
        if (Main.rand.NextBool(4)) {
            Create(MessageSource.Death, Player.Top, _messageVelocity);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (target.type == NPCID.Bunny && target.life <= 0 && Main.rand.NextBool(10)) {
            Create(MessageSource.KilledBunny, Player.Top, _messageVelocity);
        }
    }

    public override void OnHitAnything(float x, float y, Entity victim) {
        if (victim is NPC npc) {
            if (npc.type == NPCID.Bunny && npc.life <= 0 && Main.rand.NextBool(10)) {
                Create(MessageSource.KilledBunny, Player.Top, _messageVelocity);
            }
        }
        if (victim is Player player) {
            if (player.statLife <= 0 && Main.rand.NextBool(4)) {
                Create(MessageSource.KilledBunny, Player.Top, _messageVelocity);
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