using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;
using System.IO;
using Terraria.ID;
using Terraria;
using RoA.Common.Druid.Wreath;

namespace RoA.Common.Networking.Packets;

sealed class FormPacket1 : NetPacket {
    public FormPacket1(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        player.GetModPlayer<WreathHandler>().Reset1();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new FormPacket1(player), ignoreClient: sender);
        }
    }
}

