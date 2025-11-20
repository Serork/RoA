using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Claws;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Common.VisualEffects;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SunSigil : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 600;

    public enum SunSigilRequstedTextureType : byte {
        Main,
        Glow,
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)SunSigilRequstedTextureType.Main, ResourceManager.NatureProjectileTextures + "SunSigil"),
         ((byte)SunSigilRequstedTextureType.Glow, ResourceManager.NatureProjectileTextures + "SunSigil_Glow")];

    private Color _firstSlashColor, _secondSlashColor;
    private Vector2 _laserDirection;

    private Color SelectedColor => Color.Lerp(_firstSlashColor, _secondSlashColor, Helper.Wave(0f, 1f, 5f, Projectile.whoAmI));
    private float Opacity => Utils.GetLerpValue(TIMELEFT, TIMELEFT - 15, Projectile.timeLeft, true) * Utils.GetLerpValue(0, 15, Projectile.timeLeft, true);

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = TIMELEFT;

        Projectile.manualDirectionChange = true;

        Projectile.aiStyle = -1;

        ShouldChargeWreathOnDamage = false;

        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 position = Projectile.Center;
        float _ = 0f;
        bool flag = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, position + Vector2.Normalize(_laserDirection) * Projectile.localAI[2] * 8.35f, 20, ref _);
        return flag;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        Projectile.localAI[1] -= 3f;
        if (Projectile.localAI[1] < 0f) {
            Projectile.localAI[1] = 256f;
        }

        float startVelocityModifier = Ease.CubeIn(Utils.GetLerpValue(TIMELEFT + 1, TIMELEFT - 10, Projectile.timeLeft, true));
        float startVelocityModifier2 = Ease.CubeOut(Utils.GetLerpValue(TIMELEFT + 1, TIMELEFT - 20, Projectile.timeLeft, true));

        Projectile.scale = 0.75f;

        if (Projectile.localAI[0] == 0f) {
            Projectile.Center = owner.Top;
            Projectile.Center = Utils.Floor(Projectile.Center) - Vector2.UnitY * 40f + Vector2.UnitY * owner.gfxOffY;
            if (Projectile.IsOwnerLocal()) {
                Projectile.velocity = Projectile.Center.DirectionTo(owner.GetWorldMousePosition());
                Projectile.netUpdate = true;
            }
        }

        Projectile.velocity.X = Projectile.velocity.X.GetDirection() * 1f * startVelocityModifier2;

        if (Projectile.localAI[0] == 0f) {
            Projectile.direction = Projectile.velocity.X.GetDirection();
            if (Projectile.direction > 0) {
                Projectile.localAI[0] = 63f;
            }
            else {
                Projectile.localAI[0] = -125f;
            }
            Projectile.velocity.Y = 0f;
        }
        Projectile.localAI[0] += Projectile.direction;
        _laserDirection = Projectile.velocity.RotatedBy((MathHelper.PiOver2 + MathHelper.PiOver4 * Helper.Wave(Projectile.localAI[0] * 0.05f, -1f, 1f, speed: 1f)) * Projectile.direction);
        Projectile.rotation = _laserDirection.ToRotation();

        if (Opacity > 0.75f && Main.rand.NextBool(4)) {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f) + new Vector2(12f * Projectile.direction, -2f),
                ModContent.DustType<Dusts.SunSigil>(), 
                Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 5f),
                0, SelectedColor, Main.rand.NextFloat(0.825f, 1f) * 1.75f * Projectile.scale);
            dust.noGravity = true;
        }

        Vector2 samplingPoint = Projectile.Center;
        float distance = 0f;
        float opacity = Opacity * startVelocityModifier2;
        float maxdistance = 1000f * opacity;
        while (distance < maxdistance) {
            Vector2 start = Projectile.Center;
            NPC[] sortedNPC = Main.npc.Where(n => n.active && !n.friendly && !n.CountsAsACritter).OrderBy(n => (n.Center - start).Length()).ToArray();
            bool flag = false;
            for (int index = 0; index < sortedNPC.Length; index++) {
                NPC npc = sortedNPC[index];
                if (Collision.CheckAABBvAABBCollision(start, Vector2.One * 6f, npc.Hitbox.TopLeft(), npc.Hitbox.Size())) {
                    distance = npc.Distance(start);
                    flag = true;
                }
            }
            if (flag) {
                break;
            }

            if (Main.rand.NextBool(100)) {
                Dust dust = Dust.NewDustPerfect(samplingPoint + Vector2.UnitY.RotatedBy(Projectile.rotation - MathHelper.PiOver2) * distance * 0.375f + Main.rand.NextVector2Circular(20f, 20f) + new Vector2(10f * Projectile.direction, -2f),
                    ModContent.DustType<Dusts.SunSigil>(),
                    Vector2.UnitY.RotatedByRandom(Projectile.rotation - MathHelper.PiOver2) * Main.rand.NextFloat(3f, 5f) * 1.25f,
                    0, SelectedColor, Main.rand.NextFloat(0.825f, 1f) * 1.5f * Projectile.scale);
                dust.noGravity = true;
            }

            distance += 16f;
        }
        float num716 = 3f;
        float num717 = 20f;
        float[] array2 = new float[(int)num716];
        Collision.LaserScan(samplingPoint, _laserDirection, num717 * Projectile.scale, distance, array2);
        float num718 = 0f;
        for (int num719 = 0; num719 < array2.Length; num719++) {
            num718 += array2[num719];
        }
        num718 /= num716;
        Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], num718 / 7f, 0.2f);

        DelegateMethods.v3_1 = SelectedColor.ToVector3();
        Lighting.AddLight(Projectile.Center, DelegateMethods.v3_1);
        Vector2 last = Projectile.Center + _laserDirection * num718;
        Utils.PlotTileLine(Projectile.Center, last, (float)num717 * Projectile.scale, DelegateMethods.CastLight);

        Vector2 vector72 = last;
        Vector2 velocity = last.DirectionTo(Projectile.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.25f, 1.25f) * 5f;
        if (Projectile.localAI[0] % 14f == 0) {
            for (int num730 = 0; num730 < 2; num730++) {
                float num731 = velocity.ToRotation() + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                float num732 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                Vector2 vector73 = new Vector2((float)Math.Cos(num731) * num732, (float)Math.Sin(num731) * num732);
                int num733 = Dust.NewDust(vector72 - last.DirectionTo(Projectile.Center) * -20f, 0, 0, DustID.FireworksRGB, vector73.X, vector73.Y);
                Main.dust[num733].noGravity = true;
                Main.dust[num733].scale = Opacity * 1.2f;
                Main.dust[num733].velocity = velocity;
                Main.dust[num733].color = SelectedColor;
            }
        }

        if (Projectile.localAI[0] % 8 != 0) {
            return;
        }
        Vector2 position = last;
        velocity = last.DirectionTo(Projectile.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.25f, 1.25f);
        VisualEffectSystem.New<VisualEffects.SunSigil>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(position, velocity,
            scale: Opacity * Main.rand.NextFloat(1f, 1.5f),
            color: SelectedColor);
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteRGBA(_firstSlashColor);
        writer.WriteRGBA(_secondSlashColor);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _firstSlashColor = reader.ReadRGBA();
        _secondSlashColor = reader.ReadRGBA();
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner == Main.myPlayer) {
            var colorHandler = Projectile.GetOwnerAsPlayer().GetModPlayer<ClawsHandler>().SlashColors;
            _firstSlashColor = colorHandler.Item1;
            _secondSlashColor = colorHandler.Item2;

            Projectile.netUpdate = true;
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<SunSigil>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = indexedTextureAssets[(byte)SunSigilRequstedTextureType.Main].Value,
                  glowTexture = indexedTextureAssets[(byte)SunSigilRequstedTextureType.Glow].Value,
                  laser = ResourceManager.Laser1,
                  laser2 = ResourceManager.Laser0,
                  flash = ResourceManager.Flash,
                  star = ResourceManager.Star;
        Vector2 position = Projectile.Center;
        Rectangle clip = texture.Bounds;
        Rectangle clip2 = flash.Bounds;
        Rectangle clip3 = laser2.Bounds;
        Rectangle clip4 = star.Bounds;
        Vector2 origin = clip.Centered();
        Vector2 origin2 = clip2.Centered();
        Vector2 origin3 = clip3.Centered();
        Vector2 origin4 = clip4.Centered();
        Color color = SelectedColor;

        //batch.Draw(laser2, position, DrawInfo.Default with {
        //    Clip = clip3,
        //    Origin = origin3,
        //    _color = color * 0.5f
        //});

        int count = 8;
        batch.DrawWithSnapshot(() => {
            for (int i = 0; i < count; i++) {
                float circleFactor = MathHelper.TwoPi / count * i - MathHelper.Pi;
                Vector2 circleOffset = Utils.ToRotationVector2(circleFactor);
                ulong seed = (ulong)(Projectile.position.Length() * i);
                float seeded = Utils.RandomFloat(ref seed) + Projectile.whoAmI;
                Vector2 circleOffset2 = circleOffset * Vector2.One * Helper.Wave(-0.5f, 0.75f, 5f, seeded) * 15f * Projectile.scale;
                Vector2 position2 = position + circleOffset2;
                Color rayColor = color * Helper.Wave(0.475f, 0.525f, 10f, seeded) * 1.25f * Opacity;
                Vector2 scale = Projectile.scale * Helper.Wave(0.9f, 1f, 2.5f, seeded).ToRotationVector2() / MathHelper.Pi * 2.9f * new Vector2(1.3f, 1f);
                if (Projectile.timeLeft > TIMELEFT / 2) {
                    scale *= Opacity;
                }
                batch.Draw(glowTexture, position2, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = rayColor * 1.1f,
                    Scale = scale
                });
                Vector2 scale2 = new Vector2(MathHelper.Lerp(scale.X, 1f, 0.5f), scale.Y);
                float num = 5000f;
                batch.Draw(laser,
                    Projectile.Center + circleOffset2 - Main.screenPosition,
                    new Rectangle?(new Rectangle((int)Projectile.localAI[1], 0, Math.Min(laser.Width, (int)Projectile.localAI[2]), laser.Height)),
                    rayColor,
                    Projectile.rotation,
                    new Vector2(0.0f, (float)(laser.Width / 4f)),
                    new Vector2((float)((double)num / laser.Width), Projectile.scale) * scale2, SpriteEffects.None, 0.0f);

                if (i % 3 == 0) {
                    batch.Draw(flash, position2 + Vector2.UnitY * 6f, DrawInfo.Default with {
                        Clip = clip2,
                        Origin = origin2,
                        Color = rayColor * 0.35f,
                        Scale = scale2 * 1.5f * Helper.Wave(0.9f, 1.1f, 5f, 2f * seeded) * Helper.Wave(0.65f, 1.35f, 0f, seeded) * 0.75f
                    });
                }

                batch.Draw(star, position + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.localAI[2] * 8.35f * MathHelper.Lerp(scale.X, 1f, 0.75f), DrawInfo.Default with {
                    Clip = clip4,
                    Origin = origin4,
                    Scale = scale * 0.6f,
                    Rotation = Projectile.rotation + Projectile.localAI[0] * 0.05f,
                    Color = color * 0.5f * Utils.GetLerpValue(0, 10, Projectile.timeLeft, true)
                });
            }
        }, blendState: BlendState.Additive);
    }
}
