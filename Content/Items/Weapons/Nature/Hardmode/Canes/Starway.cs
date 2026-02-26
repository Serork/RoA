using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

[AutoloadGlowMask()]
sealed class Starway : CaneBaseItem<Starway.StarwayBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(42, 46);
        Item.SetWeaponValues(200, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 45, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.StrongRed10, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 400);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<StarwayWormhole>();

    public sealed class StarwayBase : CaneBaseProjectile {
        private static Asset<Texture2D> _wormTexture = null!;

        private record struct WormInfo(Vector2[] Positions, float[] Rotations, Vector2 Velocity = default);

        private WormInfo[] _wormData = null!;

        public override void SetStaticDefaults() {
            if (!Main.dedServ) {
                _wormTexture = ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<Starway>()).Texture + "_Worm");
            }
        }

        protected override void Initialize() {
            int length = 5;

            _wormData = new WormInfo[length];
            for (int i = 0; i < _wormData.Length; i++) {
                Vector2[] positions = new Vector2[5];
                float[] rotations = new float[positions.Length];
                float offset = 500f;
                Vector2 randomOffset = Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / length) * offset,
                        spawnPosition = CorePosition + randomOffset;
                for (int i2 = 0; i2 < positions.Length; i2++) {
                    positions[i2] = spawnPosition;
                }
                _wormData[i] = new WormInfo(positions, rotations);
            }
        }

        protected override void AfterProcessingCane() {
            for (int i = 0; i < _wormData.Length; i++) {
                ref WormInfo wormInfo = ref _wormData[i];
                for (int num28 = wormInfo.Positions.Length - 1; num28 > 0; num28--) {
                    if (wormInfo.Positions[num28].Distance(wormInfo.Positions[num28 - 1]) < 20f) {
                        continue;
                    }
                    wormInfo.Positions[num28] = Vector2.Lerp(wormInfo.Positions[num28], wormInfo.Positions[num28 - 1], 0.5f);
                    wormInfo.Rotations[num28] = Utils.AngleLerp(wormInfo.Rotations[num28], wormInfo.Rotations[num28 - 1], 0.5f);
                }
                Vector2 position = CorePosition;
                wormInfo.Rotations[0] = wormInfo.Positions[0].AngleTo(position);
                float attackProgress = MathUtils.Clamp01(UseTime / 120f);
                attackProgress = 1f - attackProgress;
                attackProgress += 0.5f;
                float speed = attackProgress * 9f;
                wormInfo.Positions[0] += Vector2.UnitY.RotatedBy(TimeSystem.TimeForVisualEffects * 10f * Owner.direction) * speed;
                wormInfo.Positions[0] += wormInfo.Positions[0].DirectionTo(position) * speed;
            }
        }

        public override void PostDraw(Color lightColor) {
            Texture2D texture = _wormTexture.Value;
            SpriteBatch batch = Main.spriteBatch;
            for (int i = 0; i < _wormData.Length; i++) {
                WormInfo wormInfo = _wormData[i];

                List<Vector2> oldPositions = [..wormInfo.Positions];
                for (int k = 1; k < wormInfo.Positions.Length; k++) {
                    oldPositions[k] = oldPositions[k - 1] + oldPositions[k - 1].DirectionTo(wormInfo.Positions[k]) * 12f;
                }

                for (int k = oldPositions.Count - 1; k > 0; k--) {
                    int frame = 1;
                    if (k == 1) {
                        frame = 0;
                    }
                    if (k == oldPositions.Count - 1) {
                        frame = 3;
                    }

                    float disappearOpacity = Utils.GetLerpValue(0f, 0.5f, AfterReleaseProgress01, true);
                    float opacity = MathF.Min(Utils.GetLerpValue(0f, 1f, AttackProgress01, true), disappearOpacity);
                    Color baseColor = Color.White;
                    baseColor = baseColor.MultiplyAlpha(1f - Utils.GetLerpValue(1f, 0.25f, opacity, true));
                    float mainOpacity = Utils.GetLerpValue(0f, 0.5f, opacity, true);
                    mainOpacity *= Ease.CubeIn(disappearOpacity);
                    baseColor *= mainOpacity;
                    Color color = baseColor * Helper.Wave(0.5f, 0.75f, 5f, 0f);

                    Rectangle clip = Utils.Frame(texture, 1, 4, frameY: frame);
                    Vector2 origin = clip.Centered();
                    float rotation = wormInfo.Rotations[k] + MathHelper.PiOver2;
                    DrawInfo drawInfo = new() {
                        Clip = clip,
                        Origin = origin,
                        Rotation = rotation,
                        Color = color
                    };
                    Vector2 position = oldPositions[k];
                    if (position == Vector2.Zero) {
                        continue;
                    }
                    batch.Draw(texture, position, drawInfo);

                    float num184 = Helper.Wave(2f, 6f, 1f, 0f);
                    for (int num185 = 0; num185 < 4; num185++) {
                        batch.Draw(texture, position + Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184, drawInfo with {
                            Color = new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f * mainOpacity
                        });
                    }
                }
            }
        }

        protected override Vector2 CorePositionOffsetFactor() => new(0.15f, 0f);

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {

        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            //if (step > 0.05f) {
            //    return;
            //}
            //step = Ease.CubeIn(step);
            //float offset = 200f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
            //Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), 
            //        spawnPosition = corePosition + randomOffset;
            //bool flag = !Main.rand.NextBool(3);
            //float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 2f * Math.Max(step, 0.25f) + 0.25f;
            //StarwayWormholeAdvancedDust? starwayWormholeAdvancedDust = AdvancedDustSystem.New<StarwayWormholeAdvancedDust>(AdvancedDustLayer.ABOVEDUSTS)?.
            //    Setup(
            //            spawnPosition,
            //            (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor * 2f,
            //            scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f)
            //         );
            //if (starwayWormholeAdvancedDust is not null) {
            //    starwayWormholeAdvancedDust.Destination = corePosition;
            //}
        }
    }
}
