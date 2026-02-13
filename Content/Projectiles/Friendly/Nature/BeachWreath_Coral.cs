using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BeachWreath_Coral : NatureProjectile_NoTextureLoad {
    private static byte CORALTEXTUREAMOUNT => 4;
    private static ushort MAXTIMELEFT => 300;
    private static byte PENETRATEAMOUNT => 5;

    private static Dictionary<CoralValues.CoralType, Asset<Texture2D>?>? _coralTextures;

    private static readonly HashSet<Vector2> _coralPositions = [];

    public static IReadOnlyCollection<Vector2> AllCoralsPositions => _coralPositions;

    public ref struct CoralValues(Projectile projectile) {
        public enum CoralType : byte {
            None,
            Blue,
            Red,
            Pink,
            Green,
            Count
        }

        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float WaveValue = ref projectile.localAI[1];
        public ref float CoralTypeValue = ref projectile.ai[0];
        public ref float DestinationPositionX = ref projectile.ai[1];
        public ref float DestinationPositionY = ref projectile.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public CoralType CurrentCoralType {
            readonly get => (CoralType)CoralTypeValue;
            set {
                CoralTypeValue = Utils.Clamp((byte)value, (byte)CoralType.None + 1, (byte)CoralType.Count - 1);
            }
        }

        public readonly Vector2 Destination => new(DestinationPositionX, DestinationPositionY);
        public readonly Vector2 FinalDestinationPosition => projectile.GetOwnerAsPlayer().MountedCenter + Destination;
    }

    public override void SetStaticDefaults() {
        LoadCoralTextures();
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(20);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = PENETRATEAMOUNT;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Y -= 10;
        hitbox.Inflate(6, 12);
    }

    public override void AI() {
        void randomizeTypeOnSpawn() {
            CoralValues coralValues = new(Projectile);
            if (!coralValues.Init) {
                coralValues.Init = true;

                if (Projectile.IsOwnerLocal()) {
                    coralValues.CurrentCoralType = Main.rand.GetRandomEnumValue<CoralValues.CoralType>();
                    Projectile.netUpdate = true;
                }
            }
        }
        void makeCoralFeelAlive() {
            _ = new CoralValues(Projectile) {
                WaveValue = Projectile.identity + (float)(Main.timeForVisualEffects / 10.0 % MathHelper.TwoPi)
            };
            Projectile.rotation = -Projectile.velocity.X * 0.1f;
        }
        void moveTowardsDestinationPosition() {
            CoralValues coralValues = new(Projectile);
            Helper.InertiaMoveTowards(ref Projectile.velocity, Projectile.Center, coralValues.FinalDestinationPosition);
            _coralPositions.Add(coralValues.Destination);
        }
        void makeBubbleDusts() {
            float chance = MathF.Max(0.15f, MathUtils.Clamp01(Projectile.velocity.Length() / 10f));
            chance /= 5f;
            int bubbleDustType = ModContent.DustType<Bubble>();
            Vector2 dustSpawnPosition = Projectile.position - new Vector2(8f, 0f);
            int spawnAreaWidth = Projectile.width + 4,
                spawnAreaHeight = 4;
            Vector2 dustVelocity = -Projectile.velocity * 0.5f;
            dustVelocity += -Vector2.One.RotatedBy(Main.rand.NextFloatRange(MathHelper.PiOver2)) * Main.rand.NextBool().ToDirectionInt();
            if (Main.rand.NextChance(chance * 0.5f)) {
                Dust.NewDustDirect(dustSpawnPosition, spawnAreaWidth, spawnAreaHeight, bubbleDustType, SpeedX: dustVelocity.X, SpeedY: dustVelocity.Y);
            }
            int waterDustAmount = (int)(1 + chance * 11.25f);
            int waterDustType = DustID.Water;
            for (int i = 0; i < waterDustAmount; i++) {
                if (Main.rand.NextChance(chance * 5f)) {
                    Dust.NewDustDirect(dustSpawnPosition, spawnAreaWidth, spawnAreaHeight, waterDustType, SpeedX: dustVelocity.X, SpeedY: dustVelocity.Y - 10f * Main.rand.NextFloat(), Alpha: 0, Scale: 1.25f - 0.25f * Main.rand.NextFloat());
                }
            }
        }

        randomizeTypeOnSpawn();
        makeCoralFeelAlive();
        moveTowardsDestinationPosition();
        makeBubbleDusts();
    }

    protected override void Draw(ref Color lightColor) {
        CoralValues coralValues = new(Projectile);
        if (!coralValues.Init) {
            return;
        }

        Dictionary<CoralValues.CoralType, Asset<Texture2D>?> coralTextures = _coralTextures!;
        Asset<Texture2D>? coralAsset = coralTextures[coralValues.CurrentCoralType];
        if (coralAsset?.IsLoaded != true) {
            return;
        }

        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
        SpriteBatch batch = Main.spriteBatch;
        ShaderLoader.WavyShader.WaveFactor = coralValues.WaveValue;
        ShaderLoader.WavyShader.StrengthX = 0.25f;
        ShaderLoader.WavyShader.StrengthY = 3f;
        ShaderLoader.WavyShader.DrawColor = color;
        ShaderLoader.WavyShader.Apply(batch, () => {
            Texture2D coralTexture = coralAsset.Value;
            float valueIn = Utils.GetLerpValue(MAXTIMELEFT, MAXTIMELEFT - 22, Projectile.timeLeft, true),
                  valueOut = Utils.GetLerpValue(0, 15, Projectile.timeLeft, true);
            float scaleY = Ease.QuartIn(valueIn) * Ease.QuartOut(valueOut),
                  scaleX = Ease.SineIn(valueIn) * Ease.SineOut(valueOut);
            Rectangle clip = coralTexture.Bounds;
            Vector2 origin = Utils.Bottom(clip);
            float rotation = Projectile.rotation;
            Vector2 scale = new(scaleX, scaleY);
            batch.Draw(coralTexture, Projectile.Center, DrawInfo.Default with {
                Rotation = rotation,
                Origin = origin,
                Clip = clip,
                Scale = scale
            });
        });
    }

    //public override bool? CanCutTiles() => false;

    public override void OnKill(int timeLeft) {
        CoralValues coralValues = new(Projectile);
        _coralPositions.Remove(coralValues.Destination);
    }

    private void LoadCoralTextures() {
        if (Main.dedServ) {
            return;
        }

        _coralTextures = [];
        for (int i = 0; i < CORALTEXTUREAMOUNT; i++) {
            _coralTextures.Add((CoralValues.CoralType)(i + 1), ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + $"Coral{i + 1}"));
        }
    }
}
