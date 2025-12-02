using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Content.AdvancedDusts;

using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Networking.Packets;

sealed class AdvancedDustSpawnPacket : NetPacket {
    public enum VisualEffectPacketType : byte {
        ClawsHit,
        BloodShedParticle,
        MercuriumBulletParticle,
        HardmodeClawsHit
    }

    public AdvancedDustSpawnPacket(VisualEffectPacketType packetType, Player player, int layer, Vector2 position, Vector2 velocity, Color color, float scale, float rotation, 
        bool dontEmitLight = false, bool shouldFullBright = false, float brightnessModifier = 0f) {
        Writer.TryWriteSenderPlayer(player);
        Writer.Write((byte)packetType);
        Writer.Write(layer);
        Writer.WriteVector2(position);
        Writer.WriteVector2(velocity);
        Writer.WriteRGBA(color);
        Writer.Write(scale);
        Writer.Write(rotation);
        Writer.Write(dontEmitLight);
        Writer.Write(shouldFullBright);
        if (brightnessModifier > 0f) {
            Writer.Write(brightnessModifier);
        }
    }

    public override void Read(BinaryReader reader, int sender) {
        if (!reader.TryReadSenderPlayer(sender, out var player)) {
            return;
        }

        VisualEffectPacketType packetType = (VisualEffectPacketType)reader.ReadByte();
        int layer = reader.ReadInt32();
        Vector2 position = reader.ReadVector2();
        Vector2 velocity = reader.ReadVector2();
        Color color = reader.ReadRGBA();
        float scale = reader.ReadSingle();
        float rotation = reader.ReadSingle();
        bool dontEmitLight = reader.ReadBoolean();

        bool shouldFullBright = reader.ReadBoolean();
        float brightnessModifier = 0f;
        if (shouldFullBright) {
            brightnessModifier = reader.ReadSingle();
        }

        void createVisualEffect<T>() where T : AdvancedDust<T>, new() {
            var particle = AdvancedDustSystem.New<T>(layer, onServer: true)?.
                        Setup(position,
                              velocity,
                              color,
                              scale,
                              rotation);
            if (particle is not null) {
                particle.DontEmitLight = dontEmitLight;
                if (shouldFullBright) {
                    particle.ShouldFullBright = shouldFullBright;
                    particle.BrightnessModifier = brightnessModifier;
                }
            }
        }
        switch (packetType) {
            case VisualEffectPacketType.ClawsHit:
                createVisualEffect<ClawsSlashHit>();
                break;
            case VisualEffectPacketType.BloodShedParticle:
                createVisualEffect<BloodShedDust>();
                break;
            case VisualEffectPacketType.MercuriumBulletParticle:
                createVisualEffect<MercuriumBulletParticle>();
                break;
            case VisualEffectPacketType.HardmodeClawsHit:
                createVisualEffect<HardmodeClawsSlashHit>();
                break;
        }

        if (Main.netMode == NetmodeID.Server) {
            MultiplayerSystem.SendPacket(new AdvancedDustSpawnPacket(packetType, player, layer, position, velocity, color, scale, rotation, dontEmitLight, shouldFullBright, brightnessModifier), ignoreClient: sender);
        }
    }
}
