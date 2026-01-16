using ModLiquidLib.ModLoader;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Liquids;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Tiles.Ambient;

sealed class SwellingTarTE : ModTileEntity {
    private static string TIMESAVEKEY => RoA.ModName + "swellingtartime";
    private static string TIMESAVEKEY2 => RoA.ModName + "swellingtarneededtime";

    private bool _sync;

    public double Time { get; private set; }
    public double NeededTime { get; private set; }

    public bool IsReady => Time >= NeededTime;

    public void Collect(Player player) {
        Reset();
        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new SwellingTarCollectPacket(player, (ushort)Position.X, (ushort)Position.Y));
        }
    }

    internal void Reset() {
        Time = 0.0;

        if (Helper.SinglePlayerOrServer) {
            NeededTime = Main.dayLength * 7;
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
    }

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        ushort tapperTileType = (ushort)ModContent.TileType<SwellingTar>();
        return tile.HasTile && tile.TileType == tapperTileType;
    }

    public override void Update() {
        if (!IsReady) {
            Tile tile = Main.tile[Position.X, Position.Y];
            Tile tile2 = Main.tile[Position.X, Position.Y + 1];
            if ((tile.LiquidAmount > 0 && tile.LiquidType == LiquidLoader.LiquidType<Tar>()) ||
                (tile2.LiquidAmount > 0 && tile2.LiquidType == LiquidLoader.LiquidType<Tar>())) {
                Time += Main.dayRate;
            }
            _sync = false;
        }
        else if (!_sync) {
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            _sync = true;
        }
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

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

    public override void SaveData(TagCompound tag) {
        tag[TIMESAVEKEY] = Time;
        tag[TIMESAVEKEY2] = NeededTime;
    }

    public override void LoadData(TagCompound tag) {
        Time = tag.GetDouble(TIMESAVEKEY);
        NeededTime = tag.GetDouble(TIMESAVEKEY2);
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(Time);
        writer.Write(NeededTime);
    }

    public override void NetReceive(BinaryReader reader) {
        Time = reader.ReadDouble();
        NeededTime = reader.ReadDouble();
    }
}
