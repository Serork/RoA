using RoA.Content.Items.Special;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class SphereFlyTimeStatePacket2 : NetPacket {
    public SphereFlyTimeStatePacket2(int whoAmI, int flyTime) {
        Writer.Write(whoAmI);
        Writer.Write(flyTime);
        Item item3 = Main.item[whoAmI];
        Writer.WriteVector2(item3.position);
        Writer.WriteVector2(item3.velocity);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        int flyTime = reader.ReadInt32();
        Item item3 = Main.item[whoAmI];
        item3.GetGlobalItem<SphereHandler>().UpdateFlyTime(flyTime);
        item3.position = reader.ReadVector2();
        item3.velocity = reader.ReadVector2();
    }
}
