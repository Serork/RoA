using RoA.Content.Tiles.Danger;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class JawTrapActivatedPacket : NetPacket {
    public JawTrapActivatedPacket(Player player, int i, int j) {
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

        JawTrap.JawTrapTE jawTrapTE = TileHelper.GetTE<JawTrap.JawTrapTE>(i, j);
        jawTrapTE.Activate(player);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new JawTrapActivatedPacket(player, i, j), ignoreClient: sender);
        }
    }
}
