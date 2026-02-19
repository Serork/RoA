using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class BrambleMaze : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        int width = 42; int height = 48;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 20;
        Item.autoReuse = false;

        Item.noMelee = true;
        Item.knockBack = 2f;

        Item.damage = 6;
        NatureWeaponHandler.SetPotentialDamage(Item, 16);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.25f);

        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item20;

        Item.shootSpeed = 1f;
        Item.shoot = ModContent.ProjectileType<BrambleMazeRoot>();

        Item.value = Item.sellPrice(silver: 20);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Vector2 pointPoisition = player.GetPlayerCorePoint();
        float num2 = (float)Main.mouseX + Main.screenPosition.X - pointPoisition.X;
        float num3 = (float)Main.mouseY + Main.screenPosition.Y - pointPoisition.Y;
        Vector2 vector6 = new Vector2(num2, num3);
        vector6.X = (float)Main.mouseX + Main.screenPosition.X - pointPoisition.X;
        vector6.Y = (float)Main.mouseY + Main.screenPosition.Y - pointPoisition.Y + 1000f;
        player.itemRotation = (float)Math.Atan2(vector6.Y * (float)player.direction, vector6.X * (float)player.direction);
        NetMessage.SendData(13, -1, -1, null, player.whoAmI);
        NetMessage.SendData(41, -1, -1, null, player.whoAmI);

        return true;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        position = player.GetPlayerCorePoint();
    }
}
