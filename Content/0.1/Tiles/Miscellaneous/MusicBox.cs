using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

abstract class MusicBox : ModTile {
    protected virtual int GoreOffsetX { get; }

    protected abstract int CursorItemType { get; }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;

        Main.tileLighted[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleLineSkip = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.MusicBox"));

        DustType = -1;
        HitSound = SoundID.Dig;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = CursorItemType;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        Tile tile = Main.tile[i, j];

        if (Main.gamePaused || !Main.instance.IsActive || Lighting.UpdateEveryFrame && !Main.rand.NextBool(4)) {
            return;
        }

        if (tile.TileFrameX == 36 && tile.TileFrameY % 36 == 0 && (int)Main.timeForVisualEffects % 7 == 0 && Main.rand.NextBool(3)) {
            int goreType = Main.rand.Next(570, 573);
            Vector2 position = new Vector2(i * 16 + 8 + GoreOffsetX, j * 16 - 8);
            Vector2 velocity = new Vector2(Main.WindForVisuals * 2f, -0.5f);
            velocity.X *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
            velocity.Y *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
            if (goreType == 572) {
                position.X -= 8f;
            }

            if (goreType == 571) {
                position.X -= 4f;
            }

            if (!Main.dedServ) {
                Gore.NewGore(new EntitySource_TileUpdate(i, j), position, velocity, goreType, 0.8f);
            }
        }
    }
}