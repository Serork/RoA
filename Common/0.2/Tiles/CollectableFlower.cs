using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

abstract class CollectableFlower : ModTile {
    protected abstract ushort DropItemType { get; }
    protected abstract Color MapColor { get; }
    protected abstract int[] AnchorValidTileTypes { get; }
    protected abstract ushort HitDustType { get; }

    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTileTypes;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;
        DustType = HitDustType;

        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 750;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(MapColor, name);

        MineResist = 0.01f;
    }

    public sealed override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;

    public sealed override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (CanBeCollected(i, j)) {
            yield return new(DropItemType);
        }
    }

    public sealed override void MouseOver(int i, int j) {
        if (!CanBeCollected(i, j)) {
            return;
        }

        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 40)) {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = GetItemDrops(i, j).Single().type;
        }
    }

    public sealed override bool RightClick(int i, int j) {
        if (!CanBeCollected(i, j)) {
            return false;
        }

        Player player = Main.LocalPlayer;
        if (player.IsWithinSnappngRangeToTile(i, j, 40)) {
            Tile tile = WorldGenHelper.GetTileSafely(i, j);
            WorldGen.KillTile(i, j);
            if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
            }

            return true;
        }

        return false;
    }

    protected virtual bool CanBeCollected(int i, int j) => true;
}
