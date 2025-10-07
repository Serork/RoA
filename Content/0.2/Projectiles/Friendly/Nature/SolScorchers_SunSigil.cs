using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Claws;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Common.VisualEffects;
using RoA.Content.VisualEffects;
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

using static Terraria.GameContent.Animations.Actions.Sprites;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SunSigil : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 3000;

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

    protected override void SafeSetDefaults() {
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

        Projectile.timeLeft = 2;

        Projectile.scale = 0.75f;

        if (Projectile.localAI[0] == 0f) {
            Projectile.Center = owner.Top;
            Projectile.Center = Utils.Floor(Projectile.Center) - Vector2.UnitY * 50f + Vector2.UnitY * owner.gfxOffY;
            if (Projectile.IsOwnerLocal()) {
                Projectile.velocity = Projectile.Center.DirectionTo(owner.GetWorldMousePosition());
                Projectile.netUpdate = true;
            }
        }

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

        Vector2 samplingPoint = Projectile.Center;
        float distance = 0f;
        while (distance < 2000f) {
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

        if (Projectile.localAI[0] % 8 != 0) {
            return;
        }
        Vector2 position = last;
        Vector2 velocity = last.DirectionTo(Projectile.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.25f, 1.25f);
        VisualEffectSystem.New<VisualEffects.SunSigil>(VisualEffectLayer.BEHINDPLAYERS)?.Setup(position, velocity,
            scale: Main.rand.NextFloat(1f, 1.5f),
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
        //    Color = color * 0.5f
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
                Color rayColor = color * Helper.Wave(0.475f, 0.525f, 10f, seeded) * 1.25f;
                Vector2 scale = Projectile.scale * Helper.Wave(0.9f, 1f, 2.5f, seeded).ToRotationVector2() / MathHelper.Pi * 2.9f * new Vector2(1.3f, 1f);
                batch.Draw(glowTexture, position2, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = rayColor,
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
                    Color = color * 0.5f
                });
            }
        }, blendState: BlendState.Additive);
    }
}
