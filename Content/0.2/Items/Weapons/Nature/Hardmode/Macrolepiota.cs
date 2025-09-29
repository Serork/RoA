using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask]
sealed class Macrolepiota : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(28, 40);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => Lighting.AddLight(Item.Center, new Vector3(0.1f, 0.4f, 1f));

    public override void HoldItem(Player player) {
        SpawnHeldMacrolepiota(player);
    }

    private void SpawnHeldMacrolepiota(Player player) {
        if (!player.IsLocal()) {
            return;
        }

        if (player.HasProjectile<Macrolepiota_HeldProjectile>() || player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        ProjectileUtils.SpawnPlayerOwnedProjectile<Macrolepiota_HeldProjectile>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_ItemUse(Item)));
    }
}