using RoA.Content.Items.Special;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class SphereTerraTimeStatePacket2 : NetPacket {
    public SphereTerraTimeStatePacket2(int whoAmI, int terraTime) {
        Writer.Write(whoAmI);
        Writer.Write(terraTime);
        Item item3 = Main.item[whoAmI];
        Writer.WriteVector2(item3.position);
        Writer.WriteVector2(item3.velocity);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        int terraTime = reader.ReadInt32();
        Item item3 = Main.item[whoAmI];
        item3.GetGlobalItem<SphereHandler>().UpdateTerraTime(terraTime);
        item3.position = reader.ReadVector2();
        item3.velocity = reader.ReadVector2();
    }
}
