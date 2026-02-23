using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Content.Buffs;
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
                                    _tentaclesTexture_Glow = null!;

    private Vector2 _endPosition;
    private Vector2 _endPosition2;
    private Vector2 _endPosition3;
    private Vector2 _endPosition4;

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(5);

        if (Main.dedServ) {
            return;
        }

        _tentaclesTexture = ModContent.Request<Texture2D>(Texture + "_Tentacles");
        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        _tentaclesTexture_Glow = ModContent.Request<Texture2D>(Texture + "_Tentacles_Glow");
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
        if (Main.expertMode)
            target.AddBuff(ModContent.BuffType<FilamentBinding>(), Main.rand.Next(300, 540) / 6);
        else if (Main.rand.Next(2) == 0)
            target.AddBuff(ModContent.BuffType<FilamentBinding>(), Main.rand.Next(360, 720) / 6);
    }

    public override void AI() {
        if (NPC.localAI[2] == 0f) {
            NPC.localAI[2] = 1f;

            _endPosition = NPC.Center;
            _endPosition2 = NPC.Center;
            _endPosition3 = NPC.Center;
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
        _endPosition = Vector2.Lerp(_endPosition, startPosition + new Vector2(5f + 30f * NPC.ai[2], 100f * (1f - NPC.ai[2])), lerpValue);
        _endPosition2 = Vector2.Lerp(_endPosition2, startPosition + new Vector2(-5f - 30f * NPC.ai[2], 100f * (1f - NPC.ai[2])), lerpValue);
        _endPosition3 = Vector2.Lerp(_endPosition3, startPosition + new Vector2(-5f - 40f * NPC.ai[2], 100f), lerpValue);
        _endPosition4 = Vector2.Lerp(_endPosition4, startPosition + new Vector2(5f + 40f * NPC.ai[2], 100f), lerpValue);
        if (_endPosition.Distance(_endPosition2) < 40f) {
            _endPosition += _endPosition.DirectionFrom(_endPosition2) * 1f;
            _endPosition2 += _endPosition2.DirectionFrom(_endPosition) * 1f;
        }
        if (_endPosition3.Distance(_endPosition4) < 40f) {
            _endPosition3 += _endPosition3.DirectionFrom(_endPosition4) * 1f;
            _endPosition4 += _endPosition4.DirectionFrom(_endPosition3) * 1f;
        }

        NPC.localAI[2] = 2f;
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
        void drawTentacle(Vector2 endPosition2, int count = 5) {
            Texture2D texture = _tentaclesTexture.Value;
            Rectangle clip1 = new(14, 2, 14, 12);
            Vector2 origin1 = clip1.TopCenter();
            Rectangle clip2 = new(12, 16, 16, 16);
            Vector2 origin2 = clip2.TopCenter();
            Rectangle clip3 = new(34, 16, 14, 18);
            Vector2 origin3 = clip2.TopCenter();

            int index = 0;
            Vector2 startVelocity = Vector2.UnitY.RotatedBy(NPC.rotation);
            Vector2 startPosition = NPC.Center + startVelocity * 20f;
            Vector2 endPosition = endPosition2;
            Vector2 velocity = startVelocity * 5f;
            while (true) {
                float step = clip1.Height;

                velocity = Vector2.Lerp(velocity, startPosition.DirectionTo(endPosition), 0.5f);

                SpriteEffects flip = (index % 2 == 0).ToSpriteEffects();

                float rotation = velocity.ToRotation() - MathHelper.PiOver2;
                DrawInfo drawInfo = new() {
                    Clip = clip1,
                    Origin = origin1,
                    Rotation = rotation,
                    ImageFlip = flip,
                    Color = drawColor
                };
                if (index == count) {
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
            drawTail = !drawTail;
        }

        drawTentacle(_endPosition);
        drawTentacle(_endPosition2);
        drawTentacle(_endPosition3, 3);
        drawTentacle(_endPosition4, 3);
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
