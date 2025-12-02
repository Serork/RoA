using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class PlayHellfireSoundPacket : NetPacket {
    public PlayHellfireSoundPacket(Player player, Vector2 position) {
        Writer.TryWriteSenderPlayer(player);
        Writer.WriteVector2(position);
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }
        Vector2 position = reader.ReadVector2();
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "HellfireClaws") with { PitchVariance = 0.25f, Volume = Main.rand.NextFloat(0.75f, 0.85f) }, position);
        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new PlayHellfireSoundPacket(player, position), ignoreClient: sender);
        }
    }
}
