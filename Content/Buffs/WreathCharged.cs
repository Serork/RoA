using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class WreathCharged : ModBuff {
    public override void SetStaticDefaults() {
        Color color = Color.Green;
        // DisplayName.SetDefault($"Wreath Charge [c/{color.ConvertToHex()}:I]");
        // Description.SetDefault("Nature damage is improved\nProvides charge bonuses of equipped Wreath");

        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}