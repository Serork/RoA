using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class FlederForm : BaseForm {
    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 1.4f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * (IsInAir(player) ? 1f : 0.8f));

    public override SoundStyle? HurtSound => SoundID.NPCHit27;

    public override Vector2 SetWreathOffset(Player player) => new(0f, 6f + (IsInAir(player) ? -6f : 0f));
    public override Vector2 SetWreathOffset2(Player player) => new(0f, -2f);

    protected override Vector2 GetLightingPos(Player player) => player.Center;
    protected override Color LightingColor => new(79, 124, 211);

    internal sealed class FlederFormHandler : ModPlayer, IDoubleTap {
        public const int CD = 50, DURATION = 35;
        public const float SPEED = 10f;

        private IDoubleTap.TapDirection _dashDirection;
        internal float _dashDelay, _dashTimer;
        private int[] _localNPCImmunity = new int[Main.npc.Length];
        internal int _shootCounter;
        internal bool _holdingLmb;

        public bool ActiveDash => _dashDelay > 0;

        public override void ResetEffects() {
            if (!Player.GetModPlayer<BaseFormHandler>().IsInADruidicForm) {
                _dashDelay = _dashTimer = 0;
                _dashDirection = IDoubleTap.TapDirection.None;
                _shootCounter = 0;
            }
        }

        void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
            bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
            if (!flag) {
                return;
            }
            if (!player.GetModPlayer<BaseFormHandler>().IsConsideredAs<FlederForm>()) {
                return;
            }

            player.GetModPlayer<FlederFormHandler>().UseFlederDash(direction);
        }

        public override void PreUpdateMovement() {
            bool flag = _dashDirection != IDoubleTap.TapDirection.None || ActiveDash;
            if (flag && !Player.GetModPlayer<BaseFormHandler>().IsConsideredAs<FlederForm>()) {
                _dashDirection = IDoubleTap.TapDirection.None;
                _dashDelay = _dashTimer = 0;
                return;
            }

            if (flag && !ActiveDash) {
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
                SpawnDusts(Player);
                Player.velocity = newVelocity;
                if (Player.velocity.Y == Player.gravity) {
                    Player.velocity.Y -= 5f;
                }
                Point tileCoordinates1 = (Player.Center + new Vector2((dashDirection * Player.width / 2 + 2), (float)(Player.gravDir * -Player.height / 2.0 + Player.gravDir * 2.0))).ToTileCoordinates();
                Point tileCoordinates2 = (Player.Center + new Vector2((dashDirection * Player.width / 2 + 2), 0.0f)).ToTileCoordinates();
                if (WorldGen.SolidOrSlopedTile(tileCoordinates1.X, tileCoordinates1.Y) || WorldGen.SolidOrSlopedTile(tileCoordinates2.X, tileCoordinates2.Y)) {
                    Player.velocity.X /= 2f;
                }

                SoundEngine.PlaySound(SoundID.Item169 with { Pitch = -0.8f, PitchVariance = 0.1f, Volume = 0.6f }, Player.Center);

                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(Player, 14, Player.Center));
                }
            }

            if (ActiveDash) {
                _dashDelay--;
            }

            if (_dashTimer > 0) {
                if (!IsInAir(Player)) {
                    for (int i = 0; i < 3; i++) {
                        if (Main.rand.NextBool(3)) {
                            int num = 0;
                            if (Player.gravDir == -1f)
                                num -= Player.height;
                            int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height + (float)num), Player.width + 8, 4, 59, (0f - Player.velocity.X) * 0.5f, Player.velocity.Y * 0.5f, 59, default(Color), Main.rand.NextFloat(2f, 3f) * 0.95f);
                            Main.dust[num6].velocity.X = Main.dust[num6].velocity.X * 0.2f;
                            Main.dust[num6].velocity.Y = -0.5f - Main.rand.NextFloat() * 1.5f;
                            Main.dust[num6].fadeIn = 0.5f;
                            Main.dust[num6].scale *= Main.rand.NextFloat(1.1f, 1.25f);
                            Main.dust[num6].scale *= 0.8f;
                            Main.dust[num6].noGravity = true;
                        }
                    }
                }

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
                            int damage = 40;
                            float num = Player.GetTotalDamage(DruidClass.Nature).ApplyTo(damage);
                            float num2 = 3f;
                            bool crit = false;

                            if (Main.rand.Next(100) < (4 + Player.GetTotalCritChance(DruidClass.Nature)))
                                crit = true;

                            int num3 = Player.direction;
                            if (Player.velocity.X < 0f)
                                num3 = -1;

                            if (Player.velocity.X > 0f)
                                num3 = 1;

                            if (Player.whoAmI == Main.myPlayer)
                                Player.ApplyDamageToNPC(nPC, (int)num, num2, num3, crit, DruidClass.Nature, true);

                            _dashTimer = DURATION;
                            _dashDelay = CD;
                            Player.velocity *= 0.9f;
                            _localNPCImmunity[i] = 10;
                            Player.immune = true;
                            Player.immuneTime = 10;
                            Player.immuneNoBlink = true;
                        }
                    }
                }
                _dashTimer--;
            }
        }

        internal static void SpawnDusts(Player player, int strength = 3) {
            Vector2 vector11 = player.Center;
            for (int k = 0; k < 40 - 10 * (3 - strength); k++) {
                if (Main.rand.NextChance(0.75f)) {
                    int num23 = 59;
                    float num24 = 0.4f;
                    if (k % 2 == 1) {
                        num24 = 0.65f;
                    }
                    num24 *= 3f;

                    Vector2 vector12 = vector11 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(3 * 2));
                    int num25 = Dust.NewDust(vector12 - Vector2.One * 30f, 60, 60, num23, player.velocity.X / 2f, player.velocity.Y / 2f);
                    Main.dust[num25].velocity = Vector2.Normalize(vector11 - vector12) * 1.5f * (10f - (float)3f * 2f) / 10f;
                    Main.dust[num25].noGravity = true;
                    Main.dust[num25].scale = num24;
                }
            }
        }

        internal void UseFlederDash(IDoubleTap.TapDirection direction, bool server = false) {
            var handler = Player.GetModPlayer<FlederFormHandler>();
            if (handler.ActiveDash) {
                return;
            }

            handler._dashDirection = direction;
            if (!server && Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new FlederFormPacket1(Player, direction));
            }
        }
    }

    public override float GetMaxSpeedMultiplier(Player player) => 1f;
    public override float GetRunAccelerationMultiplier(Player player) => 1.25f;

    protected override void SafeSetDefaults() {
        MountData.totalFrames = 8;
        MountData.spawnDust = 59;
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 60;
        MountData.fatigueMax = 40;

        MountData.yOffset = -7;
        MountData.playerHeadOffset = -14;
    }

    protected override void SafePostUpdate(Player player) {
        player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed = true;

        player.npcTypeNoAggro[ModContent.NPCType<Fleder>()] = true;
        player.npcTypeNoAggro[ModContent.NPCType<BabyFleder>()] = true;
        player.statDefense -= 6;

        float rotation = player.velocity.X * (IsInAir(player) ? 0.2f : 0.15f);
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        bool flag = IsInAir(player);
        float maxRotation = flag ? 0.3f : 0.2f;
        fullRotation = MathHelper.Clamp(fullRotation, -maxRotation, maxRotation);
        MountData.yOffset = flag ? -3 : -7;
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
        if (player.GetModPlayer<FlederFormHandler>().ActiveDash) {
            player.fullRotation = Utils.AngleLerp(player.fullRotation, fullRotation, 0.25f);
        }
        else {
            player.fullRotation = fullRotation;
        }
        Player.jumpHeight = 1;
        Player.jumpSpeed = 4f;
        player.gravity *= 0.75f;
        player.velocity.Y = Math.Min(5f, player.velocity.Y);
        player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2 - 6f);

        SpecialAttackHandler(player);
    }

    private void SpecialAttackHandler(Player player) {
        void dustEffects1() {
            ref int shootCounter = ref player.GetModPlayer<FlederFormHandler>()._shootCounter;
            ref bool holdingLmb = ref player.GetModPlayer<FlederFormHandler>()._holdingLmb;

            shootCounter++;

            bool flag = shootCounter >= 100;
            int num20 = 1;
            if (shootCounter >= 40f)
                num20++;
            if (shootCounter >= 70f)
                num20++;
            if (flag)
                num20++;

            Vector2 vector11 = player.Center + Vector2.UnitY * (IsInAir(player) ? 7f : 12f);
            for (int k = 0; k < num20; k++) {
                if (Main.rand.NextChance(0.9f - 0.1f * (num20 - 1))) {
                    int num23 = 59;
                    float num24 = 0.4f;
                    if (k % 2 == 1) {
                        num24 = 0.65f;
                    }
                    num24 *= 2.25f;

                    Vector2 vector12 = vector11 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(num20 * 2));
                    int num25 = Dust.NewDust(vector12 - Vector2.One * 20f, 40, 40, num23, player.velocity.X / 2f, player.velocity.Y / 2f);
                    if (Vector2.Distance(Main.dust[num25].position, vector11) > 16f) {
                        Main.dust[num25].active = false;
                        continue;
                    }
                    Main.dust[num25].velocity = Vector2.Normalize(vector11 - vector12) * 1.5f * (10f - (float)num20 * 2f) / 10f;
                    Main.dust[num25].noGravity = true;
                    Main.dust[num25].scale = num24;
                }
            }
        }

        ref int shootCounter = ref player.GetModPlayer<FlederFormHandler>()._shootCounter;
        ref bool holdingLmb = ref player.GetModPlayer<FlederFormHandler>()._holdingLmb;

        bool flag = player.whoAmI != Main.myPlayer;
        if (flag && holdingLmb) {
            dustEffects1();
        }

        if (shootCounter == 40 || shootCounter == 70 || shootCounter == 100) {
            if (player.whoAmI == Main.myPlayer) {
                SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
            }
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 13, player.Center));
            }
        }

        if (flag || Main.mouseText)
            return;
        if (player.GetModPlayer<FlederFormHandler>()._dashDelay > FlederFormHandler.CD - 5) {
            BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
        }

        if (player.controlUseItem && Main.mouseLeft) {
            if (!holdingLmb) {
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new FlederFormPacket2(player, true));
                }
            }
            holdingLmb = true;
            dustEffects1();
        }
        else {
            if (holdingLmb) {
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new FlederFormPacket2(player, false));
                }
            }
            holdingLmb = false;
        }
        string context = "flederformattack";
        int baseDamage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(30);
        float baseKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(3f);
        if (player.releaseUseItem && Main.mouseLeftRelease) {
            if (shootCounter >= 40 && shootCounter < 70) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
                FlederFormHandler.SpawnDusts(player, 1);
                Vector2 Velocity = Helper.VelocityToPoint(player.Center, Main.rand.RandomPointInArea(new Vector2(player.Center.X, player.Center.Y + 100), new Vector2(player.Center.X, player.Center.Y + 100)), 4);
                Projectile.NewProjectile(player.GetSource_Misc(context), player.Center.X, player.Center.Y, Velocity.X, Velocity.Y + 3, ModContent.ProjectileType<FlederBomb>(), baseDamage, baseKnockback, player.whoAmI, 0f, 0f);
                player.velocity.Y = 0f;
                player.velocity.Y -= 5f;

                if (player.whoAmI == Main.myPlayer) {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, player.Bottom);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 15, player.Bottom));
                }

                NetMessage.SendData(13, -1, -1, null, Main.myPlayer);
            }
            if (shootCounter >= 70 && shootCounter < 100) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
                FlederFormHandler.SpawnDusts(player, 2);
                Vector2 Velocity = Helper.VelocityToPoint(player.Center, Main.rand.RandomPointInArea(new Vector2(player.Center.X, player.Center.Y + 100), new Vector2(player.Center.X, player.Center.Y + 100)), 4);
                Projectile.NewProjectile(player.GetSource_Misc(context), player.Center.X, player.Center.Y, Velocity.X, Velocity.Y + 5, ModContent.ProjectileType<FlederBomb>(), baseDamage * 2, baseKnockback * 1.2f, player.whoAmI, 1f, 0f);
                player.velocity.Y = 0f;
                player.velocity.Y -= 7.5f;

                NetMessage.SendData(13, -1, -1, null, Main.myPlayer);

                if (player.whoAmI == Main.myPlayer) {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, player.Bottom);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 15, player.Bottom));
                }
            }
            if (shootCounter >= 100) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
                FlederFormHandler.SpawnDusts(player);
                Vector2 Velocity = Helper.VelocityToPoint(player.Center, Main.rand.RandomPointInArea(new Vector2(player.Center.X, player.Center.Y + 100), new Vector2(player.Center.X, player.Center.Y + 100)), 4);
                Projectile.NewProjectile(player.GetSource_Misc(context), player.Center.X, player.Center.Y, Velocity.X, Velocity.Y + 8, ModContent.ProjectileType<FlederBomb>(), baseDamage * 3, baseKnockback * 1.4f, player.whoAmI, 2f, 0f);
                player.velocity.Y = 0f;
                player.velocity.Y -= 10f;

                NetMessage.SendData(13, -1, -1, null, Main.myPlayer);

                if (player.whoAmI == Main.myPlayer) {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, player.Bottom);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 15, player.Bottom));
                }
            }
            shootCounter = 0;
        }
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int minFrame = 4, maxFrame = 7;
        float flightFrameFrequency = 14f, walkingFrameFrequiency = 16f;
        if (IsInAir(player)) {
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
            if (frame > minFrame - 1) {
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
            Vector2 spawnPos = player.Center + new Vector2(player.width, 0).RotatedBy(i * Math.PI * 2 / 24f * player.direction) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream1") { Pitch = -0.3f, PitchVariance = 0.1f, Volume = 0.8f }, player.Center);
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center - new Vector2(player.width, 0).RotatedBy(i * Math.PI * 2 / 24f * player.direction) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }
}