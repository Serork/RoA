using System.IO;

using Terraria.ID;
using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class ExtraMannequinInfoPacket : NetPacket {
    public ExtraMannequinInfoPacket(byte who) {
        Writer.Write(who);
        MannequinWreathSlotSupport.MannequinsInWorldSystem.SendAllMannequinExtraInfo(Writer);
    }

    public override void Read(BinaryReader reader, int sender) {
        byte who = reader.ReadByte();
        MannequinWreathSlotSupport.MannequinsInWorldSystem.ReceiveAllMannequinExtraInfo(reader);
        if (Main.netMode == NetmodeID.Server) {
            Main.player[who].GetModPlayer<MannequinWreathSlotSupport.MannequinsInWorldSystem.SyncOnJoining>().SyncPlayer(-1, sender, false);
        }
    }
}
