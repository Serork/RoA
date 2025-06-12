using Microsoft.Xna.Framework;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Core.Utility;

// terraria overhaul
static class IOExtensions {
    public static void TryWriteSenderPlayer(this BinaryWriter writer, Player player) {
        if (Main.netMode == NetmodeID.Server) {
            writer.Write((byte)player.whoAmI);
        }
    }

    public static bool TryReadSenderPlayer(this BinaryReader reader, int sender, out Player player) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            sender = reader.ReadByte();
        }

        player = Main.player[sender];

        return player != null && player.active;
    }

    public static void WriteRGBA(this BinaryWriter writer, Color c) {
        writer.Write(c.R);
        writer.Write(c.G);
        writer.Write(c.B);
        writer.Write(c.A);
    }

    public static Color ReadRGBA(this BinaryReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
}
