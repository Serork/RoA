using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Tiles;
using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

partial class Tapper : ModTile {
    public static Asset<Texture2D> BracingTexture { get; private set; } = null!;
    public static Asset<Texture2D> GalipotTexture { get; private set; } = null!;
    public static Asset<Texture2D> HighlightGalipotTexture { get; private set; } = null!;

    public string BracingTexturePath => Texture + "_Bracing";

    public static bool[] ImATapper = TileID.Sets.Factory.CreateBoolSet();

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            BracingTexture = ModContent.Request<Texture2D>(BracingTexturePath);
            GalipotTexture = ModContent.Request<Texture2D>(Texture + "_Galipot");
            HighlightGalipotTexture = ModContent.Request<Texture2D>(Texture + "_Highlight_Galipot");
        }

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1500;
        Main.tileNoFail[Type] = true;
        Main.tileAxe[Type] = true;


        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IgnoreSmartCursorPriorityAxe[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleWrapLimit = 0;
        TileObjectData.newTile.CoordinateWidth = 30;
        TileObjectData.newTile.CoordinateHeights = [28];
        TileObjectData.newTile.AnchorRight = AnchorData.Empty;
        TileObjectData.newTile.AnchorLeft = new AnchorData(Terraria.Enums.AnchorType.Tree, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = AnchorData.Empty;
        TileObjectData.newTile.DrawXOffset = -8;
        TileObjectData.newTile.DrawYOffset = -6;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TapperTE>().Hook_AfterPlacement, -1, 0, true);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorLeft = AnchorData.Empty;
        TileObjectData.newAlternate.AnchorRight = new AnchorData(Terraria.Enums.AnchorType.Tree, TileObjectData.newTile.Width, 0);
        TileObjectData.newAlternate.DrawXOffset = 8;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        ImATapper[Type] = true;
        TileSmartInteractCandidateProviderExtended.AddMe[Type] = true;

        AddMapEntry(new Color(191, 143, 111), CreateMapEntryName());

        TileID.Sets.InteractibleByNPCs[Type] = true;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) { yield return new Item((ushort)ModContent.ItemType<Items.Placeable.Crafting.Tapper>()); }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<TapperTE>().Kill(i, j);

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80)) {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
            if (tapperTE != null) {
                bool hasBottle = player.inventory[player.selectedItem].type == ItemID.Bottle;
                if (hasBottle && tapperTE.IsReadyToCollectGalipot) {
                    player.cursorItemIconID = ModContent.ItemType<Galipot>();

                    return;
                }

                player.cursorItemIconID = tapperTE.IsReadyToCollectGalipot ? ItemID.Bottle : (ushort)ModContent.ItemType<Items.Placeable.Crafting.Tapper>();
            }
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4))) {
            TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
            if (tapperTE != null && tapperTE.IsReadyToCollectGalipot) {
                float progress = 0.5f;
                Vector2 position = new((float)i, (float)j);
                if (Main.rand.NextChance(progress - 0.45f)) {
                    int dustId = Dust.NewDust(position.ToWorldCoordinates() - new Vector2(12f, 12f), 20, 6, ModContent.DustType<Dusts.Galipot>());
                    Dust dust = Main.dust[dustId];
                    progress = progress * 2.05f;
                    dust.velocity *= 0.15f + 0.15f * progress;
                    dust.scale *= 0.7f * progress;
                }
            }
        }
    }

    private static Item GetBottleInTheInventory(Player player) {
        for (int index = 0; index < Main.InventorySlotsTotal; index++) {
            Item inventoryItem = player.inventory[index];
            if (inventoryItem.stack > 0 && inventoryItem.type == ItemID.Bottle) {
                return inventoryItem;
            }
        }

        if (player.useVoidBag()) {
            for (int index = 0; index < 40; index++) {
                Item voidBagItem = player.bank4.item[index];
                if (voidBagItem.stack > 0 && voidBagItem.type == ItemID.Bottle) {
                    return voidBagItem;
                }
            }
        }

        return null;
    }

    private static Item GetBottleInTheInventory(Player player, out int index) {
        for (index = 0; index < Main.InventorySlotsTotal; index++) {
            Item inventoryItem = player.inventory[index];
            if (inventoryItem.stack > 0 && inventoryItem.type == ItemID.Bottle) {
                return inventoryItem;
            }
        }

        if (player.useVoidBag()) {
            for (index = 0; index < 40; index++) {
                Item voidBagItem = player.bank4.item[index];
                if (voidBagItem.stack > 0 && voidBagItem.type == ItemID.Bottle) {
                    return voidBagItem;
                }
            }
        }

        return null;
    }

    private class GalipotItemAnimationHandler : ModPlayer {
        public bool ShouldBeActive;

        public override void Load() {
            On_Player.ItemCheck_Inner += On_Player_ItemCheck_Inner;
        }

        private void On_Player_ItemCheck_Inner(On_Player.orig_ItemCheck_Inner orig, Player self) {
            orig(self);

            var handler = self.GetModPlayer<GalipotItemAnimationHandler>();
            if (handler.ShouldBeActive) {
                var item = GetBottleInTheInventory(self, out int index);
                if (item != null) {
                }
                else {
                    item = new Item();
                    item.SetDefaults(ItemID.Bottle);
                }
                self.lastVisualizedSelectedItem = item;
                if (self.itemAnimation <= 0) {
                    handler.ShouldBeActive = false;
                    self.selectedItem = self.oldSelectItem;
                    self.cursorItemIconEnabled = false;
                }
            }
        }
    }

    public override bool RightClick(int i, int j) {
        if (!Main.mouseItem.IsEmpty()) {
            return base.RightClick(i, j);
        }

        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80) && !player.ItemAnimationActive) {
            TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
            Vector2 position = new Vector2(i, j).ToWorldCoordinates();
            void dropItem(ushort itemType) {
                int item = Item.NewItem(new EntitySource_TileInteraction(player, i, j), i * 16, j * 16, 16, 16, itemType, 1, false, 0, false, false);
                if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0) {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
                }
            }
            if (tapperTE != null) {
                Item item = GetBottleInTheInventory(player, out int index);
                bool flag = false;
                bool hasBottle = item != null;
                var handler = player.GetModPlayer<GalipotItemAnimationHandler>();
                if (tapperTE.IsReadyToCollectGalipot) {
                    if (hasBottle) {
                        SoundEngine.PlaySound(SoundID.Item112.WithPitchOffset(-0.1f), position);
                        if (--item.stack <= 0) {
                            item.TurnToAir();
                        }
                        dropItem((ushort)ModContent.ItemType<Galipot>());
                        tapperTE.CollectGalipot(player);

                        if (!flag) {
                            if (player.selectedItem != index) {
                                player.oldSelectItem = player.selectedItem;
                            }
                            player.selectedItem = index;
                            player.SetItemAnimation(item.useAnimation);
                        }

                        player.ApplyItemTime(item);

                        handler.ShouldBeActive = true;

                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new ItemAnimationPacket2(player, item.useAnimation, index));
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }
}
