using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class BeaconTeleportPacket : NetPacket {
    public BeaconTeleportPacket(Player player, int i, int j) {
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

        Beacon.HandleTeleport(player, i, j);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new BeaconTeleportPacket(player, i, j), ignoreClient: sender);
        }
    }
}
