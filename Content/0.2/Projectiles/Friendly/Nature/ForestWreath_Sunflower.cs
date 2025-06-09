using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Sunflower : NatureProjectile {
    private static byte PETALCOUNT => 10;
    private static short TIMELEFT => 360;

    private static Asset<Texture2D>? _baseTexture, _petalTexture, _rayTexture;

    private struct PetalInfo {
        public int Index;
        public float Scale, MaxScale, ExtraScale, MaxExtraScale;
        public bool ExtraScaleDirection;
    }

    private PetalInfo[] _petalData = [];

    private ref float InitOnSpawnValue => ref Projectile.localAI[0];
    private ref float RandomRotationOnSpawn => ref Projectile.ai[0];
    private ref float PetalSpawnTimer => ref Projectile.localAI[1];
    private ref float BaseExtraScale => ref Projectile.localAI[2];

    public override string Texture => ResourceManager.EmptyTexture;

    public override void Load() {
        LoadSunflowerTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSize(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;
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
            Projectile.Opacity = Ease.SineInOut(Utils.GetLerpValue(0, TIMELEFT / 3, Projectile.timeLeft, true));
        }
        void initPetalsAndRandomRotation() {
            if (InitOnSpawnValue == 0f) {
                InitOnSpawnValue = 1f;

                _petalData = new PetalInfo[PETALCOUNT];
                for (int i = 0; i < PETALCOUNT; i++) {
                    ref PetalInfo petalData = ref _petalData[i];
                    float maxExtraScale = 1f;
                    petalData = new PetalInfo() {
                        Index = i,
                        MaxExtraScale = maxExtraScale,
                        ExtraScaleDirection = true
                    };
                }

                if (Projectile.IsOwnerLocal()) {
                    RandomRotationOnSpawn = Main.rand.NextFloatRange(MathHelper.PiOver2);
                    Projectile.netUpdate = true;
                }

                Projectile.rotation = RandomRotationOnSpawn;
            }
        }
        void scalePetals() {
            if (PetalSpawnTimer < 1f) {
                float appearanceFactor = 1.5f;
                float countSpeed = MathF.Pow(PETALCOUNT, appearanceFactor);
                PetalSpawnTimer += 1f / countSpeed;
                PetalSpawnTimer = MathF.Min(PetalSpawnTimer, 1f);
            }

            for (int i = 0; i < PETALCOUNT; i++) {
                ref PetalInfo petalData = ref _petalData[i];
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
                PetalInfo petalData = _petalData[i];
                maxScale += petalData.ExtraScale;
            }
            BaseExtraScale = maxScale * 0.1f;
        }

        makeSmoothDisappearOnDeath();
        initPetalsAndRandomRotation();
        scalePetals();
        givePlayersBuff();
    }

    public override bool PreDraw(ref Color lightColor) {
        if (_baseTexture?.IsLoaded != true || _petalTexture?.IsLoaded != true || _rayTexture?.IsLoaded != true) {
            return false;
        }

        lightColor = Color.Lerp(lightColor, Color.White, 0.85f);
        float baseOpacity = Ease.CubeInOut(Projectile.Opacity);
        Color petalColor = lightColor * Ease.ExpoIn(Projectile.Opacity),
              baseColor = lightColor * baseOpacity;
        float[] petalFills = CalculatePetalFills();
        void drawBase() {
            Texture2D baseTexture = _baseTexture!.Value;
            Main.spriteBatch.DrawWith(baseTexture, Projectile.Center, DrawInfo.Default with {
                Rotation = Projectile.rotation,
                Color = baseColor,
                Origin = baseTexture.Size() / 2f,
                Scale = Vector2.One * (Projectile.scale + BaseExtraScale * baseOpacity) * 1.35f,
                Clip = Rectangle.Empty with { Width = baseTexture.Width, Height = baseTexture.Height }
            });

            Lighting.AddLight(Projectile.Center, Vector3.One * 0.25f * baseOpacity);
        }
        void drawPetals() {
            Texture2D petalTexture = _petalTexture!.Value;
            for (int i = 0; i < PETALCOUNT; i++) {
                float petalFill = petalFills[i] * Projectile.Opacity;
                float petalRotation = Projectile.rotation + i * MathHelper.TwoPi / PETALCOUNT + 0.6f;
                float offsetValue = -5f;
                Vector2 offset = Vector2.UnitY.RotatedBy(petalRotation) * offsetValue;
                float extraScale = _petalData[i].ExtraScale * petalFill;
                Main.spriteBatch.DrawWith(petalTexture, Projectile.Center + offset, DrawInfo.Default with { 
                    Color = petalColor,
                    Rotation = petalRotation,
                    Origin = new Vector2(petalTexture.Width / 2f, petalTexture.Height),
                    Clip = Rectangle.Empty with { Width = petalTexture.Width, Height = petalTexture.Height },
                    Scale = new Vector2(Ease.CircOut(petalFill), petalFill + MathF.Sin(extraScale * 6f) / 6f)
                });
            }
        }
        void drawRays() {
            Texture2D rayTexture = _rayTexture!.Value;
            Main.spriteBatch.BeginBlendState(BlendState.Additive);
            int rayCount = 10;
            float maxScale = 0f;
            for (int i = 0; i < 6; i++) {
                PetalInfo petalData = _petalData[i];
                maxScale += petalData.ExtraScale;
            }
            for (int i = 0; i < rayCount; i++) {
                float petalFill = petalFills[i] * Projectile.Opacity;
                float rayRotation = Projectile.rotation + i * MathHelper.TwoPi / rayCount;
                float offsetValue = rayTexture.Height * 0.5f;
                Vector2 offset = Vector2.UnitY.RotatedBy(rayRotation) * offsetValue;
                float extraScale = MathF.Min(0.75f, MathF.Sin(maxScale));
                Main.spriteBatch.DrawWith(rayTexture, Projectile.Center + offset, DrawInfo.Default with {
                    Color = Utils.MultiplyRGB(Color.Yellow, petalColor) * 0.55f * Projectile.Opacity * petalFills[i] * PetalSpawnTimer,
                    Rotation = rayRotation - MathHelper.PiOver2,
                    Origin = new Vector2(0f, rayTexture.Height / 2f),
                    Clip = Rectangle.Empty with { Width = rayTexture.Width, Height = rayTexture.Height },
                    Scale = new Vector2(2f + extraScale * 0.5f, 1f * petalFill) * 0.5f * 0.85f
                });

                DelegateMethods.v3_1 = Vector3.One * 0.25f * baseOpacity * extraScale;
                Utils.PlotTileLine(Projectile.Center, Projectile.Center + offset * 0.5f, 10f * baseOpacity, DelegateMethods.CastLight);
            }
            Main.spriteBatch.EndBlendState();
        }

        drawRays();
        drawPetals();
        drawBase();

        return false;
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
        float[] fills = new float[PETALCOUNT]; 
        for (int k = 1; k <= PETALCOUNT; k++) {
            float lower = (k - 1) / (float)PETALCOUNT;
            float upper = k / (float)PETALCOUNT;
            if (PetalSpawnTimer >= upper) {
                fills[k - 1] = 1f;
            }
            else if (PetalSpawnTimer >= lower) {
                fills[k - 1] = (PetalSpawnTimer - lower) * PETALCOUNT;
            }
            else {
                fills[k - 1] = 0f;
            }
        }
        return fills;
    }
}
