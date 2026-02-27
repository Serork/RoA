using Humanizer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Common.VisualEffects;
using RoA.Content.AdvancedDusts;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class YarnRenderTargetLoader : ILoadable {
    public static RenderTarget2D RenderTarget = null!;

    void ILoadable.Load(Mod mod) {
        Main.QueueMainThreadAction(() => {
            RenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, 1680, 1050, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        });

    }

    void ILoadable.Unload() {
        if (RenderTarget is not null) {
            Main.QueueMainThreadAction(() => {
                RenderTarget.Dispose();
            });

            RenderTarget = null!;
        }
    }
}

[AutoloadGlowMask]
sealed class FilamentYarn : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(46, 48);
        Item.SetWeaponValues(100, 5f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, autoReuse: false, showItemOnUse: false);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<FilamentYarn_Use>());
        Item.SetShopValues(ItemRarityColor.StrongRed10, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 200);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public sealed class FilamentYarn_Use : ModProjectile {
        private static float MAXLENGTH => 200f;
        private static ushort LINECOUNT => 5;
        private static float TENSIONMODIFIER => 0.75f;
        private static ushort ACTIVETIME => MathUtils.SecondsToFrames(5);

        public override string Texture => ItemLoader.GetItem(ModContent.ItemType<FilamentYarn>()).Texture;

        private (Vector2, Vector2?, float)[] _connectPoints = null!;
        private Vector2 _mousePosition;
        private float _tension;

        public ref float InitValue => ref Projectile.localAI[0];
        public ref float Cooldown => ref Projectile.ai[1];
        public ref float CurrentLength => ref Projectile.ai[0];

        public bool Init {
            get => InitValue != 0f;
            set => InitValue = value.ToInt();
        }

        public bool CanSpawnMoreLines => Projectile.ai[2] < LINECOUNT + 1;

        public override bool? CanDamage() => true;
        public override bool? CanCutTiles() => false;
        public override bool ShouldUpdatePosition() => false;

        public override void SetDefaults() {
            Projectile.SetSizeValues(10);
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.penetrate = -1;

            //Projectile.hide = true;
        }

        private static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawpos, Microsoft.Xna.Framework.Color drawColor, Microsoft.Xna.Framework.Color shineColor, float rotation, Vector2 scale, Vector2 fatness) {
            Texture2D value = TextureAssets.Extra[98].Value;
            Microsoft.Xna.Framework.Color color = shineColor * 0.5f;
            color.A = 0;
            Vector2 origin = value.Size() / 2f;
            Microsoft.Xna.Framework.Color color2 = drawColor * 0.5f;
            float num = 1f;
            Vector2 vector = new Vector2(fatness.X * 0.5f, scale.X) * num;
            Vector2 vector2 = new Vector2(fatness.Y * 0.5f, scale.Y) * num;
            color *= num * opacity;
            color2 *= num * opacity;
            Main.EntitySpriteDraw(value, drawpos, null, color, (float)Math.PI / 2f + rotation, origin, vector, dir);
            Main.EntitySpriteDraw(value, drawpos, null, color, 0f + rotation, origin, vector2, dir);
            Main.EntitySpriteDraw(value, drawpos, null, color2, (float)Math.PI / 2f + rotation, origin, vector * 0.6f, dir);
            Main.EntitySpriteDraw(value, drawpos, null, color2, 0f + rotation, origin, vector2 * 0.6f, dir);
        }

        public static float EaseBackOut(float t) {
            float s = 1.70158f;
            return 1 + (--t) * t * ((s + 1) * t + s);
        }

        public static float EaseBackIn(float t) {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }

        private float Tension => EaseBackIn(_tension);
        private float Tension2 => 1f - Tension;

        private void DrawConnectedLines() {
            SpriteBatch batch = Main.spriteBatch;
            Player owner = Projectile.GetOwnerAsPlayer();
            int index = 0;

            float opacity2 = 1f;
            float disappearOpacity = 1f;
            float opacity = MathF.Min(opacity2, disappearOpacity);
            Color baseColor = Color.White;
            baseColor = baseColor.MultiplyAlpha(1f - Utils.GetLerpValue(1f, 0.25f, opacity, true));
            float mainOpacity = Utils.GetLerpValue(0f, 0.5f, opacity, true) * 1f;
            mainOpacity *= Ease.CubeIn(disappearOpacity);
            baseColor *= mainOpacity;
            Color color = baseColor * Helper.Wave(0.5f, 0.75f, 5f, 0f);
            void drawLine(bool onlyStars = false) {
                index = 0;
                foreach ((Vector2, Vector2?, float) connectedPoints in _connectPoints) {
                    index++;
                    if (index > LINECOUNT + 1) {
                        continue;
                    }
                    Vector2 from = connectedPoints.Item1;
                    if (from == Vector2.Zero) {
                        continue;
                    }
                    float waveOffset = Projectile.identity * 2 + 1 + index * 3;
                    Vector2 to = connectedPoints.Item2 ?? _mousePosition;
                    void drawStar(Vector2 startPosition) {
                        float scaleFactor = Helper.Wave(0.625f, 0.75f, 5f, waveOffset) * 0.75f;
                        DrawPrettyStarSparkle(0.875f * scaleFactor, SpriteEffects.None, startPosition - Main.screenPosition,
                            Color.Lerp(new Color(127, 153, 22), new Color(251, 232, 193, 0), 0.75f),
                            Color.Lerp(new Color(127, 153, 22), new Color(233, 206, 83), 0.75f),
                            connectedPoints.Item3, new Vector2(2f, 2f) * scaleFactor, new Vector2(2f, 2f) * scaleFactor);
                    }
                    Vector2 startPosition = from;
                    if (onlyStars) {
                        drawStar(startPosition);
                        if (index == LINECOUNT + 1 && connectedPoints.Item2 is not null) {
                            drawStar(connectedPoints.Item2.Value);
                        }
                        if (to == _mousePosition) {
                            drawStar(_mousePosition);
                        }
                    }
                    if (onlyStars) {
                        continue;
                    }
                    float distance = from.Distance(to);
                    float length = MAXLENGTH;
                    float currentLength = MathF.Max(0f, MathF.Min(length, length - distance * 1f));
                    float sineX = TimeSystem.TimeForVisualEffects * 5f,
                          sineY = TimeSystem.TimeForVisualEffects * 5f;
                    SimpleCurve curve = new(from, to, Vector2.Zero);
                    curve.Control = (curve.Begin + curve.End) / 2f + new Vector2(0f, currentLength * Tension) + new Vector2(MathF.Sin(sineX), MathF.Sin(sineY)) * 2f;

                    Vector2 start = curve.Begin;
                    int count = 16;
                    start = curve.Begin;
                    for (int i = 1; i <= count; i++) {
                        Vector2 point = curve.GetPoint(i / (float)count);

                        Color baseColor2 = Color.Lerp(new Color(127, 153, 22), new Color(196, 182, 70), 0.75f);
                        Color baseColor3 = Color.Lerp(new Color(127, 153, 22), new Color(251, 232, 193), 1f);
                        baseColor2 = Color.Lerp(baseColor2, baseColor3, Helper.Wave(0f, 0.75f, 10f, waveOffset + i * 0.5f));
                        Color color2 = color.MultiplyRGBA(baseColor2);

                        float lineThickness = Helper.Wave(2f, 6f, 1f, waveOffset + i * 1f);
                        batch.Line(start, point, color2, lineThickness);
                        float num184 = Helper.Wave(2f, 4f, 1f, waveOffset);
                        for (int num185 = 0; num185 < 4; num185++) {
                            Vector2 offset = Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184;
                            batch.Line(start + offset, point + offset, new Color(64, 64, 64, 0).MultiplyRGBA(baseColor2) * 0.25f * mainOpacity, lineThickness);
                        }
                        start = point;
                    }
                }
            }
            drawLine();
            drawLine(true);
        }

        private void AddPoint(Vector2 position) {
            ref float currentIndex = ref Projectile.ai[2];
            if (_connectPoints.Length < LINECOUNT + 2) {
                Array.Resize(ref _connectPoints, _connectPoints.Length + 1);
            }
            if (currentIndex > LINECOUNT) {
                return;
            }
            _connectPoints[(int)currentIndex].Item2 = position;
            _connectPoints[(int)(++currentIndex)] = (position, null, _connectPoints[(int)currentIndex - 1].Item3);
        }

        private void SpawnStarDust(Vector2 from, float velocitySpeed = 1f) {
            Vector2 to = from;
            Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
            velocity *= Main.rand.NextFloat(0.5f, 1f) * velocitySpeed;
            FilamentYarnDust? filamentYarnDust = AdvancedDustSystem.New<FilamentYarnDust>(AdvancedDustLayer.ABOVEDUSTS)?
                .Setup(to,
                       velocity,
                       scale: 1.5f);
            if (filamentYarnDust != null) {
                filamentYarnDust.CorePosition = to;
            }
        }

        public override void AI() {
            Projectile.localAI[2] += 0.01f;

            Player player4 = Projectile.GetOwnerAsPlayer();

            if (player4.IsLocal()) {
                Vector2 mousePosition = player4.GetViableMousePosition();
                _mousePosition = Vector2.Lerp(_mousePosition, mousePosition, 0.25f);
                if (Init) {
                    if (Projectile.ai[2] > 0) {
                        Vector2 last = _connectPoints[(int)Projectile.ai[2] - 1].Item2!.Value;
                        if (_mousePosition.Distance(last) > MAXLENGTH * TENSIONMODIFIER) {
                            _mousePosition = last + last.DirectionTo(mousePosition) * MAXLENGTH * TENSIONMODIFIER;
                        }
                    }
                }
            }

            if (!Init) {
                Init = true;

                if (player4.IsLocal()) {
                    _mousePosition = Vector2.Lerp(_mousePosition, player4.GetViableMousePosition(), 1f);
                }

                _connectPoints = new (Vector2, Vector2?, float)[1];
                AddPoint(_mousePosition);
            }
            else {
                for (int i = 0; i < _connectPoints.Length; i++) {
                    if (i > LINECOUNT) {
                        continue;
                    }
                    Vector2 from = _connectPoints[i].Item1;
                    Vector2 to = _connectPoints[i].Item2 ?? _mousePosition;
                    _connectPoints[i].Item3 += (0.01f + 0.01f * Tension2) * from.DirectionTo(to).X.GetDirection();

                    if (Main.rand.NextBool(100)) {
                        SpawnStarDust(to + Main.rand.NextVector2CircularEdge(10f, 10f), 2f);
                    }

                    float distance = from.Distance(to);
                    float length = MAXLENGTH;
                    float currentLength = MathF.Max(0f, MathF.Min(length, length - distance * 1f));
                    float sineX = TimeSystem.TimeForVisualEffects * 5f,
                          sineY = TimeSystem.TimeForVisualEffects * 5f;
                    SimpleCurve curve = new(from, to, Vector2.Zero);
                    curve.Control = (curve.Begin + curve.End) / 2f + new Vector2(0f, currentLength * Tension) + new Vector2(MathF.Sin(sineX), MathF.Sin(sineY)) * 2f;
                    Vector2 start = curve.Begin;
                    int count = 16 / 2;
                    for (int i2 = 1; i2 <= count; i2++) {
                        Vector2 point = curve.GetPoint(i2 / (float)count);
                        Lighting.AddLight(point, new Color(196, 182, 70).ToVector3() * 1f);

                        if (Main.rand.NextBool(150)) {
                            SpawnStarDust(point);
                        }

                        start = point;
                    }
                }

                void addPoint() {
                    AddPoint(_mousePosition);
                }
                if (Main.mouseLeft && Main.mouseLeftRelease) {
                    addPoint();
                }

                if (Cooldown <= 0f) {
                    foreach ((Vector2, Vector2?, float) connectedPoints in _connectPoints) {
                        if (connectedPoints.Item2 is not null) {
                            continue;
                        }
                        Vector2 from = connectedPoints.Item1;
                        Vector2 to = _mousePosition;
                        float distance = from.Distance(to);
                        float length = MAXLENGTH;
                        CurrentLength = distance;
                        if (CurrentLength >= length * TENSIONMODIFIER) {
                            addPoint();
                            break;
                        }
                    }
                }
                else {
                    Cooldown = Helper.Approach(Cooldown, 0f, 1f);
                }
            }

            Player player = Main.player[Projectile.owner];

            int owner = Projectile.owner;
            ref Vector2 velocity = ref Projectile.velocity;
            float scale = Projectile.scale;
            Vector2 vector21 = Main.player[owner].GetPlayerCorePoint();
            int timeLeftLast = ACTIVETIME;
            if (CanSpawnMoreLines) {
                if (player4.noItems || !player4.IsAliveAndFree()) {
                    Projectile.Kill();
                }

                Projectile.timeLeft = timeLeftLast;

                player4.reuseDelay = player4.itemAnimationMax;

                _tension = 1f;
                if (Main.myPlayer == owner) {
                    float num = (float)Math.PI / 2f;
                    Vector2 vector = vector21;
                    int num2 = 2;
                    float num3 = 0f;
                    float num8 = 1f * Projectile.scale;
                    Vector2 vector3 = vector;
                    Vector2 value = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector3;
                    if (player.gravDir == -1f)
                        value.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector3.Y;


                    Vector2 vector4 = Vector2.Normalize(value);
                    if (float.IsNaN(vector4.X) || float.IsNaN(vector4.Y))
                        vector4 = -Vector2.UnitY;

                    vector4 = Vector2.Normalize(Vector2.Lerp(vector4, Vector2.Normalize(Projectile.velocity), 0f));
                    vector4 *= num8;
                    if (vector4.X != Projectile.velocity.X || vector4.Y != Projectile.velocity.Y)
                        Projectile.netUpdate = true;

                    Projectile.velocity = vector4;
                }
            }
            else {
                _tension = Helper.Approach(_tension, 0f, TimeSystem.LogicDeltaTime * 3f);
            }
            //Projectile.position -= velocity.SafeNormalize() * 14f;
            //Projectile.position -= velocity.TurnLeft().SafeNormalize() * 2f * -player.direction * player.gravDir;

            if (CanSpawnMoreLines) {
                if (velocity.X > 0f)
                    Main.player[owner].ChangeDir(1);
                else if (velocity.X < 0f)
                    Main.player[owner].ChangeDir(-1);

                Projectile.spriteDirection = Projectile.direction;
                Main.player[owner].ChangeDir(Projectile.direction);
                Main.player[owner].heldProj = Projectile.whoAmI;
                Main.player[owner].SetDummyItemTime(2);
                Projectile.position.X = vector21.X - (float)(Projectile.width / 2);
                Projectile.position.Y = vector21.Y - (float)(Projectile.height / 2);
                Projectile.rotation = (float)(Math.Atan2(velocity.Y, velocity.X) + 1.5700000524520874) - MathHelper.PiOver2;
                if (Main.player[owner].direction == 1)
                    Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);
                else
                    Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);

                Projectile.Center = Utils.Floor(Projectile.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            var texture = Projectile.GetTexture();

            Player player = Projectile.GetOwnerAsPlayer();

            var pos = Projectile.Center - Main.screenPosition;

            if (CanSpawnMoreLines) {
                var effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically : Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
                if (player.gravDir < 0) {
                    effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
                }

                float rotation = player.itemRotation + 0.785f * (float)player.direction;

                Vector2 origin = new Vector2(0f, texture.Height);
                if (player.gravDir == -1f) {
                    if (player.direction == -1) {
                        rotation += MathHelper.Pi + MathHelper.PiOver2;
                        origin = new Vector2(0f, texture.Height);
                    }
                    else {
                        rotation -= 1.57f;
                        origin = Vector2.Zero;
                    }
                }
                else if (player.direction == -1) {
                    rotation += MathHelper.Pi;
                    origin = Vector2.Zero;
                }

                Main.EntitySpriteDraw(texture, pos, null, lightColor, rotation, origin, Projectile.scale, effects);

                var glowMaskInfo = ItemGlowMaskHandler.GlowMasks[ModContent.ItemType<FilamentYarn>()];
                Texture2D heldItemGlowMaskTexture = glowMaskInfo.Texture.Value;
                float brightnessFactor = Lighting.Brightness((int)pos.X / 16, (int)pos.Y / 16);
                Color color = Color.Lerp(glowMaskInfo.Color, lightColor, brightnessFactor);
                Color glowMaskColor = glowMaskInfo.ShouldApplyItemAlpha ? color * (1f - Projectile.alpha / 255f) : glowMaskInfo.Color;
                Main.EntitySpriteDraw(heldItemGlowMaskTexture, pos, null, glowMaskColor, rotation, origin, Projectile.scale, effects);
            }

            {
                var graphicsDevice = Main.instance.GraphicsDevice;
                var sb = Main.spriteBatch;

                sb.End();

                SpriteBatchSnapshot snapshot = sb.CaptureSnapshot();

                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Transparent);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                sb.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                sb.End();

                graphicsDevice.SetRenderTarget(YarnRenderTargetLoader.RenderTarget);
                graphicsDevice.Clear(Color.Transparent);

                float scaleX = (float)YarnRenderTargetLoader.RenderTarget.Width / Main.screenWidth;
                float scaleY = (float)YarnRenderTargetLoader.RenderTarget.Height / Main.screenHeight;
                Matrix scaleMatrix = Matrix.CreateScale(scaleX, scaleY, 1f);
                sb.Begin(SpriteSortMode.Deferred,
                         BlendState.AlphaBlend,
                         SamplerState.PointClamp,
                         null, null, null,
                         scaleMatrix);


                if (CanSpawnMoreLines) {
                    DrawConnectedLines();
                }
                else {
                    DrawConnectedLines();
                    DrawConnectedLines();
                }

                sb.End();

                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Transparent);

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                sb.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                sb.End();

                sb.Begin(snapshot with { samplerState = SamplerState.PointClamp });
                ShaderLoader.WormholeTentacleShader.WaveTime = TimeSystem.TimeForVisualEffects * 10f;
                ShaderLoader.WormholeTentacleShader.WaveAmplitude = 0.001f;
                ShaderLoader.WormholeTentacleShader.WaveFrequency = 2f;
                ShaderLoader.WormholeTentacleShader.WaveSpeed = 0.5f;
                ShaderLoader.WormholeTentacleShader.BendDirection = 0f;
                ShaderLoader.WormholeTentacleShader.BendStrength = 1f;
                ShaderLoader.WormholeTentacleShader.BaseStability = 0f;
                ShaderLoader.WormholeTentacleShader.TipWiggle = 0f;
                ShaderLoader.WormholeTentacleShader.Apply(sb, () => {
                    sb.Draw(YarnRenderTargetLoader.RenderTarget,
                        new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                        Color.White);
                });
                sb.End();

                sb.Begin(snapshot);
            }

            return false;
        }
    }
}
