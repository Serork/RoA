using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Defaults;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Hardmode;

sealed class Taproot : ModItem {
    public record struct TaprootNPCHitInfo(short NPCTypeToApplyIncreasedDamage, float FinalDamageModifier, int FlatBonusDamage = 0);

    public static readonly HashSet<TaprootNPCHitInfo> NPCsThatTakeIncreasedDamage =
        [new TaprootNPCHitInfo(NPCID.ArmoredSkeleton, 1.5f, 0),
         new TaprootNPCHitInfo(NPCID.SkeletonArcher, 1.5f, 0)];

    public override void SetDefaults() {
        Item.SetSizeValues(32, 60);

        {
            Item.autoReuse = true;
            Item.useStyle = 5;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;
            Item.value = Item.buyPrice(0, 45);
            Item.rare = 8;
            Item.DamageType = DamageClass.Ranged;
        }

        Item.UseSound = SoundID.Item102;
        Item.crit = 7;
        Item.damage = 80;
        Item.knockBack = 3f;
        Item.shootSpeed = 7.75f;
        Item.useAnimation = 20;
        Item.useTime = 20;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        type = ModContent.ProjectileType<TaprootArrow>();
    }

    public override Vector2? HoldoutOffset() => new Vector2(0f, 0f);
}
