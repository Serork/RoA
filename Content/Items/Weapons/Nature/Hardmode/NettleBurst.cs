using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.GameContent.Prefixes;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask(127, 127, 127, 127)]
sealed class NettleBurst : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
        PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(46, 42);

        //Item.mana = 12;
        Item.damage = 35;
        Item.useStyle = 5;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<NettleSpikeRight>();
        //Item.width = 26;
        //Item.height = 28;
        Item.useAnimation = Item.useTime = 36;
        Item.autoReuse = true;
        Item.rare = 7;
        Item.noMelee = true;
        Item.knockBack = 1f;
        Item.value = 200000;
        //Item.magic = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        ushort thornType = (ushort)ModContent.ProjectileType<NettleThorn>();
        for (int i = 0; i < 2; i++) {
            Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), position, velocity, thornType, damage, knockback, player.whoAmI, i);
        }
        if (Main.rand.Next(2) == 0) {
            type = ModContent.ProjectileType<NettleSpikeLeft>();
        }
    }
}
