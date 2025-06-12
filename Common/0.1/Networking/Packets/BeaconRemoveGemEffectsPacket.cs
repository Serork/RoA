using RoA.Content.Tiles.Crafting;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class BeaconRemoveGemEffectsPacket : NetPacket {
    public BeaconRemoveGemEffectsPacket(int i, int j) {
        Writer.Write(i);
        Writer.Write(j);
    }

    public override void Read(BinaryReader reader, int sender) {
        int i = reader.ReadInt32();
        int j = reader.ReadInt32();
        Beacon.RemoveGemEffects(i, j);
        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new BeaconRemoveGemEffectsPacket(i, j), ignoreClient: sender);
        }
    }
}
