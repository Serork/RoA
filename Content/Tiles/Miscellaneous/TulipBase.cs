using Microsoft.Xna.Framework.Graphics;
using RoA.Content.World.Generations;

using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;

namespace RoA.Content.Tiles.Miscellaneous;

abstract class TulipBase : ModTile {
    protected virtual int[] AnchorValidTiles { get; }
    protected virtual int[] AnchorValidWalls { get; } = null;

    protected virtual byte StyleX { get; }

    protected virtual bool OnSurface { get; } = true;
    protected virtual bool InDungeon { get; }

    protected virtual byte Amount { get; } = 1;

    protected virtual ushort ExtraChance { get; }

    protected abstract Color MapColor { get; }

    protected virtual ushort DropItem { get; }

    public sealed override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(MapColor, name);

        TulipGenerationSystem.Register(this, AnchorValidTiles, StyleX, OnSurface, InDungeon, Amount, ExtraChance, AnchorValidWalls);

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public override IEnumerable<Item> GetItemDrops(int i, int j) => [new(DropItem)];

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = GetItemDrops(i, j).Single().type;
    }

    public override bool RightClick(int x, int y) {
        WorldGen.KillTile(x, y, false, false, false);

        TulipGenerationSystem.ResetState(this);

        return true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => TulipGenerationSystem.ResetState(this);
}