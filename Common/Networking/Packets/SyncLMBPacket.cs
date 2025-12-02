using RoA.Common.Players;
using RoA.Core.Utility;

using System.IO;

using Terraria;

using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class SyncLMBPacket : NetPacket {
    public SyncLMBPacket(Player player, bool state) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(state);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        bool state = reader.ReadBoolean();

        player.GetModPlayer<MouseVariables>().HoldingLMB = state;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SyncLMBPacket(player, state), ignoreClient: sender);
        }
    }
}