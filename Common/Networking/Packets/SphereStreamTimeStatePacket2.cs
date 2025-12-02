using RoA.Content.Items.Special;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class SphereStreamTimeStatePacket2 : NetPacket {
    public SphereStreamTimeStatePacket2(int whoAmI, int condorTime, int condorTime2) {
        Writer.Write(whoAmI);
        Writer.Write(condorTime);
        Writer.Write(condorTime2);
        Item item3 = Main.item[whoAmI];
        Writer.WriteVector2(item3.position);
        Writer.WriteVector2(item3.velocity);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        int condorTime = reader.ReadInt32();
        int condorTime2 = reader.ReadInt32();
        Item item3 = Main.item[whoAmI];
        item3.GetGlobalItem<SphereHandler>().UpdateStreamTime(condorTime, condorTime2);
        item3.position = reader.ReadVector2();
        item3.velocity = reader.ReadVector2();
    }
}
