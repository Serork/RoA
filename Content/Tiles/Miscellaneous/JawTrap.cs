using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.Networking;
using RoA.Content.Tiles.Plants;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class JawTrap : ModTile {
    private sealed class JawTrapTE : ModTileEntity {
        public bool IsOnCooldown { get; private set; }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

                return -1;
            }

            return Place(i, j);
        }

        public override void Update() {
            float x = Position.X * 16f - 4f;
            float y = Position.Y * 16f;
            Rectangle hitbox = new((int)x, (int)y, 36, 20);
            if (Main.LocalPlayer.Hitbox.Intersects(hitbox)) {
                IsOnCooldown = true;
                return;
            }

            IsOnCooldown = false;
        }

        public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

        public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<JawTrap>());
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = [20];
        TileObjectData.newTile.CoordinateWidth = 20;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<JawTrapTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.MusicBox"));

        DustType = -1;
        HitSound = SoundID.Dig;
    }

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<JawTrapTE>().Place(i, j);
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        if (Main.netMode != NetmodeID.Server) {
            if (!fail) {
                ModContent.GetInstance<JawTrapTE>().Kill(i, j);
                //if (Main.netMode != NetmodeID.SinglePlayer) {
                //    MultiplayerSystem.SendPacket(new RemoveMiracleTileEntityOnServerPacket(i, j));
                //}
            }
        }
    }

    public override bool RightClick(int i, int j) {
        return base.RightClick(i, j);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        JawTrapTE tileEntity = TileHelper.GetTE<JawTrapTE>(i, j);
        if (tileEntity == null) {
            tileEntity = TileHelper.GetTE<JawTrapTE>(i - 1, j);
        }
        if (tileEntity == null) {
            return;
        }
        if (tileEntity.IsOnCooldown) {
            return;
        }
        tileFrameY = 20;
    }
}