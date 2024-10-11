using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class ItemAnimationPacket : NetPacket {
    public ItemAnimationPacket(Player player, int itemAnimation) {
        Writer.TryWriteSenderPlayer(player);
		Writer.Write(itemAnimation);
    }

	public override void Read(BinaryReader reader, int sender) {
		if (!reader.TryReadSenderPlayer(sender, out var player)) {
			return;
		}

        int itemAnimation = reader.ReadInt32();

		player.SetItemAnimation(itemAnimation);

        if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new ItemAnimationPacket(player, itemAnimation), ignoreClient: sender);
		}
	}
}
