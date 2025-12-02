using Microsoft.Xna.Framework;

using RoA.Common.Configs;
using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class Vilethorn : NatureItem {
    public override string Texture => ModContent.GetInstance<RoAClientConfig>().VanillaResprites ? base.Texture : $"Terraria/Images/Item_{ItemID.Vilethorn}";

    public override void SetStaticDefaults() {
        PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.damage = 2;
        Item.useStyle = 1;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Nature.Vilethorn>();
        Item.width = 32;
        Item.height = 32;
        Item.useAnimation = 32;
        Item.useTime = 32;
        Item.rare = 1;
        Item.noMelee = true;
        Item.knockBack = 1f;

        Item.value = Item.sellPrice(0, 1, 50, 0);

        NatureWeaponHandler.SetPotentialDamage(Item, 9);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.06f);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        position -= Vector2.UnitX * 1f;

        position += velocity.SafeNormalize() * 5f;
    }
}