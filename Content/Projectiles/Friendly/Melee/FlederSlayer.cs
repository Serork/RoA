using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Utilities;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.Players;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

// my first attempt at something special
// its super unoptimized but it is what it is
sealed class FlederSlayer : ModProjectile, DruidPlayerShouldersFix.IProjectileFixShoulderWhileActive {
    private static Asset<Texture2D> _bladeTexture = null!,
                                    _bladeGlowTexture = null!,
                                    _glowTexture = null!;

    private TrailInfo[] _trails;
    private float _charge;
    private Vector2 _offset;
    private float _extraRotation;
    private int _extraRotationDir;
    private bool _released;
    private SlotId _slot;
    private int _direction;
    private bool _init, _init2;
    private int _timeLeft;
    private float _timingProgress;
    private bool _empoweredAttack;
    private bool _twinkleSoundPlayed;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;

        if (Main.dedServ) {
            return;
        }

        _bladeTexture = ModContent.Request<Texture2D>(Texture + "_Blade");
        _bladeGlowTexture = ModContent.Request<Texture2D>(Texture + "_Blade_Glow");
        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetDefaults() {
        int width = 88; int height = width;
        Projectile.Size = new Vector2(width, height);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.scale = 0.8f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;

        _offset = Vector2.Zero;
        _extraRotationDir = 1;
        _slot = SlotId.Invalid;

        int count = 70;
        _trails = new TrailInfo[count];
        for (int i = 0; i < count; i++) {
            _trails[i] = new TrailInfo();
        }

        Projectile.netImportant = true;

        Projectile.noEnchantmentVisuals = true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_released);
        writer.WriteVector2(_offset);
        writer.Write(_charge);
        writer.Write(_extraRotation);
        writer.Write(_extraRotationDir);
        writer.Write(_direction);
        writer.Write(_init);
        writer.Write(_timeLeft);
        writer.Write(_empoweredAttack);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _released = reader.ReadBoolean();
        _offset = reader.ReadVector2();
        _charge = reader.ReadSingle();
        _extraRotation = reader.ReadSingle();
        _extraRotationDir = reader.ReadInt32();
        _direction = reader.ReadInt32();
        _init = reader.ReadBoolean();
        _timeLeft = reader.ReadInt32();
        _empoweredAttack = reader.ReadBoolean();
    }

    public override void CutTiles() {
        //DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        //Utils.TileActionAttempt plot = new Utils.TileActionAttempt(DelegateMethods.CutTiles);
        //Vector2 center = Projectile.Center;
        //Utils.PlotTileLine(center + Projectile._rotation.ToRotationVector2() * 30f, center + Projectile._rotation.ToRotationVector2() * 150f, Projectile.width * Projectile.scale, plot);
    }

    public override bool? CanCutTiles() => false;

    private void CalculateExtraRotation() {
        float speed = Main.rand.NextFloat(0.0075f, 0.025f) * Main.rand.NextFloat() * Main.rand.NextFloat() * Math.Clamp(Math.Abs(Main.player[Projectile.owner].velocity.X), 1f, 1.35f);
        float maxSpeed = 0.05f;
        if (Main.rand.NextBool(50)) {
            _extraRotationDir *= -1;
        }
        if (_extraRotationDir == 1) {
            if (_extraRotation < maxSpeed) {
                _extraRotation += speed;
            }
            else {
                _extraRotationDir *= -1;
            }
        }
        else {
            if (_extraRotation < -maxSpeed) {
                _extraRotationDir *= -1;
                return;
            }
            _extraRotation -= speed;
        }
    }

    public override void OnSpawn(IEntitySource source) {
        Player player = Main.player[Projectile.owner];
        if (Projectile.owner == Main.myPlayer) {
            if (!_init) {
                _direction = player.GetViableMousePosition().X > player.MountedCenter.X ? 1 : -1;
                Projectile.Center = player.MountedCenter;
                Projectile.direction = Projectile.spriteDirection = _direction;
                player.ChangeDir(Projectile.spriteDirection);
                _timeLeft = Projectile.timeLeft;
                _init = true;
                Projectile.netUpdate = true;
            }
        }
    }

    private void FlederSlayerCutTiles() {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }
        //Player player = Main.player[Projectile.owner];
        for (int i2 = 1; i2 < 6; i2++) {
            Vector2 boxPosition = Projectile.position + new Vector2(-30f * Projectile.direction, 0f) + ((Projectile.rotation + _extraRotation)).ToRotationVector2() * 20f * i2 +
                new Vector2(-20f + (Projectile.direction == 1 ? 100f : 0f), 20f);
            int boxWidth = 20;
            int boxHeight = 20;
            int num = (int)(boxPosition.X / 16f);
            int num2 = (int)((boxPosition.X + (float)boxWidth) / 16f) + 1;
            int num3 = (int)(boxPosition.Y / 16f);
            int num4 = (int)((boxPosition.Y + (float)boxHeight) / 16f) + 1;
            if (num < 0)
                num = 0;

            if (num2 > Main.maxTilesX)
                num2 = Main.maxTilesX;

            if (num3 < 0)
                num3 = 0;

            if (num4 > Main.maxTilesY)
                num4 = Main.maxTilesY;

            bool[] tileCutIgnorance = Main.player[Projectile.owner].GetTileCutIgnorance(allowRegrowth: false, Projectile.trap);
            for (int i = num; i < num2; i++) {
                for (int j = num3; j < num4; j++) {
                    if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
                        WorldGen.KillTile(i, j);
                        if (Main.netMode != 0)
                            NetMessage.SendData(17, -1, -1, null, 0, i, j);
                    }
                }
            }
        }
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        FlederSlayerCutTiles();

        if (!_released) {
            Vector2 velocity = Projectile.velocity;
            int count = 3;
            Projectile.velocity *= 0.5f;
            for (int i = 0; i < count; i++) {
                Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation * player.gravDir).ToRotationVector2() * (i + 1) * (100f / count) * Projectile.scale,
                new Vector2(40f * Projectile.scale, 40f * Projectile.scale));
                if (Main.rand.NextBool()) {
                    Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
                }
            }
            Projectile.velocity = velocity;
        }

        player.itemAnimation = player.itemTime = 2;
        player.heldProj = Projectile.whoAmI;
        float playerDirection = player.direction;
        if (player.HeldItem.type != ModContent.ItemType<Items.Weapons.Melee.FlederSlayer>()) {
            Projectile.Kill();
        }
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;

            SoundEngine.PlaySound(SoundID.Item1, player.Center);

            float scale = Main.player[Projectile.owner].CappedMeleeOrDruidScale();
            if (scale != 1f) {
                Projectile.localAI[2] *= scale;
                Projectile.scale *= scale;
            }
        }
        if (Projectile.ai[1] < 3f) {
            Lighting.AddLight(Projectile.Center + new Vector2(90, 90).RotatedBy(Projectile.rotation - 0.78f) * Projectile.scale,
            Color.White.ToVector3() * MathHelper.Clamp(_charge * 2f, 0f, 1f) * 0.5f);
        }
        int itemAnimationMax = 40;
        int min = itemAnimationMax / 2 - itemAnimationMax / 4;
        Projectile.Opacity = Utils.GetLerpValue(itemAnimationMax, itemAnimationMax - 7, Projectile.timeLeft, clamped: true) * Utils.GetLerpValue(0f, 7f, Projectile.timeLeft, clamped: true);
        Projectile.timeLeft = _timeLeft;
        bool lastSlash = Projectile.ai[1] < 3f;
        bool lastSlash2 = Projectile.ai[1] < 13f;
        bool turnOnAvailable = false;
        bool flag3 = _timeLeft > min;

        if (!player.IsAliveAndFree()) {
            Projectile.Kill();
        }
        else {
            Projectile.Center = player.MountedCenter + Vector2.Normalize(Projectile.velocity);
            float dir = (float)(Math.PI / 2.0 + (double)playerDirection * 1.0);
            float offset = MathHelper.Pi / 1.15f * playerDirection;
            SoundStyle style = new SoundStyle(ResourceManager.ItemSounds + "Whisper") { Volume = 1.15f };

            if (_charge >= 0.8f) {
                //var style2 = new SoundStyle(ResourceManager.ItemSounds + "Twinkle") { Volume = 1f };
                if (!_twinkleSoundPlayed) {
                    SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                    _twinkleSoundPlayed = true;
                }

                if (_timingProgress < 1f) {
                    _timingProgress += 0.1f;
                }
                else {
                    _timingProgress += 0.1f;
                }
            }

            if (flag3) {
                CalculateExtraRotation();

                Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.localAI[2] * 1.15f, 0.1f);

                if (Projectile.owner == Main.myPlayer) {
                    bool flag = player.channel && Projectile.Opacity == 1f;
                    if (flag) {
                        if (_charge <= 0f) {
                            _slot = SoundEngine.PlaySound(style, Projectile.Center);
                        }
                        _charge += 0.015f * player.GetTotalAttackSpeed(DamageClass.Melee);
                        _charge = Math.Clamp(_charge, 0f, 1f);
                    }
                    if (flag || ++Projectile.ai[0] < 10f) {
                        _timeLeft = min + 5;

                        //player.velocity.X *= 0.975f;
                    }
                    Projectile.netUpdate = true;
                }

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, (dir - offset).ToRotationVector2() * 50f, 0.1f);
                Projectile.velocity += Projectile.DirectionFrom(player.MountedCenter) * 3f;

                _offset = Vector2.SmoothStep(_offset, new Vector2(-10f * playerDirection, 12f), 3f);

                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else if (_timeLeft == min) {
                if (_timingProgress >= 1.3f && _timingProgress <= 2.3f && !_empoweredAttack) {
                    _empoweredAttack = true;
                    _charge = 1f;
                    //Projectile.netUpdate = true;
                }

                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

                for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
                    Projectile.localNPCImmunity[i] = 0;
                }

                Projectile.ai[0] = itemAnimationMax;

                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else {
                if (SoundEngine.TryGetActiveSound(_slot, out var sound)) {
                    sound.Volume -= 0.01f;
                }

                if (Projectile.ai[0] > 30f) {
                    _timeLeft = Projectile.timeLeft = 10;

                    Projectile.ai[0] -= 0.5f;

                    float progress = Swing(1f - Projectile.ai[0] / 40f);

                    Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.localAI[2] * 1.35f, Helper.EaseInOut2(Projectile.ai[0] / 30f));

                    offset = MathHelper.Pi / 10f * playerDirection;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, (progress * MathHelper.Pi - MathHelper.PiOver2 + offset).ToRotationVector2() * 100f, 0.1f);

                    player.bodyFrame.Y = player.bodyFrame.Height * 3;

                    _offset = Vector2.Lerp(_offset, new Vector2(6f * playerDirection, 30f), 0.05f);
                }
                else {
                    bool flag = Projectile.ai[1] <= 40f && _released;
                    if (flag) {
                        player.velocity.X *= 0.975f;

                        Projectile.rotation -= 0.0375f * Helper.EaseInOut3(1f - Projectile.ai[1] / 40f) * playerDirection;
                    }
                    if (lastSlash2 || flag) {
                        _timeLeft = Projectile.timeLeft = 10;
                    }
                    if (Projectile.ai[1] < 2f) {
                        if (player.controlLeft) {
                            Projectile.spriteDirection = -1;
                            player.ChangeDir(Projectile.spriteDirection);
                        }
                        if (player.controlRight) {
                            Projectile.spriteDirection = 1;
                            player.ChangeDir(Projectile.spriteDirection);
                        }

                        Projectile.spriteDirection = player.direction;

                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, (MathHelper.PiOver2 - offset).ToRotationVector2() * 100f, 0.45f);

                        turnOnAvailable = true;

                        CalculateExtraRotation();

                        Projectile.ai[1] = 1f;

                        player.velocity.X *= 0.935f;

                        Projectile.ai[1] = 2f;

                        player.bodyFrame.Y = player.bodyFrame.Height * 3;
                    }
                    else if (Projectile.ai[1] == 2f) {
                        turnOnAvailable = false;

                        player.ChangeDir(Projectile.spriteDirection);

                        if (SoundEngine.TryGetActiveSound(_slot, out var sound2)) {
                            sound2.Volume -= 0.01f;
                        }

                        _offset = Vector2.SmoothStep(_offset, Vector2.Zero, 0.25f);

                        RotateArm();

                        if (Projectile.ai[0] > 0f) {
                            Projectile.ai[0] -= 1.25f;
                        }
                        else {
                            Projectile.ai[1] = 3f;

                            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + (_empoweredAttack ? "PerfectSlash" : "HeavySword")) { Volume = 1f }, Projectile.Center);
                        }

                        Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.localAI[2] * 1.5f, 0.1f);

                        float progress = Swing(1f - Projectile.ai[0] / 75f);
                        offset = MathHelper.Pi / 2f * playerDirection;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, (progress * MathHelper.Pi - MathHelper.PiOver2 + offset * 1.35f).ToRotationVector2() * 130f, 0.075f);
                    }
                    else {
                        if (SoundEngine.TryGetActiveSound(_slot, out var sound2)) {
                            sound2.Volume -= 0.01f;
                        }

                        SlashDusts();

                        RotateArm();

                        Projectile.ai[1] += 1f;

                        float progress = Swing(1f - Projectile.ai[1] / 45f);
                        float slow = Math.Clamp(Projectile.ai[1] >= 10f ? Helper.EaseInOut4((Projectile.ai[1] - 10f) / 10f) : 1f, 0f, 1f);
                        float slow2 = Math.Clamp(Swing(Projectile.ai[1] >= 12f ? Projectile.ai[1] - 12f / 12f : 1f), 0f, 1f);
                        float extraRotation = playerDirection * 0.35f * Helper.EaseInOut3(progress) * slow * slow2;
                        Vector2 extra = Vector2.Normalize(Projectile.velocity) * -((Projectile.rotation + (playerDirection != 1 ? MathHelper.Pi : 0f)) * playerDirection * player.gravDir).ToRotationVector2() * 165f * Projectile.localAI[2];
                        Vector2 projectileCenter = Projectile.Center + extra;
                        if (!_released && Projectile.ai[1] <= 16f) {
                            if (Projectile.ai[1] > 4f &&
                                WorldGenHelper.SolidTile(Math.Clamp((int)projectileCenter.X / 16, 1, Main.maxTilesX), Math.Clamp((int)projectileCenter.Y / 16 + 1, 1, Main.maxTilesY))) {
                                _released = true;

                                if (_charge > 0.35f && Projectile.ai[1] > 8f) {
                                    if (Projectile.owner == Main.myPlayer) {
                                        for (int i = 0; i < Main.rand.Next(2, 4) + (int)(_charge * 3); i++) {
                                            float value = _empoweredAttack ? 1.25f : 1f;
                                            float value0 = _empoweredAttack ? 1.5f : 1f;
                                            float value2 = MathHelper.Clamp(_charge * 1.5f, 0f, 1f);
                                            Vector2 velocity = Helper.VelocityToPoint(player.MountedCenter, projectileCenter, 35f * value2 * player.GetTotalAttackSpeed(DamageClass.Melee) * value);
                                            float size = 2f * (value2 * 1.15f + 0.15f) * value * Projectile.scale;
                                            int damage = (int)((Projectile.damage + Projectile.damage / 2) * (value2 * 1.15f + 0.15f) * value0);
                                            if (Main.player[Projectile.owner].gravDir != -1f) {
                                                Projectile.NewProjectileDirect(Projectile.GetSource_FromAI("Fleder Slayer Slash"),
                                                                           projectileCenter - extra / 2f + new Vector2(i * Main.rand.Next(5, 21)),
                                                                           velocity,
                                                                           ModContent.ProjectileType<WaveSlash>(),
                                                                           damage,
                                                                           Projectile.knockBack,
                                                                           Projectile.owner,
                                                                           size);
                                            }
                                        }
                                    }
                                    if (Main.netMode != NetmodeID.Server && Main.myPlayer == Projectile.owner) {
                                        string tag = "Fleder Slayer Stomp";
                                        float strength = Main.rand.NextFloat(15f, 26f) / 3f * (_charge * 1.15f + 0.15f);
                                        PunchCameraModifier punchCameraModifier = new PunchCameraModifier(projectileCenter, MathHelper.PiOver2.ToRotationVector2(), strength, 10f, 20, 1000f, tag);
                                        Main.instance.CameraModifiers.Add(punchCameraModifier);
                                    }
                                    if (!_empoweredAttack) SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Clang") { Volume = 0.95f }, Projectile.Center);
                                }
                            }
                            Projectile.rotation += extraRotation;
                        }
                        for (int i = 0; i < _trails.Length; i++) {
                            _trails[i].Rotation += extraRotation;
                        }
                        Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.localAI[2] * 1f, Helper.EaseInOut3(Projectile.ai[1] / 20f) * Helper.EaseInOut(progress));
                    }
                }
            }
            if (lastSlash) {
                Projectile.rotation = Projectile.velocity.ToRotation() + Helper.EaseInOut2(Math.Abs(_extraRotation)) * (_extraRotation != 0f ? Math.Sign(_extraRotation) : 1f);
            }
        }
        if (/*Projectile.owner == Main.myPlayer && */flag3 && _timeLeft > 0) {
            _timeLeft--;
            //Projectile.netUpdate = true;
        }
        if (flag3) {
            player.velocity.X *= 0.975f;
        }
        if (!flag3 && _timeLeft > 0) {
            _timeLeft--;
        }
        if (!turnOnAvailable && Projectile.owner == Main.myPlayer) {
            player.ChangeDir(Projectile.spriteDirection);
        }
        Projectile.Center += _offset;
        //int trailTimeLeft = 5;
        _trails[0] = new TrailInfo(Projectile.Center, Projectile.rotation);
        Projectile.oldPos[0] = _trails[0].Position;
        Projectile.oldRot[0] = _trails[0].Rotation;
        for (int i = _trails.Length - 1; i > 0; i--) {
            _trails[i] = _trails[i - 1];
        }
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
            Projectile.oldPos[i] = Projectile.oldPos[i - 1];
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
        }

        if (_init && !_init2) {
            player.ChangeDir(_direction);
            _init2 = true;
        }
    }

    private void SlashDusts() {
        if (Main.player[Projectile.owner].gravDir == -1f) {
            return;
        }
        if (_released) {
            return;
        }
        for (int i = 0; i < 4; i++) {
            float rotation2 = Projectile.rotation;
            Vector2 rotation = rotation2.ToRotationVector2();
            float rotation3 = MathHelper.PiOver2 * -Main.player[Projectile.owner].direction;
            Vector2 velocity = rotation.RotatedBy(rotation3) * Main.rand.NextFloat(2f, 12f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + rotation * 10f + rotation * Main.rand.NextFloat(10f, 120f * Projectile.scale), ModContent.DustType<Slash>(), velocity, Math.Max(Main.rand.Next(70, 120) * (int)(_charge * 255f), 40), Color.White * Math.Max(0.3f, _charge));
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi) /*+ (Main.player[Projectile.owner].gravDir == -1f ? MathHelper.PiOver2 : 0f)*/;
            dust.scale *= Projectile.scale * 1f;
            dust.fadeIn = dust.scale + 0.1f;
            dust.noGravity = true;
        }
    }

    private struct TrailInfo(Vector2 position, float rotation) {
        public Vector2 Position = position;
        public float Rotation = rotation;
    }

    private void RotateArm() {
        Player player = Main.player[Projectile.owner];
        float armRotation = Projectile.rotation - MathHelper.PiOver2;
        player.bodyFrame.Y = 56;
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
    }

    private static float Swing(float progress) {
        if (progress > 0.5f) {
            return 0.5f - Swing(0.5f - (progress - 0.5f)) + 0.5f;
        }
        return ((float)Math.Sin(Math.Pow(progress, 2f) * MathHelper.TwoPi - MathHelper.PiOver2) + 1f) / 2f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Main.player[Projectile.owner].channel) {
            Projectile.localNPCImmunity[target.whoAmI] = 100;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Main.player[Projectile.owner].channel) {
            modifiers.Knockback *= 0f;
        }
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
        modifiers.FinalDamage *=
                            Projectile.ai[1] >= 3f
                          ? Projectile.scale * 3f * (1f + _charge)
                          : 1f;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.HitDirectionOverride = ((Main.player[Projectile.owner].Center.X < target.Center.X) ? 1 : (-1));
        modifiers.FinalDamage *=
                            Projectile.ai[1] >= 3f
                          ? Projectile.scale * 3f * (1f + _charge)
                          : 1f;
    }

    public override bool ShouldUpdatePosition()
        => false;

    private bool CanDamageInternal() {
        if (_released) {
            return false;
        }
        int min = Main.player[Projectile.owner].itemAnimationMax / 2 - Main.player[Projectile.owner].itemAnimationMax / 4;
        if (_timeLeft == min + 5 || Projectile.ai[1] == 1f) {
            return false;
        }
        return _timeLeft <= min + 4 || Projectile.ai[1] >= 3f;
    }

    public override bool? CanDamage() {
        return CanDamageInternal();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float coneLength = 130f * Projectile.scale;
        float maximumAngle = 0.3926991f;
        float coneRotation = Projectile.rotation;
        bool result = targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle);
        return result;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture2D = Projectile.GetTexture();
        Texture2D glowTexture2D = _glowTexture.Value;
        Texture2D bladeTexture2D = _bladeTexture.Value;
        Texture2D glowBladeTexture2D = _bladeGlowTexture.Value;
        Texture2D sparkTexture2D = ResourceManager.DefaultSparkle;
        Color color = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
        Rectangle? rectangle = new Rectangle?(new Rectangle(100 * (Projectile.spriteDirection != 1 ? 1 : 0), 0, 100, 100));
        Rectangle? glowRectangle = new Rectangle?(new Rectangle(0, 0, 100, 100));
        bool flag = Projectile.ai[1] < 3f;
        Vector2 velocityTo = Vector2.Normalize(Projectile.velocity);
        SpriteBatch spriteBatch = Main.spriteBatch;
        float charge = Math.Clamp(_charge - 0.5f + _charge * 1.5f, 0f, 1f);
        float osc = (float)Math.Sin(TimeSystem.TimeForVisualEffects * 10f);
        Vector2 offset = -new Vector2((_released ? 20f : 18f) * (Projectile.localAI[2] - 0.5f), 0f).RotatedBy(Projectile.rotation);
        offset += Projectile.rotation.ToRotationVector2() * (-30f * (MathHelper.Clamp(Projectile.scale - 1.25f, 0f, 1f)));
        Player player = Main.player[Projectile.owner];
        Vector2 center = Projectile.Center + Vector2.UnitY * player.gfxOffY;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        Vector2 position = center - Main.screenPosition + offset;
        Vector2 shiftFix = -(Projectile.spriteDirection == -1 ? new Vector2(0, -2) : Vector2.Zero);
        if (flag) {
            spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            spriteBatch.Draw(glowBladeTexture2D,
                                center - velocityTo * 10f + new Vector2(osc, osc) + offset - Main.screenPosition + shiftFix,
                                glowRectangle,
                                color * Projectile.Opacity * 0.6f * charge,
                                Projectile.rotation + 0.78f,
                                new Vector2(0f, texture2D.Height),
                                Projectile.scale * 1.05f,
                                SpriteEffects.None,
                                0f);
            spriteBatch.Begin(snapshot, true);
            spriteBatch.Draw(bladeTexture2D,
                            position - velocityTo.RotatedBy(MathHelper.TwoPi * (TimeSystem.TimeForVisualEffects * 5f % 1f * player.direction)) * 3f * charge + shiftFix,
                            glowRectangle,
                            color * Projectile.Opacity * 0.3f * charge,
                            Projectile.rotation + 0.78f,
                            new Vector2(0f, texture2D.Height),
                            Projectile.scale,
                            SpriteEffects.None,
                            0f);
        }
        else {
            spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            for (int i = 1; i < Projectile.oldPos.Length - 1; i += 2) {
                spriteBatch.Draw(bladeTexture2D,
                            position - center + Projectile.oldPos[i],
                            glowRectangle,
                            color * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length) * 0.6f * charge,
                                Projectile.oldRot[i] + 0.78f,
                            new Vector2(0f, texture2D.Height),
                            Projectile.scale,
                            SpriteEffects.None,
                            0f);
            }
            spriteBatch.Begin(snapshot, true);
        }
        spriteBatch.Begin(snapshot with { blendState = BlendState.AlphaBlend }, true);
        spriteBatch.Draw(texture2D,
                         position,
                         rectangle,
                         color,
                         Projectile.rotation + 0.78f,
                         new Vector2(0f, texture2D.Height),
                         Projectile.scale,
                         SpriteEffects.None,
                         0f);
        spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
        spriteBatch.Draw(glowTexture2D,
                         position + new Vector2(osc, osc),
                         glowRectangle,
                         color * 0.5f,
                         Projectile.rotation + 0.78f,
                         new Vector2(0f, texture2D.Height),
                         Projectile.scale,
                         SpriteEffects.None,
                         0f);
        spriteBatch.Begin(snapshot, true);
        if (flag) {
            spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            spriteBatch.Draw(glowTexture2D,
                             position + velocityTo * 15f + shiftFix,
                             glowRectangle,
                             color * 0.35f * charge,
                             Projectile.rotation + 0.78f,
                             new Vector2(0f, texture2D.Height),
                             Projectile.scale * 0.6f,
                             SpriteEffects.None,
                             0f);
            Texture2D bloom = ResourceManager.Bloom;
            spriteBatch.Draw(bloom,
                             center + shiftFix + offset + Vector2.Normalize(Projectile.velocity) * 110f * Projectile.scale - Main.screenPosition,
                             null,
                             color * Projectile.Opacity * 0.65f * charge,
                             Projectile.rotation + 0.78f,
                             bloom.Size() / 2f,
                             Projectile.scale * 1.05f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.Begin(snapshot, true);
        }
        color = Color.White;
        if (!flag) {
            spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            spriteBatch.Draw(sparkTexture2D,
                             center + offset + Vector2.Normalize(Projectile.rotation.ToRotationVector2()) * 127.5f * Projectile.scale - Main.screenPosition,
                             null,
                             color * Projectile.Opacity * 1f * charge,
                             MathHelper.TwoPi * (TimeSystem.TimeForVisualEffects * 0.8f % 1f) * -Projectile.direction,
                             new Vector2(sparkTexture2D.Width / 2, sparkTexture2D.Height / 2),
                             Projectile.scale * 0.8f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.Draw(sparkTexture2D,
                             center + offset + Vector2.Normalize(Projectile.rotation.ToRotationVector2()) * 127.5f * Projectile.scale - Main.screenPosition,
                             null,
                             color * Projectile.Opacity * 1f * charge,
                             MathHelper.TwoPi * (TimeSystem.TimeForVisualEffects * 0.8f % 1f) * -Projectile.direction + MathHelper.PiOver2,
                             new Vector2(sparkTexture2D.Width / 2, sparkTexture2D.Height / 2),
                             Projectile.scale * 0.8f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.Begin(snapshot, true);
        }

        float opacity = MathHelper.Clamp(_timingProgress, 0f, 1f);
        if (_timingProgress > 1.5f) {
            opacity = MathHelper.Clamp(1f - (_timingProgress - 0.5f - 1f) * 2f, 0f, 1f);
        }
        spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
        Vector2 position2 = new Vector2(90 + (Projectile.direction == 1 ? -0 : 0), 90 + (Projectile.direction == 1 ? -4 : 0)).RotatedBy(Projectile.rotation - 0.78f);
        if (Projectile.direction == 1) {
            position2 += Vector2.Normalize(Projectile.rotation.ToRotationVector2()) * 3f;
        }
        spriteBatch.Draw(sparkTexture2D,
                            center + offset + position2 * Projectile.scale - Main.screenPosition,
                            null,
                            color * Projectile.Opacity * 1f * opacity,
                            MathHelper.TwoPi * (TimeSystem.TimeForVisualEffects * 0.8f % 1f) * -Projectile.direction,
                            new Vector2(sparkTexture2D.Width / 2, sparkTexture2D.Height / 2),
                            Projectile.scale * 0.8f,
                            SpriteEffects.None,
                            0f);
        spriteBatch.Draw(sparkTexture2D,
                            center + offset + position2 * Projectile.scale - Main.screenPosition,
                            null,
                            color * Projectile.Opacity * 1f * opacity,
                            MathHelper.TwoPi * (TimeSystem.TimeForVisualEffects * 0.8f % 1f) * -Projectile.direction + MathHelper.PiOver2,
                            new Vector2(sparkTexture2D.Width / 2, sparkTexture2D.Height / 2),
                            Projectile.scale * 0.8f,
                            SpriteEffects.None,
                            0f);
        spriteBatch.Begin(snapshot, true);

        return false;
    }

    private class WaveSlash : ModProjectile {
        public override string Texture => ResourceManager.MeleeProjectileTextures + "FlederSlayer_Slash";

        public override bool PreDraw(ref Color lightColor) {
            SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
            Main.spriteBatch.BeginBlendState(BlendState.AlphaBlend);
            Texture2D texture2D = Projectile.GetTexture();
            bool flag = Main.player[Projectile.owner].gravDir == -1f;
            var effects = Main.player[Projectile.owner].direction == -1 ? (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically) :
                (SpriteEffects.None | SpriteEffects.FlipVertically);
            Main.spriteBatch.Draw(texture2D,
                                  Projectile.Center + new Vector2(0f, 0f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                                  null,
                                  Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16 - 2) * Projectile.Opacity,
                                  Projectile.rotation + (flag ? 0f : 0f),
                                  new Vector2(texture2D.Width / 2f, texture2D.Height),
                                  Projectile.scale,
                                  Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                                  0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
            return false;
        }

        public override void SetDefaults() {
            int width = 10; int height = 50;
            Projectile.Size = new Vector2(width, height);
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 70;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;

            Projectile.noEnchantmentVisuals = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if (new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - (int)(75f * Projectile.scale), (int)(20 * Projectile.scale), (int)(100f * Projectile.scale)).
                Intersects(targetHitbox)) {
                return true;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            Projectile.damage = (int)((double)Projectile.damage * 0.8);
        }

        public override bool? CanCutTiles() => true;

        //public override bool? CanDamage() => Projectile.Opacity >= 0.25f;

        public override void CutTiles() {
            Utils.PlotTileLine(Projectile.position,
                Projectile.position - Vector2.UnitY * (100f * Projectile.scale),
                20f * Projectile.scale, new Utils.TileActionAttempt(DelegateMethods.CutTiles));
        }

        public override void AI() {
            Rectangle rectangle = Utils.CenteredRectangle(
            Projectile.position - Vector2.UnitY * (20f * Projectile.scale),
            new Vector2(30f * Projectile.scale, 90f * Projectile.scale));
            Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);

            if (Projectile.owner == Main.myPlayer) {
                if (Main.rand.NextBool(5)) {
                    Projectile.ai[2]--;
                    if (Main.rand.NextBool(10)) {
                        Projectile.ai[2]--;
                    }
                    Projectile.netUpdate = true;
                }
            }
            Projectile.scale = Projectile.ai[0];
            if (Projectile.owner == Main.myPlayer && Projectile.ai[1] == 0f) {
                Projectile.ai[1] = Main.rand.NextFloat(0.7f, 1f);
                Projectile.ai[2] = Projectile.timeLeft;
                Projectile.netUpdate = true;
            }
            if (Projectile.ai[2] > 0f) {
                Projectile.ai[2]--;
            }
            else if (Projectile.ai[1] != 0f) {
                Projectile.Kill();
            }
            Projectile.Opacity = Utils.GetLerpValue(70f, 65f, Projectile.ai[2], clamped: true) * Utils.GetLerpValue(0f, 40f, Projectile.ai[2], clamped: true) * Projectile.ai[1];
            Projectile.scale -= Main.rand.NextFloat(0.001f, 0.0035f);
            Projectile.scale *= Main.rand.NextFloat(0.985f, 0.995f);
            if (Projectile.ai[2] <= 55) {
                Projectile.ai[2]--;
            }
            Projectile.position += Vector2.Normalize(Projectile.velocity) * 2f;
            Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
            Player player = Main.player[Projectile.owner];
            Projectile.velocity *= 0.9f + Math.Clamp((player.GetTotalAttackSpeed(DamageClass.Melee) - 1f) * 0.05f, 0f, 1f);
            float y = player.Center.Y - 10f;
            while (!WorldGenHelper.SolidTile((int)(Projectile.Center.X + Projectile.width / 5 * Projectile.direction * player.gravDir) / 16, (int)y / 16)) {
                y += 1;
            }
            Projectile.position.Y = MathHelper.Lerp(Projectile.position.Y, y - 22f, 0.5f);
            int amt = 3;
            float acc = Math.Clamp(Projectile.velocity.Length() / 5f, 0.5f, 1.25f);
            if (Projectile.velocity.Length() > 2f && Main.rand.NextBool(amt + 2)) {
                for (int i = 0; i < amt; i++) {
                    Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + new Vector2(Main.rand.Next(-i, i) * Projectile.width, Main.rand.NextFloat(Projectile.height)),
                        ModContent.DustType<Steam>(),
                        Vector2.UnitX * Main.WindForVisuals + new Vector2(0.6f * -Projectile.direction, -Main.rand.NextFloat(0.7f, 1.6f) * Main.rand.NextFloat(2f, 4f) * Main.rand.NextFloat(0.9f, 1.5f)) * Projectile.Opacity * acc, 0, Color.White, Main.rand.NextFloat(0.2f, 0.5f) * Main.rand.NextFloat(1.1f, 2f) * Main.rand.NextFloat(1.1f, 2f) * Main.rand.NextFloat(1.1f, 2f) / 2f * acc);
                }
            }
            Dust d = Dust.NewDustPerfect(Projectile.BottomRight - new Vector2(0f, Projectile.height / 3f + 6f), ModContent.DustType<Slash>(), new Vector2(0.6f * -Projectile.direction, -Main.rand.NextFloat(0.7f, 1.6f) * Main.rand.NextFloat(2f, 4f) * Main.rand.NextFloat(0.9f, 1.5f)) * Projectile.Opacity * acc, Main.rand.Next(120, 200), Color.White * Main.rand.NextFloat(0.4f, 0.9f) * 0.4f, 3f - Main.rand.NextFloat(0.2f, 0.5f) * Main.rand.NextFloat(1.1f, 2f) * Main.rand.NextFloat(1.1f, 2f) * Main.rand.NextFloat(1.1f, 2f) * 1.5f * acc);
            d.noLight = d.noLightEmittence = true;
            d.noGravity = true;
            float strength = 2f * Projectile.Opacity * acc;
            Point point = Projectile.TopLeft.ToTileCoordinates();
            Point point2 = Projectile.BottomRight.ToTileCoordinates();
            for (int i = point.X; i <= point2.X; i++) {
                for (int j = point.Y; j <= point2.Y; j++) {
                    if (Main.rand.NextBool(30 + (int)Math.Clamp(100f * (2f - Projectile.scale), 0f, 1f))) {
                        Tile tileSafely = Framing.GetTileSafely(i, j + 1);
                        int amount = (int)(WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j + 1) * Projectile.Opacity * acc);
                        for (int k = 0; k < amount + 2; k++) {
                            Dust dust = Main.dust[WorldGen.KillTile_MakeTileDust(i, j + 1, tileSafely)];
                            dust.position.Y -= 25f;
                            dust.position.Y *= Main.rand.NextFloat(1.5f, 2f);
                            dust.scale *= 0.8f;
                        }
                        for (int k = 0; k < amount + 2; k++) {
                            Dust dust = Main.dust[WorldGen.KillTile_MakeTileDust(i, j + 1, tileSafely)];
                            dust.position.X -= 20f * -Projectile.direction;
                            dust.position.X *= Main.rand.NextFloat(1.5f, 2f);
                            dust.scale *= 0.8f;
                        }
                        for (int k = 0; k < amount; k++) {
                            Dust dust = Main.dust[WorldGen.KillTile_MakeTileDust(i, j + 1, tileSafely)];
                            dust.position.Y -= 9f;
                            dust.velocity.Y -= 3f + (float)strength * 1.5f;
                            dust.velocity.Y *= Main.rand.NextFloat();
                            dust.velocity.Y *= 0.75f;
                            dust.scale += 0.085f;
                        }
                        for (int k = 0; k < amount; k++) {
                            Dust dust = Main.dust[WorldGen.KillTile_MakeTileDust(i, j + 1, tileSafely)];
                            dust.position.Y -= 9f;
                            dust.velocity.Y -= 10f + (float)strength * 1.5f * acc;
                            dust.velocity.Y *= Main.rand.NextFloat();
                            dust.velocity.Y *= 0.75f;
                            dust.scale += 0.085f;
                        }
                        if (strength >= 2) {
                            for (int i2 = 0; i2 < amount - 1; i2++) {
                                Dust dust2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j + 1, tileSafely)];
                                dust2.position.Y -= 9f;
                                dust2.velocity.Y -= 1f + (float)strength * acc;
                                dust2.velocity.Y *= Main.rand.NextFloat();
                                dust2.velocity.Y *= 0.75f;
                            }
                        }
                        if (strength >= 2) {
                            for (int i2 = 0; i2 < amount - 1; i2++) {
                                Dust dust2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j + 1, tileSafely)];
                                dust2.position.Y -= 9f;
                                dust2.velocity.Y -= 1f + (float)strength * acc;
                                dust2.velocity.Y *= Main.rand.NextFloat();
                                dust2.velocity.Y *= 0.75f;
                            }
                        }
                    }
                }
            }
        }
    }
}