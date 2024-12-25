using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class BoneHarpyAttackDebuff : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bone Harpy");
        // Description.SetDefault("Bone Harpy cannot attack anymore");

        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
    }
}