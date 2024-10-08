
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Druidic.Rods;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class TulipPetal : NatureProjectile {
    private static Vector2 _spawnPosition;
    private static Projectile _parent;

    private byte UsedFrameX => (byte)Projectile.ai[0];
    private bool ParentFound => !(_parent == null || !_parent.active);
    private bool IsWeepingTulip => UsedFrameX == 2;
    public float Max => IsWeepingTulip ? 180f : 120f;

    private int MeInQueue => (int)Projectile.ai[2];
    private bool IsFirst => MeInQueue < 1;

    private Vector2 Offset => Vector2.UnitY * (146 * 0.55f - Projectile.Size.Y);

    public override string Texture => ResourceManager.EmptyTexture;

    protected override void SafeSetDefaults() {
        Projectile.Size = new Vector2(35, 30);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 120;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.hide = true;
        Projectile.netImportant = true;
        Projectile.alpha = 255;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        if (IsFirst) {
            writer.WriteVector2(_spawnPosition);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        if (IsFirst) {
            _spawnPosition = reader.ReadVector2();
        }
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Player player = Main.player[Projectile.owner];
        if (IsFirst && player.whoAmI == Main.myPlayer) {
            _spawnPosition = player.GetModPlayer<TulipBase.TulipBaseExtraData>().SpawnPositionMid;

            Projectile.netUpdate = true;
        }
    }

    public override void AI() {
        if (Projectile.ai[1] == 1f) {
            Projectile.ai[1] = 0f;

            Projectile.netUpdate = true;
        }

        if (IsFirst) {
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner != Projectile.owner) {
                    continue;
                }

                if (projectile.type == ModContent.ProjectileType<TulipFlower>()) {
                    _parent = projectile;
                }
            }
        }

        if (!ParentFound) {
            Projectile.Kill();

            return;
        }

        Projectile.alpha -= 15;
        if (Projectile.alpha < 0) {
            Projectile.alpha = 0;
        }

        Vector2 parentPosition = _parent.Center + Vector2.UnitY * 2f;
        Projectile.position = parentPosition;
        Projectile.rotation = MeInQueue * (MathHelper.TwoPi / 6) + _parent.rotation;

        if (IsWeepingTulip) {
            Projectile.localAI[1] += Math.Abs(Projectile.rotation - Projectile.localAI[0]);

            if (Projectile.owner == Main.myPlayer) {
                if (Projectile.localAI[1] > 1f) {
                    if (Projectile.localAI[0] != 0f) {
                        if (Main.rand.NextChance(0.5)) {
                            Vector2 spawnPosition = Projectile.position + _parent.velocity * 8f + Offset.RotatedBy(Projectile.rotation);
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition, (Vector2.UnitX * -Math.Sign(_parent.velocity.X)).RotatedBy(Projectile.rotation) * Projectile.width * 0.1f, ModContent.ProjectileType<Bone>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        }
                    }

                    Projectile.localAI[1] = 0f;
                }

                Projectile.netUpdate = true;
            }

            Projectile.localAI[0] = Projectile.rotation;
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
        behindProjectiles.Add(index);
    }

    public override bool? CanCutTiles() => false;

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        int width = (int)Projectile.Size.X + 5;
        Vector2 position = Projectile.position + Offset.RotatedBy(Projectile.rotation);
        hitbox = new Rectangle((int)position.X - width / 2, (int)position.Y - width / 2, width, width);
    }

    public override bool PreDraw(ref Color lightColor) {
        if (!ParentFound) {
            return false;
        }

        if (IsFirst) {
            DrawStem(ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "FlowerStem").Value, _parent.Center - Vector2.UnitY * 10f, _spawnPosition);
        }

        Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "FlowerPetal").Value;

        Vector2 position = Projectile.position + Vector2.UnitY * -3f;

        float lerpValue = Utils.GetLerpValue(0f, 10f, _parent.localAI[0], clamped: true);
        Vector2 scale = new Vector2(MathHelper.Lerp(0.25f, 1f, lerpValue), 1f) * new Vector2(Utils.GetLerpValue(Max, Max - 15f, _parent.localAI[0], clamped: true)) * Projectile.scale * 0.65f;

        SpriteFrame frame = new(3, 1);
        frame = frame.With(UsedFrameX, 0);
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        Vector2 lightPosition = Projectile.position + Offset.RotatedBy(Projectile.rotation + MathHelper.Pi);
        Main.EntitySpriteDraw(texture,
                              position - Main.screenPosition,
                              sourceRectangle,
                              Lighting.GetColor(lightPosition.ToTileCoordinates()) * _parent.Opacity,
                              Projectile.rotation,
                              new Vector2(sourceRectangle.Centered().X, texture.Height),
                              scale,
                              SpriteEffects.None);
        return false;
    }

    private void DrawStem(Texture2D textureToDraw, Vector2 currentPosition, Vector2 endPosition) {
        int height = 12;
        Vector2 velocity = (endPosition - currentPosition).SafeNormalize(Vector2.UnitY) * height;
        float progress = 0f;
        float maxOffsetX = 50f;
        for (int i = 2; i < 500; i++) {
            Vector2 between = endPosition - currentPosition;
            float length = between.Length();
            Vector2 velocityToAdd = velocity;
            if (progress > 0.25f && progress <= 0.75f) {
                velocityToAdd = Vector2.Normalize(Vector2.Lerp(velocityToAdd, between, (progress - 0.1f) / 0.2f) * 0.02f) * height;
            }
            else {
                velocityToAdd = velocityToAdd.RotatedBy(Math.Sin(i + i * maxOffsetX + currentPosition.Length() / 64f) * 0.25f);
            }
            endPosition -= velocityToAdd;
            progress += Main.rand.NextFloat(0.0001f, 0.00033f);

            if (length <= height) {
                break;
            }

            Vector2 position = endPosition + Vector2.UnitY * 10f;

            if (WorldGen.SolidTile(position.ToTileCoordinates())) {
                continue;
            }

            float lerpValue = Utils.GetLerpValue(0f, 10f, _parent.localAI[0], clamped: true);
            Vector2 scale = new Vector2(MathHelper.Lerp(0.25f, 1f, lerpValue), 1f) * Projectile.scale * new Vector2(Utils.GetLerpValue(Max, Max - 15f, _parent.localAI[0], clamped: true), 1f);
            ulong seedForRandomness = (ulong)i;
            Main.EntitySpriteDraw(textureToDraw,
                                  position - Main.screenPosition,
                                  new Rectangle(20 * Utils.RandomInt(ref seedForRandomness, 5), 20 * Utils.RandomInt(ref seedForRandomness, 3) + 60 * UsedFrameX, 20, 20),
                                  Lighting.GetColor(position.ToTileCoordinates()) * _parent.Opacity,
                                  velocityToAdd.ToRotation() + MathHelper.PiOver2,
                                  new Vector2(10, 10),
                                  scale,
                                  SpriteEffects.None);
        }
    }
}

