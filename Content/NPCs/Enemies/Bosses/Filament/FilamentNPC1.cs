using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Filament;

sealed class FilamentNPC1 : ModNPC {
    private static Asset<Texture2D> _tentaclesTexture = null!,
                                    _glowTexture = null!,
                                    _tentaclesTexture_Glow = null!,
                                    _circleTexture = null!;

    private Vector2 _endPosition;
    private Vector2 _endPosition2;
    private Vector2 _endPosition3;
    private Vector2 _endPosition4;

    private bool _shouldKnock;
    private float _knockProgress, _knockProgress2;
    private float _starDirection;

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(5);

        if (Main.dedServ) {
            return;
        }

        _tentaclesTexture = ModContent.Request<Texture2D>(Texture + "_Tentacles");
        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        _tentaclesTexture_Glow = ModContent.Request<Texture2D>(Texture + "_Tentacles_Glow");
        _circleTexture = ModContent.Request<Texture2D>(Texture + "_Circle");
    }

    public override void SetDefaults() {
        NPC.width = 54;
        NPC.height = 58;
        NPC.aiStyle = -1;
        NPC.damage = 70;
        NPC.defense = 38;
        NPC.lifeMax = 1500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.1f;
        NPC.npcSlots = 3f;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {

    }

    public override void AI() {
        if (NPC.SpeedX() > 0.1f) {
            NPC.SetDirection(NPC.velocity.X.GetDirection());
        }

        if (NPC.localAI[2] == 0f) {
            NPC.localAI[2] = 1f;

            _endPosition = NPC.Center;
            _endPosition2 = NPC.Center;
            _endPosition3 = NPC.Center;
            _endPosition4 = NPC.Center;
        }

        if (!_shouldKnock) {
            _knockProgress += 1f;
            if (_knockProgress >= 60f) {
                _knockProgress = 0f;
                _shouldKnock = true;
            }
        }
        else {
            _knockProgress2 += 1f;
            if (_knockProgress2 >= 300f) {
                _knockProgress2 = 0f;
                _shouldKnock = false;
            }
        }

        NPC.TargetClosest();

        bool changedState = MathF.Abs(NPC.Center.X - NPC.GetTargetPlayer().Center.X) < 100f && NPC.Center.Y < NPC.GetTargetPlayer().Center.Y - 50f;

        if (NPC.ai[0] > 0f) {
            NPC.ai[0]--;
        }

        float to = 0f;
        if (NPC.ai[0] > 0f || changedState) {
            to = 1f;
            if (changedState) {
                NPC.ai[0] = 10f;
            }
        }
        NPC.ai[2] = Helper.Approach(NPC.ai[2], to, TimeSystem.LogicDeltaTime * 1.5f);
        NPC.ai[1] = Helper.Approach(NPC.ai[1], to, TimeSystem.LogicDeltaTime * 3f);

        {
            Vector2 destination = Main.player[NPC.target].Center - NPC.Size / 2f + new Vector2(0f, -300f * (1f - NPC.ai[1]));
            float distanceToDestination = Vector2.Distance(NPC.position, destination);
            float minDistance = 30f;
            float inertiaValue = 20, extraInertiaValue = inertiaValue * 5;
            float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
            float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
            Helper.InertiaMoveTowards(ref NPC.velocity, NPC.position, destination, inertia: inertia);
        }

        NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.X * 0.05f, 0.5f);

        float offset4 = 0.5f;
        NPC.position.Y += Helper.Wave(-offset4, offset4, 2f, NPC.whoAmI);
        NPC.position.X += Helper.Wave(-offset4, offset4, 1f, NPC.whoAmI + 1f);

        Vector2 startPosition = NPC.Center + Vector2.UnitY * 20f;
        float baseLerpValue = 0.05f;
        float velocityFactor = baseLerpValue * 2f - NPC.velocity.Length() * 0.025f;
        float lerpValue = MathF.Max(baseLerpValue, velocityFactor);
        lerpValue *= 1f + 4f * NPC.ai[2];
        if (NPC.localAI[2] != 2f) {
            lerpValue = 1f;
        }
        lerpValue *= 2f;
        _endPosition = Vector2.Lerp(_endPosition, startPosition + Vector2.UnitX * 20f, lerpValue);
        _endPosition2 = Vector2.Lerp(_endPosition2, startPosition - Vector2.UnitX * 20f, lerpValue);
        float progress = GetAttackProgress();
        lerpValue += progress;
        float progress2 = Utils.GetLerpValue(70f, 80f, _knockProgress2, true);
        progress2 *= Utils.GetLerpValue(100f, 70f, _knockProgress2, true);
        progress2 = Ease.CubeInOut(progress2);
        if (progress2 >= 0.65f && NPC.localAI[1] == 0f) {
            NPC.localAI[1] = 1f;

            //for (int num213 = 0; num213 < 4; num213++) {
            //    int num214 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
            //    Main.dust[num214].position = GetStarPosition() + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * NPC.width / 2f;
            //}

            //for (int num215 = 0; num215 < 20; num215++) {
            //    int num216 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.BubbleBurst_Blue, 0f, 0f, 200, default(Color), 3.7f);
            //    Main.dust[num216].position = GetStarPosition() + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * NPC.width / 2f;
            //    Main.dust[num216].noGravity = true;
            //    Dust dust2 = Main.dust[num216];
            //    dust2.velocity *= 3f;
            //}

            for (int num217 = 0; num217 < 20; num217++) {
                int num218 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, ModContent.DustType<FilamentDust>(), 0f, 0f,
                    0, default(Color), 2.7f);
                Main.dust[num218].position = GetStarPosition() + Main.rand.RandomPointInArea(4f);
                Main.dust[num218].velocity += Main.dust[num218].position.DirectionTo(GetStarPosition() + Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi).RotatedBy(NPC.velocity.ToRotation()) * NPC.width * 2f * (float)Main.rand.NextDouble())
                    * Main.rand.NextFloat(2f, 5f) * 0.75f;
                Main.dust[num218].noGravity = true;
                Dust dust2 = Main.dust[num218];
                dust2.velocity *= 3f;
            }

            //for (int num219 = 0; num219 < 10; num219++) {
            //    int num220 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 0, default(Color), 1.5f);
            //    Main.dust[num220].position = GetStarPosition() + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(NPC.velocity.ToRotation()) * NPC.width / 2f;
            //    Main.dust[num220].noGravity = true;
            //    Dust dust2 = Main.dust[num220];
            //    dust2.velocity *= 3f;
            //}
        }
        if (progress2 == 0f) {
            NPC.localAI[1] = 0f;
        }

        NPC.velocity *= 0f;

        float max0 = 100f;
        float max = 200f;
        _endPosition3 = Vector2.Lerp(_endPosition3, 
            startPosition + new Vector2(max0 * progress - max * progress2, 75f), lerpValue);
        _endPosition4 = Vector2.Lerp(_endPosition4,
            startPosition + new Vector2(-max0 * progress + max * progress2, 75f), lerpValue);
        //if (_endPosition.Distance(_endPosition2) < 40f) {
        //    _endPosition += _endPosition.DirectionFrom(_endPosition2) * 1f;
        //    _endPosition2 += _endPosition2.DirectionFrom(_endPosition) * 1f;
        //}
        //if (_endPosition3.Distance(_endPosition4) < 40f) {
        //    _endPosition3 += _endPosition3.DirectionFrom(_endPosition4) * 1f;
        //    _endPosition4 += _endPosition4.DirectionFrom(_endPosition3) * 1f;
        //}

        NPC.localAI[2] = 2f;

        float starOpacity2 = Utils.GetLerpValue(140f, 80f, _knockProgress2, true);
        _starDirection += NPC.direction * 1.5f * starOpacity2;

        if (GetStarOpacity() <= 0f) {
            return;
        }
        foreach (Player target in Main.ActivePlayers) {
            if (target.Distance(GetStarPosition()) > 100f) {
                continue;
            }

            if (Main.expertMode)
                target.AddBuff(ModContent.BuffType<FilamentBinding>(), Main.rand.Next(300, 540) / 6);
            else if (Main.rand.Next(2) == 0)
                target.AddBuff(ModContent.BuffType<FilamentBinding>(), Main.rand.Next(360, 720) / 6);
        }
    }

    private Vector2 GetStarPosition() => NPC.Center + Vector2.UnitY.RotatedBy(NPC.rotation) * 100f;
    private float GetStarOpacity() => Utils.GetLerpValue(74f, 80f, _knockProgress2, true) * Utils.GetLerpValue(120f, 80f, _knockProgress2, true);

    private float GetAttackProgress() {
        float progress2 = Utils.GetLerpValue(80f, 60f, _knockProgress2, true);
        float progress = 
            Utils.GetLerpValue(0f, 50f, _knockProgress2, true) * progress2;
        return Ease.CubeInOut(progress);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        DrawTentacles(spriteBatch, drawColor);

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteBatch mySpriteBatch = spriteBatch;
        NPC rCurrentNPC = NPC;

        SpriteEffects spriteEffects = SpriteEffects.None;

        int type = NPC.type;

        Color npcColor = drawColor;

        float num35 = 0f;
        float num36 = Main.NPCAddHeight(rCurrentNPC);

        Vector2 halfSize = new Vector2(TextureAssets.Npc[type].Width() / 2, TextureAssets.Npc[type].Height() / Main.npcFrameCount[type] / 2);

        Texture2D value60 = TextureAssets.Npc[type].Value;
        Vector2 vector65 = rCurrentNPC.Center - screenPos;
        Vector2 vector66 = vector65 - new Vector2(300f, 310f);
        vector65 -= new Vector2(value60.Width, value60.Height / Main.npcFrameCount[type]) * rCurrentNPC.scale / 2f;
        vector65 += halfSize * rCurrentNPC.scale + new Vector2(0f, num35 + num36 + rCurrentNPC.gfxOffY);

        mySpriteBatch.Draw(_glowTexture.Value, rCurrentNPC.Bottom - screenPos + new Vector2((float)(-TextureAssets.Npc[type].Width()) * rCurrentNPC.scale / 2f + halfSize.X * rCurrentNPC.scale, (float)(-TextureAssets.Npc[type].Height()) * rCurrentNPC.scale / (float)Main.npcFrameCount[type] + 4f + halfSize.Y * rCurrentNPC.scale + num36 + rCurrentNPC.gfxOffY), rCurrentNPC.frame, new Microsoft.Xna.Framework.Color(255 - rCurrentNPC.alpha, 255 - rCurrentNPC.alpha, 255 - rCurrentNPC.alpha, 255 - rCurrentNPC.alpha), rCurrentNPC.rotation, halfSize, rCurrentNPC.scale, spriteEffects, 0f);
        float num184 = Helper.Wave(2f, 6f, 1f, NPC.whoAmI);
        for (int num185 = 0; num185 < 4; num185++) {
            mySpriteBatch.Draw(_glowTexture.Value, rCurrentNPC.Bottom - screenPos + new Vector2((float)(-TextureAssets.Npc[type].Width()) * rCurrentNPC.scale / 2f + halfSize.X * rCurrentNPC.scale, (float)(-TextureAssets.Npc[type].Height()) * rCurrentNPC.scale / (float)Main.npcFrameCount[type] + 4f + halfSize.Y * rCurrentNPC.scale + num36 + rCurrentNPC.gfxOffY)
                + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, rCurrentNPC.frame,
                new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 1f, 
                rCurrentNPC.rotation, halfSize, rCurrentNPC.scale, spriteEffects, 0f);
        }
    }

    private void DrawTentacles(SpriteBatch spriteBatch, Color drawColor) {
        if (NPC.localAI[2] == 0f) {
            return;
        }

        bool drawTail = NPC.whoAmI % 2 == 0;
        bool flip2 = false;
        void drawTentacle(int style, int count = 8) {
            Texture2D texture = _tentaclesTexture.Value;
            Rectangle clip1 = new(14, 2, 14, 12);
            Vector2 origin1 = clip1.TopCenter();
            Rectangle clip2 = new(12, 16, 16, 16);
            Vector2 origin2 = clip2.TopCenter();
            Rectangle clip3 = new(34, 16, 14, 18);
            Vector2 origin3 = clip2.TopCenter();
            Rectangle clip4 = new(34, 2, 14, 12);
            Vector2 origin4 = clip2.TopCenter();

            int index = 0;
            Vector2 startVelocity = Vector2.Zero;
            Vector2 startPosition = NPC.Center + startVelocity * 20f;
            Vector2 endPosition = Vector2.Zero;
            if (style == 0) {
                startVelocity = Vector2.UnitY.RotatedBy(NPC.rotation + -0.2f);
                endPosition = _endPosition;
            }
            if (style == 1) {
                startVelocity = Vector2.UnitY.RotatedBy(NPC.rotation + 0.2f);
                endPosition = _endPosition2;
            }
            if (style == 2) {
                drawTail = true;
                startVelocity = Vector2.UnitY.RotatedBy(NPC.rotation - 0.15f);
                endPosition = _endPosition3;
            }
            if (style == 3) {
                drawTail = true;
                startVelocity = Vector2.UnitY.RotatedBy(NPC.rotation + 0.15f);
                endPosition = _endPosition4;
            }
            bool bottom = style > 1;
            Vector2 velocity = startVelocity * 5f;
            float progress = GetAttackProgress();
            float lerpValue = bottom ? MathHelper.Lerp(0.1f, 0.3f, progress) : 0.3f;
            while (true) {
                float step = clip1.Height;

                if (index > count * 0.5f && bottom) {
                    lerpValue = Helper.Approach(lerpValue, 1f, MathHelper.Lerp(0.05f, 0.1f, progress));
                }

                velocity = Vector2.Lerp(velocity, startPosition.DirectionTo(endPosition), lerpValue);

                SpriteEffects flip = (index % 2 == 0).ToSpriteEffects();
                if (!flip2) {
                    flip |= SpriteEffects.FlipHorizontally;
                }

                bool bottom_last = false;
                bool last = index == count;
                bool last2 = index == count - 1;
                if (bottom && last2) {
                    bottom_last = true;
                }

                float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                DrawInfo drawInfo = new() {
                    Clip = bottom_last ? clip4 : clip1,
                    Origin = bottom_last ? origin4 : origin1,
                    Rotation = rotation,
                    ImageFlip = flip,
                    Color = drawColor
                };
                if (last) {
                    drawInfo = new() {
                        Clip = drawTail ? clip3 : clip2,
                        Origin = drawTail ? origin3 : origin2,
                        Rotation = rotation,
                        ImageFlip = flip,
                        Color = drawColor
                    };
                    NPC rCurrentNPC = NPC;
                    spriteBatch.Draw(_tentaclesTexture_Glow.Value, startPosition, drawInfo with {
                        Color = new Microsoft.Xna.Framework.Color(255 - rCurrentNPC.alpha, 255 - rCurrentNPC.alpha, 255 - rCurrentNPC.alpha, 255 - rCurrentNPC.alpha)
                    });
                    float num184 = Helper.Wave(2f, 6f, 1f, NPC.whoAmI);
                    for (int num185 = 0; num185 < 4; num185++) {
                        spriteBatch.Draw(_tentaclesTexture_Glow.Value, startPosition + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, drawInfo with {
                            Color = new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 1f
                        });
                    }
                }
                spriteBatch.Draw(texture, startPosition, drawInfo);

                startPosition += velocity.SafeNormalize() * step;

                index++;
                if (index > count) {
                    break;
                }
            }
            flip2 = !flip2;
            //drawTail = !drawTail;
        }

        drawTentacle(0);
        drawTentacle(1);
        drawTentacle(2);
        drawTentacle(3);

        float starOpacity = GetStarOpacity();
        Vector2 starPosition = GetStarPosition() - Main.screenPosition;
        DrawPrettyStarSparkle(starOpacity, SpriteEffects.None,
            starPosition,
            new Microsoft.Xna.Framework.Color(251, 232, 193, 0), new Color(233, 206, 83),
            _starDirection * 0.05f, new Vector2(2f, 2f), new Vector2(2f, 2f));
        //drawTentacle(_endPosition2);
        //drawTentacle(_endPosition3, 3);
        //drawTentacle(_endPosition4, 3);

        spriteBatch.DrawWithSnapshot(() => {
            var circleTexture = _circleTexture.Value;
            var circlePosition = starPosition + Main.screenPosition;
            var circleClip = Utils.Frame(circleTexture, 1, 1, frameY: 0);
            var circleOrigin = circleClip.Centered();
            var circleColor = new Color(233, 206, 83);
            float scale = Utils.GetLerpValue(74f, 120f, _knockProgress2, true);
            circleColor *= 1f - scale;
            var circleDrawInfo = new DrawInfo() {
                Clip = circleClip,
                Origin = circleOrigin,
                Color = circleColor
            };
            ShaderLoader.WavyCircleShader.Time = TimeSystem.TimeForVisualEffects * 10;
            ShaderLoader.WavyCircleShader.WaveCount1 = 6f;
            ShaderLoader.WavyCircleShader.WaveCount2 = 12f;
            ShaderLoader.WavyCircleShader.WaveSize1 = 0.04f;
            ShaderLoader.WavyCircleShader.WaveSize2 = 0.04f;
            ShaderLoader.WavyCircleShader.WaveRadius = 0.5f;
            ShaderLoader.WavyCircleShader.Apply(spriteBatch, () => {
                spriteBatch.Draw(circleTexture, circlePosition, circleDrawInfo.WithScale(2f * Ease.CircOut(scale)));
            });
        }, sortMode: SpriteSortMode.Immediate, blendState: BlendState.Additive);
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

    public override void OnKill() {
        if (FilamentPillar.ShieldStrengthTowerFilamentTower > 0)
            Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center.X, NPC.Center.Y, 0f, 0f, ProjectileID.TowerDamageBolt, 0, 0f, Main.myPlayer, NPC.FindFirstNPC(ModContent.NPCType<FilamentPillar>()));
    }

    public override void FindFrame(int frameHeight) {
        if ((NPC.frameCounter += 1.0) >= 6.0) {
            NPC.frameCounter = 0.0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y / frameHeight >= Main.npcFrameCount[NPC.type])
                NPC.frame.Y = 0;
        }
    }
}
