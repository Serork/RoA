using RoA.Common.Players;
using RoA.Content.Items.Weapons.Summon.Hardmode;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

class EnduranceCloud1 : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = false;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.endurance += 0.08f;

        AerialConcussion.SpawnCloud(player, 1);

        ResetTier(player, buffIndex);
    }

    protected void ResetTier(Player player, int buffIndex) {
        if (player.buffTime[buffIndex] < 10) {
            player.GetModPlayer<AerialConcussionEffect>().Reset(true);
        }
    }

    public override bool RightClick(int buffIndex) {
        Main.LocalPlayer.GetModPlayer<AerialConcussionEffect>().Reset();

        return base.RightClick(buffIndex);
    }
}

class EnduranceCloud2 : EnduranceCloud1 {
    public override void Update(Player player, ref int buffIndex) {
        player.endurance += 0.16f;

        AerialConcussion.SpawnCloud(player, 2);

        ResetTier(player, buffIndex);
    }
}

class EnduranceCloud3 : EnduranceCloud1 {
    public override void Update(Player player, ref int buffIndex) {
        player.endurance += 0.24f;

        AerialConcussion.SpawnCloud(player, 3);

        ResetTier(player, buffIndex);
    }
}
