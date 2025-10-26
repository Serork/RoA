using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class CrystalVileShard : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
        PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);

        //Item.mana = 13;
        Item.damage = 25;
        Item.useStyle = 5;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<CrystalSpike>();
        Item.width = 32;
        Item.height = 32;
        Item.useAnimation = 33;
        Item.useTime = 33;
        Item.rare = 5;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = Item.sellPrice(0, 8);
        //Item.magic = true;
        Item.autoReuse = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        position -= Vector2.UnitX * 2.5f;

        position += velocity.SafeNormalize() * 5f;
    }
}
