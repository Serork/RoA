using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class ForbiddenTwig : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 42);
        Item.SetWeaponValues(30, 5f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 52, autoReuse: true);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Projectiles.Friendly.Nature.ForbiddenTwig>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Vector2 pointPoisition = player.RotatedRelativePoint(player.MountedCenter);
        float num2 = (float)Main.mouseX + Main.screenPosition.X - pointPoisition.X;
        float num3 = (float)Main.mouseY + Main.screenPosition.Y - pointPoisition.Y;
        Vector2 vector6 = new Vector2(num2, num3);
        vector6.X = (float)Main.mouseX + Main.screenPosition.X - pointPoisition.X;
        vector6.Y = (float)Main.mouseY + Main.screenPosition.Y - pointPoisition.Y - 1000f;
        player.itemRotation = (float)Math.Atan2(vector6.Y * (float)player.direction, vector6.X * (float)player.direction);
        NetMessage.SendData(13, -1, -1, null, player.whoAmI);
        NetMessage.SendData(41, -1, -1, null, player.whoAmI);

        return true;
    }
}
