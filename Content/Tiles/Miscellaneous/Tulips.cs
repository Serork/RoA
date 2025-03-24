using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class GrowTulips : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_WorldGen.plantDye += On_WorldGen_plantDye;
    }

    private void On_WorldGen_plantDye(On_WorldGen.orig_plantDye orig, int i, int j, bool exoticPlant) {
        orig(i, j, exoticPlant);

        if (!exoticPlant) {
            UnifiedRandom unifiedRandom = (WorldGen.gen ? WorldGen.genRand : Main.rand);
            if (!Main.tile[i, j].HasTile || i < 95 || i > Main.maxTilesX - 95 || j < 95 || j > Main.maxTilesY - 95)
                return;

            int num = 90;
            num = 240;

            if (((double)j < Main.worldSurface || WorldGen.remixWorldGen) && (!Main.tile[i, j - 1].HasTile ||
                Main.tileCut[Main.tile[i, j - 1].TileType])) {
                int num2 = Utils.Clamp(i - num, 1, Main.maxTilesX - 1 - 1);
                int num3 = Utils.Clamp(i + num, 1, Main.maxTilesX - 1 - 1);
                int num4 = Utils.Clamp(j - num, 1, Main.maxTilesY - 1 - 1);
                int num5 = Utils.Clamp(j + num, 1, Main.maxTilesY - 1 - 1);
                ushort tileType = (ushort)ModContent.TileType<ExoticTulip>();
                bool flag = false;
                for (int k = num2; k < num3; k++) {
                    for (int l = num4; l < num5; l++) {
                        if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tileType)
                            flag = true;
                    }
                }

                if (!flag) {
                    if (i < Main.maxTilesX / 3 || i > Main.maxTilesX - Main.maxTilesX / 3) {
                        if (Main.tile[i, j].TileType == TileID.Grass || Main.tile[i, j].TileType == TileID.GolfGrass) {
                            if (!Main.tile[i, j - 1].AnyLiquid() && !Main.tile[i, j - 1].AnyWall() &&
                                Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0) {
                                //Main.LocalPlayer.position = new Vector2(i, j).ToWorldCoordinates();
                                WorldGen.PlaceTile(i, j - 1, tileType, mute: true, forced: true, style: 0);
                            }
                        }
                    }
                }
            }
            if ((double)j > Main.worldSurface && (!Main.tile[i, j - 1].HasTile ||
                Main.tileCut[Main.tile[i, j - 1].TileType])) {
                int num2 = Utils.Clamp(i - num, 1, Main.maxTilesX - 1 - 1);
                int num3 = Utils.Clamp(i + num, 1, Main.maxTilesX - 1 - 1);
                int num4 = Utils.Clamp(j - num, 1, Main.maxTilesY - 1 - 1);
                int num5 = Utils.Clamp(j + num, 1, Main.maxTilesY - 1 - 1);
                ushort tileType = (ushort)ModContent.TileType<SweetTulip>();
                bool flag = false;
                for (int k = num2; k < num3; k++) {
                    for (int l = num4; l < num5; l++) {
                        if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tileType)
                            flag = true;
                    }
                }
                if (!flag) {
                    if (Main.tile[i, j].TileType == TileID.JungleGrass) {
                        if (!Main.tile[i, j - 1].AnyLiquid() &&
                            Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0) {
                            //Main.LocalPlayer.position = new Vector2(i, j).ToWorldCoordinates();
                            WorldGen.PlaceTile(i, j - 1, tileType, mute: true, forced: true, style: 1);
                        }
                    }
                }
            }
            if (NPC.downedBoss3) {
                if ((double)j > Main.worldSurface && (!Main.tile[i, j - 1].HasTile
                    /* || Main.tileCut[Main.tile[i, j - 1].TileType]*/)) {
                    int num2 = Utils.Clamp(i - num, 1, Main.maxTilesX - 1 - 1);
                    int num3 = Utils.Clamp(i + num, 1, Main.maxTilesX - 1 - 1);
                    int num4 = Utils.Clamp(j - num, 1, Main.maxTilesY - 1 - 1);
                    int num5 = Utils.Clamp(j + num, 1, Main.maxTilesY - 1 - 1);
                    ushort tileType = (ushort)ModContent.TileType<WeepingTulip>();
                    bool flag = false;
                    for (int k = num2; k < num3; k++) {
                        for (int l = num4; l < num5; l++) {
                            if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tileType)
                                flag = true;
                        }
                    }
                    if (!flag) {
                        TileObjectData objectData = TileObjectData.GetTileData(tileType, 0);
                        if (objectData.AnchorValidTiles.Contains(Main.tile[i, j].TileType)) {
                            if (!Main.tile[i, j - 1].AnyLiquid() && Main.wallDungeon[Main.tile[i, j - 1].WallType] &&
                                Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0) {
                                //Main.LocalPlayer.position = new Vector2(i, j).ToWorldCoordinates();
                                WorldGen.PlaceTile(i, j - 1, tileType, mute: true, forced: true, style: 2);
                            }
                        }
                    }
                }
            }
        }
    }

    void ILoadable.Unload() { }
}

sealed class ExoticTulip : ModTile {
    public override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorValidTiles = [TileID.Grass];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 750;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new(216, 78, 142), name);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 10;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) => [new((ushort)ModContent.ItemType<Items.Weapons.Druidic.ExoticTulip>())];

    public override void MouseOver(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = GetItemDrops(i, j).Single().type;
    }

    public override bool RightClick(int x, int y) {
        WorldGen.KillTile(x, y, false, false, false);

        return true;
    }
}

sealed class SweetTulip : ModTile {
    public override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorValidTiles = [TileID.JungleGrass];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 750;

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new(255, 165, 0), name);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 10;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) => [new((ushort)ModContent.ItemType<Items.Weapons.Druidic.SweetTulip>())];

    public override void MouseOver(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = GetItemDrops(i, j).Single().type;
    }

    public override bool RightClick(int x, int y) {
        WorldGen.KillTile(x, y, false, false, false);

        return true;
    }
}

sealed class WeepingTulip : ModTile {
    public override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorValidTiles = [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 750;

        HitSound = SoundID.Grass;
        DustType = DustID.Bone;

        RootsDrawing.ShouldDraw[Type] = true;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new(0, 0, 255), name);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 10;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) => [new((ushort)ModContent.ItemType<Items.Weapons.Druidic.WeepingTulip>())];

    public override void MouseOver(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = GetItemDrops(i, j).Single().type;
    }

    public override bool RightClick(int x, int y) {
        WorldGen.KillTile(x, y, false, false, false);

        return true;
    }
}