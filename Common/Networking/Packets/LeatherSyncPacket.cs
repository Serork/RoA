using RoA.Content.Buffs;
using RoA.Content.Items.Special;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class LeatherSyncPacket : NetPacket {
    public LeatherSyncPacket(int whoAmI, int timeToSpoil) {
        Writer.Write(whoAmI);
        Writer.Write(timeToSpoil);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        int timeToSpoil = reader.ReadInt32();
        Item item = Main.item[whoAmI];
        item.GetGlobalItem<SpoilLeatherHandler>().TimeToSpoil = timeToSpoil;
    }
}
