using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class ItemPositionPacket : NetPacket {
    public ItemPositionPacket(Player player, int itemIndex, Vector2 itemVelocity, bool shimmered, bool beingGrabbed) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(itemIndex);
        Writer.WriteVector2(itemVelocity);
        Writer.Write(shimmered);
        Writer.Write(beingGrabbed);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int itemIndex = reader.ReadInt32();
        Vector2 itemVelocity = reader.ReadVector2();
        bool shimmered = reader.ReadBoolean();
        bool beingGrabbed = reader.ReadBoolean();

        Main.item[itemIndex].velocity = itemVelocity;
        Main.item[itemIndex].shimmered = shimmered;
        Main.item[itemIndex].beingGrabbed = beingGrabbed;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new ItemPositionPacket(player, itemIndex, itemVelocity, shimmered, beingGrabbed), ignoreClient: sender);
        }
    }
}
