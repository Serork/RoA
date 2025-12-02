using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon;

[AutoloadGlowMask(127, 127, 127)]
sealed class FlederStaff : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
        ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SoulOfTheWoods>();

        Item.staff[Type] = true;
    }

    public override void SetDefaults() {
        int width = 34; int height = 38;
        Item.Size = new Vector2(width, height);

        Item.DamageType = DamageClass.Summon;
        Item.damage = 19;

        Item.mana = 10;

        Item.channel = true;

        Item.useAnimation = Item.useTime = 28;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item65;
        Item.autoReuse = true;
        Item.reuseDelay = 2;

        Item.knockBack = 2f;

        Item.rare = ItemRarityID.Orange;

        Item.shoot = (ushort)ModContent.ProjectileType<Projectiles.Friendly.Summon.LittleFleder>();
        Item.shootSpeed = 5f;

        Item.noMelee = true;

        Item.buffType = ModContent.BuffType<Buffs.LittleFleder>();

        Item.value = Item.sellPrice(0, 2, 25, 0);
    }

    public override bool? UseItem(Player player) {
        if (player.altFunctionUse != 0) {
            Item.useStyle = ItemUseStyleID.Swing;
        }
        else {
            Item.useStyle = ItemUseStyleID.Shoot;
        }

        return base.UseItem(player);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        player.AddBuff(Item.buffType, 2);
        player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);

        return false;
    }
}