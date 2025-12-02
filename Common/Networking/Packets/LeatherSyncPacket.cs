using RoA.Content.Buffs;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class LeatherSyncPacket : NetPacket {
    public LeatherSyncPacket(int whoAmI, ulong timeToSpoil) {
        Writer.Write(whoAmI);
        Writer.Write(timeToSpoil);
    }

    public override void Read(BinaryReader reader, int sender) {
        int whoAmI = reader.ReadInt32();
        ulong timeToSpoil = reader.ReadUInt64();
        Item item = Main.item[whoAmI];
        item.GetGlobalItem<SpoilLeatherHandler>().StartSpoilingTime = timeToSpoil;
    }
}
