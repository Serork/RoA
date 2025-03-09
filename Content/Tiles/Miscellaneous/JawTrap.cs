using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Networking.Packets;
using RoA.Common.Networking;
using RoA.Common.Tiles;
using RoA.Content.Buffs;
using RoA.Content.Tiles.Crafting;
using RoA.Core.Utility;

using System;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.IO;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class JawTrap : ModTile, TileHooks.ITileAfterPlayerDraw {
    internal sealed class JawTrapTE : ModTileEntity {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetRespawnTime")]
        public extern static int Player_GetRespawnTime(Player self, bool pvp);


        private const int RELOAD = 480;

        private bool _sync;

        public int ActivatedTimer { get; private set; }

        public bool Activated => ActivatedTimer > RELOAD / 2;

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                //NetMessage.SendTileSquare(Main.myPlayer, i, j);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

                return -1;
            }

            return Place(i, j);
        }

        internal void Activate(Player player) {
            ActivatedTimer = RELOAD;
            _sync = true;
            if (player.whoAmI == Main.myPlayer) {
                player.AddBuff(ModContent.BuffType<Root>(), ActivatedTimer / 2);
                int num = 40;
                num = Main.DamageVar(num, 0f - player.luck);
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + Language.GetOrRegister($"Mods.RoA.DeathReasons.Root{Main.rand.Next(2)}").Value),
                    num, 0, cooldownCounter: 4);
                player.AddBuff(BuffID.Bleeding, 600);
            }
            //NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }

        public override void NetSend(BinaryWriter writer) {
            writer.Write(ActivatedTimer);
        }

        public override void NetReceive(BinaryReader reader) {
            ActivatedTimer = reader.ReadInt32();
        }

        public override void Update() {
            if (Find(Position.X, Position.Y) == -1) {
                return;
            }

            if (ActivatedTimer > 0) {
                ActivatedTimer--;
            }
            if (Main.netMode == NetmodeID.Server) {
                if (ActivatedTimer == RELOAD / 2) {
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                }
                if (_sync && ActivatedTimer == 0) {
                    _sync = false;
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                }
            }
            if (ActivatedTimer > 0) {
                return;
            }
            int sizeX = 10;
            float x = Position.X * 16f + 11f;
            float y = Position.Y * 16f + 2f;
            Rectangle hitbox = new((int)x, (int)y, sizeX, 20);
            foreach (Player player in Main.ActivePlayers) {
                if (player.dead && player.respawnTimer < Player_GetRespawnTime(player, false) - 100) {
                    continue;
                }
                if (player.immune) {
                    continue;
                }
                if (player.velocity.Length() < 1f) {
                    continue;
                }
                Rectangle playerHitbox = new((int)player.position.X, (int)player.Bottom.Y - 10, player.width, 10);
                if (playerHitbox.Intersects(hitbox)) {
                    Activate(player);
                    if (Main.netMode != NetmodeID.SinglePlayer) {
                        MultiplayerSystem.SendPacket(new JawTrapActivatedPacket(player, Position.X, Position.Y));
                    }
                    break;
                }
            }
        }

        public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

        public override bool IsTileValidForEntity(int i, int j) => true/*WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<JawTrap>())*/;
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

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

        AddMapEntry(new Color(179, 165, 159), CreateMapEntryName());

        DustType = -1;
        HitSound = SoundID.Dig;
    }

    public override void PlaceInWorld(int i, int j, Item item) {
        ModContent.GetInstance<JawTrapTE>().Place(i, j);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new PlaceJawTrapTEPacket(i, j));
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        if (Main.netMode != NetmodeID.Server) {
            if (!fail) {
                ModContent.GetInstance<JawTrapTE>().Kill(i, j);
                if (Main.netMode != NetmodeID.SinglePlayer) {
                    MultiplayerSystem.SendPacket(new RemoveJawTrapTileEntityOnServerPacket(i, j));
                }
            }
        }
    }

    void TileHooks.ITileAfterPlayerDraw.PostPlayerDraw(SpriteBatch spriteBatch, Point pos) {
        int i = pos.X; int j = pos.Y;
        Tile tile = Main.tile[i, j];
        int type = ModContent.TileType<JawTrap>();
        if (tile.TileType != type) {
            return;
        }
        Vector2 zero = Vector2.Zero;
        int width = 20;
        int offsetY = 0;
        int height = 20;
        short frameX = tile.TileFrameX;
        short frameY = tile.TileFrameY;
        TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[type].Value;
        spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                              new Rectangle(frameX, frameY, width, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public override bool IsTileDangerous(int i, int j, Player player) => true;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

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