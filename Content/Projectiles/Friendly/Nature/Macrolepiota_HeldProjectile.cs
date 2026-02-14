using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Macrolepiota_HeldProjectile : NatureProjectile_NoTextureLoad, DruidPlayerShouldersFix.IProjectileFixShoulderWhileActive {
    private const byte FRAMECOUNT = 4;

    private static byte ANIMATIONSPEEDINTICKS => 4;
    private static byte SPORESPAWNCOOLDOWN => 10;

    private static Asset<Texture2D>? _holdTexture, _holdGlowMaskTexture;

    public enum MacrolepiotaFrame : byte {
        Idle,
        Shrivel1,
        Shrivel2,
        PuffUp1,
        Count
    }

    public ref struct MacrolepiotaValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];

        public ref float ScaleX = ref projectile.scale;
        public ref float ScaleY = ref projectile.localAI[1];
        public ref float SpeedX = ref projectile.ai[0];

        public ref float GlowMaskOpacityValue = ref projectile.localAI[2];

        public ref float SporeSpawnTimerValue = ref projectile.ai[1];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public float GlowMaskOpacity {
            readonly get => GlowMaskOpacityValue;
            set => GlowMaskOpacityValue = MathUtils.Clamp01(value);
        }

        public readonly MacrolepiotaFrame Frame {
            get => (MacrolepiotaFrame)projectile.frame;
            set => projectile.frame = (int)value;
        }

        public readonly Vector2 Scale => new(ScaleX, ScaleY);

        public readonly bool CanSpawnSpore {
            get {
                bool result = false;
                if (--SporeSpawnTimerValue < 0) {
                    result = true;
                    SporeSpawnTimerValue = SPORESPAWNCOOLDOWN;
                }
                return result;
            }
        }
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.netImportant = true;
    }

    public override void Load() {
        LoadMacrolepiotaTextures();
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        void init() {
            MacrolepiotaValues macrolepiotaValues = new(Projectile);
            if (!macrolepiotaValues.Init) {
                macrolepiotaValues.Init = true;

                macrolepiotaValues.ScaleX = macrolepiotaValues.ScaleY = 1f;
            }
        }
        void checkActive() {
            owner.heldProj = Projectile.whoAmI;

            Projectile.timeLeft = 2;

            if (!owner.IsHolding<Macrolepiota>() || owner.GetFormHandler().IsInADruidicForm) {
                Projectile.Kill();
            }
            if (!owner.IsAliveAndFree()) {
                Projectile.Kill();
            }

            owner.UseBodyFrame(Core.Data.PlayerFrame.Use2);

            Projectile.Center = owner.GetPlayerCorePoint() + new Vector2(owner.direction == -1 ? 4f : 0f, 0f);

            owner.GetWreathHandler().YAdjustAmountInPixels = -40;
        }
        void playShrivelAnimation() {
            MacrolepiotaValues macrolepiotaValues = new(Projectile);
            if (++Projectile.frameCounter > ANIMATIONSPEEDINTICKS) {
                Projectile.frameCounter = 0;
                if (++macrolepiotaValues.Frame > MacrolepiotaFrame.Shrivel2) {
                    macrolepiotaValues.Frame = MacrolepiotaFrame.Shrivel2;
                }
            }
        }
        void playPuffUpAnimation() {
            MacrolepiotaValues macrolepiotaValues = new(Projectile);
            if (Projectile.frame > 0) {
                if (++Projectile.frameCounter > ANIMATIONSPEEDINTICKS) {
                    Projectile.frameCounter = 0;
                    if (++macrolepiotaValues.Frame > MacrolepiotaFrame.PuffUp1) {
                        macrolepiotaValues.Frame = MacrolepiotaFrame.Idle;
                    }
                }
            }
        }
        float velocityFactor = owner.velocity.X;
        MacrolepiotaValues macrolepiotaValues = new(Projectile);
        if (MathF.Abs(macrolepiotaValues.SpeedX - velocityFactor) > 0.2f) {
            macrolepiotaValues.SpeedX = velocityFactor;
        }
        velocityFactor = macrolepiotaValues.SpeedX;
        float maxSpeedX = 5f;
        float maxRotation = 0.35f;
        float macrolepiotaRotation = Utils.Remap(-velocityFactor, -maxSpeedX, maxSpeedX, -maxRotation, maxRotation);
        void setRotationAndScale() {
            float approachSpeed = MathHelper.Pi;
            ref float projectileRotation = ref Projectile.rotation;
            MacrolepiotaValues macrolepiotaValues = new(Projectile);
            projectileRotation = Helper.Approach(projectileRotation, Utils.Remap(-velocityFactor, -maxSpeedX, maxSpeedX, -maxRotation, maxRotation), approachSpeed);
            Vector2 desiredScale = Vector2.One;
            approachSpeed = 0.06f;
            if (IsOwnerJumping()) {
                desiredScale.X = 1.2f;
                desiredScale.Y = 0.8f;

                approachSpeed *= 0.5f;
            }
            if (IsOwnerFalling()) {
                desiredScale.X = 0.9f;
                desiredScale.Y = 1.1f;

                approachSpeed *= 0.5f;

                playShrivelAnimation();
            }
            else {
                playPuffUpAnimation();
            }
            macrolepiotaValues.ScaleX = Helper.Approach(macrolepiotaValues.ScaleX, desiredScale.X, approachSpeed);
            macrolepiotaValues.ScaleY = Helper.Approach(macrolepiotaValues.ScaleY, desiredScale.Y, approachSpeed);
        }
        void directArm() {
            float armRotation = MathHelper.Pi + macrolepiotaRotation;
            owner.SetCompositeBothArms(armRotation);
        }
        void spawnSporeDusts() {
            if (!IsOwnerFalling()) {
                return;
            }

            if (!Main.rand.NextBool(5)) {
                return;
            }

            bool shouldUseGlowingMushroomDust = Main.rand.NextBool(4);
            int dustType = shouldUseGlowingMushroomDust ? ModContent.DustType<Dusts.GlowingMushroom>() : DustID.MushroomSpray;
            Vector2 dustVelocity = -new Vector2(Main.rand.NextFloatRange(0.5f), 1f).RotatedBy(Projectile.rotation) * Main.rand.NextFloat(1f, owner.velocity.Y) + Main.rand.NextVector2Circular(4f, 4f);
            Dust dust = Dust.NewDustPerfect(GetShrivelPosition(), dustType, dustVelocity);
            dust.noGravity = true;
            dust.noLightEmittence = true;
            dust.scale = Main.rand.NextFloat(1f, 2f);
            if (shouldUseGlowingMushroomDust) {
                dust.noLight = true;
            }
        }
        void lightUp() {
            DelegateMethods.v3_1 = new Vector3(0.1f, 0.4f, 1f);
            Utils.PlotTileLine(Vector2.Lerp(Projectile.Center, GetShrivelPosition(), 0.25f), GetShrivelPosition(), 4f, DelegateMethods.CastLightOpen);

            MacrolepiotaValues macrolepiotaValues = new(Projectile);
            float desiredGlowMaskOpacity = 0f;
            //if (IsActive()) {
            //    desiredGlowMaskOpacity = 1f;
            //}
            macrolepiotaValues.GlowMaskOpacity = MathHelper.Lerp(macrolepiotaValues.GlowMaskOpacity, desiredGlowMaskOpacity, 0.1f);
        }
        void spawnSpores() {
            if (!owner.IsLocal()) {
                return;
            }

            if (!IsOwnerFalling()) {
                return;
            }

            MacrolepiotaValues macrolepiotaValues = new(Projectile);
            if (!macrolepiotaValues.CanSpawnSpore) {
                return;
            }

            float sporeSpawnSpeed = 2f;
            ProjectileUtils.SpawnPlayerOwnedProjectile<Spore>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_NaturalSpawn()) with {
                Position = GetShrivelPosition(),
                Velocity = -new Vector2(Main.rand.NextFloatRange(0.5f), 1f).RotatedBy(Projectile.rotation) * owner.velocity.Y * sporeSpawnSpeed
            });
            SoundEngine.PlaySound(SoundID.Item7 with { Volume = 3f, Pitch = 1f, PitchVariance = 0.2f, MaxInstances = 3 }, Projectile.Center);
        }

        init();
        checkActive();
        setRotationAndScale();
        directArm();
        spawnSporeDusts();
        lightUp();
        spawnSpores();
    }

    protected override void Draw(ref Color lightColor) {
        if (_holdTexture?.IsLoaded != true || _holdGlowMaskTexture?.IsLoaded != true) {
            return;
        }

        Texture2D holdTexture = _holdTexture.Value, holdGlowMaskTexture = _holdGlowMaskTexture.Value;
        SpriteFrame spriteFrame = new SpriteFrame(1, FRAMECOUNT).With(0, (byte)Projectile.frame);
        Vector2 position = Projectile.Center;
        MacrolepiotaValues macrolepiotaValues = new(Projectile);
        Color color = Color.Lerp(lightColor, Color.White, macrolepiotaValues.GlowMaskOpacity);
        Rectangle clip = spriteFrame.GetSourceRectangle(holdTexture);
        float rotation = Projectile.rotation;
        Vector2 scale = macrolepiotaValues.Scale;
        Vector2 origin = new(clip.Width / 2f, clip.Height);
        Main.spriteBatch.Draw(holdTexture, position, DrawInfo.Default with {
            Origin = origin,
            Scale = scale,
            Color = color,
            Rotation = rotation,
            Clip = clip
        });
        Color glowMaskColor = Color.Lerp(Color.White, Color.Transparent, macrolepiotaValues.GlowMaskOpacity);
        Main.spriteBatch.Draw(holdGlowMaskTexture, position, DrawInfo.Default with {
            Origin = origin,
            Scale = scale,
            Color = glowMaskColor,
            Rotation = rotation,
            Clip = clip
        });
    }

    public override void OnKill(int timeLeft) { }

    private void LoadMacrolepiotaTextures() {
        if (Main.dedServ) {
            return;
        }

        string texturePath = ResourceManager.NatureProjectileTextures + "Macrolepiota_Hold";
        _holdTexture = ModContent.Request<Texture2D>(texturePath);
        _holdGlowMaskTexture = ModContent.Request<Texture2D>(texturePath + "_Glow");
    }

    private bool IsActive() => IsOwnerFalling() || IsOwnerJumping();
    private bool IsOwnerFalling() => Projectile.GetOwnerAsPlayer().velocity.Y > 0f;
    private bool IsOwnerJumping() => Projectile.GetOwnerAsPlayer().velocity.Y < -0.1f;

    private Vector2 GetShrivelPosition() {
        Vector2 result = Projectile.Center;
        MacrolepiotaValues macrolepiotaValues = new(Projectile);
        float macrolepiotaSize = 64f;
        switch (macrolepiotaValues.Frame) {
            case MacrolepiotaFrame.Idle:
                macrolepiotaSize *= 1f;
                break;
            case MacrolepiotaFrame.Shrivel1 or MacrolepiotaFrame.PuffUp1:
                macrolepiotaSize *= 1.07f;
                break;
            case MacrolepiotaFrame.Shrivel2:
                macrolepiotaSize *= 1f;
                break;
        }
        macrolepiotaSize *= macrolepiotaValues.ScaleY;
        result += (-Vector2.UnitY.RotatedBy(Projectile.rotation)) * macrolepiotaSize;
        return result;
    }
}