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
        MouseVariables mousePositionStorage = player.GetModPlayer<MouseVariables>();
        mousePositionStorage.MousePosition = mousePosition;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SyncMousePositionPacket(player, mousePosition), ignoreClient: sender);
        }
    }
}

sealed class SyncMousePositionPacket2 : NetPacket {
    public SyncMousePositionPacket2(Player player, Vector2 cappedMousePosition) {
        Writer.TryWriteSenderPlayer(player);
        Writer.WriteVector2(cappedMousePosition);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out Player player)) {
            return;
        }

        Vector2 cappedMousePosition = reader.ReadVector2();
        MouseVariables mousePositionStorage = player.GetModPlayer<MouseVariables>();
        mousePositionStorage.CappedMousePosition = cappedMousePosition;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SyncMousePositionPacket2(player, cappedMousePosition), ignoreClient: sender);
        }
    }
}

