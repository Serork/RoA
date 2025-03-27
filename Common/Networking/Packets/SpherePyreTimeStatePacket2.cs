using RoA.Content.Items.Special;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class SpherePyreTimeStatePacket2 : NetPacket {
    public SpherePyreTimeStatePacket2(int whoAmI, int pyreTime, int pyreTime2) {
        Writer.Write(whoAmI);
        Writer.Write(pyreTime);
        Writer.Write(pyreTime2);
        Item item3 = Main.item[whoAmI];
        Writer.WriteVector2(item3.position);
        Writer.WriteVector2(item3.velocity);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        int pyreTime = reader.ReadInt32();
        int pyreTime2 = reader.ReadInt32();
        Item item3 = Main.item[whoAmI];
        item3.GetGlobalItem<SphereHandler>().UpdatePyreTime(pyreTime, pyreTime2);
        item3.position = reader.ReadVector2();
        item3.velocity = reader.ReadVector2();
    }
}
