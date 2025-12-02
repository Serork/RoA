using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class WreathFullCharged2 : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault($"Wreath Charge [c/{color.ConvertToHex()}:II]");
        // Description.SetDefault("Nature damage is at full potential\nProvides full charge bonuses of equipped Wreath");

        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}