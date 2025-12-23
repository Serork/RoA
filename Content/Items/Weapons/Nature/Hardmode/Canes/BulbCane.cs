using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Nature.Bulb;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class BulbCane : CaneBaseItem<BulbCane.BulbCaneBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Projectiles.Friendly.Nature.Bulb>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(36, 44);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    public sealed class BulbCaneBase : CaneBaseProjectile, IRequestAssets {
        private static byte SHARDCOUNT => 6;

        public enum BulbCaneBase_RequstedTextureType : byte {
            SmallSummonMouth
        }

        (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
            [((byte)BulbCaneBase_RequstedTextureType.SmallSummonMouth, ResourceManager.NatureProjectileTextures + "Bulb_SmallSummonMouth")];

        public record struct SmallSummonMouthInfo(Vector2 Position, float Rotation, float Progress, float Opacity);

        private SmallSummonMouthInfo[] _smallSummonMouthData = null!;

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            spawnPosition = player.GetViableMousePosition();
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.35f, -0.175f);

        protected override void Initialize() {
            _smallSummonMouthData = new SmallSummonMouthInfo[SHARDCOUNT];
            for (int i = 0; i < _smallSummonMouthData.Length; i++) {
                ref SmallSummonMouthInfo shardInfo = ref _smallSummonMouthData[i];
                shardInfo.Position = CorePosition;
                shardInfo.Progress = shardInfo.Opacity = 0f;
            }

            ref float miscCounter = ref Projectile.localAI[0];
            miscCounter = 1f;
        }

        protected override void SpawnDustsWhenReady(Player player, Vector2 corePosition) {

        }

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            for (float num17 = 0f; num17 < 1f; num17 += 0.05f) {
                if (Main.rand.NextBool()) {
                    continue;
                }

                Vector2 vector10 = Vector2.UnitX.RotatedBy((float)Math.PI * 2f * num17);
                Vector2 center = CorePosition;
                float num18 = Main.rand.NextFloat(0.5f, 3.5f) * Main.rand.NextFloat(1f, 2f);
                int size = 4;
                Vector2 dustPosition = center + Main.rand.NextVector2CircularEdge(size, size);
                Vector2 dustVelocity = vector10 * num18 * 10f;
                dustPosition += dustVelocity;
                dustVelocity = dustPosition.DirectionTo(center) * 4f;
                Dust dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<TintableDustGlow>(), dustVelocity, Main.rand.Next(50, 150), new Color(166, 178, 53) with { A = 50 }, Main.rand.NextFloat(1.25f, 2f) * 2f);
                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat() * 1.2f;
                dust.noLightEmittence = true;
            }
            for (float num17 = 0f; num17 < 1f; num17 += 0.1f) {
                if (Main.rand.NextBool()) {
                    continue;
                }

                Vector2 vector10 = Vector2.UnitX.RotatedBy((float)Math.PI * 2f * num17);
                Vector2 center = CorePosition;
                float num18 = Main.rand.NextFloat(0.5f, 3.5f) * Main.rand.NextFloat(1f, 2f);
                int size = 4;
                Vector2 dustPosition = center + Main.rand.NextVector2CircularEdge(size, size);
                Vector2 dustVelocity = vector10 * num18;
                Dust dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<TintableDustGlow>(), dustVelocity, Main.rand.Next(50, 150), new Color(166, 178, 53) with { A = 50 }, Main.rand.NextFloat(1.25f, 2f) * 2f);
                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat() * 1.2f;
                dust.noLightEmittence = true;
            }
        }

        private float ReleaseProgress { 
            get {
                float progress3 = 1f - Utils.GetLerpValue(0.1f, 0.75f, AfterReleaseProgress01, true);
                progress3 = Ease.CubeInOut(progress3);
                return progress3;
            }
        }

        protected override void AfterProcessingCane() {
            ref float miscCounter = ref Projectile.localAI[0];
            miscCounter = MathHelper.Lerp(miscCounter, 0.1f, 0.05f);

            int totalIndexesInGroup = _smallSummonMouthData.Length;
            for (int i = 0; i < totalIndexesInGroup; i++) {
                ref SmallSummonMouthInfo shardInfo = ref _smallSummonMouthData[i];
                float progress = 1f - AttackProgress01;
                float progress2 = (float)i / totalIndexesInGroup;
                if (progress2 < progress) {
                    shardInfo.Position = CorePosition;
                    continue;
                }

                //float sizeFactor = MathHelper.Lerp(1f, 0f, Ease.CubeInOut(Utils.GetLerpValue(0.9f, 1f, ReleaseProgress, true)));
                //miscCounter *= sizeFactor;
                float miscCounter2 = miscCounter + Owner.miscCounter * 6f + i * 2;
                //miscCounter2 *= sizeFactor;
                float f = (i / (float)totalIndexesInGroup + miscCounter2 * 1f) * ((float)Math.PI * 2f);
                Vector2 vector2 = f.ToRotationVector2();
                float y = MathF.Sin(i * 3 + miscCounter2 * TimeSystem.LogicDeltaTime);
                float num = 4f + (float)totalIndexesInGroup * 2f;
                //sizeFactor = MathHelper.Lerp(1f, 1.5f, Ease.CubeInOut(Utils.GetLerpValue(0.9f, 1f, AttackProgress01, true)));
                //num *= sizeFactor;
                shardInfo.Progress = y;
                shardInfo.Opacity = Utils.GetLerpValue(0.75f, 0.1f, miscCounter, true);
                Vector2 value = CorePosition + vector2 * new Vector2(0.75f, y) * num;
                Vector2 oldPosition = shardInfo.Position;
                shardInfo.Position = Vector2.Lerp(shardInfo.Position, value, 0.3f);
                shardInfo.Position = Vector2.Lerp(shardInfo.Position, CorePosition, ReleaseProgress);
                shardInfo.Rotation = Utils.AngleLerp(shardInfo.Rotation, oldPosition.AngleTo(shardInfo.Position) - MathHelper.PiOver2, 0.5f);
            }
        }

        private void DrawSmallSummonMise(bool inPreDraw) {
            if (!AssetInitializer.TryGetRequestedTextureAssets<BulbCaneBase>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
                return;
            }
            if (!AssetInitializer.TryGetRequestedTextureAssets<Bulb>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets_Bulb)) {
                return;
            }

            Texture2D texture = indexedTextureAssets[(byte)BulbCaneBase_RequstedTextureType.SmallSummonMouth].Value,
                      leafStemTexture = indexedTextureAssets_Bulb[(byte)Bulb_RequstedTextureType.LeafStem].Value;
            SpriteBatch batch = Main.spriteBatch;

            bool shouldSkipSmallSummonMouth(SmallSummonMouthInfo smallSummonMouthInfo) => !inPreDraw && smallSummonMouthInfo.Progress > 0f;

            // LEAF STEM 1
            Rectangle leafStemClip = leafStemTexture.Bounds;
            Vector2 leafStemOrigin = leafStemClip.BottomCenter();
            DrawInfo leafStemDrawInfo = new() {
                Clip = leafStemClip,
                Origin = leafStemOrigin
            };

            int i = 0;
            foreach (SmallSummonMouthInfo smallSummonMouthInfo in _smallSummonMouthData) {
                if (shouldSkipSmallSummonMouth(smallSummonMouthInfo)) {
                    continue;
                }

                Vector2 position = smallSummonMouthInfo.Position;

                Vector2 startPosition = CorePosition,
                        endPosition = position;
                while (true) {
                    Vector2 velocityToSummonMouthPosition = startPosition.DirectionTo(endPosition);
                    float stemRotation = velocityToSummonMouthPosition.ToRotation() + MathHelper.PiOver2;

                    float step = 2;

                    if (Vector2.Distance(startPosition, position) < step) {
                        break;
                    }

                    batch.Draw(leafStemTexture, startPosition, leafStemDrawInfo.WithScale(0.75f) with {
                        Rotation = stemRotation
                    });

                    startPosition += velocityToSummonMouthPosition * step;
                }

                i += 3;

                float a = 255f;
                float progress = Ease.CubeOut(ReleaseProgress);
                a = MathHelper.Lerp(a, 50, progress);
                Color baseColor = Color.White;
                baseColor = Color.Lerp(baseColor, Color.Yellow, progress);
                Color color = baseColor with { A = (byte)a } * smallSummonMouthInfo.Opacity * 1f;
                int smallSummonMouthFrame = (int)((Main.GlobalTimeWrappedHourly * 10 + i) % 2);
                Rectangle clip = Utils.Frame(texture, 1, 2, frameY: smallSummonMouthFrame);
                Vector2 origin = clip.Centered();
                float waveFrequency = 30f;
                float attackProgress = AttackProgress01;
                attackProgress = Ease.CubeOut(attackProgress);
                float waveStartValue = MathHelper.Lerp(0.75f, 1f, 1f - attackProgress);
                Vector2 scale = new(1f * Helper.Wave(waveStartValue, 1f, waveFrequency, 1f), 1f * Helper.Wave(waveStartValue, 1f, waveFrequency, 2f));
                float rotation = smallSummonMouthInfo.Rotation;
                DrawInfo drawInfo = new() {
                    Scale = scale,
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Rotation = rotation
                };
                batch.Draw(texture, position, drawInfo);
            }
        }

        protected override void SafePreDraw() {
            if (!Init) {
                return;
            }

            DrawSmallSummonMise(true);
        }

        protected override void SafePostDraw() {
            if (!Init) {
                return;
            }

            DrawSmallSummonMise(false);

            float attackProgress = AttackProgress01;
            attackProgress = Ease.CubeOut(attackProgress);
            SpriteBatch batch = Main.spriteBatch;
            Texture2D circleTexture = ResourceManager.Circle6;
            Rectangle circleClip = circleTexture.Bounds;
            Vector2 circlePosition = CorePosition,
                    circleOrigin = circleClip.Centered();
            circlePosition -= Main.screenPosition;
            float alpha = 0.875f * attackProgress;
            float circleScale = 0.125f;
            circleScale *= attackProgress;
            float waveFrequency = 30f;
            float waveStartValue = MathHelper.Lerp(0.5f, 1f, 1f - attackProgress);
            Vector2 circleFinalScale = new(circleScale * Helper.Wave(waveStartValue, 1f, waveFrequency, 1f), circleScale * Helper.Wave(waveStartValue, 1f, waveFrequency, 2f));
            Color blackPartColor = Color.Lerp(new Color(133, 142, 48), Color.Black, 0.5f);
            batch.Draw(circleTexture, circlePosition, null, blackPartColor * 0.625f * alpha, 0f, circleOrigin, 1.6f * circleFinalScale, SpriteEffects.None, 0f);
            Color pinkColor1 = blackPartColor with { A = 0 },
                  pinkColor2 = new Color(166, 178, 53) with { A = 50 };
            batch.Draw(circleTexture, circlePosition, null, Color.White with { A = 0 } * 1f * alpha, 0f, circleOrigin, 0.90f * circleFinalScale, SpriteEffects.None, 0f);
            batch.Draw(circleTexture, circlePosition, null, pinkColor1 * 1f * alpha, 0f, circleOrigin, 1.2f * circleFinalScale, SpriteEffects.None, 0f);
            batch.Draw(circleTexture, circlePosition, null, pinkColor2 * 0.5f * alpha, 0f, circleOrigin, 2.4f * circleFinalScale, SpriteEffects.None, 0f);
            batch.Draw(circleTexture, circlePosition, null, pinkColor2 with { A = 0 } * 0.125f * alpha, 0f, circleOrigin, 7.5f * circleFinalScale, SpriteEffects.None, 0f);
        }
    }
}
