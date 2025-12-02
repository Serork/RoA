using RoA.Common.WorldEvents;

using System.IO;

namespace RoA.Common.Networking.Packets;

sealed class FogOpacityPacket : NetPacket {
    public FogOpacityPacket(float fogOpacity) {
        Writer.Write(fogOpacity);
    }

    public override void Read(BinaryReader reader, int sender) {
        float fogOpacity = reader.ReadSingle();
        BackwoodsFogHandler.Opacity = fogOpacity;
    }
}
