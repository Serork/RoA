using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Networking;
using RoA.Common.Tiles;
using RoA.Content.Buffs;
using RoA.Content.Tiles.Plants;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class JawTrap : ModTile, TileHooks.ITileAfterPlayerDraw {
    internal sealed class JawTrapTE : ModTileEntity {
        private const int RELOAD = 480;

        public int ActivatedTimer { get; private set; }

        public bool Activated => ActivatedTimer > RELOAD / 2;

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

                return -1;
            }

            return Place(i, j);
        }

        public override void Update() {
            if (ActivatedTimer > 0) {
                ActivatedTimer--;
                ActivatedTimer = Math.Max(0, ActivatedTimer);
            }
            if (ActivatedTimer > 0) {
                return;
            }
            int sizeX = 30;
            float x = Position.X * 16f + 2f;
            float y = Position.Y * 16f;
            Rectangle hitbox = new((int)x, (int)y, sizeX, 20);
            foreach (Player player in Main.ActivePlayers) {
                if (player.dead) {
                    continue;
                }
                if (player.Hitbox.Intersects(hitbox)) {
                    ActivatedTimer = RELOAD;
                    player.AddBuff(ModContent.BuffType<Root>(), ActivatedTimer / 2);
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + Language.GetOrRegister($"Mods.RoA.DeathReasons.Root{Main.rand.Next(2)}").Value),
                        40, 0, cooldownCounter: 4);
                    player.AddBuff(BuffID.Bleeding, 120);
                    break;
                }
            }
        }

        public override void OnKill() {
            ActivatedTimer = 0;
        }

        public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

        public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<JawTrap>());
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = [20];
        TileObjectData.newTile.CoordinateWidth = 20;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<JawTrapTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(179, 165, 159));

        DustType = -1;
        HitSound = SoundID.Dig;
    }

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<JawTrapTE>().Place(i, j);
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        TileHelper.RemovePostPlayerDrawPoint(i, j);

        if (Main.netMode != NetmodeID.Server) {
            if (!fail) {
                ModContent.GetInstance<JawTrapTE>().Kill(i, j);
                //if (Main.netMode != NetmodeID.SinglePlayer) {
                //    MultiplayerSystem.SendPacket(new RemoveMiracleTileEntityOnServerPacket(i, j));
                //}
            }
        }
    }

    void TileHooks.ITileAfterPlayerDraw.PostPlayerDraw(SpriteBatch spriteBatch, Point pos) {
        int i = pos.X; int j = pos.Y;
        Tile tile = Main.tile[i, j];
        Vector2 zero = Vector2.Zero;
        int width = 20;
        int offsetY = 0;
        int height = 20;
        short frameX = tile.TileFrameX;
        short frameY = tile.TileFrameY;
        TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);
        Main.spriteBatch.Draw(TextureAssets.Tile[TileLoader.GetTile(ModContent.TileType<JawTrap>()).Type].Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                              new Rectangle(frameX, frameY, width, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        TileHelper.AddPostPlayerDrawPoint(this, i, j);

        return false;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        JawTrapTE tileEntity = TileHelper.GetTE<JawTrapTE>(i, j);
        if (tileEntity == null) {
            tileEntity = TileHelper.GetTE<JawTrapTE>(i - 1, j);
        }
        if (tileEntity == null) {
            return;
        }
        if (tileEntity.Activated) {
            return;
        }
        tileFrameY = 20;
    }
}