using RoA.Content.Mounts;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class BoneHarpyMountBuff : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;

        BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
            mountID = ModContent.MountType<BoneHarpyMount>()
        };
    }
}
