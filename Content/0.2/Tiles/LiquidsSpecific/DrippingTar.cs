using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Tiles.LiquidsSpecific;

sealed class DrippingTar : ModTile {
    public override void Load() {
        On_WorldGen.TryKillingReplaceableTile += On_WorldGen_TryKillingReplaceableTile;
    }

    private bool On_WorldGen_TryKillingReplaceableTile(On_WorldGen.orig_TryKillingReplaceableTile orig, int x, int y, int tileType) {
        if (!WorldGen.InWorld(x, y, 2))
            return false;

        if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<DrippingTar>()) {
            if (Main.tile[x, y].TileType != tileType) {
                WorldGen.KillTile(x, y);
                if (!Main.tile[x, y].HasTile && Main.netMode != 0)
                    NetMessage.SendData(17, -1, -1, null, 0, x, y);

                return true;
            }

            return false;
        }

        return orig(x, y, tileType);
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;

        AddMapEntry(Liquids.Tar.LiquidColor, CreateMapEntryName());
    }

    public override bool CanDrop(int i, int j) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tile2 = Main.tile[i, j - 1];
        if (!tile2.HasTile || tile2.BottomSlope || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType])
            WorldGen.KillTile(i, j);

        return false;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        EmitTarLiquidDrops(i, j);

        return false;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rand")]
    public extern static ref UnifiedRandom TileDrawing_rand(TileDrawing self);

    private void EmitTarLiquidDrops(int i, int j) {
        int num = 60;
        Tile tileCache = Main.tile[i, j];
        //num = 180;

        var _rand = TileDrawing_rand(Main.instance.TilesRenderer);
        if (tileCache.LiquidAmount != 0 || _rand.Next(num * 2) != 0)
            return;

        Rectangle rectangle = new Rectangle(i * 16, j * 16, 16, 16);
        rectangle.X -= 34;
        rectangle.Width += 68;
        rectangle.Y -= 100;
        rectangle.Height = 400;
        var _gore = Main.gore;
        for (int k = 0; k < 600; k++) {
            /*
			if (_gore[k].active && ((_gore[k].type >= 706 && _gore[k].type <= 717) || _gore[k].type == 943 || _gore[k].type == 1147 || (_gore[k].type >= 1160 && _gore[k].type <= 1162))) {
			*/
            if (_gore[k].active && GoreID.Sets.LiquidDroplet[_gore[k].type]) {
                Rectangle value = new Rectangle((int)_gore[k].position.X, (int)_gore[k].position.Y, 16, 16);
                if (rectangle.Intersects(value))
                    return;
            }
        }

        Vector2 position = new Vector2(i * 16, j * 16);
        int type = ModContent.GoreType<Gores.TarDroplet>();

        int num2 = Gore.NewGore(null, position, default(Vector2), type);
        _gore[num2].velocity *= 0f;
    }

}
