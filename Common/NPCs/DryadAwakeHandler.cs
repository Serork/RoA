using System.IO;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.NPCs;

sealed class DryadAwakeHandler : ModSystem {
    public static bool DryadAwake;

    public override void ClearWorld() {
        DryadAwake = false;
    }

    public override void SaveWorldData(TagCompound tag) {
        if (DryadAwake) {
            tag[RoA.ModName + "DryadAwake"] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        DryadAwake = tag.ContainsKey(RoA.ModName + "DryadAwake");
    }

    public override void NetSend(BinaryWriter writer) {
        var flags = new BitsByte();
        flags[0] = DryadAwake;
        writer.Write(flags);
    }

    public override void NetReceive(BinaryReader reader) {
        BitsByte flags = reader.ReadByte();
        DryadAwake = flags[0];
    }
}
