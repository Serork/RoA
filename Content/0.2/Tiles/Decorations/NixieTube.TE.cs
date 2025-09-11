﻿using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Tiles.Decorations;

sealed class NixieTubeTE : ModTileEntity {
    public static ushort ACTIVATIONTIME => 100;

    public bool Activated;
    public int DeactivatedTimer, DeactivatedTimer2, ActivatedForTimer;
    public bool Active => DeactivatedTimer <= ACTIVATIONTIME / 3;
    public Item? Dye1 = null;
    public Item? Dye2 = null;

    public NixieTubeTE() {
        Dye1 ??= new Item();
        Dye2 ??= new Item();
    }

    public void SetDye1(Item item) => Dye1 = item;
    public void SetDye2(Item item) => Dye2 = item;

    public override void Update() {
        if (!Activated) {
            return;
        }

        if (Main.rand.NextBool(750)) {
            Activate();
        }

        if ((DeactivatedTimer -= 5) < 0) {
            DeactivatedTimer = DeactivatedTimer2 / 2;
            DeactivatedTimer2 /= 2;

            if (!Active && Main.rand.NextBool()) {
                Dust dust = Dust.NewDustPerfect(new Point16(Position.X - 1, Position.Y - 2).ToWorldCoordinates() + Main.rand.Random2(-8f, 16f + 8f, 8f, 32f), ModContent.DustType<Dusts.NixieTube>(), newColor: Color.Yellow);
                dust.velocity *= 0.5f;
                dust.alpha = 100;
            }
        }
    }

    public void Activate() {
        Activated = true;

        DeactivatedTimer = DeactivatedTimer2 = ACTIVATIONTIME;
    }

    public override void OnKill() {
        int i = Position.X, j = Position.Y;
        if (!Dye1.IsEmpty()) {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, Dye1.type);
        }
        if (!Dye2.IsEmpty()) {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, Dye2.type);
        }
    }

    public override void SaveData(TagCompound tag) {
        if (Dye1 is not null) {
            tag.Add(RoA.ModName + nameof(Dye1), ItemIO.Save(Dye1));
        }
        if (Dye2 is not null) {
            tag.Add(RoA.ModName + nameof(Dye2), ItemIO.Save(Dye2));
        }
    }

    public override void LoadData(TagCompound tag) {
        if (tag.TryGet(RoA.ModName + nameof(Dye1), out TagCompound dye1)) {
            Dye1 = ItemIO.Load(dye1);
        }
        if (tag.TryGet(RoA.ModName + nameof(Dye2), out TagCompound dye2)) {
            Dye2 = ItemIO.Load(dye2);
        }
    }

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        ushort tapperTileType = (ushort)ModContent.TileType<NixieTube>();
        return tile.HasTile && tile.TileType == tapperTileType;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        int id = Place(i, j);
        return id;
    }
}
