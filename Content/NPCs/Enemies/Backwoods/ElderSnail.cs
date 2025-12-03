using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Enemies.ElderSnailTrail;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class ElderSnail : ModNPC, IRequestAssets {
    public enum ElderSnailRequstedTextureType : byte {
        Trail1,
        Trail2,
        Trail3, 
        Count
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)ElderSnailTrailRequstedTextureType.Trail1, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail1"),
         ((byte)ElderSnailTrailRequstedTextureType.Trail2, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail2"),
         ((byte)ElderSnailTrailRequstedTextureType.Trail3, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail3")];

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement($"Mods.RoA.Bestiary.{nameof(ElderSnail)}")
        ]);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Collision_MoveSnailOnSlopes")]
    public extern static void NPC_Collision_MoveSnailOnSlopes(NPC self);

    private static byte FRAMECOUNT => 10;
    private static float UPDATEDIRECTIONEVERYNTICKS => 10f;
    private static float UPDATETARGETEVERYNTICKS => 90f;
    private static float HIDETIMEINTICKS => 120f;
    private static float CANTHIDETIME => 30f;

    private static float MAXTIMETOSPEEDUP => 30f;
    private static float MINTIMETOSPEEDUP => MAXTIMETOSPEEDUP * 0.25f;

    private float _targetTimer;
    private float _speedXFactor;

    private bool _playMoveAnimation, _playHideAnimation;
    private bool _playAppearAfterHidingAnimation;

    private bool _shouldHide, _shouldBeHiding;
    private float _hideFactor;

    private bool _init;

    private HashSet<Point16> _passedPositions = null!;

    private bool IsFalling => NPC.ai[2] > 0f;
    private int FacedDirection => (int)-NPC.ai[3];
    private bool CanHide => _hideFactor >= 0f;

    public ref double FrameCounter => ref NPC.frameCounter;

    public override void Load() {
        On_NPC.UpdateCollision += On_NPC_UpdateCollision;
        On_NPC.Collision_MoveSlopesAndStairFall += On_NPC_Collision_MoveSlopesAndStairFall;
    }

    private void On_NPC_Collision_MoveSlopesAndStairFall(On_NPC.orig_Collision_MoveSlopesAndStairFall orig, NPC self, bool fall) {
        if (self.type == ModContent.NPCType<ElderSnail>()) {
            return;
        }

        orig(self, fall);
    }

    private void On_NPC_UpdateCollision(On_NPC.orig_UpdateCollision orig, NPC self) {
        orig(self);

        if (self.type == ModContent.NPCType<ElderSnail>()) {
            NPC_Collision_MoveSnailOnSlopes(self);
        }
    }

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(26, 36);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;

        NPC.noGravity = true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_shouldBeHiding);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _shouldBeHiding = reader.ReadBoolean();
    }

    public override void AI() {
        if (!_init) {
            _passedPositions ??= [];
            _init = true;
        }

        ApplySnailAI();
    }

    private void Hide() {
        _shouldBeHiding = true;

        NPC.netUpdate = true;
    }

    private void AppearAfterHiding() {
        _playAppearAfterHidingAnimation = true;
    }

    private void ResetHideState() {
        _playHideAnimation = false;
        _speedXFactor = 0f;
        _hideFactor = -CANTHIDETIME;
        _shouldHide = false;

        AppearAfterHiding();

        ResetFrame();
    }

    private void TargetClosestPlayer() {
        _targetTimer = UPDATETARGETEVERYNTICKS;
    }

    private void ResetTargetTimeValues() {
        _targetTimer = 0f; 
    }

    private void ResetFrame() {
        FrameCounter = 0.0;
    }

    private void TargetOverTime() {
        if (!_shouldHide) {
            if (HasTargetLine()) {
                _targetTimer++;
            }
            if (_speedXFactor < MINTIMETOSPEEDUP && _targetTimer > UPDATETARGETEVERYNTICKS) {
                _targetTimer = 0f;
                NPC.ai[0] = 0f;
                NPC.localAI[0] = UPDATEDIRECTIONEVERYNTICKS;
            }
        }
    }

    private void TryToHideOverTime() {
        if (!NPC.IsGrounded()) {
            return;
        }

        if (_shouldBeHiding) {
            if (_shouldHide || !CanHide) {
                return;
            }

            ResetTargetTimeValues();

            _speedXFactor = 0f;
            _playMoveAnimation = false;
            _shouldHide = true;
            _playHideAnimation = true;

            ResetFrame();

            _shouldBeHiding = false;

            return;
        }
        if (Helper.SinglePlayerOrServer) {
            if (HasTargetLine() && Main.rand.NextBool(200)) {
                Hide();
            }
        }
    }

    private bool HasTargetLine() => Collision.CanHitLine(NPC.Center, 0, 0, NPC.GetTargetPlayer().Center, 0, 0);

    private void GetVelocitySpeeds(out float speedX, out float speedY) {
        float hideFactor = MathUtils.Clamp01(1f - _hideFactor);
        if (_playAppearAfterHidingAnimation) {
            hideFactor = 0f;
        }

        float add = 0.75f * hideFactor;
        _speedXFactor += add;
        if (_speedXFactor > MAXTIMETOSPEEDUP) {
            _speedXFactor = 0f;

            _playMoveAnimation = false;
            FrameCounter = 0.0;
        }
        float progress = Ease.CubeInOut(Utils.GetLerpValue(MINTIMETOSPEEDUP, MAXTIMETOSPEEDUP, _speedXFactor, true));

        speedX = 0.1f + 1.5f * progress;
        speedY = 0.5f + 0.5f * progress;

        speedX *= hideFactor;
        speedY *= hideFactor;
    }

    private void UpdateHideState() {
        if (!_playMoveAnimation && _speedXFactor > MINTIMETOSPEEDUP) {
            _playMoveAnimation = true;
        }

        if (_shouldHide) {
            NPC.velocity.X *= 0.8f;
        }
        float hideFactorLerpValue = 0.1f;
        if (_shouldHide) {
            _hideFactor = Helper.Approach(_hideFactor, HIDETIMEINTICKS, _hideFactor > 1f ? 1f : hideFactorLerpValue);
            if (_hideFactor >= HIDETIMEINTICKS) {
                ResetHideState();
            }
        }
        else {
            _hideFactor = Helper.Approach(_hideFactor, 0f, hideFactorLerpValue);
        }
    }

    private void ApplySnailAI() {
        Vector2 passedPosition = NPC.Center + new Vector2(0.625f * NPC.ai[3], 1f).RotatedBy(NPC.rotation) * NPC.height / 2f;
        Point16 passedPositionInTiles = passedPosition.ToTileCoordinates16();
        if (WorldGenHelper.GetTileSafely(passedPositionInTiles).HasTile) {
            _passedPositions.Add(passedPositionInTiles);
        }

        TargetOverTime();
        TryToHideOverTime();
        UpdateHideState();

        void applyAdjustedVanillaSnailAI() {
            GetVelocitySpeeds(out float speedX, out float speedY);

            if (NPC.ai[0] == 0f) {
                NPC.TargetClosest();
                NPC.directionY = 1;
                NPC.ai[0] = 1f;
                if (NPC.direction > 0)
                    NPC.spriteDirection = 1;
            }

            bool flag53 = false;
            if (Helper.SinglePlayerOrServer) {
                if (NPC.ai[2] == 0f && Main.rand.Next(7200) == 0) {
                    NPC.ai[2] = 2f;
                    NPC.netUpdate = true;
                }

                if (!NPC.collideX && !NPC.collideY) {
                    NPC.localAI[3] += 1f;
                    if (NPC.localAI[3] > 5f) {
                        NPC.ai[2] = 2f;
                        NPC.netUpdate = true;
                    }
                }
                else {
                    NPC.localAI[3] = 0f;
                }
            }

            if (IsFalling && !_shouldHide) {
                NPC.ai[1] = 0f;
                NPC.ai[0] = 1f;
                NPC.directionY = 1;
                if (NPC.velocity.Y > speedX)
                    NPC.rotation += NPC.direction * 0.1f;
                else
                    NPC.rotation = 0f;

                NPC.spriteDirection = NPC.direction;
                NPC.velocity.X = speedX * NPC.direction;
                NPC.noGravity = false;
                int num1042 = (int)(NPC.Center.X + NPC.width / 2 * -NPC.direction) / 16;
                int num1043 = (int)(NPC.position.Y + NPC.height + 8f) / 16;
                if (!Main.tile[num1042, num1043].TopSlope && NPC.collideY)
                    NPC.ai[2] -= 1f;

                num1043 = (int)(NPC.position.Y + NPC.height - 4f) / 16;
                num1042 = (int)(NPC.Center.X + NPC.width / 2 * NPC.direction) / 16;
                if (Main.tile[num1042, num1043].BottomSlope)
                    NPC.direction *= -1;

                if (NPC.collideX && NPC.velocity.Y == 0f) {
                    flag53 = true;
                    NPC.ai[2] = 0f;
                    NPC.directionY = -1;
                    NPC.ai[1] = 1f;
                }

                if (NPC.velocity.Y == 0f) {
                    if (NPC.localAI[1] == NPC.position.X) {
                        NPC.localAI[2] += 1f;
                        if (NPC.localAI[2] > 10f) {
                            NPC.direction = 1;
                            NPC.velocity.X = NPC.direction * speedX;
                            NPC.localAI[2] = 0f;
                        }
                    }
                    else {
                        NPC.localAI[2] = 0f;
                        NPC.localAI[1] = NPC.position.X;
                    }
                }
            }

            if (NPC.ai[2] != 0f)
                return;

            NPC.noGravity = true;
            if (NPC.ai[1] == 0f) {
                if (NPC.collideY)
                    NPC.ai[0] = 2f;

                if (!NPC.collideY && NPC.ai[0] == 2f) {
                    NPC.direction = -NPC.direction;
                    NPC.ai[1] = 1f;
                    NPC.ai[0] = 1f;
                }

                if (NPC.collideX) {
                    NPC.directionY = -NPC.directionY;
                    NPC.ai[1] = 1f;
                }
            }
            else {
                if (NPC.collideX)
                    NPC.ai[0] = 2f;

                if (!NPC.collideX && NPC.ai[0] == 2f) {
                    NPC.directionY = -NPC.directionY;
                    NPC.ai[1] = 0f;
                    NPC.ai[0] = 1f;
                }

                if (NPC.collideY) {
                    NPC.direction = -NPC.direction;
                    NPC.ai[1] = 0f;
                }
            }

            if (!flag53) {
                float num1044 = NPC.rotation;
                if (NPC.directionY < 0) {
                    if (NPC.direction < 0) {
                        if (NPC.collideX) {
                            NPC.rotation = 1.57f;
                            NPC.spriteDirection = -1;
                        }
                        else if (NPC.collideY) {
                            NPC.rotation = 3.14f;
                            NPC.spriteDirection = 1;
                        }
                    }
                    else if (NPC.collideY) {
                        NPC.rotation = 3.14f;
                        NPC.spriteDirection = -1;
                    }
                    else if (NPC.collideX) {
                        NPC.rotation = 4.71f;
                        NPC.spriteDirection = 1;
                    }
                }
                else if (NPC.direction < 0) {
                    if (NPC.collideY) {
                        NPC.rotation = 0f;
                        NPC.spriteDirection = -1;
                    }
                    else if (NPC.collideX) {
                        NPC.rotation = 1.57f;
                        NPC.spriteDirection = 1;
                    }
                }
                else if (NPC.collideX) {
                    NPC.rotation = 4.71f;
                    NPC.spriteDirection = -1;
                }
                else if (NPC.collideY) {
                    NPC.rotation = 0f;
                    NPC.spriteDirection = 1;
                }

                float num1045 = NPC.rotation;
                NPC.rotation = num1044;
                if (NPC.rotation > 6.28)
                    NPC.rotation -= 6.28f;

                if (NPC.rotation < 0f)
                    NPC.rotation += 6.28f;

                float num1046 = Math.Abs(NPC.rotation - num1045);
                float num1047 = 0.05f;
                if (NPC.rotation > num1045) {
                    if ((double)num1046 > 3.14) {
                        NPC.rotation += num1047;
                    }
                    else {
                        NPC.rotation -= num1047;
                        if (NPC.rotation < num1045)
                            NPC.rotation = num1045;
                    }
                }

                if (NPC.rotation < num1045) {
                    if ((double)num1046 > 3.14) {
                        NPC.rotation -= num1047;
                    }
                    else {
                        NPC.rotation += num1047;
                        if (NPC.rotation > num1045)
                            NPC.rotation = num1045;
                    }
                }
            }

            if (NPC.localAI[0]++ > UPDATEDIRECTIONEVERYNTICKS) {
                NPC.localAI[0] = 0f;

                NPC.ai[3] = NPC.spriteDirection;
            }

            NPC.velocity.X = speedX * NPC.direction;
            NPC.velocity.Y = speedY * NPC.directionY;
        }

        applyAdjustedVanillaSnailAI();
    }

    public override void FindFrame(int frameHeight) {
        ushort frameHeight2 = (ushort)frameHeight;

        byte moveFrameCount = 3,
             hideFrameCount = 3;

        double perFrameCounter = 8.0;
        if (_playAppearAfterHidingAnimation) {
            perFrameCounter *= 0.75;

            byte frame = (byte)(FrameCounter / perFrameCounter);
            if (FrameCounter < perFrameCounter * moveFrameCount) {
                FrameCounter++;
            }
            else {
                frame = moveFrameCount;

                FrameCounter++;
                if (FrameCounter > perFrameCounter * (moveFrameCount + 1)) {
                    TargetClosestPlayer();
                    _playAppearAfterHidingAnimation = false;
                    ResetFrame();
                }
            }
            frame = (byte)(moveFrameCount + (hideFrameCount - frame - 1));
            NPC.SetFrame(frame, frameHeight2);

            return;
        }
        if (_playHideAnimation) {
            byte frame = (byte)(moveFrameCount + FrameCounter / perFrameCounter);
            if (FrameCounter < perFrameCounter * hideFrameCount) {
                FrameCounter += (_hideFactor >= 0f).ToDirectionInt();
            }
            else {
                frame = (byte)(hideFrameCount + moveFrameCount - 1);
            }
            NPC.SetFrame(frame, frameHeight2);

            return;
        }
        if (_playMoveAnimation) {
            byte frame = (byte)(FrameCounter / perFrameCounter);
            if (FrameCounter < perFrameCounter * moveFrameCount) {
                FrameCounter++;
            }
            else {
                frame = 0;
            }
            NPC.SetFrame(frame, frameHeight2);

            return;
        }

        NPC.SetFrame(0, frameHeight2);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (AssetInitializer.TryGetRequestedTextureAssets<ElderSnail>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            if (_init) {
                foreach (Point16 passedPosition in _passedPositions) {
                    Vector2 worldPosition = passedPosition.ToWorldCoordinates() - Vector2.One * 6f;
                    if (WorldGenHelper.GetTileSafely(passedPosition).IsHalfBlock) {
                        worldPosition.Y += 8f;
                    }
                    ulong seed = (((ulong)worldPosition.X << 32) | (uint)worldPosition.Y);
                    byte a = 150;
                    Color color = new(a, a, a, a);
                    for (int i = 0; i < 4; i++) {
                        Vector2 scale = new Vector2(1.5f, 1f);
                        Texture2D texture = indexedTextureAssets[(byte)Utils.RandomInt(ref seed, 0, 3)].Value;
                        Rectangle clip = texture.Bounds;
                        Vector2 origin = clip.Centered();
                        spriteBatch.Draw(texture, worldPosition + new Vector2(Utils.RandomInt(ref seed, -1, 2), Utils.RandomInt(ref seed, -1, 2)), DrawInfo.Default with {
                            Clip = clip,
                            Scale = scale,
                            Origin = origin,
                            Color = color
                        });
                    }
                }
            }
        }

        SpriteEffects flip = FacedDirection.ToSpriteEffects();
        NPC.QuickDraw(spriteBatch, screenPos, drawColor, effect: flip);

        return false;
    }
}
