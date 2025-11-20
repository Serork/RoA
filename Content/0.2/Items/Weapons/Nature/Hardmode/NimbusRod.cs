using Microsoft.Xna.Framework;

using RoA.Common.Configs;
using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

// also see ItemGlowMaskHandler
[AutoloadGlowMask]
sealed class NimbusRod : NatureItem {
    public override string Texture => ModContent.GetInstance<RoAClientConfig>().VanillaResprites ? base.Texture : $"Terraria/Images/Item_{ItemID.NimbusRod}";

    public override void SetStaticDefaults() {
        PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(42, 36);

        //Item.mana = 30;
        Item.damage = 30;
        Item.useStyle = 1;
        Item.shootSpeed = 16f;
        Item.shoot = ModContent.ProjectileType<RainCloudMoving>();
        //Item.width = 26;
        //Item.height = 28;
        Item.UseSound = SoundID.Item66;
        Item.useAnimation = 22;
        Item.useTime = 22;
        Item.rare = 6;
        Item.noMelee = true;
        Item.knockBack = 0f;
        Item.value = Item.sellPrice(0, 3, 50);
        //Item.magic = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.whoAmI == Main.myPlayer) {
            Vector2 mousePosition = player.GetWorldMousePosition();
            Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, mousePosition.X, mousePosition.Y);
        }

        return false;
    }
}
