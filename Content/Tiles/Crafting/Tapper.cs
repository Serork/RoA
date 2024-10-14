using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Tiles;
using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Drawing;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

partial class Tapper : ModTile {
    public static bool[] ImATapper = TileID.Sets.Factory.CreateBoolSet();

    public string BracingTexture => Texture + "_Bracing";
    public string GalipotTexture => Texture + "_Galipot";

    public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileShine2[Type] = true;
		Main.tileShine[Type] = 1500;
		Main.tileNoFail[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

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
        TileSmartInteractCandidateProviderExtended.ShouldBeAdded[Type] = true;

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
                if (Main.rand.NextChance(progress - 0.2f)) {
                    int dustId = Dust.NewDust(position.ToWorldCoordinates() - new Vector2(12f, 12f), 20, 6, ModContent.DustType<Dusts.Galipot>());
                    Dust dust = Main.dust[dustId];
                    progress = progress * 2.05f;
                    dust.velocity *= 0.15f + 0.15f * progress;
                    dust.scale *= 0.7f * progress;
                }
            }
        }
    }

    public override bool RightClick(int i, int j) {
        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 80)) {
            TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
            Vector2 position = new Vector2(i, j).ToWorldCoordinates();
            void dropItem(ushort itemType) {
                int item = Item.NewItem(new EntitySource_TileInteraction(player, i, j), i * 16, j * 16, 26, 24, itemType, 1, false, 0, false, false);
                if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0) {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
                }
            }
            if (tapperTE != null) {
                Item item = player.GetSelectedItem();
                bool hasBottle = item.type == ItemID.Bottle;
                if (hasBottle && tapperTE.IsReadyToCollectGalipot) {
                    SoundEngine.PlaySound(SoundID.Item112.WithPitchOffset(-0.1f), position);
                    if (--item.stack <= 0) {
                        item.TurnToAir();
                    }
                    dropItem((ushort)ModContent.ItemType<Galipot>());
                    tapperTE.CollectGalipot(player);

                    player.ApplyItemTime(item);
                    player.SetItemAnimation(item.useAnimation);
                    if (Main.netMode == NetmodeID.MultiplayerClient) {
                        MultiplayerSystem.SendPacket(new ItemAnimationPacket(player, item.useAnimation));
                    }

                    return true;
                }

                //dropItem((ushort)ModContent.ItemType<Items.Placeable.Crafting.Tapper>());
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                WorldGen.KillTile(i, j);
                if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
                }

                return true;
            }
        }

        return false;
    }
}
