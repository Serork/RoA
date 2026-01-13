using System.IO;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.NPCs;

sealed class DownedBossSystem : ModSystem {
    public static bool DownedLothorBoss = false;
    // public static bool downedOtherBoss = false;

    public override void ClearWorld() {
        DownedLothorBoss = false;
        // downedOtherBoss = false;
    }

    // We save our data sets using TagCompounds.
    // NOTE: The tag instance provided here is always empty by default.
    public override void SaveWorldData(TagCompound tag) {
        if (DownedLothorBoss) {
            tag[RoA.ModName + "DownedLothorBoss"] = true;
        }

        // if (downedOtherBoss) {
        //	tag["downedOtherBoss"] = true;
        // }
    }

    public override void LoadWorldData(TagCompound tag) {
        DownedLothorBoss = tag.ContainsKey(RoA.ModName + "DownedLothorBoss");
        // downedOtherBoss = tag.ContainsKey("downedOtherBoss");
    }

    public override void NetSend(BinaryWriter writer) {
        // Order of parameters is important and has to match that of NetReceive
        writer.WriteFlags(DownedLothorBoss/*, downedOtherBoss*/);
        // WriteFlags supports up to 8 entries, if you have more than 8 flags to sync, call WriteFlags again.
        // If you need to send a large number of flags, such as a flag per item type or something similar, BitArray can be used to efficiently send them. See Utils.SendBitArray documentation.
    }

    public override void NetReceive(BinaryReader reader) {
        // Order of parameters is important and has to match that of NetSend
        reader.ReadFlags(out DownedLothorBoss/*, out downedOtherBoss*/);
        // ReadFlags supports up to 8 entries, if you have more than 8 flags to sync, call ReadFlags again.
    }
}
