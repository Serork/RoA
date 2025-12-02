using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class TectonicCaneProjectile : NatureProjectile {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Projectile.localAI[2] = 0f;

        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Projectile.spriteDirection = Main.rand.NextBool().ToDirectionInt();

        Player player = Main.player[Projectile.owner];
        EvilBranch.GetPos(player, out Point point, out Point point2, maxDistance: 800f);
        Projectile.Center = point2.ToWorldCoordinates();

        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Quake") { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);

        Projectile.netUpdate = true;
    }

    private void SpawnDebris(Vector2 position, Vector2 center, float endPositionY) {
        if (Projectile.owner == Main.myPlayer) {
            Vector2 velocity;
            int type = ModContent.ProjectileType<TectonicCaneProjectile2>();
            float length = Main.rand.NextFloat(30f, 60f) * 0.03f;
            Vector2 speed = (position - center).SafeNormalize(Vector2.One) * length;
            speed = speed.RotatedBy(Main.rand.NextFloatRange(MathHelper.PiOver4 * 0.75f));
            velocity = speed;
            position += Main.rand.RandomPointInArea(8, 16);
            if (position.Y > endPositionY) {
                position.Y = endPositionY;
            }
            for (int i = 0; i < 4; i++) {
                bool flag = !Main.rand.NextBool(3);
                Dust dust = Dust.NewDustPerfect(position - Vector2.One * 2f + Main.rand.RandomPointInArea(4f, 4f),
                    flag ? ModContent.DustType<TectonicDust>() : DustID.Torch,
                    Scale: Main.rand.NextFloat(1.5f, 2f) * (flag ? Main.rand.NextFloat(0.5f, 0.75f) : 1f));
                dust.customData = 1;
            }
            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), position, velocity, type, 0, 0f, Projectile.owner, speed.X.GetDirection());
        }
    }

    public override void AI() {
        Vector2 size = new(60f, 90f * Projectile.ai[0]);
        Vector2 offset = new(0f, size.Y / 3f);
        Vector2 startPosition = Projectile.position - new Vector2(size.X / 2f, size.Y) - offset;
        Vector2 endPosition = startPosition + size;

        if (++Projectile.ai[2] > 60f) {
            Projectile.ai[2] = 0f;

            if (Projectile.owner == Main.myPlayer) {
                for (int i = 0; i < 7; i++) {
                    Vector2 velocity = new Vector2(0f, 7.5f + Main.rand.NextFloatRange(2.5f)).RotatedBy(MathHelper.Pi / i + Main.rand.NextFloatRange(MathHelper.PiOver4) + MathHelper.PiOver2);
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), startPosition + Main.rand.Random2(0f, size.X - 4f, 0f, size.X / 2f), velocity * 0.4f, ModContent.ProjectileType<TectonicCaneFlames>(),
                        Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
                SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.35f, Volume = 0.3f }, Projectile.Center);
            }
        }

        if (Projectile.localAI[2] < 30f) {
            Projectile.localAI[2]++;
            if (Projectile.localAI[2] % 2f == 0f) {
                SoundEngine.PlaySound(SoundID.WormDig with { Pitch = -0.35f - Projectile.localAI[2] * 0.1f, Volume = 2f - Projectile.localAI[2] * 0.1f }, Projectile.Center);
            }
        }
        if (Projectile.ai[0] < 1f) {
            Projectile.ai[0] += TimeSystem.LogicDeltaTime * 2.5f;
            int dustType = TileHelper.GetKillTileDust((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16, Main.tile[(int)Projectile.position.X / 16, (int)Projectile.position.Y / 16]);
            for (int k = 0; k < 9; k++) {
                Dust.NewDust(Projectile.position - new Vector2(32f, 0f), 60, 2, dustType, 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));
            }
        }
        else {
            Projectile.ai[0] = 1f;
        }
        Projectile.ai[1] = Ease.SineOut(Projectile.ai[0]);
        float lerpAmount = 1f - Projectile.ai[0] * 0.5f;
        Projectile.localAI[0] = MathHelper.SmoothStep(Projectile.localAI[0], Main.rand.NextFloatRange(35f) * Main.rand.NextFloatDirection(), lerpAmount);
        Projectile.localAI[1] = MathHelper.SmoothStep(Projectile.localAI[1], Main.rand.NextFloatRange(50f) * Main.rand.NextFloatDirection(), lerpAmount);

        Vector2 center = startPosition + size / 2f;
        center += Vector2.UnitY * 10f;
        center.X -= 4f;
        Vector2 length = endPosition - startPosition;
        float num56 = 0.45f;
        if (num56 > 0.6f)
            num56 = 0.6f;
        DelegateMethods.v3_1 = new Vector3(num56, num56 * 0.65f, num56 * 0.4f);
        Utils.PlotTileLine(center - size / 2f - Vector2.One * 4f, center + size / 2f - Vector2.One * 4f, (float)8f * Projectile.scale, DelegateMethods.CastLight);

        if (Projectile.timeLeft < 100) {
            Projectile.timeLeft = 100;
            float max = length.X / 20;
            float max2 = length.Y / 17;
            for (int index2 = 0; (double)index2 < max2; ++index2) {
                float value = index2 / (max2 * 1.5f);
                value = MathHelper.Clamp(value, 0.2f, 0.35f);
                for (int index1 = (int)(max * (value)); (double)index1 < max * (1f - value); ++index1) {
                    Vector2 position = startPosition + new Vector2((float)(10 + index1 * 20), (float)(7 + index2 * 14));
                    float length2 = Math.Abs(position.Length());
                    float length3 = Math.Abs(center.Length());
                    if (length2 > length3 || length2 < length3) {
                        SpawnDebris(position, center, endPosition.Y);
                    }
                }
            }
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Stonebreak") { PitchVariance = 0.5f, Volume = 0.4f }, center);
            Projectile.Kill();
        }
    }

    public override void SafePostAI() {
        if (++Projectile.frameCounter > 8) {
            Projectile.frameCounter = 0;
            if (++Projectile.frame > 3) {
                Projectile.frame = 0;
            }
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.005f;
        target.AddBuff(ModContent.BuffType<Burning>(), (int)(60f * num2 * 2f));
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.005f;
        target.AddBuff(ModContent.BuffType<Burning>(), (int)(60f * num2 * 2f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float sizeY = 130f;
        return Collision.CheckAABBvAABBCollision(
            targetHitbox.Location.ToVector2(),
            targetHitbox.Size(),
            Projectile.position - new Vector2(30f, sizeY),
            new Vector2(60f, sizeY));
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

    public override bool PreDraw(ref Color lightColor) {
        Vector2 size = new(60f, 90f);
        Vector2 offset = new(0f, size.Y / 3f);
        Vector2 startPosition = Projectile.position - new Vector2(size.X / 2f, size.Y) - offset;
        Vector2 endPosition = startPosition + size;
        Vector2 center = startPosition + size / 2f;
        SpriteFrame frame = new(1, 4);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Color drawColor = Lighting.GetColor(center.ToTileCoordinates());
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection == 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        frame = frame.With(0, (byte)Projectile.frame);
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        int height = (int)(sourceRectangle.Height * Projectile.ai[1]);
        sourceRectangle.Height = Math.Clamp(height, 2, sourceRectangle.Height);
        bool flag = Projectile.ai[0] != 1f;
        if (flag) {
            position += new Vector2(Main.rand.NextFloatRange(0.5f) * Projectile.localAI[0], Main.rand.NextFloatRange(1f) * Projectile.localAI[1]) * 0.75f * (1f - Projectile.ai[0]);
        }
        if (position.Y > Projectile.position.Y) {
            position.Y = Projectile.position.Y;
        }
        //texture = ModContent.Request<Texture2D>(Texture + "3").Value;
        //Main.EntitySpriteDraw(texture, position + Vector2.One, null, drawColor, Projectile._rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);
        texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, drawColor * Projectile.Opacity, Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);
        texture = _glowTexture.Value;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.Lerp(drawColor, Color.White, 0.5f) * (drawColor.A / 255f) * Ease.QuartIn(Projectile.Opacity), Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);

        return false;
    }
}
