using RoA.Common.Items;

using System.IO;

namespace RoA.Common.Networking.Packets;

sealed class ExtraMannequinInfoPlacementPacket : NetPacket {
    public ExtraMannequinInfoPlacementPacket(int x, int y) {
        Writer.Write(x);
        Writer.Write(y);
    }

    public override void Read(BinaryReader reader, int sender) {
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        MannequinWreathSlotSupport.MannequinsInWorldSystem.AddExtraMannequinInfo(x, y);
    }
}
