using RoA.Content.Items.Special;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class SphereTerraTimeStatePacket : NetPacket {
    public SphereTerraTimeStatePacket(int whoAmI, bool value) {
        Writer.Write(whoAmI);
        Writer.Write(value);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        Item item = Main.item[whoAmI];
        bool value = reader.ReadBoolean();
        item.GetGlobalItem<SphereHandler>().ChangeTerraTimeState(item, value, true);
    }
}
