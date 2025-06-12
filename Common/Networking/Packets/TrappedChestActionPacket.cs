using RoA.Content.Tiles.Miscellaneous;

using System.IO;

using Terraria;

namespace RoA.Common.Networking.Packets;

sealed class TrappedChestActionPacket : NetPacket {
    public TrappedChestActionPacket(int left, int top) {
        Writer.Write((short)left);
        Writer.Write((short)top);
    }

    public override void Read(BinaryReader reader, int sender) {
        int num1 = (int)reader.ReadInt16();
        int num2 = (int)reader.ReadInt16();
        Wiring.SetCurrentUser(sender);
        TrappedChests.Trigger(num1, num2);
        Wiring.SetCurrentUser();
    }
}

