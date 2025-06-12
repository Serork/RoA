using RoA.Core.Utility;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class NPCExtraValuePacket : NetPacket {
    public NPCExtraValuePacket(int whoAmI, int extraValue) {
        Writer.Write(whoAmI);
        Writer.Write(extraValue);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        int extraValue = reader.ReadInt32();
        if (whoAmI >= 0 && whoAmI <= 200) {
            Main.npc[whoAmI].extraValue = extraValue;
            Main.npc[whoAmI].moneyPing(Main.npc[whoAmI].Center + Helper.VelocityToPoint(Main.npc[whoAmI].Center, Main.player[Main.npc[whoAmI].target].Center, 25f));
        }
    }
}
