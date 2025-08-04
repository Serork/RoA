using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Magic;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic.Hardmode;

sealed class Pixiellation : ModItem {
    public override Color? GetAlpha(Color lightColor) {
        int num5 = lightColor.A - Item.alpha;
        int num2 = (int)((double)(int)lightColor.R * 1.5);
        int num3 = (int)((double)(int)lightColor.G * 1.5);
        int num4 = (int)((double)(int)lightColor.B * 1.5);
        if (num2 > 255)
            num2 = 255;

        if (num3 > 255)
            num3 = 255;

        if (num4 > 255)
            num4 = 255;

        if (num5 < 0)
            num5 = 0;

        if (num5 > 255)
            num5 = 255;

        return new Color(num2, num3, num4, num5);
    }

    public override void SetDefaults() {
        Item.SetSizeValues(44);
        Item.SetWeaponValues(40, 5f, damageClass: DamageClass.Magic);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 24, autoReuse: true);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Pixie>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        Item.staff[Type] = true;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
    }
}
