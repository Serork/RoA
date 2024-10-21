using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class DruidSoul : RoANPC {
    private const float OPACITYACC = 0.005f;
    private const float VELOCITYY = 0.25f;

    private Vector2 _velocity2, _velocity3;

    public override bool PreAI() {
        KillNPCIfIsntInBackwoods();

        return base.PreAI();
    }

    public override void AI() {
        if (Appearance()) {
            return;
        }

        UpdatePositionsAndRotation();
        NormalBehaviourHandler();
        AbsorbSoulHandler();
    }

    private bool Appearance() {
        if (StateTimer < 1f) {
            StateTimer += TimeSystem.LogicDeltaTime / 2f;
            float factor = Helper.EaseInOut3(StateTimer);
            NPC.Opacity = Math.Min(1f, factor / 2f);

            return true;
        }

        return false;
    }

    public bool ConsumesItself() {
        Vector2 altarPosition = GetAltarPosition();
        Vector2 npcCenter = NPC.Center;
        bool closeToAltar = Math.Abs(altarPosition.X - npcCenter.X) < 40f && altarPosition.Y - npcCenter.Y < 80f;
        Player player = Main.player[NPC.target];
        bool flag = NPC.Distance(altarPosition) <= 150f && Collision.CanHit(NPC.Center, 2, 2, altarPosition, 2, 2);
        bool altarCondition = (Math.Abs(NPC.Center.X - altarPosition.X) < 65f && player.Distance(altarPosition) < 100f) || (flag && (player.Distance(NPC.Center) < 125f));
        float altarStrength = AltarHandler.GetAltarStrength();
        return Helper.EaseInOut3(altarStrength) > 0.4f || NPC.Opacity <= 0.05f || (altarCondition && flag && closeToAltar && Collision.CanHit(player.Center, 0, 0, altarPosition - Vector2.One * 1f, 2, 2));
    }

    private static Vector2 GetAltarPosition() => AltarHandler.GetAltarPosition().ToWorldCoordinates() - Vector2.UnitX * 5f;

    private void KillNPCIfIsntInBackwoods() {
        int target = NPC.target;
        if (target == 255 || target < 0 || Main.player[target].dead || !Main.player[target].active) {
            NPC.TargetClosest(false);
        }
        target = NPC.target;
        Player player = Main.player[target];
        if (player.dead || !player.InModBiome<BackwoodsBiome>() || NPC.CountNPCS(Type) > 1 || !NPC.downedBoss2) {
            NPC.KillNPC();
            if (Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.2f, Volume = 0.5f }, NPC.Center);
            }
        }
    }

    private void UpdatePositionsAndRotation() {
        float rotation = NPC.velocity.X * 0.05f;
        NPC.rotation = rotation;

        for (int i = 0; i < NPC.oldPos.Length; i++) {
            float randomOffset = Helper.Wave(0.25f, 1.5f, Main.rand.NextFloat(1f, 3f), 0.5f);
            Vector2 randomness = Main.rand.Random2(randomOffset);
            NPC.oldPos[i] += randomness;
        }
    }

    private void NormalBehaviourHandler() {
        //if (ConsumesItself()) {
        //    return;
        //}

        Movement();

        if (NPC.Opacity <= 0.05f && Helper.EaseInOut3(AltarHandler.GetAltarStrength()) > 0.925f) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.AmbientSounds + "AltarEcho") { Volume = 0.45f }, AltarHandler.GetAltarPosition().ToWorldCoordinates());

            PunchCameraModifier punchCameraModifier = new PunchCameraModifier(NPC.Center, ((Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2()), 12f, 20f, 10, 1000f, "Druid Soul");
            Main.instance.CameraModifiers.Add(punchCameraModifier);
            //OvergrownCoords.Strength = OvergrownCoords.Factor = 1f;
            //NPC.SetEventFlagCleared(ref LothorInvasion.preArrivedLothorBoss.Item1, -1);
            //if (Main.netMode == NetmodeID.Server) {
            //    NetMessage.SendData(MessageID.WorldData);
            //}
            NPC.KillNPC();
            return;
        }

        if (!ConsumesItself()) {
            float minOpacity = 0.4f;
            if (NPC.Opacity > minOpacity) {
                NPC.Opacity -= OPACITYACC;
            }
        }
    }

    private void Movement() {
        bool flag2 = Helper.EaseInOut3(AltarHandler.GetAltarStrength()) > 0.65f;

        if (ConsumesItself() || flag2) {
            return;
        }

        Player player3 = Main.player[NPC.target];
        float num66 = 0.1f;
        NPC.noTileCollide = true;
        int num67 = 150;
        Vector2 center = NPC.Center;
        Vector2 playerCenter = player3.Center + Main.rand.Random2(player3.width);
        float num68 = playerCenter.X - center.X;
        float num69 = playerCenter.Y - center.Y;
        //num69 -= 65f;
        num68 -= (float)(30 * player3.direction);
        float num70 = (float)Math.Sqrt(num68 * num68 + num69 * num69);
        float num71 = 8f;
        float num72 = num70;
        float num73 = 2000f;

        MoveToAltar();

        float dist = NPC.Distance(playerCenter);
        if (dist < 100f) {
            NPC.velocity *= 0.925f;
            if (NPC.velocity.Length() < 0.25f)
                NPC.velocity = Vector2.Zero;
        }

        bool num74 = num70 > num73;
        if (num70 < (float)num67 && player3.velocity.Y == 0f && NPC.position.Y + (float)NPC.height <= player3.position.Y + (float)player3.height && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height) && NPC.velocity.Y < -6f)
            NPC.velocity.Y = -6f;

        if (num70 < 10f) {
            NPC.velocity *= 0.9f;
            if (NPC.velocity.Length() < 0.5f)
                NPC.velocity = Vector2.Zero;

            num66 = 0f;
        }
        else {
            if (num70 > (float)num67) {
                num66 = 0.2f;
                num71 = 12f;
            }

            num70 = num71 / num70;
            num68 *= num70;
            num69 *= num70;
        }

        if (num74) {
            NPC.Center = player3.Center;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        float speed = 4f;
        if (NPC.velocity.X < num68) {
            if (NPC.velocity.X < speed) {
                NPC.velocity.X += num66;
            }
            if (NPC.velocity.X < 0f)
                NPC.velocity.X += num66;
        }

        if (NPC.velocity.X > num68) {
            if (NPC.velocity.X > -speed) {
                NPC.velocity.X -= num66;
            }
            if (NPC.velocity.X > 0f)
                NPC.velocity.X -= num66;
        }

        if (NPC.velocity.Y < num69) {
            if (NPC.velocity.Y < speed) {
                NPC.velocity.Y += num66;
            }
            if (NPC.velocity.Y < 0f)
                NPC.velocity.Y += num66;
        }

        if (NPC.velocity.Y > num69) {
            if (NPC.velocity.Y > -speed) {
                NPC.velocity.Y -= num66;
            }
            if (NPC.velocity.Y > 0f)
                NPC.velocity.Y -= num66;
        }

        //if (Math.Abs(NPC.velocity.X) > 0.1f) {
        //    NPC.direction = NPC.velocity.X.GetDirection();
        //}
        center = NPC.Center + Vector2.UnitX * 8f;
        if (Math.Abs(player3.Center.X - center.X) > 8f) {
            NPC.direction = (player3.Center.X - center.X).GetDirection();
            NPC.spriteDirection = -NPC.direction;
        }
    }

    private void MoveToAltar() {
        Player player3 = Main.player[NPC.target];
        Vector2 altarCoords = GetAltarPosition();
        if (NPC.Distance(altarCoords) < 45f || player3.Center.Y < NPC.Center.Y) {
            NPC.velocity.Y -= VELOCITYY * 0.75f;
        }
        float distanceBetween = Math.Clamp(NPC.Distance(altarCoords), 0.01f, 600f) * 0.01f;
        Vector2 moveTo = NPC.DirectionFrom(altarCoords);
        Rectangle altarArea = Utils.CenteredRectangle(new Vector2((float)(altarCoords.X), (float)(altarCoords.Y)), Vector2.One * 5f);
        float speed = 0.1f + distanceBetween * 0.0007f;
        _velocity2 += moveTo * -speed;
        _velocity3 = NPC.CircleMovementVector2(++NPC.ai[1] / 3f);
        _velocity2 += NPC.DirectionTo(altarArea.ClosestPointInRect(NPC.Center)) * speed;
        float maxSpeed = 2.5f + distanceBetween * 0.005f;
        float velocity = _velocity2.Length();
        if (velocity > maxSpeed) {
            _velocity2 *= maxSpeed / _velocity2.Length();
        }
        NPC.position += _velocity2 + _velocity3;
    }

    private void AbsorbSoulHandler() {
        float altarStrength = AltarHandler.GetAltarStrength();
        Vector2 altarPosition = GetAltarPosition();
        Vector2 towards = altarPosition + new Vector2(0f, 40f);
        bool flag2 = Helper.EaseInOut3(altarStrength) > 0.65f;
        if (flag2) {

        }
        if (!ConsumesItself()) {
            return;
        }
        bool flag = Helper.EaseInOut3(altarStrength) > 0.4f;
        bool flag3 = Helper.EaseInOut3(altarStrength) > 0.1f;
        bool flag4 = Helper.EaseInOut3(altarStrength) > 0.02f;
        _velocity2 *= 0.925f;
        //NPC.velocity *= 0.925f;
        if ((altarPosition.Y > NPC.Center.Y && !flag) || flag) {
            Vector2 towards2 = towards + Vector2.UnitX * 8f;
            if (Math.Abs(towards2.X - NPC.Center.X) > 8f) {
                NPC.spriteDirection = -NPC.direction;
                NPC.direction = NPC.DirectionTo(towards2).X.GetDirection();
            }

            if (!flag2) {
                _velocity3 = NPC.CircleMovementVector2(++NPC.ai[1] / 3f, 0.3f, 10);
                Helper.InertiaMoveTowards(ref _velocity2, NPC.Center, towards);
                _velocity2 *= 0.5f;
                _velocity3.Y *= 0.9f;
                NPC.position += _velocity2 + _velocity3;
                if (Math.Abs(NPC.Center.X - towards.X) < 50f) {
                    NPC.velocity.Y -= VELOCITYY / 2f;
                }
                if (NPC.Distance(altarPosition) > 60f || Math.Abs(NPC.Center.X - towards.X) >= 60f) {
                    NPC.velocity.Y -= VELOCITYY * 0.75f;
                    NPC.SlightlyMoveTo(towards);
                }
                NPC.velocity.Y -= VELOCITYY / 2f;
                if (flag3) {
                    if (NPC.Opacity < 0.8f) {
                        NPC.Opacity += OPACITYACC * 0.75f;
                    }
                }
                else {

                }
                NPC.velocity.Y *= 0.9f;
                int max = 4;
                if (Main.GameUpdateCount % 10 == 0) {
                    if (NPC.ai[2] < max) {
                        NPC.ai[2]++;
                    }
                }
                int count = (int)(max - NPC.ai[2]);
                if (flag4) {
                    if (Main.rand.NextBool(3 + (int)((5 + count) * MathHelper.Clamp(1f - altarStrength * 1.5f - NPC.scale, 0f, 1f)))) {
                        Vector2 center = NPC.position + new Vector2(3f + Main.rand.Next(NPC.width - 3), NPC.height / 2f + 8f);
                        center.X += Main.rand.Next(-100, 100) * 0.05f;
                        center.Y += Main.rand.Next(-100, 100) * 0.05f;
                        Vector2 position = center + NPC.velocity;
                        //velocity *= MathHelper.Clamp(altarStrength + 0.5f, 0.5f, 1f);
                        VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDTILESBEHINDNPCS).
                                SetupPart(1,
                                        Vector2.Zero,
                                        position,
                                        towards + Main.rand.Random2(15f) + Vector2.UnitY * 10f,
                                        Main.rand.Next(70, 85) * Main.rand.NextFloat(0.01f, 0.015f),
                                        0.8f);
                    }
                }
            }
            if (flag) {
                if (NPC.Opacity > 0.025f) {
                    NPC.Opacity -= OPACITYACC * 1.615f;
                }
                //NPC.velocity *= 0.1f;
                //NPC.SlightlyMoveTo(towards, 4f, 10f);
            }
            if (NPC.velocity.Length() < 2.5f) {
                if (Main.rand.NextBool(2 + (int)(7 * MathHelper.Clamp(1f - altarStrength * 1.25f - NPC.scale, 0.25f, 1f)))) {
                    Vector2 center = NPC.position + new Vector2(6f + Main.rand.Next(NPC.width - 6), NPC.height / 2f + 10f);
                    center.X += Main.rand.Next(-100, 100) * 0.05f;
                    center.Y += Main.rand.Next(-100, 100) * 0.05f;
                    VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDNPCS).
                            SetupPart(0,
                                    Vector2.Zero,
                                    center,
                                    towards,
                                    Main.rand.Next(70, 85) * 0.01f,
                                    NPC.Opacity + 0.15f);
                }
            }
            if (flag2) {
                if (NPC.scale > 0f) {
                    NPC.scale -= OPACITYACC * 0.5f;
                }
                _velocity2 *= 0.8f;
                _velocity3 *= 0.8f;
                NPC.velocity *= 0.8f;
                NPC.SlightlyMoveTo(towards, 4f, 10f);
                if (NPC.Opacity > 0.025f) {
                    NPC.Opacity -= OPACITYACC * 1.615f;
                }
                return;
            }
            return;
        }

        //SpawnEffects();
    }

    private void SpawnEffects() {
        Vector2 towards = GetAltarPosition() + new Vector2(4f, 40f);
        if (Main.netMode != NetmodeID.Server) {
            if (!Main.rand.NextBool(2)) {
                Vector2 center = NPC.position + new Vector2(3f + Main.rand.Next(NPC.width - 3), NPC.height / 2f + 8f);
                center.X += Main.rand.Next(-100, 100) * 0.05f;
                center.Y += Main.rand.Next(-100, 100) * 0.05f;
                VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDNPCS).
                      SetupPart(1,
                                Vector2.Zero,
                                center + NPC.velocity,
                                towards + Main.rand.Random2(15f),
                                Main.rand.Next(70, 85) * Main.rand.NextFloat(0.01f, 0.015f),
                                0.8f);
            }
            if (!Main.rand.NextBool(3)) {
                Vector2 center = NPC.position + new Vector2(6f + Main.rand.Next(NPC.width - 6), NPC.height / 2f + 10f);
                center.X += Main.rand.Next(-100, 100) * 0.05f;
                center.Y += Main.rand.Next(-100, 100) * 0.05f;
                VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDNPCS).
                      SetupPart(0,
                                Vector2.Zero,
                                center + NPC.velocity,
                                towards,
                                Main.rand.Next(70, 85) * 0.01f,
                                NPC.Opacity + 0.15f);
            }
        }
    }
}