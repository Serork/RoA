using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Content.Tiles.Plants;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

abstract class PlantBase : ModTile, TileHooks.IGetTileDrawData {
    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        tileHeight += 4;
        addFrY -= 1;

        Tile tile = WorldGenHelper.GetTileSafely(x, y + 1);
        if (Main.tileSolidTop[tile.TileType]) {
            addFrY -= 1;
        }
    }

    protected virtual short FrameWidth => 18;

    protected virtual int[] AnchorValidTiles => [];

    protected virtual ushort DropItem { get; set; }

    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.DrawXOffset -= 4;
        TileObjectData.newTile.DrawYOffset -= 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.AnchorAlternateTiles = [TileID.ClayPot, TileID.PlanterBox];
        TileObjectData.newTile.UsesCustomCanPlace = true;
        PreAddNewTile();
        TileObjectData.addTile(Type);

        SafeSetStaticDefaults();
    }

    public override bool CanPlace(int i, int j) {
        Tile tile = Framing.GetTileSafely(i, j);

        if (tile.HasTile) {
            int tileType = tile.TileType;
            if (tileType == Type) {
                return IsGrown(i, j);
            }

            if (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType] || tileType == TileID.WaterDrip || tileType == TileID.LavaDrip || tileType == TileID.HoneyDrip || tileType == TileID.SandDrip) {
                bool foliageGrass = tileType == TileID.Plants || tileType == TileID.Plants2;
                bool moddedFoliage = tileType >= TileID.Count && (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType]);
                bool harvestableVanillaHerb = Main.tileAlch[tileType] && WorldGen.IsHarvestableHerbWithSeed(tileType, tile.TileFrameX / 18);

                if (foliageGrass || moddedFoliage || harvestableVanillaHerb) {
                    WorldGen.KillTile(i, j);
                    if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
                    }

                    return true;
                }
            }

            return false;
        }

        return true;
    }

    protected virtual void PreAddNewTile() { }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {

    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        if (GetStage(i, j) == PlantStage.Planted/* && !AnchorValidTiles.Contains(WorldGenHelper.GetTileSafely(i, j + 1).TileType)*/) {
            Tile tile = WorldGenHelper.GetTileSafely(i, j);
            Vector2 origin = new Vector2(FrameWidth, 21) / 2f;
            bool flag = true;
            bool flag2 = true/*Main.tileSolidTop[WorldGenHelper.GetTileSafely(i, j + 1).TileType]*/;
            SpriteEffects spriteEffects = flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (flag2) {
                flag = flag2;
                spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            int offsetY = flag2 ? !Main.tileSolidTop[WorldGenHelper.GetTileSafely(i, j + 1).TileType] ? 2 : 1 : 0;
            Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
            texture ??= TextureAssets.Tile[Type].Value;
            spriteBatch.Draw(texture, new Vector2(i * 16f, j * 16f - 5f + offsetY) + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange)) - Main.screenPosition
                + origin + new Vector2(flag ? -4f : 0f, 0f),
                new Rectangle(tile.TileFrameX, tile.TileFrameY, FrameWidth, 21), Lighting.GetColor(i, j), 0f,
                origin,
                1f,
                spriteEffects, 0f);

            return false;
        }

        return base.PreDraw(i, j, spriteBatch);
    }

    protected virtual void SafeSetStaticDefaults() { }

    public virtual PlantStage GetStage(int i, int j) => (PlantStage)(WorldGenHelper.GetTileSafely(i, j).TileFrameX / FrameWidth);
    public virtual bool IsGrowing(int i, int j) => GetStage(i, j) == PlantStage.Growing;
    public virtual bool IsGrown(int i, int j) => GetStage(i, j) == PlantStage.Grown;

    protected virtual bool CanBloom() => true;

    protected virtual int PlantDrop { get; }
    protected virtual int SeedsDrop { get; }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
        Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];

        int plantStack = 0;

        int seedStack = 0;

        if (nearestPlayer.active && (nearestPlayer.HeldItem.type == ItemID.StaffofRegrowth || nearestPlayer.HeldItem.type == ItemID.AcornAxe)) {
            plantStack = Main.rand.Next(1, 3);
            seedStack = Main.rand.Next(1, 6);
        }
        else {
            bool flag = this is Cloudberry && IsGrown(i, j);
            plantStack = flag ? 2 : 1;
            if (IsGrown(i, j) || (GetStage(i, j) != PlantStage.Planted && CanBloom())) {
                seedStack = Main.rand.Next(1, 4);
            }
        }

        if (plantStack > 0) {
            yield return new Item(PlantDrop, plantStack);
        }

        if (seedStack > 0) {
            yield return new Item(SeedsDrop, seedStack);
        }
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    public override bool IsTileSpelunkable(int i, int j) => IsGrown(i, j);

    public override void RandomUpdate(int i, int j) {
        bool flag = Main.rand.NextBool(40);
        if (!CanBloom() && !flag) {
            return;
        }

        if (!IsGrown(i, j) && (Main.rand.NextBool(40) || flag)) {
            WorldGenHelper.GetTileSafely(i, j).TileFrameX += FrameWidth;           
            if (Main.netMode != NetmodeID.SinglePlayer) {
                NetMessage.SendTileSquare(-1, i, j, 1);
            }
        }
    }

    public static bool TryPlacePlant(int i, int j, ushort tileTypeToGrow, int style = 0, int checkRadius = 15, int maxAlchNearby = 5, params int[] validTiles) {
        int num3 = checkRadius;
        int num4 = maxAlchNearby;
        int num5 = 0;
        num3 = (int)((double)num3 * ((double)Main.maxTilesX / 4200.0));
        int num6 = Utils.Clamp(i - num3, 4, Main.maxTilesX - 4);
        int num7 = Utils.Clamp(i + num3, 4, Main.maxTilesX - 4);
        int num8 = Utils.Clamp(j - num3, 4, Main.maxTilesY - 4);
        int num9 = Utils.Clamp(j + num3, 4, Main.maxTilesY - 4);
        for (int i2 = num6; i2 <= num7; i2++) {
            for (int j2 = num8; j2 <= num9; j2++) {
                int checkTileType = Main.tile[i2, j2].TileType;
                if (Main.tileAlch[checkTileType] || (checkTileType >= TileID.Count && TileLoader.GetTile(checkTileType) is PlantBase)) {
                    num5++;
                }
            }
        }
        if (num5 < num4) {
            bool flag = false;
            for (int k = 0; k < validTiles.Length; k++) {
                if (Main.tile[i, j].TileType == validTiles[k]) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                return false;
            }
            j -= 1;
            if (!Main.tile[i, j].HasTile && Main.tile[i, j + 1].HasUnactuatedTile && !Main.tile[i, j + 1].IsHalfBlock && Main.tile[i, j + 1].Slope == 0) {
                Tile tile = Main.tile[i, j];
                PlantBase plant = TileLoader.GetTile(tileTypeToGrow) as PlantBase;
                tile.ClearTile();
                tile.TileType = tileTypeToGrow;
                tile.HasTile = true;
                tile.TileFrameX = (short)(plant.FrameWidth * style);
                if (Main.tile[i, j].HasTile && Main.netMode == NetmodeID.Server) {
                    NetMessage.SendTileSquare(-1, i, j);
                }
            }

            return true;
        }

        return false;
    }

    public static bool TryPlacePlant2(int i, int j, ushort tileTypeToGrow, int style = 0, int checkRadius = 15, int maxAlchNearby = 5, Action<Point>? onPlaced = null, params int[] validTiles) {
        int num3 = checkRadius;
        int num4 = maxAlchNearby;
        int num5 = 0;
        num3 = (int)((double)num3 * ((double)Main.maxTilesX / 4200.0));
        int num6 = Utils.Clamp(i - num3, 4, Main.maxTilesX - 4);
        int num7 = Utils.Clamp(i + num3, 4, Main.maxTilesX - 4);
        int num8 = Utils.Clamp(j - num3, 4, Main.maxTilesY - 4);
        int num9 = Utils.Clamp(j + num3, 4, Main.maxTilesY - 4);
        for (int i2 = num6; i2 <= num7; i2++) {
            for (int j2 = num8; j2 <= num9; j2++) {
                int checkTileType = Main.tile[i2, j2].TileType;
                if (Main.tileAlch[checkTileType] || (checkTileType >= TileID.Count && TileLoader.GetTile(checkTileType) is PlantBase)) {
                    num5++;
                }
            }
        }
        if (num5 < num4) {
            bool flag = false;
            for (int k = 0; k < validTiles.Length; k++) {
                if (Main.tile[i, j].TileType == validTiles[k]) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                return false;
            }
            j -= 1;
            if (!Main.tile[i, j].HasTile && Main.tile[i, j + 1].HasUnactuatedTile && !Main.tile[i, j + 1].IsHalfBlock && Main.tile[i, j + 1].Slope == 0) {
                Tile tile = Main.tile[i, j];
                PlantBase plant = TileLoader.GetTile(tileTypeToGrow) as PlantBase;
                tile.ClearTile();
                tile.TileType = tileTypeToGrow;
                tile.HasTile = true;
                tile.TileFrameX = (short)(plant.FrameWidth * style);
                onPlaced?.Invoke(new Point(i, j));
                if (Main.tile[i, j].HasTile && Main.netMode == NetmodeID.Server) {
                    NetMessage.SendTileSquare(-1, i, j);
                }
            }

            return true;
        }

        return false;
    }

    private class ReplaceCutTilesWithSeed : ILoadable {
        public void Load(Mod mod) {
            On_Player.PlaceThing_Tiles += On_Player_PlaceThing_Tiles;
        }

        private void On_Player_PlaceThing_Tiles(On_Player.orig_PlaceThing_Tiles orig, Player self) {
            orig(self);
            return;

            Item item = self.inventory[self.selectedItem];
            int tileToCreate = item.createTile;
            if (tileToCreate < 0 || !(self.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost - (float)self.blockRange <= (float)Player.tileTargetX) || !((self.position.X + (float)self.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f + (float)self.blockRange >= (float)Player.tileTargetX) || !(self.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost - (float)self.blockRange <= (float)Player.tileTargetY) || !((self.position.Y + (float)self.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f + (float)self.blockRange >= (float)Player.tileTargetY))
                return;

            self.cursorItemIconEnabled = true;
            bool flag = PlaceCheckHooks.Player_PlaceThing_Tiles_CheckLavaBlocking(self);
            bool canUse = true;
            canUse = PlaceCheckHooks.Player_PlaceThing_Tiles_CheckGamepadTorchUsability(self, canUse);
            canUse = PlaceCheckHooks.Player_PlaceThing_Tiles_CheckWandUsability(self, canUse);
            canUse = PlaceCheckHooks.Player_PlaceThing_Tiles_CheckRopeUsability(self, canUse);
            canUse = PlaceCheckHooks.Player_PlaceThing_Tiles_CheckFlexibleWand(self, canUse);
            if (self.TileReplacementEnabled)
                canUse = PlaceCheckHooks.Player_PlaceThing_TryReplacingTiles(self, canUse);

            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (tile.HasTile) {
                if (tileToCreate == 23 && tile.TileType == 59)
                    tileToCreate = 661;

                if (tileToCreate == 199 && tile.TileType == 59)
                    tileToCreate = 662;
            }

            if (canUse && ((!tile.HasTile && !flag) || (Main.tileCut[tile.TileType] && tile.TileType != 484) || (tile.TileType >= 373 && tile.TileType <= 375) || tile.TileType == 461 || tileToCreate == 199 || tileToCreate == 23 || tileToCreate == 662 || tileToCreate == 661 || tileToCreate == 2 || tileToCreate == 109 || tileToCreate == 60 || tileToCreate == 70 || tileToCreate == 633 || tileToCreate == ModContent.TileType<BackwoodsGrass>() || Main.tileMoss[tileToCreate] || TileID.Sets.BreakableWhenPlacing[tile.TileType]) && self.ItemTimeIsZero && self.itemAnimation > 0 && self.controlUseItem) {
                bool canPlace = false;
                bool newObjectType = false;
                bool? overrideCanPlace = null;
                int? forcedRandom = null;
                TileObject objectData = default(TileObject);
                PlaceCheckHooks.Player_FigureOutWhatToPlace(self, tile, item, out tileToCreate, out var previewPlaceStyle, out overrideCanPlace, out forcedRandom);
                if (overrideCanPlace.HasValue) {
                    canPlace = overrideCanPlace.Value;
                }
                else if (TileObjectData.CustomPlace(tileToCreate, previewPlaceStyle) && !(tileToCreate >= TileID.Count && TileLoader.GetTile(tileToCreate) is PlantBase) && tileToCreate != 82 && tileToCreate != 227) {
                    newObjectType = true;
                    canPlace = TileObject.CanPlace(Player.tileTargetX, Player.tileTargetY, (ushort)tileToCreate, previewPlaceStyle, self.direction, out objectData, onlyCheck: false, forcedRandom);
                    PlaceCheckHooks.Player_PlaceThing_Tiles_BlockPlacementIfOverPlayers(self, ref canPlace, ref objectData);
                    PlaceCheckHooks.Player_PlaceThing_Tiles_BlockPlacementForRepeatedPigronatas(self, ref canPlace, ref objectData);
                    PlaceCheckHooks.Player_PlaceThing_Tiles_BlockPlacementForRepeatedPumpkins(self, ref canPlace, ref objectData);
                    PlaceCheckHooks.Player_PlaceThing_Tiles_BlockPlacementForRepeatedCoralAndBeachPiless(self, ref canPlace, ref objectData);
                }
                else {
                    canPlace = PlaceCheckHooks.Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(self, canPlace);
                }

                if (canPlace) {
                    PlaceCheckHooks.Player_PlaceThing_Tiles_PlaceIt(self, newObjectType, objectData, tileToCreate);
                }
            }
        }

        public void Unload() { }
    }
}
