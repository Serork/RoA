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
        if (soundStyle == -1) {
            SoundEngine.PlaySound(SoundID.PlayerHit, position);
        }
        else if (soundStyle == 1) {
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
        else if (soundStyle == 5) {
            SoundEngine.PlaySound(SoundID.Item77, position);
        }
        else if (soundStyle == 6) {
            SoundEngine.PlaySound(SoundID.Item5, position);
        }
        else if (soundStyle == 7) {
            SoundEngine.PlaySound(SoundID.Item36, position);
        }
        else if (soundStyle == 8) {
            SoundEngine.PlaySound(SoundID.Unlock, position);
        }
        else if (soundStyle == 9) {
            SoundEngine.PlaySound(SoundID.Grab, position);
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "HealQuick") { Volume = 0.8f, PitchVariance = 0.2f }, position);
        }
        else if (soundStyle == 10) {
            SoundEngine.PlaySound(SoundID.Item74, position);
        }
        else if (soundStyle == 11) {
            SoundEngine.PlaySound(SoundID.NPCHit32, position);
        }
        else if (soundStyle == 12) {
            SoundEngine.PlaySound(SoundID.Item17, position);
        }
        else if (soundStyle == 13) {
            SoundEngine.PlaySound(SoundID.MaxMana, position);
        }
        else if (soundStyle == 14) {
            SoundEngine.PlaySound(SoundID.Item169 with { Pitch = -0.8f, PitchVariance = 0.1f, Volume = 0.6f }, position);
        }
        else if (soundStyle == 15) {
            SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, position);
        }
        else if (soundStyle == 16) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Twinkle") { Pitch = 0.3f, Volume = 0.2f }, position);
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = -0.4f, Volume = 0.4f }, position);
        }
        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, soundStyle, position), ignoreClient: sender);
        }
    }
}
