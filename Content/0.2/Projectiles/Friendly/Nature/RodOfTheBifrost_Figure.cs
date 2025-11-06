using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class MagicalBifrostBlock : NatureProjectile {
    private static ushort TIMELEFT => 300;
    private static byte BLOCKCOUNTINFIGURE => 9;
    private static float ATTACKMOVESPEEDPERTICK => 10f;
    private static bool SHOULDSCALEWITHWREATHCHARGE => false;

    public enum BifrostFigureType : byte {
        Red1,
        Red2,
        Purple1,
        Purple2,
        Purple3,
        Purple4,
        Blue1,
        Blue2,
        Aqua1,
        Green1,
        Green2,
        Green3,
        Green4,
        DarkOrange1,
        DarkOrange2,
        DarkOrange3,
        DarkOrange4,
        Orange1,
        Orange2,
        Count
    }

    public struct MagicalBifrostBlockInfo() {
        public Point16 StartOffset = Point16.Zero;
        public Point16 FrameCoords = Point16.Zero;

        public readonly bool Active => FrameCoords != Point16.Zero;
    }

    private float _trailOpacity;

    public MagicalBifrostBlockInfo[] MagicalBifrostBlockData { get; private set; } = null!;

    public IEnumerable<MagicalBifrostBlockInfo> ActiveMagicalBifrostBlockData => MagicalBifrostBlockData.Where(blockInfo => blockInfo.Active);

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float MouseX => ref Projectile.localAI[1];
    public ref float MouseY => ref Projectile.localAI[2];
    public ref float FigureTypeValue => ref Projectile.ai[0];
    public ref float EmpressOfLightColor => ref Projectile.ai[1];
    public ref float StoppedMovingValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public Vector2 SavedMousePosition {
        get => new(MouseX, MouseY);
        set {
            MouseX = value.X;
            MouseY = value.Y;
        }
    }

    public bool StoppedMoving {
        get => StoppedMovingValue == 1f;
        set => StoppedMovingValue = value.ToInt();
    }

    public BifrostFigureType FigureType {
        get => (BifrostFigureType)FigureTypeValue;
        set => FigureTypeValue = Utils.Clamp((byte)value, (byte)BifrostFigureType.Red1, (byte)(BifrostFigureType.Count - 1));
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.aiStyle = -1;
    }

    public override void AI() {
        float trailOpacityTarget = 1f;
        if (StoppedMoving) {
            trailOpacityTarget = 0f;
        }
        _trailOpacity = MathHelper.Lerp(_trailOpacity, trailOpacityTarget, 0.2f);

        void init() {
            if (Init) {
                return;
            }

            if (Projectile.IsOwnerLocal()) {
                Player owner = Projectile.GetOwnerAsPlayer();
                Vector2 mousePosition = owner.GetWorldMousePosition();
                Vector2 screenStart = owner.Center - Vector2.UnitY * Main.screenHeight / 2f;
                Projectile.Center = new Vector2(mousePosition.X, screenStart.Y);

                SavedMousePosition = mousePosition;
                FigureType = owner.GetCommon().ActiveFigureType;

                Projectile.netUpdate = true;
            }

            _trailOpacity = 1f;

            InitializeFigure();

            Init = true;
        }

        void moveSlowly() {
            bool reachedEnd = Projectile.Center.Y > SavedMousePosition.Y;
            if (reachedEnd || StoppedMoving) {
                if (reachedEnd) {
                    StopMoving();
                }
                return;
            }
            foreach (Projectile otherFigure in TrackedEntitiesSystem.GetTrackedProjectile<MagicalBifrostBlock>(checkProjectile => checkProjectile.SameAs(Projectile))) {
                MagicalBifrostBlock otherMagicalBlock = otherFigure.As<MagicalBifrostBlock>();
                if (!otherMagicalBlock.Init) {
                    continue;
                }
                if (CollidingWith(otherMagicalBlock)) {
                    StopMoving();
                    return; ;
                }
            }
            Player owner = Projectile.GetOwnerAsPlayer();
            float attackMoveValue = ATTACKMOVESPEEDPERTICK;
            if (SHOULDSCALEWITHWREATHCHARGE) {
                attackMoveValue *= 1f + WreathHandler.GetWreathChargeProgress(owner);
            }
            Projectile.position.Y += attackMoveValue;
        }

        init();
        moveSlowly();

        Projectile.Center = Vector2.Lerp(Projectile.Center, Projectile.Center.ToTileCoordinates16().ToWorldCoordinates(), TimeSystem.LogicDeltaTime * 5f);

        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            Vector2 position = GetBlockPosition(blockInfo);
            if (Main.rand.Next(6) == 0) {
                Dust dust = Dust.NewDustPerfect(position, 267);
                dust.velocity.X *= 0.25f;
                dust.fadeIn = 1f;
                dust.noGravity = true;
                dust.alpha = 100;
                dust.color = Projectile.GetFairyQueenWeaponsColor(1f, Main.rand.NextFloat() * 0.4f);
                dust.noLightEmittence = true;
                dust.scale *= 1.5f;
            }
        }
    }

    public override void OnKill(int timeLeft) {
        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            Color fairyQueenWeaponsColor = Projectile.GetFairyQueenWeaponsColor();
            Vector2 position = GetBlockPosition(blockInfo);
            Vector2 target = position;
            Main.rand.NextFloat();
            int num20 = 10;
            for (int num21 = 0; num21 < num20; num21++) {
                Vector2 vector5 = position - Vector2.UnitY * num21;
                int num22 = Main.rand.Next(1, 3);
                float num23 = MathHelper.Lerp(0.3f, 1f, Utils.GetLerpValue(num20, 0f, num21, clamped: true));
                vector5.DirectionTo(target).SafeNormalize(Vector2.Zero);
                target = vector5;
                for (float num24 = 0f; num24 < (float)num22; num24++) {
                    int num25 = Dust.NewDust(vector5 - Vector2.One * 8f, 16, 16, 267, 0f, 0f, 0, fairyQueenWeaponsColor);
                    Dust dust2 = Main.dust[num25];
                    dust2.velocity *= Main.rand.NextFloat() * 0.8f;
                    Main.dust[num25].noGravity = true;
                    Main.dust[num25].scale = 0.9f + Main.rand.NextFloat() * 1.2f;
                    Main.dust[num25].fadeIn = Main.rand.NextFloat() * 1.2f * num23;
                    dust2 = Main.dust[num25];
                    dust2.scale *= num23;
                    if (num25 != 6000) {
                        Dust dust11 = Dust.CloneDust(num25);
                        dust2 = dust11;
                        dust2.scale /= 2f;
                        dust2 = dust11;
                        dust2.fadeIn *= 0.85f;
                        dust11.color = new Color(255, 255, 255, 255);
                    }
                }
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            if (GetRect(blockInfo).Intersects(targetHitbox)) {
                return true;
            }
        }

        return false;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Color baseColor = Color.White * Projectile.Opacity;
        Color color = baseColor;
        color.A /= 2;
        Color fairyQueenWeaponsColor = Projectile.GetFairyQueenWeaponsColor(0f) * 0.5f;
        Vector2 scale = Vector2.One * Projectile.scale;
        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            Vector2 position = GetBlockPosition(blockInfo);
            Point16 frameCoords = blockInfo.FrameCoords;
            Rectangle clip = new(frameCoords.X * 18, frameCoords.Y * 18, 16, 16);
            Vector2 origin = clip.Centered();
            int count = 10;
            float step = 1f / count;
            float maxOffset = 12f * count;
            for (float num2 = step; num2 < 1f; num2 += step) {
                Vector2 vector2 = Vector2.UnitY * -maxOffset * num2;
                batch.Draw(texture, position + vector2, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = fairyQueenWeaponsColor * Projectile.Opacity * (1f - num2) * _trailOpacity,
                    Scale = scale
                });
            }
        }
        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            Vector2 position = GetBlockPosition(blockInfo);
            Point16 frameCoords = blockInfo.FrameCoords;
            Rectangle clip = new(frameCoords.X * 18, frameCoords.Y * 18, 16, 16);
            Vector2 origin = clip.Centered();
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = baseColor,
                Scale = scale
            });
            for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
                Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 2f * scale;
                batch.Draw(texture, position + vector2, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = fairyQueenWeaponsColor * 0.3f,
                    Scale = scale * 1.5f
                });
            }
        }

        return false;
    }

    public Rectangle GetRect(MagicalBifrostBlockInfo blockInfo, int offsetX = 0, int offsetY = 0, bool tiled = false) => GeometryUtils.CenteredSquare(GetBlockPosition(blockInfo, offsetX, offsetY, tiled), (int)TileHelper.TileSize);

    public bool CollidingWith(MagicalBifrostBlock otherMagicalBlock) {
        bool result = false;
        MagicalBifrostBlockInfo[] otherData = otherMagicalBlock.MagicalBifrostBlockData;
        if (otherData is null) {
            return result;
        }
        IEnumerable<MagicalBifrostBlockInfo> otherActiveMagicalBifrostBlockData = otherData.Where(blockInfo => blockInfo.Active);
        foreach (MagicalBifrostBlockInfo blockInfo in ActiveMagicalBifrostBlockData) {
            foreach (MagicalBifrostBlockInfo otherBlockInfo in otherActiveMagicalBifrostBlockData) {
                if (GetRect(blockInfo, offsetY: 1, tiled: true).Intersects(otherMagicalBlock.GetRect(otherBlockInfo, tiled: true))) {
                    return result = true;
                }
            }
        }
        return result;
    }

    private void StopMoving() {
        if (StoppedMoving) {
            return;
        }

        StoppedMoving = true;
        SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
    }

    private Vector2 GetBlockPosition(MagicalBifrostBlockInfo blockInfo, int offsetX = 0, int offsetY = 0, bool tiled = false) {
        Vector2 result;
        if (tiled) {
            result = (Projectile.Center.ToTileCoordinates16() + blockInfo.StartOffset + Point16.NegativeOne + new Point16(offsetX, offsetY)).ToWorldCoordinates();
            return result;
        }
        result = Projectile.Center + (blockInfo.StartOffset + Point16.NegativeOne + new Point16(offsetX, offsetY)).ToWorldCoordinates();
        return result;
    }

    private void InitializeFigure() {
        MagicalBifrostBlockData = new MagicalBifrostBlockInfo[BLOCKCOUNTINFIGURE];

        int currentIndexToAdd = 0;
        void addBlockInFigure(Point16 startOffset, Point16 frameCoords) {
            ref MagicalBifrostBlockInfo blockInfo = ref MagicalBifrostBlockData[currentIndexToAdd];
            if (blockInfo.Active) {
                currentIndexToAdd++;
                return;
            }
            blockInfo = new MagicalBifrostBlockInfo {
                StartOffset = startOffset,
                FrameCoords = frameCoords
            };
            currentIndexToAdd++;
        }
        switch (FigureType) {
            case BifrostFigureType.Red1:
                addBlockInFigure(new Point16(1, 0), new Point16(1, 0));
                addBlockInFigure(new Point16(2, 0), new Point16(2, 0));
                addBlockInFigure(new Point16(0, 1), new Point16(0, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(1, 1));
                break;
            case BifrostFigureType.Red2:
                addBlockInFigure(new Point16(0, 0), new Point16(0, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(0, 3));
                addBlockInFigure(new Point16(1, 1), new Point16(1, 3));
                addBlockInFigure(new Point16(1, 2), new Point16(1, 4));
                break;
            case BifrostFigureType.Purple1:
                addBlockInFigure(new Point16(0, 0), new Point16(3, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(4, 0));
                addBlockInFigure(new Point16(2, 0), new Point16(5, 0));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 1));
                break;
            case BifrostFigureType.Purple2:
                addBlockInFigure(new Point16(1, 0), new Point16(4, 2));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 3));
                addBlockInFigure(new Point16(1, 2), new Point16(4, 4));
                addBlockInFigure(new Point16(0, 1), new Point16(3, 3));
                break;
            case BifrostFigureType.Purple3:
                addBlockInFigure(new Point16(1, 0), new Point16(4, 5));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 6));
                addBlockInFigure(new Point16(0, 1), new Point16(3, 6));
                addBlockInFigure(new Point16(2, 1), new Point16(5, 6));
                break;
            case BifrostFigureType.Purple4:
                addBlockInFigure(new Point16(0, 0), new Point16(3, 7));
                addBlockInFigure(new Point16(0, 1), new Point16(3, 8));
                addBlockInFigure(new Point16(0, 2), new Point16(3, 9));
                addBlockInFigure(new Point16(1, 1), new Point16(4, 8));
                break;
            case BifrostFigureType.Blue1:
                addBlockInFigure(new Point16(0, 0), new Point16(6, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(7, 0));
                addBlockInFigure(new Point16(1, 1), new Point16(7, 1));
                addBlockInFigure(new Point16(2, 1), new Point16(8, 1));
                break;
            case BifrostFigureType.Blue2:
                addBlockInFigure(new Point16(1, 0), new Point16(7, 2));
                addBlockInFigure(new Point16(1, 1), new Point16(7, 3));
                addBlockInFigure(new Point16(0, 1), new Point16(6, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(6, 4));
                break;
            case BifrostFigureType.Aqua1:
                addBlockInFigure(new Point16(0, 0), new Point16(9, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(10, 0));
                addBlockInFigure(new Point16(0, 1), new Point16(9, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(10, 1));
                break;
            case BifrostFigureType.Green1:
                addBlockInFigure(new Point16(0, 0), new Point16(11, 0));
                addBlockInFigure(new Point16(0, 1), new Point16(11, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(12, 1));
                addBlockInFigure(new Point16(2, 1), new Point16(13, 1));
                break;
            case BifrostFigureType.Green2:
                addBlockInFigure(new Point16(0, 0), new Point16(11, 2));
                addBlockInFigure(new Point16(1, 0), new Point16(12, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(11, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(11, 4));
                break;
            case BifrostFigureType.Green3:
                addBlockInFigure(new Point16(0, 0), new Point16(11, 5));
                addBlockInFigure(new Point16(1, 0), new Point16(12, 5));
                addBlockInFigure(new Point16(2, 0), new Point16(13, 5));
                addBlockInFigure(new Point16(2, 1), new Point16(13, 6));
                break;
            case BifrostFigureType.Green4:
                addBlockInFigure(new Point16(1, 0), new Point16(12, 7));
                addBlockInFigure(new Point16(1, 1), new Point16(12, 8));
                addBlockInFigure(new Point16(1, 2), new Point16(12, 9));
                addBlockInFigure(new Point16(0, 2), new Point16(11, 9));
                break;
            case BifrostFigureType.DarkOrange1:
                addBlockInFigure(new Point16(0, 1), new Point16(14, 1));
                addBlockInFigure(new Point16(1, 1), new Point16(15, 1));
                addBlockInFigure(new Point16(2, 1), new Point16(16, 1));
                addBlockInFigure(new Point16(2, 0), new Point16(16, 0));
                break;
            case BifrostFigureType.DarkOrange2:
                addBlockInFigure(new Point16(0, 0), new Point16(14, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(14, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(14, 4));
                addBlockInFigure(new Point16(1, 2), new Point16(15, 4));
                break;
            case BifrostFigureType.DarkOrange3:
                addBlockInFigure(new Point16(0, 0), new Point16(14, 5));
                addBlockInFigure(new Point16(1, 0), new Point16(15, 5));
                addBlockInFigure(new Point16(2, 0), new Point16(16, 5));
                addBlockInFigure(new Point16(0, 1), new Point16(14, 6));
                break;
            case BifrostFigureType.DarkOrange4:
                addBlockInFigure(new Point16(0, 0), new Point16(14, 7));
                addBlockInFigure(new Point16(1, 0), new Point16(15, 7));
                addBlockInFigure(new Point16(1, 1), new Point16(15, 8));
                addBlockInFigure(new Point16(1, 2), new Point16(15, 9));
                break;
            case BifrostFigureType.Orange1:
                addBlockInFigure(new Point16(0, 0), new Point16(17, 0));
                addBlockInFigure(new Point16(1, 0), new Point16(18, 0));
                addBlockInFigure(new Point16(2, 0), new Point16(19, 0));
                addBlockInFigure(new Point16(3, 0), new Point16(20, 0));
                break;
            case BifrostFigureType.Orange2:
                addBlockInFigure(new Point16(0, 0), new Point16(17, 2));
                addBlockInFigure(new Point16(0, 1), new Point16(17, 3));
                addBlockInFigure(new Point16(0, 2), new Point16(17, 4));
                addBlockInFigure(new Point16(0, 3), new Point16(17, 5));
                break;
        }
    }
}
