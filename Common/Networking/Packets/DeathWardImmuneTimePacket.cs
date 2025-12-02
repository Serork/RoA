using RoA.Content.Buffs;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class DeathWardImmuneTimePacket : NetPacket {
    public DeathWardImmuneTimePacket(Player player, int immuneTime) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(immuneTime);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int immuneTime = reader.ReadInt32();

        player.GetModPlayer<BehelitPlayer>().DeathWardImmune(immuneTime);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new DeathWardImmuneTimePacket(player, immuneTime), ignoreClient: sender);
        }
    }
}
