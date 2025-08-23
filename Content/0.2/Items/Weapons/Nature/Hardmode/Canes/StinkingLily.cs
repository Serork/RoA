﻿using Microsoft.Xna.Framework;
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
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Items.Weapons.Nature.PreHardmode.Canes.TulipBase;
using static RoA.Content.Projectiles.Friendly.Nature.Rafflesia;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class StinkingLily : CaneBaseItem<StinkingLily.StinkingLilyBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Rafflesia>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38);
        Item.SetWeaponValues(30, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public sealed class StinkingLilyBase : CaneBaseProjectile, IRequestAssets {
        private static byte CHARGECOUNT => 5;
        private static byte CHARGEFRAMECOUNT => 3;

        private struct ChargeInfo {
            public Vector2 Position;
            public float Rotation;
            public bool ShouldBeShowed;
            public byte Index;
            public byte Frame;
        }

        private ChargeInfo[]? _chargeData;

        (byte, string)[] IRequestAssets.IndexedPathsToTexture => [(0, ResourceManager.DustTextures + "StinkingLily")];

        protected override bool ShouldWaitUntilProjDespawns() => false;

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
            int dustCount = 10;
            for (int i = 0; i < dustCount; i++) {
                Dust dust = Dust.NewDustPerfect(corePosition + Main.rand.NextVector2CircularEdge(20f + 5f * Main.rand.NextFloat(), 20f + 5f * Main.rand.NextFloat()), ModContent.DustType<Dusts.StinkingLily>());
                dust.velocity = Vector2.UnitY.RotatedBy(i * MathHelper.TwoPi / dustCount) * Owner.direction;
                dust.scale *= Main.rand.NextFloat(1f, 1.5f);
            }
        }

        public void SpawnGroundDusts(Player player, ushort dustType, float velocityFactor) {
            if (!Main.rand.NextChance(AttackProgress01 * 0.8f)) {
                return;
            }

            player.SyncMousePosition();
            Vector2 mousePosition = player.GetWorldMousePosition();
            Vector2 position = GetTilePosition(player, mousePosition, false).ToWorldCoordinates(),
                    velocity = (mousePosition + Main.rand.NextVector2Circular(8f, 8f) - position).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver2) * 3f * velocityFactor;
            Vector2 dustPos = position + Vector2.UnitY * 4f + Main.rand.NextVector2Circular(8f, 8f).RotatedByRandom(MathHelper.Pi);
            //int x = (int)dustPos.X / 16, y = (int)dustPos.Y / 16;

            bool flag = true;
            if (flag) {
                Point tileCoords = position.ToTileCoordinates();
                dustType = (ushort)TileHelper.GetKillTileDust(tileCoords.X, tileCoords.Y, WorldGenHelper.GetTileSafely(tileCoords.X, tileCoords.Y));
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
                    position = Vector2.Lerp(position, CorePosition + Vector2.UnitY.RotatedBy(MathHelper.Pi * direction + i * (float)chargeCount / MathHelper.TwoPi * 1.5f * -direction) * (30f * AttackProgress01), 1f);
                    if (AttackProgress01 < 1f) {
                        chargeData.Rotation += 0.05f * direction;
                    }
                }
            }
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.15f, Owner.direction == 1 ? -0.1f : 0f);

        protected override void SafePreDraw() {
            if (!AssetInitializer.TryGetRequestedTextureAssets<StinkingLilyBase>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets) ||
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
            //    Color color = Lighting.GetColor(position.ToTileCoordinates()) * chargeData.Opacity;
            //    Main.spriteBatch.Draw(chargeTexture, position, DrawInfo.Default with {
            //        Clip = clip,
            //        Origin = origin,
            //        Color = color,
            //        Rotation = rotation
            //    });
            //}
        }

        public override void PostDraw(Color lightColor) {
        }
    }
}
