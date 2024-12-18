using Microsoft.Xna.Framework;

using RoA.Content.Items.Materials;
using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class Herbarium : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Herbarium");
        //Tooltip.SetDefault("Enemies can drop healing herbs when wreath is fully charged");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;
        Item.value = Item.sellPrice(gold: 2);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<HerbariumPlayer>().healingHerb = true;

    //public override void AddRecipes() {
    //    CreateRecipe()
    //        .AddIngredient(ItemID.Book)
    //        .AddIngredient(ItemID.Daybloom)
    //        .AddIngredient(ItemID.Shiverthorn)
    //        .AddIngredient(ItemID.Blinkroot)
    //        .AddIngredient(ItemID.Waterleaf)
    //        .AddIngredient(ItemID.Deathweed)
    //        .AddIngredient(ItemID.Moonglow)
    //        .AddIngredient(ModContent.ItemType<MiracleMint>())
    //        .AddIngredient(ModContent.ItemType<Bonerose>())
    //        .AddIngredient(ItemID.Fireblossom)
    //        .AddTile(TileID.Bookcases)
    //        .Register();
    //}
}

internal class HerbariumPlayer : ModPlayer {
    public bool healingHerb;

    public override void ResetEffects()
        => healingHerb = false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (healingHerb && target.life <= damageDone && Player.GetSelectedItem().IsADruidicWeapon() && Main.rand.Next(2) == 0) {
            int getHerb() {
                int rand = Main.rand.Next(3);
                return rand switch {
                    0 => ModContent.ItemType<MagicHerb1>(),
                    1 => ModContent.ItemType<MagicHerb2>(),
                    _ => ModContent.ItemType<MagicHerb3>(),
                };
            }
            Item.NewItem(Player.GetSource_OnHit(target), (int)target.position.X, (int)target.position.Y, target.width, target.height, getHerb());
        }
    }
}
