using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class ForestWreath_Sunflower : NatureProjectile_NoTextureLoad {
    private static byte PETALCOUNT => 10;
    private static ushort MAXTIMELEFT => 360;

    private static Asset<Texture2D>? _baseTexture, _petalTexture, _rayTexture;

    private struct PetalInfo {
        public byte Index;
        public float Scale, MaxScale, ExtraScale, MaxExtraScale;
        public bool ExtraScaleDirection;
    }

    public ref struct SunflowerValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float RandomRotationOnSpawn = ref projectile.ai[0];
        public ref float PetalSpawnTimer = ref projectile.localAI[1];
        public ref float BaseExtraScale = ref projectile.localAI[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }
    }

    private PetalInfo[]? _petalData;

    public override void SetStaticDefaults() {
        LoadSunflowerTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;
    }

    public override void AI() {
        void givePlayersBuff() {
            float neededDistanceInPixels = 200f;
            foreach (Player player in Main.ActivePlayers) {
                if (player.Distance(Projectile.Center) < neededDistanceInPixels) {
                    player.AddBuff<SunflowerBuff>(2);
                }
            }
        }
        void makeSmoothDisappearOnDeath() {
            Projectile.Opacity = Ease.SineInOut(Utils.GetLerpValue(0, MAXTIMELEFT / 3, Projectile.timeLeft, true));
        }
        void initPetalsAndRandomRotation() {
            SunflowerValues sunflowerValues = new(Projectile);
            if (!sunflowerValues.Init) {
                sunflowerValues.Init = true;

                _petalData = new PetalInfo[PETALCOUNT];
                for (int i = 0; i < PETALCOUNT; i++) {
                    ref PetalInfo petalData = ref _petalData[i];
                    float maxExtraScale = 1f;
                    petalData = new PetalInfo() {
                        Index = (byte)i,
                        MaxExtraScale = maxExtraScale,
                        ExtraScaleDirection = true
                    };
                }

                if (Projectile.IsOwnerLocal()) {
                    sunflowerValues.RandomRotationOnSpawn = Main.rand.NextFloatRange(MathHelper.PiOver4);
                    Projectile.netUpdate = true;
                }

                Projectile.rotation = sunflowerValues.RandomRotationOnSpawn;
            }
        }
        void scalePetals() {
            SunflowerValues sunflowerValues = new(Projectile);
            if (sunflowerValues.PetalSpawnTimer < 1f) {
                float appearanceFactor = 1.5f;
                float countSpeed = MathF.Pow(PETALCOUNT, appearanceFactor);
                sunflowerValues.PetalSpawnTimer += 1f / countSpeed;
                sunflowerValues.PetalSpawnTimer = MathF.Min(sunflowerValues.PetalSpawnTimer, 1f);
            }

            for (int i = 0; i < PETALCOUNT; i++) {
                ref PetalInfo petalData = ref _petalData![i];
                float edge = petalData.MaxExtraScale * 0.2f;
                bool canScale = !(petalData.ExtraScale > edge * 1.1f || petalData.ExtraScale < -edge * 1.1f);
                if (canScale) {
                    petalData.ExtraScale += edge * 0.04f * petalData.ExtraScaleDirection.ToDirectionInt();
                }
                bool scaled = petalData.ExtraScale > edge || petalData.ExtraScale < -edge;
                if (scaled) {
                    petalData.ExtraScaleDirection = !petalData.ExtraScaleDirection;
                }
            }

            float maxScale = 0f;
            for (int i = 0; i < 6; i++) {
                PetalInfo petalData = _petalData![i];
                maxScale += petalData.ExtraScale;
            }
            sunflowerValues.BaseExtraScale = maxScale * 0.1f;
        }

        makeSmoothDisappearOnDeath();
        initPetalsAndRandomRotation();
        scalePetals();
        givePlayersBuff();
    }

    protected override void Draw(ref Color lightColor) {
        SunflowerValues sunflowerValues = new(Projectile);
        if (!sunflowerValues.Init) {
            return;
        }

        if (_baseTexture?.IsLoaded != true || _petalTexture?.IsLoaded != true || _rayTexture?.IsLoaded != true) {
            return;
        }

        lightColor = Color.Lerp(lightColor, Color.White, 0.85f);
        float baseOpacity = Ease.CubeInOut(Projectile.Opacity);
        Color petalColor = lightColor * Ease.ExpoIn(Projectile.Opacity),
              baseColor = lightColor * baseOpacity;
        float[] petalFills = CalculatePetalFills();
        void drawBaseAndAddLight() {
            SunflowerValues sunflowerValues = new(Projectile);
            Texture2D baseTexture = _baseTexture!.Value;
            Rectangle clip = baseTexture.Bounds;
            Vector2 origin = baseTexture.Size() / 2f;
            float rotation = Projectile.rotation;
            Vector2 scale = Vector2.One * (Projectile.scale + sunflowerValues.BaseExtraScale * baseOpacity) * 1.35f;
            Main.spriteBatch.Draw(baseTexture, Projectile.Center, DrawInfo.Default with {
                Rotation = rotation,
                Color = baseColor,
                Origin = origin,
                Scale = scale,
                Clip = clip
            });

            Lighting.AddLight(Projectile.Center, Vector3.One * 0.25f * baseOpacity);
        }
        void drawPetals() {
            Texture2D petalTexture = _petalTexture!.Value;
            Rectangle clip = petalTexture.Bounds;
            for (int i = 0; i < PETALCOUNT; i++) {
                float petalFill = petalFills[i] * Projectile.Opacity;
                float rotation = Projectile.rotation + i * MathHelper.TwoPi / PETALCOUNT + 0.6f;
                float offsetValue = -5f;
                Vector2 offset = Vector2.UnitY.RotatedBy(rotation) * offsetValue;
                float extraScale = _petalData![i].ExtraScale * petalFill;
                Vector2 origin = Utils.Bottom(clip);
                Vector2 scale = new(Ease.CircOut(petalFill), petalFill + MathF.Sin(extraScale * 6f) / 6f);
                Main.spriteBatch.Draw(petalTexture, Projectile.Center + offset, DrawInfo.Default with {
                    Color = petalColor,
                    Rotation = rotation,
                    Origin = origin,
                    Clip = clip,
                    Scale = scale
                });
            }
        }
        void drawRaysAndAddLights() {
            SunflowerValues sunflowerValues = new(Projectile);
            Texture2D rayTexture = _rayTexture!.Value;
            SpriteBatch batch = Main.spriteBatch;
            SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
            batch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            int rayCount = 10;
            float maxScale = 0f;
            for (int i = 0; i < 6; i++) {
                PetalInfo petalData = _petalData![i];
                maxScale += petalData.ExtraScale;
            }
            for (int i = 0; i < rayCount; i++) {
                float petalFill = petalFills[i] * Projectile.Opacity;
                float rayRotation = Projectile.rotation + i * MathHelper.TwoPi / rayCount;
                float offsetValue = rayTexture.Height * 0.5f;
                Vector2 offset = Vector2.UnitY.RotatedBy(rayRotation) * offsetValue;
                float extraScale = MathF.Min(0.75f, MathF.Sin(maxScale));
                Main.spriteBatch.Draw(rayTexture, Projectile.Center + offset, DrawInfo.Default with {
                    Color = Utils.MultiplyRGB(Color.Yellow, petalColor) * 0.55f * Projectile.Opacity * petalFills[i] * sunflowerValues.PetalSpawnTimer,
                    Rotation = rayRotation - MathHelper.PiOver2,
                    Origin = new Vector2(0f, rayTexture.Height / 2f),
                    Clip = rayTexture.Bounds,
                    Scale = new Vector2(2f + extraScale * 0.5f, 1f * petalFill) * 0.5f * 0.85f
                });

                DelegateMethods.v3_1 = Vector3.One * 0.25f * baseOpacity * extraScale;
                Utils.PlotTileLine(Projectile.Center, Projectile.Center + offset * 0.5f, 10f * baseOpacity, DelegateMethods.CastLight);
            }
            batch.Begin(snapshot, true);
        }

        drawRaysAndAddLights();
        drawPetals();
        drawBaseAndAddLight();
    }

    private void LoadSunflowerTextures() {
        if (Main.dedServ) {
            return;
        }

        _baseTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "SunflowerBase");
        _petalTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "SunflowerPetal");
        _rayTexture = ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "SunflowerRay");
    }

    private float[] CalculatePetalFills() {
        SunflowerValues sunflowerValues = new(Projectile);
        float[] fills = new float[PETALCOUNT];
        for (int k = 1; k <= PETALCOUNT; k++) {
            float lower = (k - 1) / (float)PETALCOUNT;
            float upper = k / (float)PETALCOUNT;
            if (sunflowerValues.PetalSpawnTimer >= upper) {
                fills[k - 1] = 1f;
            }
            else if (sunflowerValues.PetalSpawnTimer >= lower) {
                fills[k - 1] = (sunflowerValues.PetalSpawnTimer - lower) * PETALCOUNT;
            }
            else {
                fills[k - 1] = 0f;
            }
        }
        return fills;
    }
}
