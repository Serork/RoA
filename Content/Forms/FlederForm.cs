using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Players;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class FlederForm : BaseForm {
    private class FlederFormDashHandler : ModPlayer, IDoubleTap {
        public const int CD = 50, DURATION = 35;
        public const float SPEED = 10f;

        private IDoubleTap.TapDirection _dashDirection;
        private float _dashDelay, _dashTimer;
        private int[] _localNPCImmunity = new int[200];

        public bool ActiveDash => _dashDelay > 0;

        void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
            bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
            if (!flag) {
                return;
            }
            if (!player.GetModPlayer<BaseFormHandler>().Is<FlederForm>()) {
                return;
            }

            player.GetModPlayer<FlederFormDashHandler>().UseFlederDash(direction);
        }



        public override void PreUpdateMovement() {
            if (_dashDirection != IDoubleTap.TapDirection.None && !ActiveDash) {
                Vector2 newVelocity = Player.velocity;
                int dashDirection = (_dashDirection == IDoubleTap.TapDirection.Right).ToDirectionInt();
                switch (_dashDirection) {
                    case IDoubleTap.TapDirection.Left:
                    case IDoubleTap.TapDirection.Right: {
                        newVelocity.X = dashDirection * SPEED;
                        break;
                    }
                }
                _dashDirection = IDoubleTap.TapDirection.None;
                _dashDelay = CD;
                _dashTimer = DURATION;
                Player.velocity = newVelocity;
                if (Player.velocity.Y == Player.gravity) {
                    Player.velocity.Y -= 5f;
                }
                Point tileCoordinates1 = (Player.Center + new Vector2((dashDirection * Player.width / 2 + 2), (float)(Player.gravDir * -Player.height / 2.0 + Player.gravDir * 2.0))).ToTileCoordinates();
                Point tileCoordinates2 = (Player.Center + new Vector2((dashDirection * Player.width / 2 + 2), 0.0f)).ToTileCoordinates();
                if (WorldGen.SolidOrSlopedTile(tileCoordinates1.X, tileCoordinates1.Y) || WorldGen.SolidOrSlopedTile(tileCoordinates2.X, tileCoordinates2.Y)) {
                    Player.velocity.X /= 2f;
                }
            }

            if (ActiveDash) {
                _dashDelay--;
            }

            if (_dashTimer > 0) {
                for (int k = 0; k < 200; k++) {
                    if (_localNPCImmunity[k] > 0) {
                        _localNPCImmunity[k]--;
                    }
                }
                Player.eocDash = (int)_dashTimer;
                Player.armorEffectDrawShadowEOCShield = true;
                if (Player.velocity.Length() > 5f) {
                    Rectangle rectangle = new((int)((double)Player.position.X + (double)Player.velocity.X * 0.5 - 4.0), (int)((double)Player.position.Y + (double)Player.velocity.Y * 0.5 - 4.0), Player.width + 8, Player.height + 8);
                    for (int i = 0; i < 200; i++) {
                        NPC nPC = Main.npc[i];
                        if (!nPC.active || nPC.dontTakeDamage || nPC.friendly || (nPC.aiStyle == 112 && !(nPC.ai[2] <= 1f)) || !Player.CanNPCBeHitByPlayerOrPlayerProjectile(nPC))
                            continue;

                        if (_localNPCImmunity[i] > 0) {
                            continue;
                        }

                        Rectangle rect = nPC.getRect();
                        if (rectangle.Intersects(rect) && (nPC.noTileCollide || Player.CanHit(nPC))) {
                            int damage = 10;
                            float num = Player.GetTotalDamage(DruidClass.NatureDamage).ApplyTo(damage);
                            float num2 = 3f;
                            bool crit = false;

                            if (Main.rand.Next(100) < (4 + Player.GetTotalCritChance(DruidClass.NatureDamage)))
                                crit = true;

                            int num3 = Player.direction;
                            if (Player.velocity.X < 0f)
                                num3 = -1;

                            if (Player.velocity.X > 0f)
                                num3 = 1;

                            if (Player.whoAmI == Main.myPlayer)
                                Player.ApplyDamageToNPC(nPC, (int)num, num2, num3, crit, DruidClass.NatureDamage, true);

                            _dashTimer = DURATION;
                            _dashDelay = CD;
                            //Player.velocity.X = -num3 * 9;
                            //Player.velocity.Y = -4f;
                            Player.velocity *= 0.9f;
                            Player.GiveImmuneTimeForCollisionAttack(4);
                            _localNPCImmunity[i] = 10;
                            //Player.immune = true;
                            //Player.immuneTime = 10;
                            //Player.immuneNoBlink = true;
                        }
                    }
                }
                //if ((!Player.controlLeft || !(Player.velocity.X < 0f)) && (!Player.controlRight || !(Player.velocity.X > 0f))) {
                //    Player.velocity.X *= 0.95f;
                //}
                _dashTimer--;
            }
        }

        private void UseFlederDash(IDoubleTap.TapDirection direction) {
            if (ActiveDash) {
                return;
            }

            _dashDirection = direction;
        }
    }

    private static bool IsFlying(Player player) {
        bool onTile = Math.Abs(player.velocity.Y) < 1.25f && WorldGenHelper.SolidTile((int)player.Center.X / 16, (int)(player.Bottom.Y + 10f) / 16);
        return !player.sliding && !onTile && player.gfxOffY == 0f;
    }

    protected override float GetMaxSpeedMultiplier(Player player) => 1f;
    protected override float GetRunAccelerationMultiplier(Player player) => 1.5f;

    protected override void SafeSetDefaults() {
        MountData.totalFrames = 8;
        MountData.spawnDust = 59;
        MountData.heightBoost = -14;
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 60;
        MountData.fatigueMax = 40;
        MountData.jumpHeight = 1;
        MountData.jumpSpeed = 4f;

        if (!Main.dedServ) {
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    protected override void SafeUpdateEffects(Player player) {
        player.npcTypeNoAggro[ModContent.NPCType<Fleder>()] = true;
        player.npcTypeNoAggro[ModContent.NPCType<BabyFleder>()] = true;
        player.statDefense -= 6;

        float rotation = player.velocity.X * (IsFlying(player) ? 0.2f : 0.15f);
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        bool flag = IsFlying(player);
        float maxRotation = flag ? 0.3f : 0.2f;
        fullRotation = MathHelper.Clamp(fullRotation, -maxRotation, maxRotation);
        if (flag) {
            float maxFlightSpeedX = Math.Min(3.5f, player.maxRunSpeed * 0.75f);
            float acceleration = player.runAcceleration / 2f;
            if (player.velocity.X > maxFlightSpeedX) {
                player.velocity.X -= acceleration;
            }
            if (player.velocity.X < -maxFlightSpeedX) {
                player.velocity.X += acceleration;
            }
        }
        if (player.GetModPlayer<FlederFormDashHandler>().ActiveDash) {
            player.fullRotation = Utils.AngleLerp(player.fullRotation, fullRotation, 0.25f);
        }
        else {
            player.fullRotation = fullRotation;
        }
        player.gravity *= 0.75f;
        player.velocity.Y = Math.Min(5f, player.velocity.Y);
        player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2 - 6f);
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int minFrame = 4, maxFrame = 7;
        float flightFrameFrequency = 14f, walkingFrameFrequiency = 16f;
        if (IsFlying(player)) {
            if (player.velocity.Y < 0f) {
                frameCounter += Math.Abs(player.velocity.Y) * 0.5f;
                float frequency = flightFrameFrequency;
                while (frameCounter > frequency) {
                    frameCounter -= frequency;
                    frame++;
                }
                if (frame < minFrame || frame > maxFrame) {
                    frame = minFrame;
                }
            }
            else if (player.velocity.Y > 0f) {
                frame = maxFrame;
            }
        }
        else if (player.velocity.X != 0f) {
            frameCounter += Math.Abs(player.velocity.X) * 1.5f;
            float frequency = walkingFrameFrequiency;
            while (frameCounter > frequency) {
                frameCounter -= frequency;
                frame++;
            }
            if (frame > 3) {
                frame = 0;
            }
        }
        else {
            frameCounter = 0f;
            frame = 0;
        }

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center + new Vector2(30f, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }

    public override void Dismount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center - new Vector2(30f, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }
}