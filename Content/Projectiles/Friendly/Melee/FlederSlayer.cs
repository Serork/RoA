using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Utilities;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class FlederSlayer : ModProjectile {
    private TrailInfo[] _trails;
    private float _charge;
    private Vector2 _offset;
    private float _extraRotation;
    private int _extraRotationDir;
    private bool _released;
    private SlotId _slot;

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
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_released);
        writer.WriteVector2(_offset);
        writer.Write(_charge);
        writer.Write(_extraRotation);
        writer.Write(_extraRotationDir);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _released = reader.ReadBoolean();
        _offset = reader.ReadVector2();
        _charge = reader.ReadSingle();
        _extraRotation = reader.ReadSingle();
        _extraRotationDir = reader.ReadInt32();
    }

    public override bool? CanCutTiles() {
        DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        Utils.TileActionAttempt plot = new Utils.TileActionAttempt(DelegateMethods.CutTiles);
        Vector2 center = Projectile.Center;
        return Utils.PlotTileLine(center, center + Projectile.rotation.ToRotationVector2() * 150f, Projectile.width * Projectile.scale, plot);
    }

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

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        player.itemAnimation = player.itemTime = 2;
        player.heldProj = player.itemAnimationMax;
        float playerDirection = player.direction;
        if (player.HeldItem.type != ModContent.ItemType<Items.Weapons.Melee.FlederSlayer>()) {
            Projectile.Kill();
        }
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;
            float scale = Main.player[Projectile.owner].CappedMeleeScale();
            if (scale != 1f) {
                Projectile.localAI[2] *= scale;
                Projectile.scale *= scale;
            }
        }
        Lighting.AddLight(Projectile.Center + new Vector2(90, 90).RotatedBy(Projectile.rotation - 0.78f) * Projectile.scale, 
        Color.White.ToVector3() * MathHelper.Clamp(_charge * 2f, 0f, 1f) * 0.35f);
        int itemAnimationMax = 40;
        int min = itemAnimationMax / 2 - itemAnimationMax / 4;
        Projectile.Opacity = Utils.GetLerpValue(itemAnimationMax, itemAnimationMax - 7, Projectile.timeLeft, clamped: true) * Utils.GetLerpValue(0f, 7f, Projectile.timeLeft, clamped: true);
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            Projectile.spriteDirection = player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
            Projectile.Center = player.Center + new Vector2(10f * player.direction, 0);
            Projectile.timeLeft = itemAnimationMax;
        }
        bool lastSlash = Projectile.ai[1] < 3f;
        bool lastSlash2 = Projectile.ai[1] < 13f;
        bool turnOnAvailable = false;
        if (player.dead || !player.active) {
            Projectile.Kill();
        }
        else {
            Projectile.Center = player.Center + Vector2.Normalize(Projectile.velocity);
            float dir = (float)(Math.PI / 2.0 + (double)playerDirection * 1.0);
            float offset = MathHelper.Pi / 1.15f * playerDirection;
            SoundStyle style = new SoundStyle(ResourceManager.ItemSounds + "Whisper") { Volume = 1.15f };
            if (Projectile.timeLeft > min) {
                CalculateExtraRotation();

                Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.localAI[2] * 1.15f, 0.1f);

                bool flag = player.channel && Projectile.Opacity == 1f;
                if (flag) {
                    if (_charge <= 0f) {
                        _slot = SoundEngine.PlaySound(style, Projectile.Center);
                    }
                    _charge += 0.01f;
                    _charge = Math.Clamp(_charge, 0f, 1f);
                }
                if (flag || ++Projectile.ai[0] < 10f) {
                    Projectile.timeLeft = min + 5;

                    player.velocity.X *= 0.975f;
                }

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, (dir - offset).ToRotationVector2() * 50f, 0.1f);
                Projectile.velocity += Projectile.DirectionFrom(player.Center) * 3f;

                _offset = Vector2.SmoothStep(_offset, new Vector2(-10f * playerDirection, 12f), 3f);

                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else if (Projectile.timeLeft == min) {
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

                Projectile.ai[0] = itemAnimationMax;

                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            else {
                if (SoundEngine.TryGetActiveSound(_slot, out var sound)) {
                    sound.Volume -= 0.01f;
                }

                if (Projectile.ai[0] > 30f) {
                    Projectile.timeLeft = 10;

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
                        Projectile.timeLeft = 10;
                    }
                    if (Projectile.ai[1] < 2f) {
                        Projectile.spriteDirection = player.direction;

                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, (MathHelper.PiOver2 - offset).ToRotationVector2() * 100f, 0.45f);

                        turnOnAvailable = true;

                        CalculateExtraRotation();

                        Projectile.ai[1] = 1f;

                        player.velocity.X *= 0.935f;

                        //if (Projectile.owner == Main.myPlayer && Main.mouseLeft && Main.mouseLeftRelease) {
                        //    Main.NewText(123);
                        //    Projectile.ai[1] = 2f;

                        //    SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                        //}

                        Projectile.ai[1] = 2f;

                        //SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

                        player.bodyFrame.Y = player.bodyFrame.Height * 3;
                    }
                    else if (Projectile.ai[1] == 2f) {
                        turnOnAvailable = false;

                        player.direction = Projectile.spriteDirection;

                        if (SoundEngine.TryGetActiveSound(_slot, out sound)) {
                            sound.Stop();
                        }

                        _offset = Vector2.SmoothStep(_offset, Vector2.Zero, 0.25f);

                        RotateArm();

                        if (Projectile.ai[0] > 0f) {
                            Projectile.ai[0] -= 1.25f;
                        }
                        else {
                            Projectile.ai[1] = 3f;

                            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "HeavySword") { Volume = 0.75f }, Projectile.Center);
                        }

                        Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.localAI[2] * 1.5f, 0.1f);

                        float progress = Swing(1f - Projectile.ai[0] / 75f);
                        offset = MathHelper.Pi / 2f * playerDirection;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, (progress * MathHelper.Pi - MathHelper.PiOver2 + offset * 1.35f).ToRotationVector2() * 130f, 0.075f);
                    }
                    else {
                        if (SoundEngine.TryGetActiveSound(_slot, out sound)) {
                            sound.Stop();
                        }

                        SlashDusts();

                        RotateArm();

                        Projectile.ai[1] += 1f;

                        float progress = Swing(1f - Projectile.ai[1] / 45f);
                        float slow = Math.Clamp(Projectile.ai[1] >= 10f ? Helper.EaseInOut4((Projectile.ai[1] - 10f) / 10f) : 1f, 0f, 1f);
                        float slow2 = Math.Clamp(Swing(Projectile.ai[1] >= 12f ? Projectile.ai[1] - 12f / 12f : 1f), 0f, 1f);
                        float extraRotation = playerDirection * 0.35f * Helper.EaseInOut3(progress) * slow * slow2;
                        Vector2 extra = Vector2.Normalize(Projectile.velocity) * -((Projectile.rotation + (playerDirection != 1 ? MathHelper.Pi : 0f)) * playerDirection).ToRotationVector2() * 165f * Projectile.localAI[2];
                        Vector2 projectileCenter = Projectile.Center + extra;
                        //Dust.NewDustDirect(projectileCenter, 0, 0, DustID.Dirt);
                        if (!_released && Projectile.ai[1] <= 16f) {
                            if (Projectile.ai[1] > 4f && WorldGen.SolidTile(Math.Clamp((int)projectileCenter.X / 16, 1, Main.maxTilesX), Math.Clamp((int)projectileCenter.Y / 16 + 1, 1, Main.maxTilesY))) {
                                _released = true;

                                if (_charge > 0.35f) {
                                    for (int i = 0; i < Main.rand.Next(2, 4) + (int)(_charge * 3); i++) {
                                        Projectile.NewProjectileDirect(Projectile.GetSource_FromAI("Fleder Slayer Slash"),
                                                                       projectileCenter - extra / 2f + new Vector2(i * Main.rand.Next(5, 21)),
                                                                       Helper.VelocityToPoint(player.Center, projectileCenter, 35f * _charge),
                                                                       ModContent.ProjectileType<WaveSlash>(),
                                                                       (int)((Projectile.damage + Projectile.damage / 2) * (_charge * 1.15f + 0.15f)),
                                                                       Projectile.knockBack,
                                                                       Projectile.owner,
                                                                       Main.rand.NextFloat(1f, 1.75f) * Main.rand.NextFloat(1.1f, 1.8f) * (_charge * 1.15f + 0.15f));
                                    }
                                    if (Main.netMode != NetmodeID.Server && Main.myPlayer == Projectile.owner) {
                                        string tag = "Fleder Slayer Stomp";
                                        float strength = Main.rand.NextFloat(15f, 26f) / 3f * (_charge * 1.15f + 0.15f);
                                        PunchCameraModifier punchCameraModifier = new PunchCameraModifier(projectileCenter, MathHelper.PiOver2.ToRotationVector2(), strength, 10f, 20, 1000f, tag);
                                        Main.instance.CameraModifiers.Add(punchCameraModifier);
                                    }
                                    SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Clang") { Volume = 0.95f }, Projectile.Center);
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
        if (!turnOnAvailable) {
            player.direction = Projectile.spriteDirection;
        }
        Projectile.Center += _offset;
        int trailTimeLeft = 5;
        _trails[0] = new TrailInfo(trailTimeLeft, Projectile.Opacity, Projectile.scale * Main.rand.NextFloat(1.1f, 1.3f), Projectile.Center, Projectile.rotation);
        Projectile.oldPos[0] = _trails[0].Position;
        Projectile.oldRot[0] = _trails[0].Rotation;
        for (int i = _trails.Length - 1; i > 0; i--) {
            _trails[i] = _trails[i - 1];
            _trails[i].TimeLeft--;
            _trails[i].Opacity = _trails[i].TimeLeft / trailTimeLeft;
            _trails[i].Scale -= 0.035f;
        }
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
            Projectile.oldPos[i] = Projectile.oldPos[i - 1];
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
        }
    }

    private void SlashDusts() {
        if (_released) {
            return;
        }
        for (int i = 0; i < 4; i++) {
            Vector2 rotation = Projectile.rotation.ToRotationVector2();
            Vector2 velocity = rotation.RotatedBy(MathHelper.PiOver2 * -Main.player[Projectile.owner].direction) * Main.rand.NextFloat(2f, 12f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + rotation * 10f + rotation * Main.rand.NextFloat(10f, 120f * Projectile.scale), ModContent.DustType<Slash>(), velocity, Math.Max(Main.rand.Next(70, 120) * (int)(_charge * 255f), 40), Color.White * Math.Max(0.3f, _charge));
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.scale *= Projectile.scale * 1f;
            dust.fadeIn = dust.scale + 0.1f;
            dust.noGravity = true;
        }
    }

    private struct TrailInfo {
        public int TimeLeft { get; set; }
        public float Opacity { get; set; }
        public float Scale { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public TrailInfo(int timeLeft, float opacity, float scale, Vector2 position, float rotation) {
            TimeLeft = timeLeft;
            Opacity = opacity;
            Scale = scale;
            Position = position;
            Rotation = rotation;
        }
    }

    private void RotateArm() {
        Player player = Main.player[Projectile.owner];
        float armRotation = Projectile.rotation - MathHelper.PiOver2;
        if (armRotation > 1.1f) {
            player.bodyFrame.Y = player.bodyFrame.Height;
        }
        else if (armRotation > 0.5f) {
            player.bodyFrame.Y = player.bodyFrame.Height * 2;
        }
        else if (armRotation < -0.5f) {
            player.bodyFrame.Y = player.bodyFrame.Height * 4;
        }
        else {
            player.bodyFrame.Y = player.bodyFrame.Height * 3;
        }
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
    }

    private static float Swing(float progress) {
        if (progress > 0.5f) {
            return 0.5f - Swing(0.5f - (progress - 0.5f)) + 0.5f;
        }
        return ((float)Math.Sin(Math.Pow(progress, 2f) * MathHelper.TwoPi - MathHelper.PiOver2) + 1f) / 2f;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        => modifiers.FinalDamage *=
                            Projectile.ai[1] >= 3f
                          ? Projectile.scale * 3f * (1f + _charge)
                          : 1f;

    public override bool ShouldUpdatePosition()
        => false;

    public override bool? CanDamage() {
        if (Main.player[Projectile.owner].channel) {
            return false;
        }
        if (_released) {
            return false;
        }
        int min = Main.player[Projectile.owner].itemAnimationMax / 2 - Main.player[Projectile.owner].itemAnimationMax / 4;
        if (Projectile.timeLeft == min + 5 || Projectile.ai[1] == 1f) {
            return false;
        }
        return Projectile.timeLeft <= min + 4 || Projectile.ai[1] >= 3f;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float coneLength = 130f * Projectile.scale;
        float maximumAngle = 0.3926991f;
        float coneRotation = Projectile.rotation;
        bool result = targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle);
        return result;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture2D = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Texture2D glowTexture2D = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");
        Texture2D bladeTexture2D = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Blade");
        Texture2D glowBladeTexture2D = (Texture2D)ModContent.Request<Texture2D>(Texture + "_BladeGlow");
        Texture2D sparkTexture2D = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Spark");
        Color color = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
        Rectangle? rectangle = new Rectangle?(new Rectangle(100 * (Projectile.spriteDirection != 1 ? 1 : 0), 0, 100, 100));
        Rectangle? glowRectangle = new Rectangle?(new Rectangle(0, 0, 100, 100));
        bool flag = Projectile.ai[1] < 3f;
        Vector2 velocityTo = Vector2.Normalize(Projectile.velocity);
        SpriteBatch spriteBatch = Main.spriteBatch;
        float charge = Math.Clamp(_charge - 0.5f + _charge * 1.5f, 0f, 1f);
        float osc = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);
        Vector2 offset = -new Vector2(15f * (Projectile.localAI[2] - 0.5f), 0f).RotatedBy(Projectile.rotation);
        Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY) +
            offset
            ;
        if (flag) {
            spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(glowBladeTexture2D,
                                Projectile.Center - velocityTo * 10f + new Vector2(osc, osc) + offset - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
                                glowRectangle,
                                color * Projectile.Opacity * 0.6f * charge,
                                Projectile.rotation + 0.78f,
                                new Vector2(0f, texture2D.Height),
                                Projectile.scale * 1.05f,
                                SpriteEffects.None,
                                0f);
            spriteBatch.EndBlendState();
            Vector2 shiftFix = Projectile.spriteDirection == -1 ? new Vector2(2, 0) : Vector2.Zero;
            spriteBatch.Draw(bladeTexture2D,
                            position - velocityTo.RotatedBy(MathHelper.TwoPi * (Main.GlobalTimeWrappedHourly * 5f * charge % 1f)) * 3f * charge - shiftFix,
                            glowRectangle,
                            color * Projectile.Opacity * 0.3f * charge,
                            Projectile.rotation + 0.78f,
                            new Vector2(0f, texture2D.Height),
                            Projectile.scale,
                            SpriteEffects.None,
                            0f);
        }
        else {
            for (int i = 1; i < Projectile.oldPos.Length - 1; i += 2) {
                spriteBatch.Draw(bladeTexture2D,
                            position - Projectile.Center + Projectile.oldPos[i],
                            glowRectangle,
                            color * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length) * 0.3f * charge,
                                Projectile.oldRot[i] + 0.78f,
                            new Vector2(0f, texture2D.Height),
                            Projectile.scale,
                            SpriteEffects.None,
                            0f);
            }
        }
        spriteBatch.Draw(texture2D,
                         position,
                         rectangle,
                         color,
                         Projectile.rotation + 0.78f,
                         new Vector2(0f, texture2D.Height),
                         Projectile.scale,
                         SpriteEffects.None,
                         0f);
        spriteBatch.BeginBlendState(BlendState.Additive);
        spriteBatch.Draw(glowTexture2D,
                         position + new Vector2(osc, osc),
                         glowRectangle,
                         color * 0.5f,
                         Projectile.rotation + 0.78f,
                         new Vector2(0f, texture2D.Height),
                         Projectile.scale,
                         SpriteEffects.None,
                         0f);
        spriteBatch.EndBlendState();
        if (flag) {
            spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(glowTexture2D,
                             position + velocityTo * 15f,
                             glowRectangle,
                             color * 0.35f * charge,
                             Projectile.rotation + 0.78f,
                             new Vector2(0f, texture2D.Height),
                             Projectile.scale * 0.6f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.EndBlendState();
            Texture2D bloom = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.Textures + "Bloom0");
            spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(bloom,
                             Projectile.Center + offset + Vector2.Normalize(Projectile.velocity) * 110f * Projectile.scale - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
                             null,
                             color * Projectile.Opacity * 0.65f * charge,
                             Projectile.rotation + 0.78f,
                             bloom.Size() / 2f,
                             Projectile.scale * 1.05f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.EndBlendState();
        }
        else {
            spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(sparkTexture2D,
                             Projectile.Center + offset + new Vector2(90, 90).RotatedBy(Projectile.rotation - 0.78f) * Projectile.scale - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
                             null,
                             color * Projectile.Opacity * 0.7f * charge,
                             MathHelper.TwoPi * (Main.GlobalTimeWrappedHourly * 0.8f % 1f),
                             new Vector2(sparkTexture2D.Width / 2, sparkTexture2D.Height / 2),
                             Projectile.scale * 0.8f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.Draw(sparkTexture2D,
                             Projectile.Center + offset + new Vector2(90, 90).RotatedBy(Projectile.rotation - 0.78f) * Projectile.scale - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY),
                             null,
                             color * Projectile.Opacity * 0.7f * charge,
                             MathHelper.TwoPi * (Main.GlobalTimeWrappedHourly * 0.8f % 1f) + MathHelper.PiOver2,
                             new Vector2(sparkTexture2D.Width / 2, sparkTexture2D.Height / 2),
                             Projectile.scale * 0.8f,
                             SpriteEffects.None,
                             0f);
            spriteBatch.EndBlendState();
        }
        return false;
    }

    private sealed class WaveSlash : ModProjectile {
        public override string Texture => ResourceManager.Textures + "FlederSlayerSlash";

        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture2D = (Texture2D)ModContent.Request<Texture2D>(Texture);
            Main.spriteBatch.Draw(texture2D,
                                  Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                                  null,
                                  Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16 - 2) * Projectile.Opacity,
                                  Projectile.rotation,
                                  new Vector2(texture2D.Width / 2f, texture2D.Height),
                                  Projectile.scale,
                                  Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                                  0f);
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
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
            => hitbox = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - (int)(75f * Projectile.scale), (int)(20 * Projectile.scale), (int)(100f * Projectile.scale));

        private float _Opacity = Main.rand.NextFloat(0.7f, 1f);

        public override void AI() {
            if (Main.rand.NextBool(5)) {
                Projectile.timeLeft--;
                if (Main.rand.NextBool(10)) {
                    Projectile.timeLeft--;
                }
            }
            if (Projectile.localAI[0] == 0f) {
                Projectile.scale = Projectile.ai[0];
                Projectile.localAI[0] = 1f;
            }
            Projectile.Opacity = Utils.GetLerpValue(70f, 65f, Projectile.timeLeft, clamped: true) * Utils.GetLerpValue(0f, 40f, Projectile.timeLeft, clamped: true) * _Opacity;
            Projectile.scale -= Main.rand.NextFloat(0.001f, 0.0035f);
            Projectile.scale *= Main.rand.NextFloat(0.985f, 0.995f);
            if (Projectile.timeLeft <= 55) {
                Projectile.timeLeft--;
            }
            Projectile.position += Vector2.Normalize(Projectile.velocity) * 2f;
            Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
            Projectile.velocity *= 0.9f;
            Player player = Main.player[Projectile.owner];
            float y = player.Center.Y - 10f;
            while (!WorldGen.SolidTile((int)(Projectile.Center.X + Projectile.width / 5 * Projectile.direction) / 16, (int)y / 16)) {
                y++;
            }
            Projectile.position.Y = y - 22f;
            int amt = 3;
            float acc = Math.Clamp(Projectile.velocity.Length() / 5f, 0.5f, 1.25f);
            if (Projectile.velocity.Length() > 2f && Main.rand.NextBool(amt + 2)) {
                for (int i = 0; i < amt; i++) {
                    Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + new Vector2(Main.rand.Next(-i, i) * Projectile.width, Main.rand.NextFloat(Projectile.height)), ModContent.DustType<Steam>(), new Vector2(0.6f * -Projectile.direction, -Main.rand.NextFloat(0.7f, 1.6f) * Main.rand.NextFloat(2f, 4f) * Main.rand.NextFloat(0.9f, 1.5f)) * Projectile.Opacity * acc, 0, Color.White, Main.rand.NextFloat(0.2f, 0.5f) * Main.rand.NextFloat(1.1f, 2f) * Main.rand.NextFloat(1.1f, 2f) * Main.rand.NextFloat(1.1f, 2f) / 2f * acc);
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