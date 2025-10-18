using RoA.Content.Items.Equipables.Armor.Summon;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class BoneHarpyOptionsPacket : NetPacket {
    public BoneHarpyOptionsPacket(byte who, bool isInIdle, int harpyThatRideWhoAmI, ushort stateTimer) {
        Writer.Write(who);
        Writer.Write(isInIdle);
        Writer.Write(harpyThatRideWhoAmI);
        Writer.Write(stateTimer);
    }

    public override void Read(BinaryReader reader, int sender) {
        byte who = reader.ReadByte();
        bool isInIdle = reader.ReadBoolean();
        int harpyThatRideWhoAmI = reader.ReadInt32();
        ushort stateTime = reader.ReadUInt16();
        WorshipperBonehelm.BoneHarpyOptions handler = Main.player[who].GetModPlayer<WorshipperBonehelm.BoneHarpyOptions>();
        handler.ReceivePlayerSync(isInIdle, harpyThatRideWhoAmI, stateTime);
        if (Main.netMode == NetmodeID.Server) {
            handler.SyncPlayer(-1, sender, false);
        }
    }
}
