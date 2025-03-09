using RoA.Common.Items;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class ExtraMannequinInfoItemsPacket : NetPacket {
    public ExtraMannequinInfoItemsPacket(Player player, int mannequinIndex, bool dye) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(mannequinIndex);
        Writer.Write(dye);
        MannequinWreathSlotSupport.MannequinsInWorldSystem.WriteItem(mannequinIndex, dye, Writer);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int mannequinIndex = reader.ReadInt32();
        bool dye = reader.ReadBoolean();
        MannequinWreathSlotSupport.MannequinsInWorldSystem.ReadItem(mannequinIndex, dye, reader);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new ExtraMannequinInfoItemsPacket(player, mannequinIndex, dye), ignoreClient: sender);
        }
    }
}
