using System.IO;

using Terraria.DataStructures;
using Terraria;
using RoA.Content.Tiles.Crafting;
using Terraria.ModLoader;
using RoA.Content.Tiles.Plants;

namespace RoA.Common.Networking.Packets;

sealed class PlaceMiracleMintPacket : NetPacket {
    public PlaceMiracleMintPacket(int i, int j) {
        Writer.Write(i);
        Writer.Write(j);
    }

    public override void Read(BinaryReader reader, int sender) {
        int i = reader.ReadInt32();
        int j = reader.ReadInt32();
        if (WorldGen.InWorld(i, j) && !TileEntity.ByPosition.ContainsKey(new Point16(i, j))) {
            ModContent.GetInstance<MiracleMintTE>().Place(i, j);
        }
    }
}
