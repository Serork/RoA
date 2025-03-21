using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class RecognizeHitPacket : NetPacket {
    public RecognizeHitPacket(Player player, int npcWhoAmI) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(npcWhoAmI);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int npcWhoAmI = reader.ReadInt32();
        NPC npc = Main.npc[npcWhoAmI];
        npc.As<GrimDefender>().MakeAngry();

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new RecognizeHitPacket(player, npcWhoAmI), ignoreClient: sender);
        }
    }
}
