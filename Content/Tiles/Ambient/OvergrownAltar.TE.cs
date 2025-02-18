using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.WorldEvents;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class OvergrownAltarTE : ModTileEntity {
    public float Counting { get; private set; }
    public float Counting2 { get; private set; }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        return Place(i, j);
    }

    public override void Update() {
        float counting = MathHelper.Clamp(1f - Counting, 0f, 1f);
        float factor = AltarHandler.GetAltarFactor();
        Counting2 = MathHelper.Lerp(Counting2, counting, factor > 0.5f ? Math.Max(0.1f, counting * 0.1f) : counting < 0.5f ? 0.075f : Math.Max(0.05f, counting * 0.025f));
        Counting += TimeSystem.LogicDeltaTime / (3f - MathHelper.Min(0.9f, factor) * 2.5f) * Math.Max(0.05f, Counting) * 7f;
        if (Counting > 0.8f) {
            if (factor > 0f && Main.rand.NextChance(1f - (double)Math.Min(0.25f, factor - 0.5f))/* || LothorInvasion.preArrivedLothorBoss.Item2*/) {
                float volume = 2.5f * Math.Max(0.3f, factor + 0.1f);
                var style = new SoundStyle(ResourceManager.AmbientSounds + "Heartbeat") { Volume = volume };
                var sound = SoundEngine.FindActiveSound(in style);
                if (Main.netMode == NetmodeID.Server) {
                    MultiplayerSystem.SendPacket(new PlayHeartbeatSoundPacket(Main.LocalPlayer, Position.X, Position.Y, volume), ignoreClient: -1);
                }
                else {
                    SoundEngine.PlaySound(style, new Microsoft.Xna.Framework.Vector2(Position.X, Position.Y) * 16f);
                }
            }
        }
        if (Counting >= 1.25f) {
            Counting = 0f;
        }

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(Counting);
        writer.Write(Counting2);
    }

    public override void NetReceive(BinaryReader reader) {
        Counting = reader.ReadSingle();
        Counting2 = reader.ReadSingle();
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<OvergrownAltar>());
}