using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Nature.CoralClarionet;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class CoralClarionet : CaneBaseItem<CoralClarionet.CoralClarionetBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Projectiles.Friendly.Nature.CoralClarionet>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 40);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    public sealed class CoralClarionetBase : CaneBaseProjectile {
        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            spawnPosition = GetSpawnPosition(player);
        }

        public static Vector2 GetSpawnPosition(Player player) {
            Vector2 spawnPosition = player.Center;
            int maxChecks = 50;
            while (maxChecks-- > 0 && !WorldGenHelper.SolidTileNoPlatform(spawnPosition.ToTileCoordinates())) {
                spawnPosition += spawnPosition.DirectionTo(player.GetWorldMousePosition()) * WorldGenHelper.TILESIZE;
            }
            return spawnPosition;
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.3f, 0.025f);

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            float baseStep = step;
            step = Ease.CubeIn(step);
            int num548 = 2;
            int width = 25;
            int height = 25;
            width = (int)(width * MathF.Max(0.5f, step));
            height = (int)(height * MathF.Max(0.5f, step));
            for (int i = 0; i < 2; i++) {
                for (int num549 = 0; num549 < num548; num549++) {
                    if (Main.rand.NextChance(0.5f - Utils.Remap(baseStep, 0f, 1f, 0f, 0.5f))) {
                        continue;
                    }
                    if (Main.rand.NextChance(Utils.Remap(baseStep, 0f, 1f, 0.5f, 0f))) {
                        continue;
                    }
                    if (Main.rand.NextBool()) {
                        continue;
                    }
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                    Vector2 spinningpoint = Vector2.Normalize(velocity) * new Vector2((float)width / 2f, height) * 0.75f;
                    spinningpoint = spinningpoint.RotatedBy((double)(num549 - (num548 / 2 - 1)) * Math.PI / (double)num548) + corePosition;
                    Vector2 vector42 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - (float)Math.PI / 2f).ToRotationVector2();
                    Vector2 circleSize = Vector2.UnitY * 10f;
                    Vector2 vector422 = vector42;
                    vector42 += circleSize.RotatedBy(((i == 0 ? (1f - AttackProgress01) : AttackProgress01) * MathHelper.Pi + MathHelper.PiOver4) * -player.direction)
                        .RotatedBy(corePosition.DirectionTo(player.GetWorldMousePosition()).ToRotation() + MathHelper.Pi + (player.direction > 0).ToInt() * MathHelper.Pi) * 1f;
                    int num550 = Dust.NewDust(spinningpoint + vector422 + Vector2.UnitY * 3f - new Vector2(width / 4f, height / 4f),
                        0, 0, Main.rand.NextBool() ? DustID.Water : ModContent.DustType<Water>(), vector42.X * 2f, vector42.Y * 2f,
                        Main.rand.Next(100, 200), default(Color), 1f + 0.4f * Main.rand.NextFloat());
                    Main.dust[num550].noGravity = Main.rand.NextBool();
                    Main.dust[num550].noLight = true;
                    Dust dust2 = Main.dust[num550];
                    dust2.velocity /= 4f;
                    dust2 = Main.dust[num550];
                    dust2.velocity -= velocity;
                    dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
                    dust2.velocity *= baseStep;
                }
            }

            for (int i = 0; i < 4; i++) {
                if (Main.rand.NextChance(0.5f - Utils.Remap(baseStep, 0f, 1f, 0f, 0.5f))) {
                    continue;
                }
                if (Main.rand.NextChance(Utils.Remap(baseStep, 0f, 1f, 0.5f, 0f))) {
                    continue;
                }
                if (Main.rand.NextBool()) {
                    continue;
                }
                if (Main.rand.NextBool()) {
                    continue;
                }
                int type = Main.rand.NextBool() ? DustID.Water : ModContent.DustType<Water>();
                Vector2 spinningpoint = Vector2.UnitX.RotatedBy((double)Main.rand.NextFloat() * MathHelper.TwoPi);
                Vector2 center = CorePosition + Vector2.UnitX.RotatedBy(Projectile.rotation) * 4f * -Projectile.direction + new Vector2(Projectile.direction == 1 ? 3f : -3f, 0f) + spinningpoint * 15;
                Vector2 rotationPoint = spinningpoint.RotatedBy(0.785 * Projectile.direction);
                Vector2 position = center + rotationPoint * 5f;
                int dust = Dust.NewDust(position, 0, 0, type);
                Main.dust[dust].position = position;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f;
                Main.dust[dust].velocity = rotationPoint * -2f;
                Main.dust[dust].scale = Main.rand.NextFloat() * Main.rand.NextFloat(1f, 1.25f);
                Main.dust[dust].position += Main.dust[dust].velocity * -5f;
                Main.dust[dust].alpha = Main.rand.Next(100, 200);
            }

            player.SyncMousePosition();
            Vector2 destination = GetSpawnPosition(player);
            Vector2 destination2 = destination + Vector2.UnitY * 50f * 3f;
            int attempt = 50;
            while (attempt > 0 && Collision.SolidCollision(destination2, 0, 0)) {
                destination -= Vector2.UnitY * 1f * 3f;
                attempt--;
            }
            float progress = Ease.CircIn(Ease.CircOut(MathUtils.Clamp01(AttackProgress01 * 1.5f)));
            if (progress < 1f) {
                for (int i = -1; i < 2; i += 2) {
                    Vector2 spawnPosition = destination + new Vector2(MathHelper.Lerp(50f, 25f, MathUtils.YoYo(progress)) * i, MathHelper.Lerp(-60f, 40f, progress));
                    for (int num807 = 0; (float)num807 < 10f; num807++) {
                        if (Main.rand.NextBool()) {
                            continue;
                        }
                        Vector2 velocity = spawnPosition.DirectionTo(destination);
                        int num808 = Dust.NewDust(spawnPosition + Main.rand.RandomPointInArea(10f), width, height, Main.rand.NextBool() ? DustID.Water : ModContent.DustType<Water>(),
                            velocity.X, velocity.Y, Main.rand.Next(100, 200), default(Color), 1.1f);
                        Main.dust[num808].noGravity = true;
                        Dust dust2 = Main.dust[num808];
                        dust2.velocity *= 0.1f;
                        dust2 = Main.dust[num808];
                        dust2.velocity += velocity * Main.rand.NextFloat(1f, 10f);
                        dust2 = Main.dust[num808];
                    }
                }
            }
        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            float baseStep = 1f;
            int num548 = 10;
            int width = 25;
            int height = 25;
            width = (int)(width * MathF.Max(0.5f, baseStep));
            height = (int)(height * MathF.Max(0.5f, baseStep));
            for (int i = 0; i < 2; i++) {
                for (int num549 = 0; num549 < num548; num549++) {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
                    Vector2 spinningpoint = Vector2.Normalize(velocity) * new Vector2((float)width / 2f, height) * 0.75f;
                    spinningpoint = spinningpoint.RotatedBy((double)(num549 - (num548 / 2 - 1)) * Math.PI / (double)num548) + corePosition;
                    Vector2 vector42 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - (float)Math.PI / 2f).ToRotationVector2();
                    Vector2 circleSize = Vector2.UnitY * 10f;
                    Vector2 vector422 = vector42;
                    vector42 += circleSize.RotatedBy(((i == 0 ? (1f - AttackProgress01) : AttackProgress01) * MathHelper.Pi + MathHelper.PiOver4) * -player.direction)
                        .RotatedBy(corePosition.DirectionTo(player.GetWorldMousePosition()).ToRotation() + MathHelper.Pi + (player.direction > 0).ToInt() * MathHelper.Pi) * 1f;
                    int num550 = Dust.NewDust(spinningpoint + vector422 + Vector2.UnitY * 3f - new Vector2(width / 4f, height / 4f),
                        0, 0, Main.rand.NextBool() ? DustID.Water : ModContent.DustType<Water>(), vector42.X * 2f, vector42.Y * 2f,
                        Main.rand.Next(100, 200), default(Color), 1f + 0.4f * Main.rand.NextFloat());
                    Main.dust[num550].noGravity = Main.rand.NextBool();
                    Main.dust[num550].noLight = true;
                    Dust dust2 = Main.dust[num550];
                    dust2.velocity /= 4f;
                    dust2 = Main.dust[num550];
                    dust2.velocity -= velocity;
                    dust2.velocity *= Main.rand.NextFloat(0.75f, 1f);
                    dust2.velocity *= baseStep;
                }
            }
        }

        protected override void AfterProcessingCane() {
            if (!ShotWhenEndedAttackAnimation) {
                return;
            }

            for (int i = 0; i < 4; i++) {
                if (!Main.rand.NextBool(3)) {
                    continue;
                }
                int type = Main.rand.NextBool() ? DustID.Water : ModContent.DustType<Water>();
                Vector2 spinningpoint = Vector2.UnitX.RotatedBy((double)Main.rand.NextFloat() * MathHelper.TwoPi);
                Vector2 center = CorePosition + Vector2.UnitX.RotatedBy(Projectile.rotation) * 4f * -Projectile.direction + new Vector2(Projectile.direction == 1 ? 3f : -3f, 0f) + spinningpoint * 15;
                Vector2 rotationPoint = spinningpoint.RotatedBy(0.785 * Projectile.direction);
                Vector2 position = center + rotationPoint * 5f;
                int dust = Dust.NewDust(position, 0, 0, type);
                Main.dust[dust].position = position;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f;
                Main.dust[dust].velocity = rotationPoint * -2f;
                Main.dust[dust].scale = Main.rand.NextFloat() * Main.rand.NextFloat(1f, 1.25f);
                Main.dust[dust].position += Main.dust[dust].velocity * -5f;
                Main.dust[dust].alpha = Main.rand.Next(100, 150);
            }
        }

        public override void PostDraw(Color lightColor) {
            if (!AssetInitializer.TryGetRequestedTextureAssets<Projectiles.Friendly.Nature.CoralClarionet>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
                return;
            }

            Texture2D waterTexture = indexedTextureAssets[(byte)CoralClarionetRequstedTextureType.Water2].Value;
            float attackProgress = AttackProgress01;
            float waveValue = (TimeSystem.TimeForVisualEffects * 60f + Projectile.identity * 3) * MathHelper.Lerp(0.1f, 0.2f, attackProgress);
            bool facedRight = Projectile.FacedRight();
            SpriteFrame waterFrame = new(1, 3, 0, (byte)(facedRight ? (3 - waveValue % 3) : (waveValue % 3)));
            Rectangle waterClip = waterFrame.GetSourceRectangle(waterTexture);
            Vector2 waterOrigin = waterClip.Centered();
            Color waterColor = lightColor;
            //waterColor.A = (byte)MathHelper.Lerp(255, 200, attackProgress);
            waterColor *= 0.25f;
            waterColor *= attackProgress;
            Vector2 waterScale = new Vector2(0.75f, 0.625f) * 2f * MathHelper.Lerp(0.5f, 0.75f, attackProgress);
            float waterRotation = Projectile.rotation + Helper.Wave(waveValue, -0.25f, 0.25f, 10f, Projectile.identity);
            DrawInfo waterDrawInfo = DrawInfo.Default with {
                Clip = waterClip,
                Origin = waterOrigin,
                Color = waterColor,
                Scale = waterScale,
                Rotation = waterRotation
            };
            Vector2 position = CorePosition - Vector2.One;
            position += Vector2.UnitX.RotatedBy(waterRotation) * 4f * -Projectile.direction;
            SpriteBatch batch = Main.spriteBatch;
            batch.Draw(waterTexture, position, waterDrawInfo);
            for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
                Color waterColor2 = waterColor * 0.75f;
                waterColor2 = waterColor.MultiplyAlpha(Helper.Wave(waveValue, 0.625f, 1f, 10f, Projectile.identity + num5 * 5f));
                waterColor2 *= 0.5f;
                Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 6f * MathF.Sin(waveValue * 10f + num5);
                float waterRotation2 = waterRotation + Helper.Wave(waveValue, -0.25f, 0.25f, 10f, Projectile.identity + num5 * 5f);
                batch.Draw(waterTexture, position + vector2, waterDrawInfo with { Color = waterColor2, Rotation = waterRotation2 });
            }
        }
    }
}
