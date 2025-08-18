using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class SyncMousePositionPacket : NetPacket {
    public SyncMousePositionPacket(Player player, Vector2 mousePosition) {
        Writer.TryWriteSenderPlayer(player);
        Writer.WriteVector2(mousePosition);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out Player player)) {
            return;
        }

        Vector2 mousePosition = reader.ReadVector2();
        MousePositionStorage mousePositionStorage = player.GetModPlayer<MousePositionStorage>();
        mousePositionStorage.MousePosition = mousePosition;

        Helper.NewMessage(mousePositionStorage.MousePosition);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SyncMousePositionPacket(player, mousePosition), ignoreClient: sender);
        }
    }
}
