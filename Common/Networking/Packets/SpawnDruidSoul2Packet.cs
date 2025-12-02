using Microsoft.Xna.Framework;

using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Networking.Packets;

sealed class SpawnDruidSoul2Packet : NetPacket {
    public SpawnDruidSoul2Packet(Vector2 position) {
        Writer.WriteVector2(position);
    }

    public override void Read(BinaryReader reader, int sender) {
        Vector2 position = reader.ReadVector2();

        int npc = NPC.NewNPC(null, (int)position.X, (int)position.Y, ModContent.NPCType<DruidSoul2>());
        if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
            NetMessage.SendData(MessageID.SyncNPC, number: npc);
        }

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new SpawnDruidSoul2Packet(position), ignoreClient: sender);
        }
    }
}
