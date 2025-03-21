using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class ExtraRegen : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Extra Regen");
        //Description.SetDefault("Slowly recovering from injuries");
    }

    public override void Update(Player player, ref int buffIndex) {
        if (player.miscCounter % 60 == 0) {
            player.statLife += 1;
            player.HealEffect(1);
        }
    }
}