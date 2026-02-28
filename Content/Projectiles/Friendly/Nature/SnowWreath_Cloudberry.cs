using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SnowWreath_Cloudberry : NatureProjectile_NoTextureLoad {
    private static ushort MAXTIMELEFT => 260;
    private static byte FRAMECOUNT => 4;
    private static float STARTFRAMINGSPEED => 0.05f;
    private static byte MAXCOPIES => 10;
    private static byte MAXSNOWBLOCKS => 10;
    private static byte EXPLOSIONFRAME => 2;

    private static Asset<Texture2D>? _cloudberryTexture, _snowBlockTexture;

    private struct CopyInfo {
        private float _opacity;
        private byte _usedFrame;

        public Vector2 Position;
        public float Rotation;
        public float Scale;

        public float Opacity {
            readonly get => _opacity;
            set => _opacity = MathUtils.Clamp01(value);
        }

        public byte UsedFrame {
            readonly get => _usedFrame;
            set => _usedFrame = Utils.Clamp<byte>(value, 0, FRAMECOUNT);
        }
    }

    private struct SnowBlockInfo {
        private float _opacity;

        public Point Position;
        public Rectangle Clip;
        public SlopeType Slope;
        public bool HalfBlock;

        public float Opacity {
            readonly get => _opacity;
            set => _opacity = MathUtils.Clamp01(value);
        }
    }

    public ref struct CloudberryValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float ApplyGravityTimerValue = ref projectile.localAI[1];
        public ref float ShouldMakeCopyValue = ref projectile.localAI[2];
        public ref float CurrentFrameValue = ref projectile.ai[0];
        public ref float FramingSpeedValue = ref projectile.ai[1];
        public ref float CanMakeCopyValue = ref projectile.ai[2];

        public readonly byte UsedFrame => (byte)CurrentFrameValue;

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public bool ShouldMakeCopy {
            readonly get => ShouldMakeCopyValue == 1f;
            set => ShouldMakeCopyValue = value.ToInt();
        }

        public bool CanMakeCopy {
            readonly get => CanMakeCopyValue == 1f;
            set => CanMakeCopyValue = value.ToInt();
        }
    }

    private CopyInfo[]? _copyData;
    private byte _currentCopyIndex;
    private SnowBlockInfo[]? _snowBlockData;
    private byte _currentTileIndex;
    private Point _lastTilePosition;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;

        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
        ProjectileID.Sets.Explosive[Type] = true;
        ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true;

        LoadCloudberryTexture();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(16);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        void explodeOnDeath() {
            if (Projectile.IsOwnerLocal() && Projectile.timeLeft <= 3) {
                Projectile.PrepareBombToBlow();
            }
        }
        void makeSmoothDisappearOnDeath() {
            Projectile.Opacity = Utils.GetLerpValue(0, MAXTIMELEFT / 3, Projectile.timeLeft, true);
        }
        void initTrailInfoAndAccelerate() {
            CloudberryValues cloudberryValues = new(Projectile);
            if (!cloudberryValues.Init) {
                cloudberryValues.Init = true;

                float velocityMultiplier = 3f;
                Projectile.velocity *= velocityMultiplier;

                _copyData = new CopyInfo[MAXCOPIES];
                cloudberryValues.CanMakeCopy = true;
                _snowBlockData = new SnowBlockInfo[MAXSNOWBLOCKS];
            }
        }
        void rotate() {
            Projectile.rotation += Projectile.velocity.X * 0.075f;
            if (Projectile.velocity.Y != 0f) {
                Projectile.rotation += Projectile.spriteDirection * 0.01f;
            }
        }
        void applyAdaptedEarthBoulderAI() {
            CloudberryValues cloudberryValues = new(Projectile);
            if (cloudberryValues.ApplyGravityTimerValue++ > 15f) {
                if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f) {
                    Projectile.velocity.X *= 0.97f;
                }
                cloudberryValues.ApplyGravityTimerValue = 15f;
                Projectile.velocity.Y += 0.2f;
            }
        }
        void framing() {
            CloudberryValues cloudberryValues = new(Projectile);
            cloudberryValues.CurrentFrameValue = MathF.Min(cloudberryValues.CurrentFrameValue, FRAMECOUNT);
            if (cloudberryValues.FramingSpeedValue == 0f) {
                cloudberryValues.FramingSpeedValue = STARTFRAMINGSPEED;
            }
            if ((cloudberryValues.CurrentFrameValue += cloudberryValues.FramingSpeedValue) > FRAMECOUNT) {
                cloudberryValues.CurrentFrameValue = 0f;
                cloudberryValues.FramingSpeedValue += STARTFRAMINGSPEED;
                cloudberryValues.CanMakeCopy = true;
            }
            MakeCopy();
        }
        void updateCopies() {
            for (int i = 0; i < MAXCOPIES; i++) {
                ref CopyInfo copyData = ref _copyData![i];
                if (copyData.Opacity > 0f) {
                    copyData.Scale -= 0.05f;
                    copyData.Opacity -= 0.05f;
                    copyData.Opacity = MathF.Max(0f, copyData.Opacity);
                }
            }
        }

        explodeOnDeath();
        makeSmoothDisappearOnDeath();
        initTrailInfoAndAccelerate();
        rotate();
        applyAdaptedEarthBoulderAI();
        framing();
        updateCopies();
    }

    protected override void Draw(ref Color lightColor) {
        CloudberryValues cloudberryValues = new(Projectile);
        if (!cloudberryValues.Init) {
            return;
        }

        if (_cloudberryTexture?.IsLoaded != true || _snowBlockTexture?.IsLoaded != true) {
            return;
        }

        Color color = lightColor;
        Texture2D cloudberryTexture = _cloudberryTexture.Value;
        int cloudberryWidth = cloudberryTexture.Width,
            cloudberryHeight = cloudberryTexture.Height / FRAMECOUNT;
        Texture2D snowBlockTexture = _snowBlockTexture.Value;
        void drawSelf() {
            CloudberryValues cloudberryValues = new(Projectile);
            Vector2 position = Projectile.Center;
            Vector2 origin = new Vector2(cloudberryWidth, cloudberryHeight) / 2f;
            Rectangle clip = new(0, cloudberryValues.UsedFrame * cloudberryHeight, cloudberryWidth, cloudberryHeight);
            float rotation = Projectile.rotation;
            Main.spriteBatch.Draw(cloudberryTexture, position, DrawInfo.Default with {
                Color = color,
                Rotation = rotation,
                Origin = origin,
                Clip = clip
            });
        }
        void drawCopies() {
            void drawTrails() {
                for (int i = 0; i < MAXCOPIES; i++) {
                    CopyInfo copyInfo = _copyData![i];
                    if (copyInfo.Opacity <= 0f) {
                        continue;
                    }
                    if (MathUtils.Approximately(copyInfo.Position, Projectile.Center, 2f)) {
                        continue;
                    }
                    Main.spriteBatch.Draw(cloudberryTexture, copyInfo.Position, DrawInfo.Default with {
                        Color = color * MathUtils.Clamp01(copyInfo.Opacity) * Projectile.Opacity,
                        Rotation = copyInfo.Rotation,
                        Scale = Vector2.One * MathF.Max(copyInfo.Scale, 1f),
                        Origin = new Vector2(cloudberryWidth, cloudberryHeight) / 2f,
                        Clip = new Rectangle(0, copyInfo.UsedFrame * cloudberryHeight, cloudberryWidth, cloudberryHeight)
                    });
                }
            }
            void drawSnowBlocks() {
                for (int i = 0; i < MAXSNOWBLOCKS; i++) {
                    SnowBlockInfo tileInfo = _snowBlockData![i];
                    Color tileColor = color * MathUtils.Clamp01(tileInfo.Opacity);
                    DrawUtils.DrawSingleTile(new DrawUtils.SingleTileDrawInfo(snowBlockTexture, tileInfo.Position, tileInfo.Clip, tileColor, tileInfo.Slope, tileInfo.HalfBlock));
                }
            }

            drawTrails();
            drawSnowBlocks();
        }

        drawCopies();
        drawSelf();
    }

    public override void PrepareBombToBlow() {
        Projectile.alpha = 255;
        Projectile.position = Projectile.Center;
        Projectile.Center = Projectile.position;
        Projectile.tileCollide = false;
        Projectile.alpha = 255;
        Projectile.Resize(128, 128);
        Projectile.knockBack = 8f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 12;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        void reflectOnTileCollision() {
            if (Projectile.velocity.X != oldVelocity.X) {
                Projectile.velocity.X = oldVelocity.X * -0.1f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 1.5f) {
                Projectile.velocity.Y = oldVelocity.Y * -0.4f;
            }
            Projectile.velocity.X *= 0.925f;
            MakeTileSnow();
        }

        reflectOnTileCollision();

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        void explode() {
            if (Projectile.timeLeft > 4) {
                Projectile.timeLeft = 4;
            }
        }

        explode();
    }

    public override bool CanHitPlayer(Player target) => false;
    //public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) => modifiers.FinalDamage /= 3;

    public override void OnKill(int timeLeft) {
        void makeHitboxBiggerAndPlayExplosionSound() {
            Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);

            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        }
        void createGroundExplosionEffects() {
            float mAX_SPREAD = 20f;
            int fluff = 10;
            int distFluff = 50;
            int layerStart = 1;
            int num0 = 6;
            if (Projectile.velocity.Length() < 8f || Math.Abs(Projectile.velocity.Y) < 4f) {
                mAX_SPREAD = 15f;
                fluff = 7;
                distFluff = 30;
                num0 = 4;
            }

            if (Projectile.velocity.Length() < 4f || Math.Abs(Projectile.velocity.Y) < 2f) {
                mAX_SPREAD = 15f;
                fluff = 7;
                distFluff = 30;
                num0 = 2;
                layerStart = 0;
            }

            int layerEnd = num0;
            int layerJump = num0 - 2;
            if (layerJump < 1)
                layerJump = 1;
            Point point = Projectile.TopLeft.ToTileCoordinates();
            Point point2 = Projectile.BottomRight.ToTileCoordinates();
            point.X -= fluff;
            point.Y -= fluff;
            point2.X += fluff;
            point2.Y += fluff;
            int num = point.X / 2 + point2.X / 2;
            int num2 = Projectile.width / 2 + distFluff;
            for (int i = layerStart; i < layerEnd; i += layerJump) {
                int num3 = i;
                for (int j = point.X; j <= point2.X; j++) {
                    for (int k = point.Y; k <= point2.Y; k++) {
                        if (!WorldGen.InWorld(j, k, 10))
                            return;

                        if (Vector2.Distance(Projectile.Center, new Vector2(j * 16, k * 16)) > (float)num2)
                            continue;

                        Tile tileSafely = Framing.GetTileSafely(j, k);
                        if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType])
                            continue;

                        Tile tileSafely2 = Framing.GetTileSafely(j, k - 1);
                        if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType])
                            continue;

                        int num4 = WorldGen.KillTile_GetTileDustAmount(true, tileSafely, j, k);
                        for (int l = 0; l < num4 / 2; l++) {
                            Dust obj = Dust.NewDustPerfect(new Point(j, k).ToWorldCoordinates() - new Vector2(0f, -2f + Main.rand.NextFloat() * 3f), DustID.SnowBlock);
                            obj.velocity.Y -= 3f + (float)num3 * 1.5f;
                            obj.velocity.Y *= Main.rand.NextFloat();
                            obj.scale += (float)num3 * 0.03f;
                        }

                        if (num3 >= 2) {
                            for (int m = 0; m < num4 / 2 - 1; m++) {
                                Dust obj2 = Dust.NewDustPerfect(new Point(j, k).ToWorldCoordinates() - new Vector2(0f, -2f + Main.rand.NextFloat() * 3f), DustID.SnowBlock);
                                obj2.velocity.Y -= 1f + (float)num3;
                                obj2.velocity.Y *= Main.rand.NextFloat();
                            }
                        }

                        if (num4 > 0 && Main.rand.NextBool(3)) {
                            float num5 = (float)Math.Abs(num - j) / (mAX_SPREAD / 2f);
                            Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, Vector2.Zero, 61 + Main.rand.Next(3), 1f - (float)num3 * 0.5f + num5 * 0.5f);
                            gore.velocity.Y -= 0.1f + (float)num3 * 0.5f + num5 * (float)num3 * 1f;
                            gore.velocity.Y *= Main.rand.NextFloat();
                            gore.position = new Vector2(j * 16 + 20, k * 16 + 20);
                        }
                    }
                }
            }
        }
        void createCloudberryDusts() {
            for (int i = 0; i < 10; i++) {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), ModContent.DustType<Dusts.Cloudberry>(), Main.rand.NextVector2Circular(3f, 3f));
                dust.scale = 0.6f;
                if (i < 5) {
                    dust.noGravity = true;
                    dust.scale = 1.8f;
                }
            }
        }
        void createSnowBlockDusts() {
            for (int i = 0; i < MAXSNOWBLOCKS; i++) {
                SnowBlockInfo tileInfo = _snowBlockData![i];
                int j = tileInfo.Position.X,
                    k = tileInfo.Position.Y;
                Tile tileSafely = WorldGenHelper.GetTileSafely(j, k);
                int num4 = WorldGen.KillTile_GetTileDustAmount(true, tileSafely, j, k);
                int num3 = 1;
                for (int l = 0; l < num4 / 2; l++) {
                    Dust obj = Dust.NewDustPerfect(new Point(j, k).ToWorldCoordinates() - new Vector2(0f, -2f + Main.rand.NextFloat() * 3f), DustID.SnowBlock);
                    obj.velocity.Y -= 3f + (float)num3 * 1.5f;
                    obj.velocity.Y *= Main.rand.NextFloat();
                    obj.scale += (float)num3 * 0.03f;
                }
            }
        }

        createGroundExplosionEffects();
        makeHitboxBiggerAndPlayExplosionSound();
        createCloudberryDusts();
        createSnowBlockDusts();
    }

    private void MakeCopy() {
        CloudberryValues cloudberryValues = new(Projectile);
        if (cloudberryValues.ShouldMakeCopy) {
            if (!cloudberryValues.CanMakeCopy) {
                return;
            }

            if (_currentCopyIndex >= MAXCOPIES) {
                _currentCopyIndex = 0;
            }
            _copyData![_currentCopyIndex++] = new CopyInfo() {
                Position = Projectile.Center,
                UsedFrame = EXPLOSIONFRAME,
                Rotation = Projectile.rotation,
                Opacity = 1.25f,
                Scale = 1.5f
            };
            cloudberryValues.ShouldMakeCopy = cloudberryValues.CanMakeCopy = false;
        }
        else if (cloudberryValues.UsedFrame == EXPLOSIONFRAME) {
            cloudberryValues.ShouldMakeCopy = true;
        }
    }

    private void MakeTileSnow() {
        Point positionInTiles = Projectile.Center.ToTileCoordinates();
        while (!WorldGenHelper.SolidTile(positionInTiles.X, positionInTiles.Y)) {
            positionInTiles.Y++;
        }
        if (_lastTilePosition == positionInTiles) {
            return;
        }

        void makeTileSnow() {
            if (_currentTileIndex >= MAXSNOWBLOCKS) {
                _currentTileIndex = 0;
            }
            _lastTilePosition = positionInTiles;
            Tile tile = WorldGenHelper.GetTileSafely(positionInTiles);
            _snowBlockData![_currentTileIndex++] = new SnowBlockInfo() {
                Position = positionInTiles,
                Slope = tile.Slope,
                HalfBlock = tile.IsHalfBlock,
                Clip = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                Opacity = 1.25f
            };
        }
        void createSnowDusts() {
            Vector2 dustPosition = positionInTiles.AdjustY(-1).ToWorldCoordinates();
            int dustCount = Main.rand.Next(6, 11);
            for (int k = 0; k < dustCount; k++) {
                Dust.NewDust(new Vector2(dustPosition.X, dustPosition.Y), 16, 16, DustID.SnowBlock, SpeedX: -Projectile.velocity.X * 0.4f, SpeedY: -Projectile.velocity.Y * 0.4f, Alpha: Main.rand.Next(255), Scale: Main.rand.NextFloat(1.5f) * 0.75f);
            }
        }

        createSnowDusts();
        makeTileSnow();
    }
    private void LoadCloudberryTexture() {
        if (Main.dedServ) {
            return;
        }

        _cloudberryTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "Cloudberry");
        _snowBlockTexture = TextureAssets.Tile[TileID.SnowBlock];
    }
}
