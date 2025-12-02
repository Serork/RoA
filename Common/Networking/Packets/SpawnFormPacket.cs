using RoA.Common.Druid.Forms;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class SpawnFormPacket : NetPacket {
    public SpawnFormPacket(Player player, string serializedData) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(serializedData);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        string serializedData = reader.ReadString();
        BaseFormHandler handler = player.GetFormHandler();
        handler.InternalSetCurrentForm(handler.Deserialize(serializedData));

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SpawnFormPacket(player, serializedData), ignoreClient: sender);
        }
    }
}

sealed class SpawnFormPacket2 : NetPacket {
    public SpawnFormPacket2(Player player) {
        Writer.TryWriteSenderPlayer(player);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        BaseFormHandler handler = player.GetFormHandler();
        handler._sync = 0;

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SpawnFormPacket2(player), ignoreClient: sender);
        }
    }
}