sealed class TulipFlower : NatureProjectile {
    private byte UsedFrameX => (byte)Projectile.ai[0];
    private bool IsWeepingTulip => UsedFrameX == 2;
    public float Max => IsWeepingTulip ? 180f : 120f;

    public override string Texture => ResourceManager.EmptyTexture;

    protected override void SafeSetDefaults() {
        Projectile.Size = 8 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
    }

    public override bool? CanDamage() => false;

    protected override void SafeOnSpawn(IEntitySource source) {
        Projectile.timeLeft = (int)Max;

        if (Projectile.owner == Main.myPlayer) {
            Player player = Main.player[Projectile.owner];
            for (int i = 0; i < 6; i++) {
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(player.GetSelectedItem()),
                                               Projectile.Center, Vector2.Zero,
                                               ModContent.ProjectileType<TulipPetal>(),
                                               Projectile.damage, Projectile.knockBack,
                                               Projectile.owner,
                                               UsedFrameX,
                                               1f,
                                               i);
                Projectile.netUpdate = true;
            }
        }
    }

    public override void AI() {
        Projectile.localAI[0] += 1f;
        if (Projectile.localAI[0] >= Max) {
            Projectile.Kill();

            return;
        }

        Projectile.scale = Utils.GetLerpValue(0f, 10f, Projectile.localAI[0], clamped: true) * Utils.GetLerpValue(Max, Max - 15f, Projectile.localAI[0], clamped: true);

        Player player = Main.player[Projectile.owner];
        float inertia = 15f * Projectile.scale, speed = (IsWeepingTulip ? 4f : 3.35f) * Projectile.scale;
        if (Projectile.owner == Main.myPlayer) {
            Vector2 pointPosition = IsWeepingTulip ? player.GetViableMousePosition(640f, 420f) : player.GetViableMousePosition(480f, 300f);
            Projectile.ai[1] = pointPosition.X;
            Projectile.ai[2] = pointPosition.Y;

            Projectile.netUpdate = true;
        }
        Vector2 direction = new Vector2(Projectile.ai[1], Projectile.ai[2]) - Projectile.Center;
        if (direction.Length() > 10f) {
            direction.Normalize();
            Projectile.velocity = (Projectile.velocity * inertia + direction * speed) / (inertia + 1f);
        }
        else {
            Projectile.velocity *= (float)Math.Pow(0.97, inertia * 2.0 / inertia);
        }
        Projectile.rotation += Projectile.velocity.Length() * 0.01f * Projectile.direction;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.ProjectileTextures + "Flower").Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        float rotation = Projectile.rotation;
        float scale = Projectile.scale;

        SpriteFrame frame = new(3, 1);
        frame = frame.With(UsedFrameX, 0);
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);

        Main.EntitySpriteDraw(texture,
                              position,
                              sourceRectangle,
                              lightColor * Projectile.Opacity,
                              rotation,
                              sourceRectangle.Centered(),
                              scale,
                              SpriteEffects.None);

        return false;
    }
}

sealed class Bone : NatureProjectile {
    public override string Texture => $"Terraria/Images/Projectile_{21}";

    protected override void SafeSetDefaults() {
        Projectile.CloneDefaults(ProjectileID.Bone);
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.scale *= 0.9f;
    }
}