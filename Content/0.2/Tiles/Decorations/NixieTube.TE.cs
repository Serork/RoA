using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Decorations;

sealed class NixieTubeTE : ModTileEntity {
    public static ushort ACTIVATIONTIME => 100;

    public bool Activated;
    public int DeactivatedTimer, DeactivatedTimer2, ActivatedForTimer;
    public bool Active => DeactivatedTimer <= ACTIVATIONTIME / 3;
    public Item? Dye = null;

    public override void Update() {
        Dye ??= new Item();

        if (!Activated) {
            return;
        }

        if ((DeactivatedTimer -= 5) < 0) {
            DeactivatedTimer = DeactivatedTimer2 / 2;
            DeactivatedTimer2 /= 2;
        }
    }

    public void Activate() {
        Activated = true;

        DeactivatedTimer = DeactivatedTimer2 = ACTIVATIONTIME;
    }

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        ushort tapperTileType = (ushort)ModContent.TileType<NixieTube>();
        return tile.HasTile && tile.TileType == tapperTileType;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        int id = Place(i, j);
        return id;
    }
}
