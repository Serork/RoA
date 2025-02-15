using RoA.Content.Tiles.Miscellaneous;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class TrappedChestActionPacket : NetPacket {
    public TrappedChestActionPacket(Player player, int left, int top) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write((short)left);
        Writer.Write((short)top);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }
        int num1 = (int)reader.ReadInt16();
        int num2 = (int)reader.ReadInt16();
        Wiring.SetCurrentUser(player.whoAmI);
        TrappedChests.Trigger(num1, num2);
        Wiring.SetCurrentUser();
        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new TrappedChestActionPacket(player, num1, num2), ignoreClient: sender);
        }
    }
}

