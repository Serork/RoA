using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Core.Utility;
using RoA.Utilities;

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
        BaseFormHandler handler = player.GetModPlayer<BaseFormHandler>();
        handler.InternalSetCurrentForm(handler.Deserialize(serializedData));

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SpawnFormPacket(player, serializedData), ignoreClient: sender);
        }
    }
}
