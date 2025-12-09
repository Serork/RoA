using RoA.Content.Items.Equipables.Accessories;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class OniMask : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Oni Mask");
        //Description.SetDefault("Decreased enemy spawn rate");
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<CalmPlayer>().oniMask = true;
}