using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class ElathaAmuletUsagePacket : NetPacket {
    public ElathaAmuletUsagePacket(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        ElathaAmulet.ChangeMoonPhase(player);
    }
}
