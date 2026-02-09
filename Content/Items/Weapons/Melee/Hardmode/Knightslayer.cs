using RoA.Common.GlowMasks;
using RoA.Core.Defaults;

using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee.Hardmode;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class Knightslayer : ModItem {
    private record struct KnightSlayerNPCHitInfo(short NPCTypeToApplyIncreasedDamage, float FinalDamageModifier, int FlatBonusDamage = 0);

    private static readonly HashSet<KnightSlayerNPCHitInfo> NPCsThatTakeIncreasedDamage =
        [new KnightSlayerNPCHitInfo(NPCID.ArmoredSkeleton, 1.5f, 0),
         new KnightSlayerNPCHitInfo(NPCID.SkeletonArcher, 1.5f, 0)];

    public override void SetDefaults() {
        Item.SetSizeValues(68);

        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
            Item.DamageType = DamageClass.Melee;
        }

        Item.damage = 100;
        Item.knockBack = 6.5f;
        Item.scale = 1f;
        Item.useAnimation = Item.useTime = 20;
        Item.UseSound = SoundID.Item1;
    }

    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
        foreach (KnightSlayerNPCHitInfo knightSlayerNPCHitInfo in NPCsThatTakeIncreasedDamage) {
            if (knightSlayerNPCHitInfo.NPCTypeToApplyIncreasedDamage != target.type) {
                continue;
            }
            modifiers.FlatBonusDamage += knightSlayerNPCHitInfo.FlatBonusDamage;
            modifiers.FinalDamage *= knightSlayerNPCHitInfo.FinalDamageModifier;
        }
    }
}
