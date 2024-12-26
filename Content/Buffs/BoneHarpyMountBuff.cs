using RoA.Content.Mounts;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class BoneHarpyMountBuff : ModBuff {
    public override void SetStaticDefaults() {
        BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
            mountID = ModContent.MountType<BoneHarpyMount>()
        };
    }
}
