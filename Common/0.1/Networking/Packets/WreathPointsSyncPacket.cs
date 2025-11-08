using RoA.Common.Druid.Wreath;
using RoA.Core.Utility.Vanilla;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class WreathPointsSyncPacket : NetPacket {
    public WreathPointsSyncPacket(byte who, ushort resource/*, ushort tempResource, float changingTimeValue, float currentChangingTime, bool shouldDecrease1, bool shouldDecrease2, float currentChangingMult, ushort increaseValue, float stayTime, bool startSlowlyIncreasingUntilFull, bool startSlowlyIncreasingUntilFull2*/) {
        Writer.Write(who);
        Writer.Write(resource);
        //Writer.Write(tempResource);
        //Writer.Write(changingTimeValue);
        //Writer.Write(currentChangingTime);
        //Writer.Write(shouldDecrease1);
        //Writer.Write(shouldDecrease2);
        //Writer.Write(currentChangingMult);
        //Writer.Write(increaseValue);
        //Writer.Write(stayTime);
        //Writer.Write(startSlowlyIncreasingUntilFull);
        //Writer.Write(startSlowlyIncreasingUntilFull2);
    }

    public override void Read(BinaryReader reader, int sender) {
        byte who = reader.ReadByte();
        ushort resource = reader.ReadUInt16();
        //ushort tempResource = reader.ReadUInt16();
        //float changingTimeValue = reader.ReadSingle();
        //float currentChangingTime = reader.ReadSingle();
        //bool shouldDecrease1 = reader.ReadBoolean();
        //bool shouldDecrease2 = reader.ReadBoolean();
        //float currentChangingMult = reader.ReadSingle();
        //ushort increaseValue = reader.ReadUInt16();
        //float stayTime = reader.ReadSingle();
        //bool startSlowlyIncreasingUntilFull = reader.ReadBoolean();
        //bool startSlowlyIncreasingUntilFull2 = reader.ReadBoolean();
        WreathHandler handler = Main.player[who].GetWreathHandler();
        handler.ReceivePlayerSync(resource/*, tempResource, changingTimeValue, currentChangingTime, shouldDecrease1, shouldDecrease2, currentChangingMult, increaseValue, stayTime, startSlowlyIncreasingUntilFull, startSlowlyIncreasingUntilFull2*/);
        if (Main.netMode == NetmodeID.Server) {
            handler.SyncPlayer(-1, sender, false);
        }
    }
}
