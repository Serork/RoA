using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.NPCs;
using RoA.Common.World;
using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class ElderSnail : ModNPC, IRequestAssets {
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Collision_MoveSnailOnSlopes")]
    public extern static void NPC_Collision_MoveSnailOnSlopes(NPC self);

    public enum ElderSnailRequstedTextureType : byte {
        Trail1,
        Trail2,
        Trail3, 
        Count
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)ElderSnailRequstedTextureType.Trail1, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail1"),
         ((byte)ElderSnailRequstedTextureType.Trail2, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail2"),
         ((byte)ElderSnailRequstedTextureType.Trail3, ResourceManager.BackwoodsEnemyNPCTextures + "ElderSnail_Trail3")];

    private record struct PassedPositionInfo(Point16 Position, float Rotation, float Opacity = 0f);

    private static byte FRAMECOUNT => 10;
    private static float UPDATEDIRECTIONEVERYNTICKS => 10f;
    private static float UPDATETARGETEVERYNTICKS => 90f;
    private static float HIDETIMEINTICKS => 300f;
    private static float CANTHIDETIMEINWHATAEVER => 30f;
    private static byte TRAILPOSITIONCOUNT => 20;

    private static float MAXTIMETOSPEEDUP => 30f;
    private static float MINTIMETOSPEEDUP => MAXTIMETOSPEEDUP * 0.25f;

    private float _targetTimer;
    private float _speedXFactor;

    private bool _playMoveAnimation, _playHideAnimation, _playAttackAnimation;
    private bool _playAppearAfterHidingAnimation;

    private bool _shouldHide, _shouldAttack, _shouldBeHiding, _shouldBeAttacking;
    private float _hideFactor;

    private bool _init;

    private PassedPositionInfo[] _passedPositions = null!;
    private byte _passedPositionNextIndex;

    private bool IsFalling => NPC.ai[2] > 0f;
    private int FacedDirection => (int)-NPC.ai[3];
    private bool CanHide => _hideFactor >= 0f && !_shouldAttack;
    private bool IsHiding => _hideFactor > 1f;

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

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement($"Mods.RoA.Bestiary.{nameof(ElderSnail)}")
        ]);
    }

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(30, 30);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;

        NPC.noGravity = true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_shouldBeHiding);
        writer.Write(_shouldBeAttacking);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _shouldBeHiding = reader.ReadBoolean();
        _shouldBeAttacking = reader.ReadBoolean();
    }

    public override void AI() {
        if (!_init) {
            _passedPositions ??= new PassedPositionInfo[TRAILPOSITIONCOUNT];
            _init = true;
        }

        NPC.dontTakeDamage = IsHiding;

        ApplySnailAI();
    }

    private void Attack() {
        _shouldBeAttacking = true;

        NPC.netUpdate = true;
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
        _hideFactor = -CANTHIDETIMEINWHATAEVER;
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

    private void TryToAttackOverTime() {
        //if (_playAppearAfterHidingAnimation || _shouldHide || _hideFactor < -CANTHIDETIMEINWHATAEVER / 2f) {
        //    return;
        //}

        //if (!NPC.IsGrounded()) {
        //    return;
        //}

        //if (_shouldBeAttacking) {
        //    if (_shouldAttack) {
        //        return;
        //    }

        //    ResetTargetTimeValues();

        //    _speedXFactor = 0f;
        //    _playMoveAnimation = false;
        //    _shouldAttack = true;
        //    _playAttackAnimation = true;

        //    _shouldBeAttacking = false;

        //    _hideFactor = -CANTHIDETIMEINWHATAEVER;

        //    ResetFrame();

        //    return;
        //}
        //if (Helper.SinglePlayerOrServer) {
        //    if (Main.rand.NextBool(200)) {
        //        Attack();
        //    }
        //}
    }

    private bool HasTargetLine() => Collision.CanHitLine(NPC.Center, 0, 0, NPC.GetTargetPlayer().Center, 0, 0);

    private void GetVelocitySpeeds(out float speedX, out float speedY) {
        float hideFactor = MathUtils.Clamp01(1f - _hideFactor);
        if (_playAppearAfterHidingAnimation) {
            hideFactor = 0f;
        }

        _playMoveAnimation = true;

        float add = 0.75f * hideFactor;
        _speedXFactor += add;
        if (_speedXFactor > MAXTIMETOSPEEDUP) {
            _speedXFactor = 0f;

            //_playMoveAnimation = false;
            FrameCounter = 0.0;
        }
        float progress = Ease.CubeInOut(Utils.GetLerpValue(MINTIMETOSPEEDUP, MAXTIMETOSPEEDUP, _speedXFactor, true));

        speedX = 1f/* + 0.5f * progress*/;
        speedY = 1f/* + 0.5f * progress*/;

        speedX *= hideFactor;
        speedY *= hideFactor;
    }

    private void UpdateHideState() {
        //if (!_playMoveAnimation && _speedXFactor > MINTIMETOSPEEDUP) {
        //    _playMoveAnimation = true;
        //}

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
        Vector2 passedPosition = NPC.Center + new Vector2(0.5f * -FacedDirection, 1.25f).RotatedBy(NPC.rotation) * NPC.height / 2f;
        Point16 passedPositionInTiles = passedPosition.ToTileCoordinates16();

        if (NPC.SpeedX() > 0.1f && NPC.IsGrounded()) {
            if (Main.rand.NextBool(3)) {
                if (!Main.rand.NextBool(3)) {
                    int num242 = Utils.SelectRandom<int>(Main.rand, 4);
                    Dust dust38 = Main.dust[Dust.NewDust(passedPosition - Vector2.UnitY * 8f + Vector2.UnitX * Main.rand.NextFloat(16f) * -FacedDirection - Vector2.UnitX * Main.rand.NextFloat(36f) * -FacedDirection, 0, 8, num242, 0f, 0f, 150)];
                    dust38.scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                    dust38.fadeIn = 1f;
                    dust38.velocity *= 0.1f;
                    dust38.noGravity = true;
                    Dust dust2 = dust38;
                    dust38.noLight = true;
                    if (dust38.type == 4)
                        dust38.color = new Color(71, 107, 95, 150);
                }
            }
        }

        if (WorldGenHelper.SolidTile(passedPositionInTiles.ToPoint()) && WorldGenHelper.GetTileSafely(passedPositionInTiles).HasUnactuatedTile && !_passedPositions.Any(checkInfo => checkInfo.Position == passedPositionInTiles)) {
            if (_passedPositions[_passedPositionNextIndex].Opacity != 0f) {
                Point16 position = _passedPositions[_passedPositionNextIndex].Position;
                float rotation = _passedPositions[_passedPositionNextIndex].Rotation;
                Vector2 worldPosition = position.ToWorldCoordinates() - Vector2.UnitY.RotatedBy(rotation) * 6f;
                if (WorldGenHelper.GetTileSafely(position).IsHalfBlock) {
                    worldPosition.Y += 8f;
                }
                for (int i = 0; i < 12; i++) {
                    int num242 = Utils.SelectRandom<int>(Main.rand, 4, 256);
                    Dust dust38 = Main.dust[Dust.NewDust(worldPosition - Vector2.UnitY * 8f - Vector2.UnitX * 8f, 24, 16, num242, 0f, 0f, 175)];
                    dust38.scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                    dust38.fadeIn = 0.5f;
                    dust38.velocity *= 0.25f;
                    dust38.noGravity = true;
                    Dust dust2 = dust38;
                    dust38.noLight = true;
                    if (dust38.type == 4)
                        dust38.color = new Color(71, 107, 95, 150);
                }
            }

            _passedPositions[_passedPositionNextIndex++] = new PassedPositionInfo(passedPositionInTiles, NPC.rotation);

            for (int i = 0; i < 4; i++) {
                if (!Main.rand.NextBool(3)) {
                    continue;
                }
                int num242 = Utils.SelectRandom<int>(Main.rand, 4);
                Dust dust38 = Main.dust[Dust.NewDust(passedPosition - Vector2.UnitY * 8f + Vector2.UnitX * Main.rand.NextFloat(16f) * -FacedDirection, 0, 16, num242, 0f, 0f, 150)];
                dust38.scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                dust38.fadeIn = 1f;
                dust38.velocity *= 0.1f;
                dust38.noGravity = true;
                Dust dust2 = dust38;
                dust38.noLight = true;
                if (dust38.type == 4)
                    dust38.color = new Color(71, 107, 95, 150);
            }

            if (_passedPositionNextIndex > _passedPositions.Length - 1) {
                _passedPositionNextIndex = 0;
            }
        }
        for (int i = 0; i < _passedPositions.Length; i++) {
            ref PassedPositionInfo passedPositionInfo = ref _passedPositions[i];
            passedPositionInfo.Opacity = Helper.Approach(_passedPositions[i].Opacity, 1f, 0.1f);
        }

        TargetOverTime();
        TryToHideOverTime();
        TryToAttackOverTime();
        UpdateHideState();

        if (_shouldAttack) {
            NPC.velocity.X *= 0.8f;
            _speedXFactor++;

            TargetClosestPlayer();

            if (_speedXFactor >= 120f) {
                _speedXFactor = 0f;

                ResetFrame();

                _shouldAttack = _playAttackAnimation = false;
            }
        }

        void applyAdjustedVanillaSnailAI() {
            if (_shouldAttack) {
                return;
            }

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

        foreach (Player target in Main.ActivePlayers) {
            Rectangle targetRect = target.getRect();
            targetRect.Inflate(10, 10);
            if (_passedPositions.Any(checkInfo => targetRect.Contains(checkInfo.Position.ToWorldCoordinates().ToPoint()))) {
                target.AddBuff<ElderSnailSlow>(2);
            }
        }

        bool flag28 = true;
        int num585 = 0;
        if (NPC.velocity.X < 0f)
            num585 = -1;

        if (NPC.velocity.X > 0f)
            num585 = 1;

        Vector2 vector72 = NPC.position;
        vector72.X += NPC.velocity.X;
        int num586 = (int)((vector72.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * num585)) / 16f);
        int num587 = (int)((vector72.Y + (float)NPC.height - 1f) / 16f);
        if ((float)(num586 * 16) < vector72.X + (float)NPC.width && (float)(num586 * 16 + 16) > vector72.X) {
            Tile tileSafely = Framing.GetTileSafely(num586, num587 - 4);
            Tile tileSafely2 = Framing.GetTileSafely(num586 - num585, num587 - 3);
            Tile tileSafely3 = Framing.GetTileSafely(num586, num587 - 3);
            Tile tileSafely4 = Framing.GetTileSafely(num586, num587 - 2);
            Tile tileSafely5 = Framing.GetTileSafely(num586, num587 - 1);
            Tile tileSafely6 = Framing.GetTileSafely(num586, num587);
            if (((tileSafely6.HasUnactuatedTile && !tileSafely6.TopSlope && !tileSafely5.TopSlope && ((Main.tileSolid[tileSafely6.TileType] && !Main.tileSolidTop[tileSafely6.TileType]) || (flag28 && Main.tileSolidTop[tileSafely6.TileType] && (!Main.tileSolid[tileSafely5.TileType] || !tileSafely5.HasUnactuatedTile) && tileSafely6.TileType != 16 && tileSafely6.TileType != 18 && tileSafely6.TileType != 134))) || (tileSafely5.IsHalfBlock && tileSafely5.HasUnactuatedTile)) && (!tileSafely5.HasUnactuatedTile || !Main.tileSolid[tileSafely5.TileType] ||
                Main.tileSolidTop[tileSafely5.TileType] || (tileSafely5.IsHalfBlock && (!tileSafely.HasUnactuatedTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType]))) &&
                (!tileSafely4.HasUnactuatedTile || !Main.tileSolid[tileSafely4.TileType] || Main.tileSolidTop[tileSafely4.TileType]) && 
                (!tileSafely3.HasUnactuatedTile || !Main.tileSolid[tileSafely3.TileType] || Main.tileSolidTop[tileSafely3.TileType]) &&
                (!tileSafely2.HasUnactuatedTile || !Main.tileSolid[tileSafely2.TileType] || Main.tileSolidTop[tileSafely2.TileType])) {
                float num588 = num587 * 16;
                if (tileSafely6.IsHalfBlock)
                    num588 += 8f;

                if (tileSafely5.IsHalfBlock)
                    num588 -= 8f;

                if (num588 < vector72.Y + (float)NPC.height) {
                    float num589 = vector72.Y + (float)NPC.height - num588;
                    if ((double)num589 <= 16.1) {
                        NPC.gfxOffY += NPC.position.Y + (float)NPC.height - num588;
                        NPC.position.Y = num588 - (float)NPC.height;
                        if (num589 < 9f)
                            NPC.stepSpeed = 0.75f;
                        else
                            NPC.stepSpeed = 1.5f;
                    }
                }
            }
        }
    }

    public override void FindFrame(int frameHeight) {
        ushort frameHeight2 = (ushort)frameHeight;

        byte moveFrameCount = 3,
             hideFrameCount = 3,
             attackFrameCount = 4;

        double perFrameCounter = 8.0,
               perFrameCounter_Attack = 4.0;
        double moveFrameCounter = 12.0;
        if (_shouldAttack) {
            byte startFrame = (byte)(moveFrameCount + hideFrameCount);
            NPC.SetFrame(startFrame, frameHeight2);
            byte frame = (byte)(FrameCounter / perFrameCounter_Attack);
            if (_playAttackAnimation) {
                if (FrameCounter < perFrameCounter_Attack * attackFrameCount) {
                    if (FrameCounter == 0.0) {
                        NPC.SetFrame(startFrame, frameHeight2);
                    }
                    FrameCounter++;
                }
                else {
                    frame = (byte)(startFrame + attackFrameCount - 1);
                    NPC.SetFrame(frame, frameHeight2);

                    FrameCounter = 0.0;
                    _playAttackAnimation = false;

                    return;
                }
                frame = (byte)(startFrame + frame);
                NPC.SetFrame(frame, frameHeight2);
            }
            else {
                if (FrameCounter < perFrameCounter_Attack * attackFrameCount) {
                    if (FrameCounter == 0.0) {
                        NPC.SetFrame(startFrame, frameHeight2);
                    }
                    FrameCounter++;
                }
                else {
                    frame = startFrame;
                    NPC.SetFrame(frame, frameHeight2);

                    FrameCounter = 0.0;
                    _playAttackAnimation = true;

                    return;
                }
                frame = (byte)(startFrame + attackFrameCount - frame - 1);
                NPC.SetFrame(frame, frameHeight2);
            }

            return;
        }
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
            byte frame = (byte)(FrameCounter / moveFrameCounter);
            if (FrameCounter < moveFrameCounter * moveFrameCount) {
                FrameCounter++;
            }
            else {
                frame = 0;
                FrameCounter = 0.0;
            }
            NPC.SetFrame(frame, frameHeight2);

            return;
        }

        NPC.SetFrame(0, frameHeight2);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (AssetInitializer.TryGetRequestedTextureAssets<ElderSnail>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            if (_init) {
                foreach (PassedPositionInfo passedPositionInfo in _passedPositions) {
                    Point16 position = passedPositionInfo.Position;
                    float rotation = passedPositionInfo.Rotation;
                    Vector2 worldPosition = position.ToWorldCoordinates() - Vector2.UnitY.RotatedBy(rotation) * 6f;
                    if (WorldGenHelper.GetTileSafely(position).IsHalfBlock) {
                        worldPosition.Y += 8f;
                    }
                    ulong seed = (((ulong)worldPosition.X << 32) | (uint)worldPosition.Y);
                    byte a = 150;
                    Color color = new(a, a, a, a);
                    if (_shouldAttack) {
                        color = Color.Lerp(color, Color.Lerp(color, Color.Red, 0.5f), 0.5f);
                    }
                    for (int i = 0; i < 4; i++) {
                        Vector2 scale = new(1.5f, 1f);
                        Texture2D texture = indexedTextureAssets[(byte)Utils.RandomInt(ref seed, 0, 3)].Value;
                        Rectangle clip = texture.Bounds;
                        Vector2 origin = clip.Centered();
                        spriteBatch.Draw(texture, worldPosition + new Vector2(Utils.RandomInt(ref seed, -2, 3), Utils.RandomInt(ref seed, -1, 2)), DrawInfo.Default with {
                            Clip = clip,
                            Scale = scale,
                            Origin = origin,
                            Color = color.MultiplyRGB(drawColor) with { A = a } * passedPositionInfo.Opacity,
                            Rotation = rotation
                        });
                    }
                }
            }
        }

        Vector2 tempPosition = NPC.position;
        NPC.position += Vector2.UnitY.RotatedBy(NPC.rotation) * -3f;

        SpriteEffects flip = FacedDirection.ToSpriteEffects();
        NPC.QuickDraw(spriteBatch, screenPos, drawColor, effect: flip);

        NPC.position = tempPosition;

        return false;
    }
}
