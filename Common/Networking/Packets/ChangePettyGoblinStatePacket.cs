using RoA.Content.NPCs.Enemies.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class ChangePettyGoblinStatePacket : NetPacket {
    public ChangePettyGoblinStatePacket(Player player, int whoAmI) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(whoAmI);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int whoAmI = reader.ReadInt32();

        NPC npc = Main.npc[whoAmI];
        if (npc != null && npc.active) {
            (npc.ModNPC as PettyGoblin).CurrentState = 1;
            (npc.ModNPC as PettyGoblin).Attacking = false;
            npc.netUpdate = true;
        }


        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new ChangePettyGoblinStatePacket(player, whoAmI), ignoreClient: sender);
        }
    }
}
