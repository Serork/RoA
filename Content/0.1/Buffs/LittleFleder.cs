using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class LittleFleder : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Little Fleder");
        // Description.SetDefault("The little fleder will fight for you");

        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Friendly.Summon.LittleFleder>()] > 0)
            player.buffTime[buffIndex] = 18000;
        else {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}