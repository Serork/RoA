using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using RoA.Core.Utility;
using RoA.Common;

using System;
using Terraria.Audio;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;

namespace RoA.Content.Tiles.Ambient;

sealed class OvergrownAltarTE : ModTileEntity {
    public float Counting { get; private set; }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        return Place(i, j);
    }

    public override void Update() {
        if (Main.netMode != NetmodeID.Server) {
            float factor = AltarHandler.GetAltarFactor();
            Counting += TimeSystem.LogicDeltaTime / (3f - factor) * Math.Max(0.05f, Counting) * 7f;
            var style = new SoundStyle(ResourceManager.AmbientSounds + "Heartbeat") { Volume = 1.5f };
            var sound = SoundEngine.FindActiveSound(in style);
            if (Counting >= 1f && (sound == null || !sound.IsPlaying)) {
                if ((factor > 0f && Main.rand.NextChance(1f - (double)Math.Min(0f, factor - 0.5f)))/* || LothorInvasion.preArrivedLothorBoss.Item2*/) {
                    SoundEngine.PlaySound(style, new Microsoft.Xna.Framework.Vector2(Position.X, Position.Y) * 16f);
                }
            }
            if (Counting >= 1.25f) {
                Counting = 0f;
            }
        }
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<OvergrownAltar>());
}