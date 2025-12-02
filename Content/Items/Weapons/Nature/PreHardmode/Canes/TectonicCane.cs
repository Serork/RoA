using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Common.Projectiles;
using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Content.AdvancedDusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

[AutoloadGlowMask(requirement: "")]
sealed class TectonicCane : CaneBaseItem<TectonicCane.TectonicCaneBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<TectonicCaneProjectile>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(36, 38);
        Item.SetUsableValues(-1, 40, useSound: SoundID.Item7);
        Item.SetWeaponValues(12, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 30);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.15f);

        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(0, 0, 65, 0);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class TectonicCaneBase : CaneBaseProjectile {
        private Vector2 _tempMousePosition;

        protected override ushort TimeAfterShootToExist(Player player) => (byte)(NatureWeaponHandler.GetUseSpeed(AttachedNatureWeapon, player) * 2);

        protected override void SetAttackSound(ref SoundStyle attackSound) => attackSound = SoundID.Item69 with { Pitch = 0.25f, Volume = 0.625f };

        protected override bool ShouldWaitUntilProjDespawn() => false;

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            int count = 10;
            for (int i = 0; i < count; i++) {
                float progress = (float)i / count;
                AdvancedDustSystem.New<TectonicDebris>(AdvancedDustLayer.BEHINDPROJS)?.
                    Setup(
                    corePosition - Vector2.One * 5f + Main.rand.RandomPointInArea(10f, 10f),
                    Vector2.One.RotatedByRandom(MathHelper.TwoPi * progress).SafeNormalize(Vector2.One) * Main.rand.NextFloat(2f, 5f),
                    scale: Main.rand.NextFloat(0.9f, 1.1f) * 1.5f);
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            step = Ease.CubeIn(step);
            float offset = 40f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), spawnPosition = corePosition + randomOffset;
            bool flag = !Main.rand.NextBool(3);
            int dustType = flag ? ModContent.DustType<TectonicDust>() : DustID.Torch;
            float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 2f * Math.Max(step, 0.25f) + 0.25f;
            Dust dust = Dust.NewDustPerfect(spawnPosition, dustType,
                Scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f));
            dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
            dust.velocity *= 0.9f;
            dust.noGravity = true;

            if (player.whoAmI == Main.myPlayer) {
                EvilBranch.GetPos(player, out Point point, out Point point2, maxDistance: 800f);
                _tempMousePosition = point2.ToWorldCoordinates();
                Projectile.netUpdate = true;
            }
            Vector2 position;
            position = _tempMousePosition;
            dustType = Main.rand.NextBool(6) ? DustID.Torch : TileHelper.GetKillTileDust((int)position.X / 16, (int)position.Y / 16, Main.tile[(int)position.X / 16, (int)position.Y / 16]);
            float progress = 1.25f * Ease.ExpoInOut(Math.Max(step, 0.25f)) + 0.25f;
            int count = (int)(4 * Math.Max(0.25f, progress));
            for (int k = 0; k < count; k++) {
                Dust.NewDust(position - new Vector2(32f, 0f), 60, 2, dustType, 0, Main.rand.NextFloat(-(2f + 1.5f * Main.rand.NextFloat() * progress), -1f) * progress, count < 2 ? 0 : Main.rand.Next(255), default,
                    Main.rand.NextFloat(0.4f, 1.5f) * MathHelper.Clamp(progress, 0.7f, 0.925f));
            }
        }

        protected override void SafeSendExtraAI(BinaryWriter writer) {
            base.SafeSendExtraAI(writer);

            writer.WriteVector2(_tempMousePosition);
        }

        protected override void SafeReceiveExtraAI(BinaryReader reader) {
            base.SafeReceiveExtraAI(reader);

            _tempMousePosition = reader.ReadVector2();
        }
    }
}