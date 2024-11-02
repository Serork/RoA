using RoA.Core;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class PlayHeartbeatSoundPacket : NetPacket {
    public PlayHeartbeatSoundPacket(Player player, int i, int j, float volume) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write(i);
        Writer.Write(j);
        Writer.Write(volume);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        int i = reader.ReadInt32();
        int j = reader.ReadInt32();
        float volume = reader.ReadSingle();
        var style = new SoundStyle(ResourceManager.AmbientSounds + "Heartbeat") { Volume = volume };
        SoundEngine.PlaySound(style, new Microsoft.Xna.Framework.Vector2(i, j) * 16f);

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PlayHeartbeatSoundPacket(player, i, j, volume), ignoreClient: sender);
        }
    }
}
