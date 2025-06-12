using RoA.Content.NPCs.Friendly;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class TradedWithHunterPacket1 : NetPacket {
    public TradedWithHunterPacket1(Player player, bool active) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(active);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        bool active = reader.ReadBoolean();
        player.GetModPlayer<Hunter.DropHunterRewardHandler>().Trade1(active);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new TradedWithHunterPacket1(player, active), ignoreClient: sender);
        }
    }
}
