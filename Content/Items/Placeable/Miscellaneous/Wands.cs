using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class LivingPrimordialWand : Wand {
    protected override ushort ItemToConsume => (ushort)ModContent.ItemType<Solid.Elderwood>();
    protected override ushort TileToPlace => (ushort)ModContent.TileType<LivingElderwood>();

    protected override void SafeSetDefaults() {
        Item.width = 32;
        Item.height = 34;
    }
}

sealed class LivingPrimordialWand2 : Wand {
    protected override ushort ItemToConsume => (ushort)ModContent.ItemType<Solid.Elderwood>();
    protected override ushort TileToPlace => (ushort)ModContent.TileType<LivingElderwoodlLeaves>();

    protected override void SafeSetDefaults() {
        Item.width = 32;
        Item.height = 34;
    }
}

abstract class Wand : ModItem {
    protected abstract ushort ItemToConsume { get; }
    protected abstract ushort TileToPlace { get; }

    private class ExtraLoader : ILoadable {
        public void Load(Mod mod) {
            On_Player.PlaceThing_Tiles_PlaceIt_AutoPaintAndActuate += On_Player_PlaceThing_Tiles_PlaceIt_AutoPaintAndActuate;
            //On_WorldGen.ReplaceTile += On_WorldGen_ReplaceTile;
            //On_Player.PlaceThing_Tiles_PlaceIt_KillGrassForSolids += On_Player_PlaceThing_Tiles_PlaceIt_KillGrassForSolids;
        }

        private void On_Player_PlaceThing_Tiles_PlaceIt_KillGrassForSolids(On_Player.orig_PlaceThing_Tiles_PlaceIt_KillGrassForSolids orig, Player self) {
            orig(self);

            ConsumeWandItem(self);
        }

        private bool On_WorldGen_ReplaceTile(On_WorldGen.orig_ReplaceTile orig, int x, int y, ushort targetType, int targetStyle) {
            bool result = orig(x, y, targetType, targetStyle);

            if (!Main.dedServ) {
                Player self = Main.LocalPlayer;
                Item heldItem = self.HeldItem;
                if (x == Player.tileTargetX && y == Player.tileTargetY) {
                    ConsumeWandItem(self);
                }
            }
            return result;
        }

        private void ConsumeWandItem(Player self) {
            int selectedItemType = self.GetSelectedItem().type;
            ModItem modItem = ItemLoader.GetItem(selectedItemType);
            if (modItem != null && modItem is Wand wand) {
                int tileWand = wand.ItemToConsume;
                for (int num15 = 0; num15 < 58; num15++) {
                    if (tileWand == self.inventory[num15].type && self.inventory[num15].stack > 0) {
                        self.inventory[num15].stack--;
                        if (self.inventory[num15].stack <= 0)
                            self.inventory[num15] = new Item();

                        break;
                    }
                }
            }
        }

        private void On_Player_PlaceThing_Tiles_PlaceIt_AutoPaintAndActuate(On_Player.orig_PlaceThing_Tiles_PlaceIt_AutoPaintAndActuate orig, Player self, int[,] typeCaches, int tileToCreate, Point topLeft) {
            orig(self, typeCaches, tileToCreate, topLeft);

            int selectedItemType = self.GetSelectedItem().type;
            ModItem modItem = ItemLoader.GetItem(selectedItemType);
            if (modItem != null && modItem is Wand wand) {
                int tileWand = wand.ItemToConsume;
                for (int num15 = 0; num15 < 58; num15++) {
                    if (tileWand == self.inventory[num15].type && self.inventory[num15].stack > 0) {
                        self.inventory[num15].stack--;
                        if (self.inventory[num15].stack <= 0)
                            self.inventory[num15] = new Item();

                        break;
                    }
                }
            }
        }

        public void Unload() { }
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        int tileWand = ItemToConsume;
        if (!Main.playerInventory) {
            int num10 = 0;
            var inv = Main.LocalPlayer.inventory;
            for (int l = 0; l < 58; l++) {
                if (inv[l].type == tileWand)
                    num10 += inv[l].stack;
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, num10.ToString(),
                position + new Vector2(-18f, 4f) * Main.inventoryScale, drawColor, 0f, Vector2.Zero, new Vector2(Main.inventoryScale * 0.8f), -1f, Main.inventoryScale);
        }
    }

    public override bool CanUseItem(Player player) {
        int tileWand = ItemToConsume;
        bool flag = false;
        for (int j = 0; j < 58; j++) {
            if (tileWand == player.inventory[j].type && player.inventory[j].stack > 0) {
                flag = true;
                break;
            }
        }

        return flag;
    }

    public override bool? UseItem(Player player) {
        return base.UseItem(player);
    }

    public sealed override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.createTile = TileToPlace;
        Item.width = 32;
        Item.height = 34;
        Item.rare = 1;

        Item.value = Item.sellPrice(0, 0, 40, 0);
    }

    protected virtual void SafeSetDefaults() { }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        => itemGroup = ContentSamples.CreativeHelper.ItemGroup.Wands;

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        foreach (TooltipLine tooltipLine in tooltips) {
            if (tooltipLine.Mod == "Terraria" && tooltipLine.Name == "Placeable") {
                tooltipLine.Hide();
            }
        }
    }
}
