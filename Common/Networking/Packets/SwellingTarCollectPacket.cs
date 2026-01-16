using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class SwellingTarCollectPacket : NetPacket {
    public SwellingTarCollectPacket(Player player, ushort i, ushort j) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(i);
        Writer.Write(j);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        ushort i = reader.ReadUInt16();
        ushort j = reader.ReadUInt16();
        SwellingTarTE swellingTarTE = TileHelper.GetTE<SwellingTarTE>(i, j)!;
        swellingTarTE?.Reset();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SwellingTarCollectPacket(player, i, j), ignoreClient: sender);
        }
    }
}
