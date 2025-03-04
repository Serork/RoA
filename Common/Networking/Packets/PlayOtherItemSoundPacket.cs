using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.IO;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class PlayOtherItemSoundPacket : NetPacket {
    public PlayOtherItemSoundPacket(Player player, int soundStyle, Vector2 position) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(soundStyle);
        Writer.WriteVector2(position);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }
        int soundStyle = reader.ReadInt32();
        Vector2 position = reader.ReadVector2();
        if (soundStyle == 1) {
            SoundEngine.PlaySound(SoundID.Item95, position);
        }
        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, soundStyle, position), ignoreClient: sender);
        }
    }
}
