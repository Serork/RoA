﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using RoA.Core.Utility;
using RoA.Common;

using System;

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
            Counting += (float)Math.Round(TimeSystem.LogicDeltaTime / 3f, 2) * Math.Min(0.1f, Counting) * 10f;
            if (Counting >= 1f) {
                Counting = 0f;
            }
        }
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<OvergrownAltar>());
}