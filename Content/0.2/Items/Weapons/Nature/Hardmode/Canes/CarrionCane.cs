using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Nature.Rafflesia;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class CarrionCane : CaneBaseItem<CarrionCane.CarrionCaneBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Rafflesia>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38);
        Item.SetWeaponValues(30, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public sealed class CarrionCaneBase : CaneBaseProjectile, IRequestAssets {
        private static byte CHARGECOUNT => 5;
        private static byte CHARGEFRAMECOUNT => 3;

        public static float CAPPEDMOUSEPOSITIONWIDTH => 800f;
        public static float CAPPEDMOUSEPOSITIONHEIGHT => 600f;

        private struct ChargeInfo {
            public Vector2 Position;
            public float Rotation;
            public bool ShouldBeShowed;
            public byte Index;
            public byte Frame;
        }

        private ChargeInfo[]? _chargeData;

        (byte, string)[] IRequestAssets.IndexedPathsToTexture => [(0, ResourceManager.DustTextures + "CarrionCane")];

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void Initialize() {
            _chargeData = new ChargeInfo[CHARGECOUNT];
            for (int i = 0; i < CHARGECOUNT; i++) {
                _chargeData[i] = new ChargeInfo() {
                    Index = (byte)i,
                    ShouldBeShowed = false,
                    Frame = (byte)Main.rand.Next(CHARGEFRAMECOUNT),
                    Position = CorePosition + Main.rand.NextVector2CircularEdge(30f, 30f),
                };
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            int length = _chargeData!.Length;
            for (int i = 0; i < length; i++) {
                ref ChargeInfo chargeData = ref _chargeData[i];
                if (/*step > (float)i / length && */!chargeData.ShouldBeShowed) {
                    chargeData.ShouldBeShowed = true;
                }
            }

            SpawnGroundDusts(player, 0, Main.rand.NextFloat(0.5f, 1f));
        }

        protected override void SpawnDustsWhenReady(Player player, Vector2 corePosition) {
            int dustCount = 20;
            float num12 = 6f;
            for (int i = 0; i < dustCount; i++) {
                Vector2 vector = Vector2.UnitX.RotatedBy(MathUtils.Clamp01(i / (float)dustCount + Main.rand.NextFloatRange(1f)) * ((float)Math.PI * 2f));
                int num11 = Projectile.width + 5 + (int)(15 * Main.rand.NextFloat());
                Vector2 vector2 = vector * ((float)num11 * Projectile.scale);
                Vector2 vector3 = corePosition + vector2;
                Vector2 vector4 = vector.RotatedBy(MathHelper.PiOver2);
                vector3 += vector4 * num12;
                int num14 = Dust.NewDust(vector3, 0, 0, !Main.rand.NextBool(3) ? ModContent.DustType<Dusts.CarrionCane>() : DustID.Torch, 0f, 0f, 0, default);
                Main.dust[num14].position = vector3;
                Main.dust[num14].noGravity = true;
                Main.dust[num14].scale = 1f + Main.rand.NextFloatRange(0.5f);
                float num13 = Main.rand.NextFloat(1f, 2f);
                Main.dust[num14].fadeIn = Main.rand.NextFloat() * 1.2f * Projectile.scale;
                Main.dust[num14].velocity = vector4 * Projectile.scale * (0f - num13) * player.direction;
                Main.dust[num14].scale *= Projectile.scale;
                Main.dust[num14].velocity += Projectile.velocity * 0.5f;
                Main.dust[num14].position += Main.dust[num14].velocity * -5f;
            }
        }

        public void SpawnGroundDusts(Player player, ushort dustType, float velocityFactor) {
            player.SyncCappedMousePosition();
            Vector2 mousePosition = player.GetCappedWorldMousePosition(CAPPEDMOUSEPOSITIONWIDTH, CAPPEDMOUSEPOSITIONHEIGHT);
            Vector2 position = GetTilePosition(player, mousePosition, false, cappedWidth: (int)CAPPEDMOUSEPOSITIONWIDTH, cappedHeight: (int)CAPPEDMOUSEPOSITIONHEIGHT).ToWorldCoordinates() - Vector2.UnitY * 4f,
                    velocity = (mousePosition + Main.rand.NextVector2Circular(8f, 8f) - position).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver2) * 3f * velocityFactor;
            Vector2 dustPos = position + Vector2.UnitY * 4f + Main.rand.NextVector2Circular(8f, 8f).RotatedByRandom(MathHelper.Pi);
            //int x = (int)dustPos.X / 16, y = (int)dustPos.Y / 16;

            bool flag = true;
            if (flag) {
                Point tileCoords = position.ToTileCoordinates();
                if (!WorldGenHelper.ActiveTile(tileCoords)) {
                    dustType = (ushort)ModContent.DustType<Dusts.CarrionCane3>();
                }
                else {
                    dustType = (ushort)TileHelper.GetKillTileDust(tileCoords.X, tileCoords.Y, WorldGenHelper.GetTileSafely(tileCoords.X, tileCoords.Y));
                }
            }

            dustType = Main.rand.NextBool(4) ? (ushort)ModContent.DustType<Dusts.CarrionCane3>() : !Main.rand.NextBool(3) ? dustType : (ushort)DustID.Torch;

            if (!Main.rand.NextChance(AttackProgress01 * 0.8f)) {
                return;
            }

            Dust dust = Dust.NewDustPerfect(dustPos,
                                            dustType,
                                            Scale: Main.rand.NextFloat(1.5f, 2f));
            dust.velocity = velocity;
            // hardcoded for now
            dust.customData = 0;
            //dust.scale *= 0.75f;
            dust.velocity *= 0.9f;
            dust.noGravity = true;

            if (flag) {
                dust.scale *= 0.65f;
            }
        }

        protected override void AfterProcessingCane() {
            int chargeCount = _chargeData!.Count(x => x.ShouldBeShowed);
            int direction = Owner.direction;
            for (int i = 0; i < _chargeData!.Length; i++) {
                ref ChargeInfo chargeData = ref _chargeData[i];
                if (chargeData.ShouldBeShowed) {
                    ref Vector2 position = ref chargeData.Position;
                    position = Vector2.Lerp(position, CorePosition + Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.Pi * direction + i * (float)chargeCount / MathHelper.TwoPi * 1.5f * -direction) * (30f * AttackProgress01), 1f);
                    if (AttackProgress01 < 1f) {
                        chargeData.Rotation += 0.05f * direction;
                    }
                }
            }
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.15f, Owner.direction == 1 ? -0.1f : 0f);

        protected override void SafePreDraw() {
            if (!AssetInitializer.TryGetRequestedTextureAssets<CarrionCaneBase>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets) ||
                !AssetInitializer.TryGetRequestedTextureAssets<Rafflesia>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets2)) {
                return;
            }

            void drawStems(float colorFactor = 1f, float scaleFactor = 1f) {
                Texture2D textureToDraw = indexedTextureAssets2[(byte)RafflesiaRequstedTextureType.Stem].Value;
                for (int i = 0; i < _chargeData!.Length; i++) {
                    ChargeInfo chargeData = _chargeData[i];
                    if (!chargeData.ShouldBeShowed) {
                        return;
                    }

                    Vector2 endPosition = CorePosition;
                    Vector2 currentPosition = chargeData.Position;
                    if (currentPosition == Vector2.Zero) {
                        continue;
                    }
                    float progress = 0f;
                    int count = 500;
                    float maxLength = (endPosition - currentPosition).Length();
                    float sinOffsetX = 50f;
                    for (int i2 = 0; i2 < count; i2++) {
                        Vector2 between = endPosition - currentPosition;
                        float length = between.Length();
                        float currentProgress = length / maxLength;
                        float scaleProgress = Utils.Clamp(currentProgress, 0f, 1f);
                        int height = 8;
                        Vector2 velocity = (endPosition - currentPosition).SafeNormalize(Vector2.UnitY) * height * scaleProgress;
                        Vector2 velocityToAdd = velocity;
                        velocityToAdd = velocityToAdd.RotatedBy(Math.Sin(i2 * sinOffsetX * Owner.direction) * scaleProgress) * 0.75f;
                        progress += Main.rand.NextFloat(0.0001f, 0.00033f);

                        if (length <= height * 0.3f || 
                            (CorePosition - currentPosition).Length() < 4f) {
                            break;
                        }

                        Vector2 position2 = endPosition;

                        //if (WorldGen.SolidTile(position2.ToTileCoordinates())) {
                        //    continue;
                        //}

                        Vector2 scale = Vector2.One * scaleFactor * scaleProgress;
                        float gradientValue = 1f - scaleProgress;
                        ulong seedForRandomness = (ulong)i;
                        int frameToUse = i == 0 ? Rafflesia.STEMFRAMECOUNT - 1 : Utils.RandomInt(ref seedForRandomness, Rafflesia.STEMFRAMECOUNT);
                        Main.EntitySpriteDraw(textureToDraw,
                                              position2 - scale / 2f - Main.screenPosition,
                                              new Rectangle(0, 10 * frameToUse, 18, height),
                                              Lighting.GetColor(position2.ToTileCoordinates()),
                                              velocityToAdd.ToRotation() + MathHelper.PiOver2,
                                              new Vector2(9, height / 2),
                                              scale,
                                              SpriteEffects.None);

                        endPosition -= velocityToAdd;
                    }
                }
            }

            drawStems();

            //Texture2D chargeTexture = indexedTextureAssets[0].Value;
            //for (int i = 0; i < _chargeData!.Length; i++) {
            //    ChargeInfo chargeData = _chargeData[i];
            //    if (!chargeData.ShouldBeShowed) {
            //        return;
            //    }

            //    float rotation = chargeData.Rotation;
            //    Vector2 position = chargeData.Position;
            //    Rectangle clip = new SpriteFrame(1, CHARGEFRAMECOUNT, 0, chargeData.Frame).GetSourceRectangle(chargeTexture);
            //    Vector2 origin = clip.Size() / 2f;
            //    DrawColor color = Lighting.GetColor(position.ToTileCoordinates()) * chargeData.Opacity;
            //    Main.spriteBatch.Draw(chargeTexture, position, DrawInfo.Default with {
            //        Clip = clip,
            //        Origin = origin,
            //        DrawColor = color,
            //        Rotation = rotation
            //    });
            //}
        }

        public override void PostDraw(Color lightColor) {
        }

        // adapted vanilla
        public static Point GetTilePosition(Player player, Vector2 targetSpot, bool randomlySelected = true, int adjustYForPlatformAmount = 0, int cappedWidth = 0, int cappedHeight = 0) {
            Point point;
            Vector2 center = player.Center;
            Vector2 endPoint = targetSpot;
            int samplesToTake = 3;
            float samplingWidth = 4f;
            Collision.AimingLaserScan(center, endPoint, samplingWidth, samplesToTake, out Vector2 vectorTowardsTarget, out float[] samples);
            float num = float.PositiveInfinity;
            for (int i = 0; i < samples.Length; i++) {
                if (samples[i] < num) {
                    num = samples[i];
                }
            }
            float distanceToTarget = targetSpot.Distance(center);
            targetSpot = center + vectorTowardsTarget.SafeNormalize(Vector2.Zero) * num;
            point = targetSpot.ToTileCoordinates();
            while (!WorldGenHelper.SolidTile(point.X, point.Y)) {
                point.Y++;
            }
            Rectangle value = new Rectangle(point.X, point.Y, 1, 1);
            value.Inflate(1, 1);
            Rectangle value2 = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
            value2.Inflate(-40, -40);
            value = Rectangle.Intersect(value, value2);
            List<Point> list = new List<Point>();
            for (int j = value.Left; j <= value.Right; j++) {
                for (int k = value.Top; k <= value.Bottom; k++) {
                    if (!WorldGenHelper.SolidTile(j, k)) {
                        continue;
                    }
                    Point checkPosition = new(j, k);
                    Vector2 checkPositionWorld = checkPosition.ToWorldCoordinates();
                    if (cappedWidth != 0) {
                        if (MathF.Abs(checkPositionWorld.X - center.X) > cappedWidth) {
                            continue;
                        }
                    }
                    if (cappedHeight != 0) {
                        if (MathF.Abs(checkPositionWorld.Y - center.Y) > cappedHeight) {
                            continue;
                        }
                    }
                    if (WorldGenHelper.IsPlatform(checkPosition) || WorldGenHelper.IsPlatform(checkPosition + new Point(0, 1))) {
                        checkPosition.Y += adjustYForPlatformAmount;
                    }
                    while (WorldGenHelper.SolidTile(checkPosition - new Point(0, 1))) {
                        checkPosition.Y--;
                    }
                    list.Add(checkPosition);
                }
            }
            if (list.Count == 0) {
                list.Add((center + Vector2.UnitY * 6f).ToTileCoordinates().ToVector2().ToPoint());
            }
            int index = Main.rand.Next(list.Count);
            return randomlySelected ? list[index] : list[list.Count / 2];
        }
    }
}
