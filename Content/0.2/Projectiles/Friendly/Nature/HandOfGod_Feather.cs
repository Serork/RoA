using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class GodFeather : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 600;
    public static ushort DEBUFFTIME => 60;
    private static ushort MINTIMELEFT => 30;

    public enum GodFeatherRequstedTextureType : byte {
        Base,
        Glow,
        Eye,
        Base_New,
        Base_New_Glow,
        Base_New_Eye,
        Base_Sun,
        Base_Sun_Glow,
        Base_Sun_Eye
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)GodFeatherRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "GodFeather"),
         ((byte)GodFeatherRequstedTextureType.Glow, ResourceManager.NatureProjectileTextures + "GodFeather_Glow"),
         ((byte)GodFeatherRequstedTextureType.Eye, ResourceManager.NatureProjectileTextures + "GodFeather_Eye"),
         ((byte)GodFeatherRequstedTextureType.Base_New, ResourceManager.NatureProjectileTextures + "GodFeather_New"),
         ((byte)GodFeatherRequstedTextureType.Base_New_Glow, ResourceManager.NatureProjectileTextures + "GodFeather_New_Glow"),
         ((byte)GodFeatherRequstedTextureType.Base_New_Eye, ResourceManager.NatureProjectileTextures + "GodFeather_New_Eye"),
         ((byte)GodFeatherRequstedTextureType.Base_Sun, ResourceManager.NatureProjectileTextures + "GodFeather_New_Sun"),
         ((byte)GodFeatherRequstedTextureType.Base_Sun_Glow, ResourceManager.NatureProjectileTextures + "GodFeather_New_Sun_Glow"),
         ((byte)GodFeatherRequstedTextureType.Base_Sun_Eye, ResourceManager.NatureProjectileTextures + "GodFeather_New_Sun_Eye")];

    private readonly Vector2[] _scales = new Vector2[8];

    public ref float WaveValue => ref Projectile.localAI[1];
    public ref float RotationLerpValue => ref Projectile.localAI[2];

    public ref float ActivatedValue => ref Projectile.ai[0];
    public ref float Activated2Value => ref Projectile.ai[1];
    public ref float SpawnDirection => ref Projectile.ai[2];

    public static SoundStyle ActivationSound { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "DivineShieldAttack");
    public static SoundStyle DeathSound { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "DivineShieldDeath");
    public static SoundStyle FadeSound { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "DivineShieldFade");
    public static SoundStyle CastSound { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "DivineShieldCast");

    public bool Activated {
        get => ActivatedValue > 0f;
        set {
            if (ActivatedValue < 0.5f) {
                ActivatedValue = 1f;
                SoundEngine.PlaySound(ActivationSound with { PitchVariance = 0.1f }, Projectile.Center);
            }
        }
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;

        Projectile.rotation = 0f;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        ActivatedValue = Helper.Approach(ActivatedValue, 0f, TimeSystem.LogicDeltaTime);
        Activated2Value = Helper.Approach(Activated2Value, ActivatedValue, TimeSystem.LogicDeltaTime * 5f);

        Player owner = Projectile.GetOwnerAsPlayer();
        owner.SyncMousePosition();
        Vector2 mousePosition = owner.GetWorldMousePosition();
        Vector2 center = owner.Center;
        float distance = 50f;

        float moveFactor = MathUtils.Clamp01(1f - WaveValue);
        moveFactor = 1f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, center.DirectionTo(mousePosition) * distance, 0.05f * moveFactor);
        Projectile.Center = Utils.Floor(center) + Projectile.velocity + Vector2.UnitY * owner.gfxOffY;

        if (!owner.IsAlive() && Projectile.timeLeft > MINTIMELEFT) {
            Projectile.timeLeft = MINTIMELEFT;
        }

        int direction = Projectile.DirectionTo(owner.Center).X.GetDirection();
        Projectile.SetDirection(direction);

        //if (SpawnDirection == 0f) {
        //    SpawnDirection = Projectile.direction;
        //}
        if (SpawnDirection == 0f) {
            float scale = owner.CappedMeleeOrDruidScale();
            Projectile.scale = scale * 1.1f;

            SoundEngine.PlaySound(CastSound with { Volume = 0.5f }, Projectile.Center);
        }
        SpawnDirection = 1f;

        RotationLerpValue = Helper.Approach(RotationLerpValue, 0.0375f * (1f + 0.5f * Activated2Value) * Projectile.direction, TimeSystem.LogicDeltaTime);
        float rotationValue = RotationLerpValue;
        //Projectile.rotation += rotationValue;
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (WaveValue == 0) {
            for (int i = 0; i < _scales.Length; i++) {
                _scales[i] = Vector2.One;
            }
        }

        WaveValue += TimeSystem.LogicDeltaTime;

        if (Projectile.timeLeft == MINTIMELEFT) {
            SoundEngine.PlaySound(FadeSound with { Volume = 0.75f }, Projectile.Center);
        }

        int minDistance = (int)(TileHelper.TileSize * 7 * Projectile.scale);
        foreach (NPC activeNPC in Main.ActiveNPCs) {
            if (!activeNPC.CanBeChasedBy()) {
                continue;
            }
            if (activeNPC.Distance(Projectile.Center) < minDistance) {
                Activated = true;
                activeNPC.AddBuff<GodDescent>(DEBUFFTIME);
            }
        }
        // TODO: pvp support?
        foreach (Player activePlayer in Main.ActivePlayers) {
            if (activePlayer.whoAmI == owner.whoAmI) {
                continue;
            }
            if (activePlayer.Distance(Projectile.Center) < minDistance) {
                Activated = true;
                activePlayer.AddBuff<GodDescent>(DEBUFFTIME);
            }
        }
        minDistance = (int)(TileHelper.TileSize * 3 * Projectile.scale);
        foreach (Projectile activeProjectile in Main.ActiveProjectiles) {
            if (activeProjectile.type == Type) {
                continue;
            }
            if (activeProjectile.friendly) {
                continue;
            }
            if (activeProjectile.Distance(Projectile.Center) < minDistance) {
                Activated = true;
                activeProjectile.GetGlobalProjectile<GodDescent.GodDescent_ProjectileHandler>().IsEffectActive = true;
            }
        }

        float spawnProgress = Ease.CircOut(Utils.GetLerpValue(TIMELEFT, TIMELEFT - 20, Projectile.timeLeft, true));
        int count = 8;
        for (int i = 0; i < count; i++) {
            float baseRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            //baseRotation += MathHelper.Pi;
            float partRotation = (float)i / count * MathHelper.TwoPi;
            float spawnProgress3 = spawnProgress;
            if (i == count / 4 || i == count - 2) {
                spawnProgress3 = 1f;
            }
            float rotation = Utils.AngleLerp(baseRotation, partRotation + Projectile.rotation, spawnProgress3 * SpawnDirection);
            Vector2 position = Projectile.Center;
            position += Vector2.UnitY.RotatedBy(Projectile.rotation - MathHelper.PiOver2) * 50f;
            float distance2 = 26f;
            position += Vector2.UnitY.RotatedBy(rotation) * distance2;
            float fadeOutProgress = Utils.GetLerpValue(0, 50, Projectile.timeLeft, true);
            float fadeOutProgress3 = Utils.GetLerpValue(0f, 0.375f, fadeOutProgress, true);

            Vector3 lightColor = Color.LightYellow.ToVector3() * spawnProgress * MathHelper.Lerp(0.5f, 0.75f, Activated2Value) * fadeOutProgress3 * Projectile.scale;

            Lighting.AddLight(position, lightColor);

            if (i == 0) {
                Lighting.AddLight(Projectile.Center, lightColor);
            }
        }
    }

    public override void OnKill(int timeLeft) {
        
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<GodFeather>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        int max = TIMELEFT;
        float spawnProgress = Ease.CircOut(Utils.GetLerpValue(max, max - 40, Projectile.timeLeft, true));
        float spawnProgress2 = Ease.CircOut(Utils.GetLerpValue(max, max - 60, Projectile.timeLeft, true));
        float fadeOutProgress = Utils.GetLerpValue(0, MINTIMELEFT, Projectile.timeLeft, true);
        float fadeOutProgress2 = Utils.GetLerpValue(0.75f, 0.2f, fadeOutProgress, true);
        float fadeOutProgress3 = Utils.GetLerpValue(0f, 0.375f, fadeOutProgress, true);
        float fadeOutProgress4 = Utils.GetLerpValue(0f, 0.2f, fadeOutProgress, true);
        float lightingModifier = MathHelper.Lerp(0.375f, 0.625f, Activated2Value);

        Texture2D baseTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base].Value,
                  glowTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Glow].Value,
                  eyeTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Eye].Value;

        Texture2D baseTexture_New = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base_New].Value,
                  baseTexture_New_Glow = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base_New_Glow].Value,
                  baseTexture_New_Eye = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base_New_Eye].Value,
                  sunTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base_Sun].Value,
                  sunGlowTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base_Sun_Glow].Value,
                  sunEyeTexture = indexedTextureAssets[(byte)GodFeatherRequstedTextureType.Base_Sun_Eye].Value;

        float backgroundEffectOpacity = 1f;
        float sunEyeOpacity = 1f,
              featherEyeOpacity = 1f;

        SpriteBatch batch = Main.spriteBatch;
        int count = 8;
        float waveOffset = Projectile.whoAmI;
        List<(float, Vector2)> positions = [];
        List<Vector2> positions2 = [];
        float distance = 16f;
        for (int i = 0; i < count; i++) {
            float baseRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            //baseRotation += MathHelper.Pi;
            float partRotation = (float)i / count * MathHelper.TwoPi;
            float spawnProgress3 = spawnProgress;
            if (i == count / 4 || i == count - 2) {
                spawnProgress3 = 1f;
            }
            float rotation = Utils.AngleLerp(baseRotation, partRotation + Projectile.rotation, spawnProgress3 * SpawnDirection);
            float rotation2 = Utils.AngleLerp(baseRotation, partRotation, spawnProgress3 * SpawnDirection);
            Vector2 position = Projectile.Center;
            positions2.Add(position + Vector2.UnitY.RotatedBy(rotation2) * distance);
            position += Vector2.UnitY.RotatedBy(rotation) * distance;
            positions.Add((rotation, position));
        }
        List<Vector2> scales = [];
        for (int i = 0; i < positions.Count; i++) {
            Vector2 baseScale = Vector2.One;
            Vector2 position = positions2[i];

            float distanceY = MathF.Abs(position.Y - Projectile.Center.Y);
            float distanceX = MathF.Abs(position.X - Projectile.Center.X);
            if (distanceY < distance / 2) {
                baseScale.Y *= 0.7f;
            }
            else if (distanceX < distance / 2) {
                baseScale.X *= 0.7f;
            }
            else {
                baseScale.X *= 0.775f;
                baseScale.Y *= 0.775f;
            }
            scales.Add(baseScale);
        }
        for (int i = 0; i < scales.Count; i++) {
            _scales[i] = Helper.Approach(_scales[i], scales[i], TimeSystem.LogicDeltaTime);
        }

        Color getColor(bool glow = false) {
            Color color = Color.Lerp(Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Color.White, lightingModifier * glow.ToInt()) * 0.875f;
            color.A = (byte)MathHelper.Lerp(255, 185, Activated2Value);
            color.A = (byte)MathHelper.Lerp(color.A, 100, fadeOutProgress2);
            color *= spawnProgress2;
            color *= fadeOutProgress3;
            return color;
        }

        Rectangle sunClip = sunTexture.Bounds;
        Vector2 sunOrigin = sunClip.Centered();
        Vector2 sunPosition = Projectile.Center;
        Color sunColor = getColor(),
              sunGlowColor = getColor(true) * 0.5f;
        Vector2 sunScale = Vector2.One * Projectile.scale * fadeOutProgress4 * 0.75f;
        float sunRotation = Projectile.rotation;
        DrawInfo sunDrawInfo = DrawInfo.Default with {
            Clip = sunClip,
            Origin = sunOrigin,
            Color = sunColor,
            Scale = sunScale,
            Rotation = sunRotation
        };
        batch.Draw(sunTexture, sunPosition, sunDrawInfo);
        batch.Draw(sunGlowTexture, sunPosition, sunDrawInfo with {
            Color = sunGlowColor
        });
        batch.Draw(sunEyeTexture, sunPosition, sunDrawInfo with {
            Color = sunDrawInfo.Color * sunEyeOpacity
        });

        float getScaleWave(int i) => Helper.Wave(WaveValue, 0.9f, 1.1f, 2.5f, waveOffset + i * count);
        for (int i = 0; i < positions.Count; i++) {
            float rotation = positions[i].Item1;
            Vector2 position = positions[i].Item2;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = clip.BottomCenter();
            Color color = getColor();
            rotation += MathHelper.Pi;
            Vector2 baseScale = _scales[i] * Projectile.scale;
            Vector2 scale = baseScale * fadeOutProgress4;
            DrawInfo drawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            };

            float maxRotation = 0.3f;

            bool second = i % 2 == 0;
            if (second) {
                batch.Draw(glowTexture, position, drawInfo with {
                    Scale = scale * 1.5f,
                    Color = Color.LightYellow.MultiplyRGB(getColor(true)).MultiplyAlpha(Helper.Wave(WaveValue, 0.25f, 0.75f, 10f, waveOffset)) * 0.5f * spawnProgress * 0.375f * backgroundEffectOpacity,
                    Rotation = rotation + Helper.Wave(WaveValue, -maxRotation, maxRotation, 5f, waveOffset)
                });
            }
            //batch.Draw(baseTexture, position, drawInfo with {
            //    Scale = scale * getScaleWave(i)
            //});
            //batch.Draw(glowTexture, position, drawInfo with {
            //    Color = getColor(true) * 0.5f,
            //    Scale = scale * getScaleWave(i)
            //});
            batch.Draw(baseTexture_New, position, drawInfo with {
                Scale = scale * getScaleWave(i)
            });
            batch.Draw(baseTexture_New_Glow, position, drawInfo with {
                Color = getColor(true) * 0.5f,
                Scale = scale * getScaleWave(i)
            });
            if (!second) {
                batch.Draw(glowTexture, position, drawInfo with {
                    Scale = scale * 1.5f * Helper.Wave(WaveValue, 0.75f, 1.25f, 5f, waveOffset + i * count),
                    Color = getColor(true).MultiplyAlpha(Helper.Wave(WaveValue, 0.25f, 0.75f, 10f, waveOffset)) * 0.625f * spawnProgress * 0.375f * backgroundEffectOpacity,
                    Rotation = rotation + MathHelper.PiOver4 + Helper.Wave(WaveValue, -maxRotation, maxRotation, 5f, waveOffset)
                });
            }
        }

        for (int i = 0; i < positions.Count; i++) {
            float rotation = positions[i].Item1;
            Vector2 position = positions[i].Item2;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = clip.BottomCenter();
            Color color = Color.Lerp(new Color(61, 72, 73), Color.Yellow, Activated2Value).MultiplyRGB(Color.Lerp(Lighting.GetColor(position.ToTileCoordinates()), Color.White, lightingModifier));
            color.A = (byte)MathHelper.Lerp(color.A, 100, fadeOutProgress2);
            color *= spawnProgress2;
            color *= fadeOutProgress3;
            color *= featherEyeOpacity;
            rotation += MathHelper.Pi;
            Vector2 baseScale = _scales[i] * Projectile.scale;
            Vector2 scale = baseScale * fadeOutProgress4;
            DrawInfo drawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Rotation = rotation,
                Scale = scale
            };
            scale *= getScaleWave(i);
            float scale2 = Helper.Wave(WaveValue, 1.05f, 1.5f, 5f, waveOffset + i * count);
            scale *= scale2;
            position += Vector2.UnitY.RotatedBy(rotation) * 3f * scale2;
            //batch.Draw(eyeTexture, position, drawInfo with {
            //    Scale = scale,
            //    Origin = origin
            //});
            batch.Draw(baseTexture_New_Eye, position, drawInfo with {
                Scale = scale,
                Origin = origin
            });
        }
    }
}
