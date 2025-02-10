﻿using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using ReLogic.Utilities;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;

sealed partial class DruidSoul : RoANPC {
    private const float OPACITYACC = 0.00475f;
    private const float VELOCITYY = 0.25f;

    private Vector2 _velocity, _velocity2, _velocity3, _velocity4;
    private SlotId _lothorSummonSound;
    private bool _lothorSummonSoundPlayed;
    private float _consumeValue;
    private float _canChangeDirectionAgain;

    public bool ShouldConsumeItsEnergy { get; private set; }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_velocity);
        writer.WriteVector2(_velocity2);
        writer.WriteVector2(_velocity3);
        writer.WriteVector2(_velocity4);
        writer.Write(_consumeValue);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _velocity = reader.ReadVector2();
        _velocity2 = reader.ReadVector2();
        _velocity3 = reader.ReadVector2();
        _velocity4 = reader.ReadVector2();
        _consumeValue = reader.ReadSingle();    
    }

    public override void AI() {
        NPC.ShowNameOnHover = NPC.Opacity > 0.38f;

        KillNPCIfIsntInBackwoods();

        Appearance();
        NPC.velocity *= 1f - StateTimer;

        if (ConsumesItself()) {
            ShouldConsumeItsEnergy = true;
        }
        else if (ShouldConsumeItsEnergy) {
            Vector2 altarPosition = GetAltarPosition();
            Player player = Main.player[NPC.target];
            if (player.Distance(altarPosition) > 120f || NPC.Distance(altarPosition) > 120f) {
                ShouldConsumeItsEnergy = false;
            }
        }
        //if (ConsumesItself() && !ShouldConsumeItsEnergy) {
        //    if (StateTimer < 2f) {
        //        StateTimer += TimeSystem.LogicDeltaTime * 5f;
        //    }
        //}
        //else {
        //    StateTimer = 1f;
        //}
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
        Player player = Main.player[NPC.target];
        float altarStrength = AltarHandler.GetAltarStrength();
        bool playerCanReachAltar = player.Distance(altarPosition) < 75f && Collision.CanHit(player.Center, 0, 0, altarPosition, 0, 0);
        bool closeToAltar = (/*playerCanReachAltar || */Math.Abs(altarPosition.X - npcCenter.X) < 40f) && altarPosition.Y - npcCenter.Y < 75f;
        bool flag = NPC.Distance(altarPosition) <= 70f && (playerCanReachAltar || Collision.CanHit(NPC.Center, 2, 2, altarPosition, 2, 2) || NPC.Top.Y + 4f > altarPosition.Y);
        bool flag3 = Helper.EaseInOut3(altarStrength) > 0.0025f;
        bool flag2 = flag3 ? player.Distance(altarPosition) < 75f : player.Distance(NPC.Center) < 70f;
        bool altarCondition = (Math.Abs(NPC.Center.X - altarPosition.X) < 70f && player.Distance(altarPosition) < 75f) || (flag && flag2);
        return NPC.Distance(altarPosition) <= 100f && ((altarCondition && NPC.Center.Y - 4f > altarPosition.Y) || (Helper.EaseInOut3(altarStrength) > 0.4f || NPC.Opacity <= 0.05f || (altarCondition && flag && closeToAltar && (playerCanReachAltar))));
    }

    private static Vector2 GetAltarPosition() => AltarHandler.GetAltarPosition().ToWorldCoordinates() - Vector2.UnitX * 5f;

    private void KillNPCIfIsntInBackwoods() {
        int target = NPC.target;
        if (target == 255 || target < 0 || Main.player[target].dead || !Main.player[target].active) {
            NPC.TargetClosest(false);
        }
        target = NPC.target;
        Player player = Main.player[target];
        if (LothorSummoningHandler.PreArrivedLothorBoss.Item1 || player.dead || !player.InModBiome<BackwoodsBiome>() || NPC.CountNPCS(Type) > 1 || !NPC.downedBoss2) {
            NPC.KillNPC();
            if (Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.2f, Volume = 0.5f }, NPC.Center);
            }
        }
    }

    private void UpdatePositionsAndRotation() {
        Vector2 velocity = NPC.velocity + _velocity + _velocity2 + _velocity3 + _velocity4;
        float rotation = velocity.X * 0.05f;
        NPC.rotation = rotation;

        for (int i = 0; i < NPC.oldPos.Length; i++) {
            float randomOffset = Helper.Wave(0.25f, 1.5f, Main.rand.NextFloat(1f, 3f), 0.5f);
            Vector2 randomness = Main.rand.Random2(randomOffset);
            NPC.oldPos[i] += randomness;
        }
    }

    private void NormalBehaviourHandler() {
        //if (ShouldConsumeItsEnergy) {
        //    return;
        //}

        Movement();

        if (NPC.Opacity <= 0.05f && Helper.EaseInOut3(AltarHandler.GetAltarStrength()) > 0.925f) {
            PunchCameraModifier punchCameraModifier = new(NPC.Center, ((Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2()), 12f, 20f, 10, 2000f, "Druid Soul");
            Main.instance.CameraModifiers.Add(punchCameraModifier);
            //OvergrownCoords.Strength = OvergrownCoords.Factor = 1f;
            //NPC.SetEventFlagCleared(ref LothorInvasion.preArrivedLothorBoss.Item1, -1);
            //if (Main.netMode == NetmodeID.Server) {
            //    NetMessage.SendData(MessageID.WorldData);
            //}
            NPC.SetEventFlagCleared(ref LothorSummoningHandler.PreArrivedLothorBoss.Item1, -1);
            LothorSummoningHandler._summonedNaturally = false;
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DruidSoul2>());
                if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                    NetMessage.SendData(MessageID.SyncNPC, number: npc);
                }
            }
            NPC.KillNPC();
            return;
        }

        if (!ShouldConsumeItsEnergy) {
            float minOpacity = 0.4f;
            if (NPC.Opacity > minOpacity) {
                NPC.Opacity -= OPACITYACC;
            }
        }
    }

    private void MoveToAltar2() {
        Player player = Main.player[NPC.target];
        Vector2 altarPos = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        if (!Collision.CanHit(player, NPC) && !ShouldConsumeItsEnergy) {
            if (_velocity.Y > -2f) {
                _velocity.Y -= 0.05f;
            }
        }
        //if (Collision.CanHit(player.Center, 0, 0, altarPos, 0, 0) && Math.Abs(player.Center.X - altarPos.X) < 130f) {
        //    Vector2 vector = NPC.DirectionTo(altarPos) * 0.25f;
        //    NPC.position += vector;
        //}
        //float altarStrength = AltarHandler.GetAltarStrength();
        //Vector2 altarCoords = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        //if (Collision.CanHit(player3.position, player3.width, player3.height, altarCoords, 0, 0) &&
        //    !Collision.CanHit(NPC.Center - Vector2.One * 2f, 4, 4, altarCoords, 0, 0) &&
        //    (NPC.Top.Y > altarCoords.Y || altarStrength > 0.01f || NPC.Distance(altarCoords) > 30f)) {
        //    Main.NewText(123);
        //    NPC.velocity.Y -= VELOCITYY / 4f;
        //    Vector2 movement = NPC.position - altarCoords;
        //    Vector2 movement2 = movement * (10f / movement.Length());
        //    Vector2 velocity = NPC.velocity + _velocity + _velocity2 + _velocity3 + _velocity4;
        //    NPC.position += (movement2 - velocity) / 15f;
        //    //NPC.velocity *= 0.95f;
        //    //_velocity *= 0.95f;
        //    //_velocity2 *= 0.95f;
        //    //_velocity3 *= 0.95f;
        //    //_velocity4 *= 0.95f;
        //    ////Main.NewText(123);
        //    //NPC.position.X += (altarCoords - NPC.position).SafeNormalize(Vector2.Zero).X * Math.Max(Math.Abs(velocity.X), 1f) * 0.5f * NPC.direction;
        //}
    }

    private void Movement() {
        float altarStrength = AltarHandler.GetAltarStrength();
        bool flag2 = Helper.EaseInOut3(altarStrength) > 0.65f;

        NPC.position += _velocity * (1f - altarStrength);

        Player player3 = Main.player[NPC.target];
        Vector2 altarCoords = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        float dist2 = Math.Min(player3.Distance(altarCoords), NPC.Distance(altarCoords));
        if (/*ShouldConsumeItsEnergy || */flag2 /*|| dist2 > MAXDISTANCETOALTAR*/) {
            //_velocity *= 0.975f;

            return;
        }

        _velocity4 *= 0.975f;

        MoveToAltar2();

        _consumeValue = 0f;
        NPC.ai[2] = NPC.ai[3] = 0f;
        bool flag4 = Helper.EaseInOut3(altarStrength) > 0.0025f;
        if (!flag4) {
            _lothorSummonSoundPlayed = false;
            if (SoundEngine.TryGetActiveSound(_lothorSummonSound, out ActiveSound activeSound)) {
                activeSound.Stop();
            }
        }

        float num66 = 0.1f;
        NPC.noTileCollide = true;
        int num67 = 150;
        Vector2 center = NPC.Center;
        //center.X -= 20 * NPC.direction;
        Vector2 playerCenter = player3.Center/* + Main.rand.Random2(player3.width) * player3.direction*/;
        float num68 = playerCenter.X - center.X;
        int dir = 0;
        if (NPC.direction == 1 && num68 > 0f) {
            dir = -1;
        }
        if (NPC.direction == -1 && num68 < 0f) {
            dir = 1;
        }
        if ((player3.Distance(altarCoords) < 100f || (center.Distance(altarCoords) < 100f && !ShouldConsumeItsEnergy)) && !Collision.CanHit(player3.Center, 0, 0, altarCoords - Vector2.One * 1f, 2, 2)) {
            if (dir == 1) {
                playerCenter.X += player3.width + 20f;
                num68 = playerCenter.X - center.X;
            }
        }
        float num69 = playerCenter.Y - center.Y;
        //num69 -= 65f;
        num68 -= (float)(30 * -dir);
        float num70 = (float)Math.Sqrt(num68 * num68 + num69 * num69);
        float num71 = 8f;
        float num72 = num70;
        float num73 = 2000f;

        MoveToAltar();

        float dist = NPC.Distance(playerCenter);
        if (dist < 100f) {
            _velocity *= 0.925f;
            if (_velocity.Length() < 0.35f)
                _velocity = Vector2.Zero;
        }

        bool num74 = num70 > num73;
        if (num70 < (float)num67 && player3.velocity.Y == 0f && NPC.position.Y + (float)NPC.height <= player3.position.Y + (float)player3.height && _velocity.Y < -6f)
            _velocity.Y = -6f;

        if (num70 < 10f) {
            _velocity *= 0.95f;
            if (_velocity.Length() < 0.5f)
                _velocity = Vector2.Zero;

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
            _velocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        float speed = 4f;
        if (_velocity.X < num68) {
            if (_velocity.X < speed) {
                _velocity.X += num66;
            }
            if (_velocity.X < 0f)
                _velocity.X += num66;
        }

        if (_velocity.X > num68) {
            if (_velocity.X > -speed) {
                _velocity.X -= num66;
            }
            if (_velocity.X > 0f)
                _velocity.X -= num66;
        }
        speed = 2f;
        if (_velocity.Y < num69) {
            if (_velocity.Y < speed) {
                _velocity.Y += num66;
            }
            if (_velocity.Y < 0f)
                _velocity.Y += num66;
        }

        if (_velocity.Y > num69) {
            if (_velocity.Y > -speed) {
                _velocity.Y -= num66;
            }
            if (_velocity.Y > 0f)
                _velocity.Y -= num66;
        }

        //if (Math.Abs(NPC.velocity.X) > 0.1f) {
        //    NPC.direction = NPC.velocity.X.GetDirection();
        //}
        center = NPC.Center;
        Vector2 velocity = NPC.velocity + _velocity + _velocity2 + _velocity3;
        if (Math.Abs(player3.Center.X - center.X) > 8f && Math.Abs(velocity.X) > 0.25f && !ShouldConsumeItsEnergy) {
            NPC.direction = (player3.Center.X - center.X).GetDirection();
            NPC.spriteDirection = -NPC.direction;
        }

        //if (ShouldConsumeItsEnergy) {
        //    NPC.velocity *= 0.925f;
        //}

        NPC.netUpdate = true;
    }

    private void MoveToAltar() {
        Player player3 = Main.player[NPC.target];
        Vector2 altarCoords = GetAltarPosition();
        Vector2 center = NPC.Center;
        Vector2 playerCenter = player3.Center/* + Main.rand.Random2(player3.width) * player3.direction*/;
        float num68 = playerCenter.X - center.X;
        int dir = 0;
        if (NPC.direction == 1 && num68 > -25f) {
            dir = -1;
        }
        if (NPC.direction == -1 && num68 < 25f) {
            dir = 1;
        }
        bool canPlayerReachAltar = Collision.CanHit(player3.Center, 0, 0, altarCoords - Vector2.One * 1f, 2, 2);
        bool flag = (player3.Distance(altarCoords) < 100f || (center.Distance(altarCoords) < 100f && !ShouldConsumeItsEnergy)) && !canPlayerReachAltar;
        //if (flag) {
        //    if (dir == 1) {
        //        playerCenter.X += player3.width + 20f;
        //    }
        //}
        bool playerReachAltar = canPlayerReachAltar && player3.Distance(altarCoords) < 150f /*|| center.Distance(altarCoords) < 100f*/;
        if (player3.Distance(altarCoords) < 200f && playerCenter.Y < NPC.Center.Y && !ShouldConsumeItsEnergy /*|| player3.Center.Y < NPC.Center.Y*/) {
            _velocity.Y -= VELOCITYY * 0.75f;
        }
        float distanceBetween = Math.Clamp(NPC.Distance(altarCoords), 0.01f, 600f) * 0.01f;
        Vector2 moveTo = NPC.DirectionFrom(altarCoords);
        Rectangle altarArea = Utils.CenteredRectangle(new Vector2((float)(altarCoords.X), (float)(altarCoords.Y)), Vector2.One * 2f);
        float speed = 0.1f + distanceBetween * 0.0007f;
        bool flag5 = NPC.Distance(altarCoords) > 50f && !ShouldConsumeItsEnergy;
        if (flag5) {
            _velocity2 += moveTo * -speed;
        }
        else {
            _velocity2 *= 0.95f;
        }
        if (playerCenter.Distance(NPC.Center.MoveTowards(playerCenter, 5f) - _velocity3.SafeNormalize(Vector2.Zero) * 2f) > 15f) {
            NPC.ai[1] += 1 * dir;
            if (!playerReachAltar) {
                if (Main.GameUpdateCount % 15 == 0) {
                    if (Main.rand.NextBool(3)) {
                        NPC.localAI[3] = Main.rand.NextFloat(0.45f, 0.65f);
                    }
                }
                _velocity3 = Vector2.Lerp(_velocity3, NPC.CircleMovementVector2(NPC.ai[1] * NPC.localAI[3] / 3f), 0.1f);
            }
            else {
                _velocity3 = Vector2.Lerp(_velocity3, NPC.CircleMovementVector2(NPC.ai[1] / 3f), 0.6f);
            }
        }
        else {
            _velocity3 *= 0.9f;
            //NPC.position += NPC.DirectionTo(altarCoords) * 0.5f;
        }
        //_velocity3 *= 5f;
        if (flag5) {
            _velocity2 += NPC.DirectionTo(altarArea.ClosestPointInRect(NPC.Center)) * speed;
        }
        else {
            _velocity2 *= 0.95f;
        }
        if (playerReachAltar && NPC.Center.Y > playerCenter.Y) {
            _velocity2.Y -= VELOCITYY / 2f;
        }
        if (playerReachAltar && NPC.Center.Y > altarCoords.Y) {
            _velocity2.Y -= VELOCITYY / 2f;
        }
        if (!playerReachAltar && _velocity2.Y > _velocity.Y && playerCenter.Y < NPC.Center.Y && !ShouldConsumeItsEnergy) {
            _velocity.Y -= _velocity2.Y * 0.025f;
        }
        bool flag3 = Collision.CanHit(player3, NPC) && (player3.Distance(altarCoords) > 225f || (!Collision.CanHit(player3.Center, 0, 0, altarCoords - Vector2.One * 1f, 2, 2) && player3.Distance(altarCoords) < 50f)) /*|| center.Distance(altarCoords) < 100f*/;
        if (flag3 && _velocity3.Y < _velocity2.Y) {
            _velocity3.Y += _velocity2.Y * 0.05f;
        }
        float maxSpeed = 2.5f + distanceBetween * 0.005f;
        float velocity = _velocity2.Length();
        if (velocity > maxSpeed) {
            _velocity2 *= maxSpeed / _velocity2.Length();
        }
        float altarStrength = AltarHandler.GetAltarStrength();
        float inertia = NPC.Distance(altarCoords) * 0.1f;
        float value = (float)Math.Pow(0.97, inertia * 2.0 / inertia);
        _velocity2 *= value;
        _velocity3 *= value;
        NPC.position += (_velocity2 + _velocity3) * (1f - altarStrength);
        Vector2 vector = NPC.DirectionTo(altarCoords) * 0.1f;
        NPC.position += vector;
    }

    private void AbsorbSoulHandler() {
        Player player3 = Main.player[NPC.target];
        float altarStrength = AltarHandler.GetAltarStrength();
        Vector2 altarPosition = GetAltarPosition();
        Vector2 towards = altarPosition + new Vector2(0f, 40f);
        bool flag2 = Helper.EaseInOut3(altarStrength) > 0.65f;
        if (flag2) {

        }
        bool flag10 = altarPosition.Distance(NPC.Center) < 400f;
        if (flag10) {
            float value = altarPosition.Distance(NPC.Center) / 400f;
            float value3 = altarPosition.Distance(NPC.Center);
            float value2 = 5f * (player3.Distance(altarPosition) < 100f ? player3.Distance(altarPosition) / 100f : 0f);
            Vector2 vector = NPC.DirectionTo(altarPosition) * value2 * value;
            NPC.position += vector;
        }
        if (!ShouldConsumeItsEnergy) {
            return;
        }
        MoveToAltar2();
        bool flag = Helper.EaseInOut3(altarStrength) > 0.4f;
        bool flag3 = Helper.EaseInOut3(altarStrength) > 0.1f;
        bool flag4 = Helper.EaseInOut3(altarStrength) > 0.0001f;
        bool flag5 = Helper.EaseInOut3(altarStrength) > 0.6f;
        bool flag7 = Helper.EaseInOut3(altarStrength) > 0.8f;
        _velocity *= 0.9f;
        _velocity2 *= 0.9f;
        _velocity3 *= 0.9f;
        _velocity4 *= 0.9f;
        float value4 = 0.9f - 0.25f * Helper.EaseInOut3(altarStrength);
        _velocity.Y *= value4;
        _velocity2.Y *= value4;
        _velocity3.Y *= value4;
        _velocity4.Y *= value4;
        //NPC.velocity *= 0.925f;
        bool flag6 = altarPosition.Y > NPC.Center.Y;
        /*if ((flag6 && !flag) || flag)*/ {
            //_velocity.Y *= 0.95f;
            Vector2 towards2 = towards + Vector2.UnitX * 6f;
            Vector2 velocity = NPC.velocity + _velocity + _velocity2 + _velocity3 + _velocity4;
            if (Math.Abs(NPC.Center.X - towards2.X) > 2f && --_canChangeDirectionAgain <= 0f) {
                NPC.spriteDirection = -NPC.direction;
                NPC.direction = NPC.DirectionTo(towards2).X.GetDirection();
                _canChangeDirectionAgain = 20f;
            }
            if (!flag2) {
                NPC.velocity.X += (towards - NPC.Center).SafeNormalize(Vector2.Zero).X * 0.1f;
                NPC.velocity.X *= 0.9f;
                float dist = 55f;
                Vector2 pos = altarPosition - Vector2.UnitY * dist;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, Helper.VelocityToPoint(NPC.Center, pos, 1f).Y, 0.075f);
                if (Vector2.Distance(pos, NPC.Center) > dist) {
                    NPC.velocity.Y *= 0.9f;
                    //Vector2 vector = NPC.DirectionTo(altarPosition) * 2f;
                    //NPC.position += vector;
                }
                else {
                    float value8 = Vector2.Distance(pos, NPC.Center) / dist;
                    value8 *= 3.5f;
                    float value9 = 0.2f * value8;
                    NPC.position.Y -= value9;
                    NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, Helper.VelocityToPoint(NPC.Center, pos, 1f).Y, value9);
                    //NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, Helper.VelocityToPoint(NPC.Center, pos, 2f).Y, 2f * value8);
                }
                if (flag3) {
                    if (NPC.Opacity < 0.8f) {
                        NPC.Opacity += OPACITYACC * 0.7f;
                    }
                }
                else {

                }
                if (flag4) {
                    int max = 5;
                    if (Main.GameUpdateCount % 3 == 0) {
                        if (NPC.ai[2] < max) {
                            NPC.ai[2]++;
                        }
                    }
                    int count = (int)(max - NPC.ai[2]);
                    if (Main.rand.NextBool(1 + Main.rand.Next(2) + (int)((3 + count) * MathHelper.Clamp(1f - altarStrength * 3f - (1f - NPC.scale), 0f, 1f)))) {
                        if (!_lothorSummonSoundPlayed) {
                            _lothorSummonSoundPlayed = true;
                            _lothorSummonSound = SoundEngine.PlaySound(new SoundStyle(ResourceManager.AmbientSounds + "LothorAltarSummon3") { Volume = 1f }, NPC.Center);
                        }
                        Vector2 center = NPC.position + new Vector2(3f + Main.rand.Next(NPC.width - 3), NPC.height / 2f + 8f);
                        center.X += Main.rand.Next(-100, 100) * 0.05f;
                        center.Y += Main.rand.Next(-100, 100) * 0.05f;
                        Vector2 npcVelocity = NPC.velocity + _velocity + _velocity2 + _velocity3 + _velocity4;
                        Vector2 position = center/* + npcVelocity*/;
                        //if (Main.rand.NextBool()) {
                        //    SoundEngine.PlaySound(new SoundStyle(ResourceManager.AmbientSounds + "Test2") { PitchVariance = 0.5f, Volume = 2f }, position);
                        //}
                        //velocity *= MathHelper.Clamp(altarStrength + 0.5f, 0.5f, 1f);
                        VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDTILESBEHINDNPCS)?.
                                SetupPart(1,
                                        Vector2.Zero,
                                        position,
                                        towards + Vector2.UnitX * 6f + Main.rand.Random2(15f) + Vector2.UnitY * (10f + 20f * altarStrength),
                                        Main.rand.Next(70, 85) * Main.rand.NextFloat(0.01f, 0.015f),
                                        0.8f);
                    }
                }
            }
            if (flag) {
                if (++NPC.ai[3] >= 130f) {
                    NPC.ai[3] = 0f;
                    SoundEngine.PlaySound(new SoundStyle(ResourceManager.AmbientSounds + "AltarEcho") { Volume = 0.85f }, AltarHandler.GetAltarPosition().ToWorldCoordinates());
                }

                if (NPC.Opacity > 0.01f) {
                    NPC.Opacity -= OPACITYACC * 1.56f;
                }
            }
            if (flag || (NPC.velocity.Length() < 3.5f && _velocity.Length() < 3.5f) || flag2) {
                int max = 3;
                int count = (int)(max - NPC.ai[2]);
                if (Main.rand.NextBool(3 + Main.rand.Next(2) + (int)((4 + count) * MathHelper.Clamp(1f - altarStrength - (1f - NPC.scale) - NPC.Opacity, 0.2f, 1f)))) {
                    Vector2 center = NPC.position + new Vector2(6f + Main.rand.Next(NPC.width - 6), NPC.height / 2f + 10f);
                    center.X += Main.rand.Next(-100, 100) * 0.05f;
                    center.Y += Main.rand.Next(-100, 100) * 0.05f;
                    VisualEffectSystem.New<SoulPart>(VisualEffectLayer.BEHINDNPCS)?.
                            SetupPart(0,
                                    _velocity2 + _velocity3 + _velocity,
                                    center - Vector2.UnitY * (2f + -8f * Ease.CircIn(altarStrength)),
                                    towards + Vector2.UnitX * 6f,
                                    Main.rand.Next(70, 85) * 0.01f,
                                    NPC.Opacity + 0.15f);
                }
            }
            else if (!flag3) {
                //NPC.velocity *= 0.975f;
            }
            if (flag2) {
                if (NPC.Distance(towards) < 100f) {
                    float dist2 = NPC.Distance(towards) / 100f;
                    NPC.Opacity = Math.Min(NPC.Opacity, MathHelper.Clamp(dist2 * 0.75f, 0f, 1f));
                }
                if (NPC.scale > 0f) {
                    NPC.scale -= OPACITYACC * 1.56f;
                }
                NPC.velocity *= 0.85f;
                NPC.SlightlyMoveTo(towards, 3f, 10f);
                NPC.velocity *= 0.75f;
                if (NPC.Opacity > 0.01f) {
                    NPC.Opacity -= OPACITYACC * 1.6f;
                }
                return;
            }
            return;
        }
    }
}