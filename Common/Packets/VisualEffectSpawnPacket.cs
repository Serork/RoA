﻿using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Content.VisualEffects;

using RoA.Core.Networking;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.ID;

namespace RoA.Common.Packets;

sealed class VisualEffectSpawnPacket : NetPacket {
    public enum VisualEffectPacketType {
        ClawsHit
    }

    private readonly VisualEffectPacketType _packetType;

    public VisualEffectSpawnPacket(VisualEffectPacketType packetType, Player player, int layer, Vector2 position, Vector2 velocity, Color color, float scale, float rotation) {
        _packetType = packetType;

        Writer.TryWriteSenderPlayer(player);
		Writer.Write(layer);
        Writer.WriteVector2(position);
        Writer.WriteVector2(velocity);
        Writer.WriteRGBA(color);
        Writer.Write(scale);
        Writer.Write(rotation);
    }

	public override void Read(BinaryReader reader, int sender) {
		if (!reader.TryReadSenderPlayer(sender, out var player)) {
			return;
		}

        int layer = reader.ReadInt32();
        Vector2 position = reader.ReadVector2();
        Vector2 velocity = reader.ReadVector2();
        Color color = reader.ReadRGBA();
        float scale = reader.ReadSingle();
        float rotation = reader.ReadSingle();

        void createVisualEffect<T>() where T : VisualEffect<T>, new() {
            VisualEffectSystem.New<T>(layer).
                        Setup(position,
                              velocity,
                              color,
                              scale,
                              rotation);
        }
        switch (_packetType) {
            case VisualEffectPacketType.ClawsHit:
                createVisualEffect<ClawsSlashHit>();
                break;
        }

        if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(_packetType, player, layer, position, velocity, color, scale, rotation), ignoreClient: sender);
		}
	}
}
