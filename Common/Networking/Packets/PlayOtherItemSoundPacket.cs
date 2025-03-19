using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using System.IO;

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
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "ClawsWave") { Volume = 0.75f }, position);
        }
        else if (soundStyle == 2) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves2") { Pitch = 0.3f, Volume = 1.2f }, position);
        }
        else if (soundStyle == 3) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "ClawsRoot") { Volume = 2.5f }, position);
        }
        else if (soundStyle == 4) {
            SoundEngine.PlaySound(SoundID.NPCDeath22, position);
        }
        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, soundStyle, position), ignoreClient: sender);
        }
    }
}
