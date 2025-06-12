using RoA.Content.Tiles.Crafting;

using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Networking.Packets;

sealed class PlaceBeaconTEPacket : NetPacket {
    public PlaceBeaconTEPacket(int i, int j) {
        Writer.Write(i);
        Writer.Write(j);
    }

    public override void Read(BinaryReader reader, int sender) {
        int i = reader.ReadInt32();
        int j = reader.ReadInt32();
        if (WorldGen.InWorld(i, j) && !TileEntity.ByPosition.ContainsKey(new Point16(i, j))) {
            ModContent.GetInstance<BeaconTE>().Place(i, j);
        }
    }
}
