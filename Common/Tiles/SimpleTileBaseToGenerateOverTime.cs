using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;

namespace RoA.Common.Tiles;

abstract class SimpleTileBaseToGenerateOverTime : ModTile {
    protected virtual int[] AnchorValidTiles { get; }
    protected virtual int[] AnchorValidWalls { get; } = null;

    protected virtual byte StyleX { get; }

    protected virtual bool OnSurface { get; } = true;
    protected virtual bool InDungeon { get; }

    protected virtual byte Amount { get; } = 1;

    protected virtual ushort ExtraChance { get; }

    protected abstract Color MapColor { get; }

    protected virtual ushort DropItem { get; }

    protected virtual byte XSize { get; } = 1;
    protected virtual byte YSize { get; } = 1;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(MapColor, name);

        SimpleTileGenerationOverTimeSystem.Register(this, AnchorValidTiles, StyleX, OnSurface, InDungeon, Amount, ExtraChance, AnchorValidWalls, XSize, YSize);

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public override IEnumerable<Item> GetItemDrops(int i, int j) => [new(DropItem)];

    public override void MouseOver(int i, int j) {
        Player player = Main.player[Main.myPlayer];
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = GetItemDrops(i, j).Single().type;
    }

    public override bool RightClick(int x, int y) {
        WorldGen.KillTile(x, y, false, false, false);

        SimpleTileGenerationOverTimeSystem.ResetState(this);

        return true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => SimpleTileGenerationOverTimeSystem.ResetState(this);
}