﻿using System.IO;

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
            tag["downedLothorBoss"] = true;
        }

        // if (downedOtherBoss) {
        //	tag["downedOtherBoss"] = true;
        // }
    }

    public override void LoadWorldData(TagCompound tag) {
        DownedLothorBoss = tag.ContainsKey("downedLothorBoss");
        // downedOtherBoss = tag.ContainsKey("downedOtherBoss");
    }

    public override void NetSend(BinaryWriter writer) {
        // Order of operations is important and has to match that of NetReceive
        var flags = new BitsByte();
        flags[0] = DownedLothorBoss;
        // flags[1] = downedOtherBoss;
        writer.Write(flags);

        /*
			Remember that Bytes/BitsByte only have up to 8 entries. If you have more than 8 flags you want to sync, use multiple BitsByte:
				This is wrong:
			flags[8] = downed9thBoss; // an index of 8 is nonsense.
				This is correct:
			flags[7] = downed8thBoss;
			writer.Write(flags);
			BitsByte flags2 = new BitsByte(); // create another BitsByte
			flags2[0] = downed9thBoss; // start again from 0
			// up to 7 more flags here
			writer.Write(flags2); // write this byte
			*/

        // If you prefer, you can use the BitsByte constructor approach as well.
        // BitsByte flags = new BitsByte(downedMinionBoss, downedOtherBoss);
        // writer.Write(flags);

        // This is another way to do the same thing, but with bitmasks and the bitwise OR assignment operator (the |=)
        // Note that 1 and 2 here are bit masks. The next values in the pattern are 4,8,16,32,64,128. If you require more than 8 flags, make another byte.
        // byte flags = 0;
        // if (downedMinionBoss)
        // {
        //	flags |= 1;
        // }
        // if (downedOtherBoss)
        // {
        //	flags |= 2;
        // }
        // writer.Write(flags);

        // If you plan on having more than 8 of these flags and don't want to use multiple BitsByte, an alternative is using a System.Collections.BitArray
        /*
			bool[] flags = new bool[] {
				downedMinionBoss,
				downedOtherBoss,
			};
			BitArray bitArray = new BitArray(flags);
			byte[] bytes = new byte[(bitArray.Length - 1) / 8 + 1]; // Calculation for correct length of the byte array
			bitArray.CopyTo(bytes, 0);

			writer.Write(bytes.Length);
			writer.Write(bytes);
			*/
    }

    public override void NetReceive(BinaryReader reader) {
        // Order of operations is important and has to match that of NetSend
        BitsByte flags = reader.ReadByte();
        DownedLothorBoss = flags[0];
        // downedOtherBoss = flags[1];

        // As mentioned in NetSend, BitBytes can contain up to 8 values. If you have more, be sure to read the additional data:
        // BitsByte flags2 = reader.ReadByte();
        // downed9thBoss = flags2[0];

        // System.Collections.BitArray approach:
        /*
			int length = reader.ReadInt32();
			byte[] bytes = reader.ReadBytes(length);

			BitArray bitArray = new BitArray(bytes);
			downedMinionBoss = bitArray[0];
			downedOtherBoss = bitArray[1];
			*/
    }
}
