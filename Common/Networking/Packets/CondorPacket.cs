using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class CondorPacket : NetPacket {
    public CondorPacket(Player player, bool active, Vector2 mousePosition) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(active);
        Writer.WriteVector2(mousePosition);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        bool active = reader.ReadBoolean();
        Vector2 mousePosition = reader.ReadVector2();

        player.GetModPlayer<RodOfTheCondor.CondorWingsHandler>().ReceivePacket(active, mousePosition, player);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new CondorPacket(player, active, mousePosition), ignoreClient: sender);
        }
    }
}
