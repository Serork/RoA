using System.IO;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.NPCs;

sealed class DryadAwakeHandler : ModSystem {
    public static bool DryadAwake, DryadAwake2;

    public override void ClearWorld() {
        DryadAwake = false;
        DryadAwake2 = false;
    }

    public override void SaveWorldData(TagCompound tag) {
        if (DryadAwake) {
            tag[RoA.ModName + nameof(DryadAwake)] = true;
        }
        if (DryadAwake2) {
            tag[RoA.ModName + nameof(DryadAwake2)] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        DryadAwake = tag.ContainsKey(RoA.ModName + nameof(DryadAwake));
        DryadAwake2 = tag.ContainsKey(RoA.ModName + nameof(DryadAwake2));
    }

    public override void NetSend(BinaryWriter writer) {
        var flags = new BitsByte();
        flags[0] = DryadAwake;
        flags[1] = DryadAwake2;
        writer.Write(flags);
    }

    public override void NetReceive(BinaryReader reader) {
        BitsByte flags = reader.ReadByte();
        DryadAwake = flags[0];
        DryadAwake2 = flags[1];
    }
}
