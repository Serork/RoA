using RoA.Common.Networking;
using RoA.Common.Networking.Packets;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class DeathWard : ModBuff {
	public override void SetStaticDefaults() {
		// DisplayName.SetDefault("Death Ward");
		// Description.SetDefault("Your next fatal hit will be prevented");
	}

	public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BehelitPlayer>().behelitPotion = true;
}

sealed class BehelitPlayer : ModPlayer {
	public bool behelitPotion;

	public override void ResetEffects() => behelitPotion = false;

    public override bool FreeDodge(Player.HurtInfo info) {
		if (behelitPotion && Player.statLife <= info.Damage) {
            int time = Player.longInvince ? 80 : 40;
            DeathWardImmune(time);

            Player.statLife += info.Damage;
            Player.HealEffect(info.Damage, true);
            Player.ClearBuff(ModContent.BuffType<DeathWard>());
            //Player.AddBuff(BuffID.PotionSickness, 3600);

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new DeathWardImmuneTimePacket(Player, time));
            }

            return true;
        }

        return base.FreeDodge(info);
    }

    public void DeathWardImmune(int time) {
        Player.SetImmuneTimeForAllTypes(time);
        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Volume = 1.5f }, Player.Center);
    }
}