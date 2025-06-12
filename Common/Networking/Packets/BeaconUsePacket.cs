using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System.IO;

namespace RoA.Common.Networking.Packets;

sealed class BeaconUsePacket : NetPacket {
    public BeaconUsePacket(int i, int j) {
        Writer.Write(i);
        Writer.Write(j);
    }

    public override void Read(BinaryReader reader, int sender) {
        int i = reader.ReadInt32();
        int j = reader.ReadInt32();
        BeaconTE beaconTE = TileHelper.GetTE<BeaconTE>(i, j);
        beaconTE?.UseAnimation();
    }
}
