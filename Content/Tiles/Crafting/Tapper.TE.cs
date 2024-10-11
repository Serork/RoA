using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Content.Dusts;
using RoA.Core.Utility;

using System.Drawing;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Tiles.Crafting;

class TapperTE : ModTileEntity {
    sealed class GalipotCollectPacket : NetPacket {
        public GalipotCollectPacket(Player player, int i, int j) {
            Writer.TryWriteSenderPlayer(player);
            Writer.Write(i);
            Writer.Write(j);
        }

        public override void Read(BinaryReader reader, int sender) {
            if (!reader.TryReadSenderPlayer(sender, out var player)) {
                return;
            }

            int i = reader.ReadInt32();
            int j = reader.ReadInt32();
            TapperTE tapperTE = TileHelper.GetTE<TapperTE>(i, j);
            if (tapperTE != null) {
                tapperTE.Reset();
            }

            if (Main.netMode == NetmodeID.Server) {
                MultiplayerSystem.SendPacket(new GalipotCollectPacket(player, i, j), ignoreClient: sender);
            }
        }
    }

    private const double TIMETOBECOLLECTABLE = 200 /*43200.0 / 2.0*/; // half of day length

    private bool _sync;

    public double Time { get; private set; }

    public bool IsReadyToCollectGalipot => Time >= TIMETOBECOLLECTABLE;

    public float Progress => (float)(Time / TIMETOBECOLLECTABLE) * 0.9f;

    public void CollectGalipot(Player player) {
        Reset();
        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new GalipotCollectPacket(player, Position.X, Position.Y));
        }
    }

    public void Reset() {
        Time = 0.0;
        CollectDust(15, true);
    }

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        ushort tapperTileType = (ushort)ModContent.TileType<Tapper>();
        return tile.HasTile && tile.TileType == tapperTileType;
    }

    public override void Update() {
        if (!IsReadyToCollectGalipot) {
            Time += Main.dayRate;
            _sync = false;
        }
        else if (!_sync) {
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            _sync = true;
        }
    }

    public override void OnKill() {
        CollectDust();
    }

    public void CollectDust(int count = 30, bool flag = false) {
        for (int i2 = 0; i2 < count; i2++) {
            if (Main.rand.NextChance(flag ? 0.3f : (Progress * 0.4f))) {
                int dustId = Dust.NewDust(Position.ToWorldCoordinates() - new Vector2(16f, 12f), 24, 20, ModContent.DustType<Galipot>());
                Dust dust = Main.dust[dustId];
                float progress = (flag ? 0.9f : Progress) * 1.25f;
                dust.velocity *= 1.25f;
                dust.velocity *= 0.25f + 0.15f * progress;
                dust.scale *= 0.85f;
            }
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
        tag["tapperTime"] = Time;
    }

    public override void LoadData(TagCompound tag) {
        Time = tag.GetDouble("tapperTime");
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(Time);
    }

    public override void NetReceive(BinaryReader reader) {
        Time = reader.ReadDouble();
    }
}
