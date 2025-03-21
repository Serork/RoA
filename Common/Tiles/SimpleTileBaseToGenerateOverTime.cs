using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

[Autoload(false)]
abstract class SimpleTileBaseToGenerateOverTime : ModTile {
    public virtual int[] AnchorValidTiles { get; }
    public virtual int[] AnchorValidWalls { get; } = null;
    public virtual Predicate<ushort> ConditionForWallToBeValid { get; } = null;

    public virtual byte StyleX { get; }

    public virtual bool OnSurface { get; } = true;
    public virtual bool InUnderground { get; }

    public virtual byte Amount { get; } = 1;

    public virtual ushort ExtraChance { get; } = 30;

    public virtual Color MapColor { get; }

    public virtual ushort DropItem { get; }

    public virtual byte XSize { get; } = 1;
    public virtual byte YSize { get; } = 1;

    public override void SetStaticDefaults() {
        SafeSetStaticDefaults();

        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(MapColor, name);

        SimpleTileGenerationOverTimeSystem.Register(this);
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