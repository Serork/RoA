using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Tiles;
using RoA.Content.Achievements;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodChest2 : ModTile, TileHooks.IPostDraw {
    // separate
    private static List<Point> _drawPoints = [];
    private static float _rotationOffset, _scaleOffset;
    private float _opacity;

    public override void SetStaticDefaults() {
        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;

        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        RegisterItemDrop(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChest>(), 1);
        RegisterItemDrop(ItemID.Chest);

        AdjTiles = [TileID.Containers];

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Furniture>();

        Color mapColor = new(133, 111, 91);
        AddMapEntry(mapColor, CreateMapEntryName(), MapChestName);
        RegisterItemDrop(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChest2>(), 1);
        HitSound = SoundID.Dig;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorInvalidTiles = [
                TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
            ];
        TileObjectData.addTile(Type);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChest2>());
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        TileHelper.AddPostSolidTileDrawPoint(this, i, j);

        return true;
    }

    void TileHooks.IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 pos) {
        int i = pos.X;
        int j = pos.Y;
        ulong speed = ((ulong)j << 32) | (ulong)i;
        float posX = Utils.RandomInt(ref speed, -12, 13) * 0.1f;
        float directionMax = posX;
        Tile tile = Main.tile[i, j];
        float colorValue = MathHelper.Lerp(0.2f, 0.8f, (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 1.3f) + 1f) * 0.5f));
        var modTile = TileLoader.GetTile(tile.TileType);
        if (modTile == null) {
            return;
        }
        bool flag2 = modTile.IsLockedChest(i, j);
        if (flag2 && tile.TileFrameX == 36 && tile.TileFrameY == 0) {
            Vector2 zero = Vector2.Zero;
            Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.TileTextures + "WoodbinderRune").Value;
            Vector2 origin = new Vector2(74, 74) / 2f;
            for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.Pi) {
                Main.spriteBatch.Draw(texture,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero + origin / 2f +
                                  new Vector2(-3.5f, 0.5f) +
                                  Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly, new Vector2()) * Helper.Wave(-1.5f, 1.5f, speed: 1f),
                                  null,
                                  Color.White * 0.1f * _opacity, (Main.GlobalTimeWrappedHourly * 0.35f + _rotationOffset) * directionMax, origin, 1.4f + Helper.Wave(0f, 1.5f, speed: 1f) * 0.1f + _scaleOffset, SpriteEffects.None, 0f);
            }
        }
        if (_opacity > 0f) _opacity -= 0.0002f;
    }

    // separate
    public override void Load() {
        On_Main.DrawTileEntities += On_Main_DrawTileEntities;
    }

    private void On_Main_DrawTileEntities(On_Main.orig_DrawTileEntities orig, Main self, bool solidLayer, bool overRenderTargets, bool intoRenderTargets) {
        bool flag = intoRenderTargets || Lighting.UpdateEveryFrame;
        if (solidLayer && flag) {
            _rotationOffset = MathHelper.SmoothStep(_rotationOffset, Main.rand.NextFloatRange(0.1f), 0.2f);
            _scaleOffset = MathHelper.SmoothStep(_scaleOffset, Main.rand.NextFloatRange(0.1f), 0.15f);
        }

        orig(self, solidLayer, overRenderTargets, intoRenderTargets);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => !IsLockedChest(i, j);

    public override bool IsLockedChest(int i, int j) {
        return Main.tile[i, j].TileFrameX / 36 == 1 && !NPC.downedBoss2;
    }

    public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) {
        bool flag = NPC.downedBoss2;
        if (flag) {
            return false;
        }

        return true;
    }

    public static string MapChestName(string name, int i, int j) {
        int left = i;
        int top = j;
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0) {
            left--;
        }

        if (tile.TileFrameY != 0) {
            top--;
        }

        int chest = Chest.FindChest(left, top);
        if (chest < 0) {
            return Language.GetTextValue("LegacyChestType.0");
        }

        if (Main.chest[chest].name == string.Empty) {
            return name;
        }

        return name + ": " + Main.chest[chest].name;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0/*fail ? 3 : 9*/;

    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

    public override bool RightClick(int i, int j) {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        Main.mouseRightRelease = false;
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0) {
            left--;
        }

        if (tile.TileFrameY != 0) {
            top--;
        }

        if (player.sign >= 0) {
            SoundEngine.PlaySound(SoundID.MenuClose);
            player.sign = -1;
            Main.editSign = false;
            Main.npcChatText = string.Empty;
        }

        if (Main.editChest) {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = string.Empty;

        }

        if (player.editedChestName) {
            NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
            player.editedChestName = false;
        }

        bool isLocked = IsLockedChest(left, top);
        if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked) {
            if (left == player.chestX && top == player.chestY && player.chest >= 0) {
                player.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
            else {
                NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
                Main.stackSplit = 600;
            }
        }
        else {
            if (isLocked) {
                _opacity = 1f;
            }
            else {
                int chest = Chest.FindChest(left, top);
                if (chest >= 0) {
                    Main.stackSplit = 600;
                    if (chest == player.chest) {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else {
                        ModContent.GetInstance<OpenRootboundChest>().OpenRootboundChestCondition.Complete();
                        //RoA.CompleteAchievement("OpenRootboundChest");
                        //player.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>().OpenRootboundChest = true;

                        player.chest = chest;
                        Main.playerInventory = true;
                        Main.recBigList = false;
                        player.chestX = left;
                        player.chestY = top;
                        SoundEngine.PlaySound(SoundID.MenuOpen);
                    }

                    Recipe.FindRecipes();
                }
            }
        }

        return true;
    }

    public override LocalizedText DefaultContainerName(int frameX, int frameY) {
        return this.GetLocalization("MapEntry");
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0) {
            left--;
        }

        if (tile.TileFrameY != 0) {
            top--;
        }

        int chest = Chest.FindChest(left, top);
        if (chest < 0) {
            player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
        }
        else {
            bool isLocked = IsLockedChest(left, top);
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
            if (player.cursorItemIconText == defaultName) {
                player.cursorItemIconID = !isLocked ? (ushort)ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChest2>() : player.GetSelectedItem().type;

                player.cursorItemIconText = "";
            }
        }
    }

    public override void MouseOverFar(int i, int j) {
        MouseOver(i, j);
        Player player = Main.LocalPlayer;
        if (player.cursorItemIconText == string.Empty) {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = 0;
        }
    }
}