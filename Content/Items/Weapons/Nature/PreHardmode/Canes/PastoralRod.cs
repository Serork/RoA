using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

sealed class PastoralRod : CaneBaseItem<PastoralRod.PastoralRodBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<ShepherdLeaves>();

    protected override ushort TimeToCastAttack(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2 - NatureWeaponHandler.GetUseSpeed(Item, player) / 3);

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);
        Item.SetUsableValues(-1, 27, useSound: SoundID.Item7);
        Item.SetWeaponValues(8, 1f);

        NatureWeaponHandler.SetPotentialDamage(Item, 23);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.6f);

        Item.value = Item.sellPrice(0, 0, 1, 0);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class PastoralRodBase : CaneBaseProjectile {
        private Color? _color = null;

        //protected override bool ShouldStopUpdatingRotationAndDirection() => false;

        protected override void SafestOnSpawn(IEntitySource source) {
            //DrawColor[] colors = [new DrawColor(255, 0, 0), new DrawColor(0, 255, 0), new DrawColor(255, 255, 0)];
            //_color = Main.rand.NextFromList(colors);
        }

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) {
            count = 3;
            Vector2 pointPosition = player.GetViableMousePosition();
            Vector2 direction = (pointPosition - Projectile.Center).SafeNormalize(Vector2.One);
            velocity = direction.RotatedBy(Main.rand.NextFloatRange(MathHelper.PiOver2) + MathHelper.Pi) * 4f * NatureWeaponHandler.GetUseSpeedMultiplier(player.GetSelectedItem(), player);

            _color = Color.Lerp(new Color(200, 70, 100), new Color(200, 230, 80), Main.rand.NextFloat());

            string hexString = _color.Value.Hex3();
            ai0 = Helper.FromHexString(hexString);
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            Color color = Color.Lerp(new Color(200, 0, 80), new Color(200, 255, 80), Main.rand.NextFloat());
            for (int i = 0; i < 2; i++) {
                Vector2 randomPosition = Main.rand.NextVector2Unit();
                float areaSize = step * 2f;
                float speed = step;
                float scale = Math.Clamp(step, 0.1f, 0.8f) * 1.125f;
                Dust dust = Dust.NewDustPerfect(corePosition - Vector2.One * 1f + randomPosition * Main.rand.NextFloat(areaSize, areaSize + 2f),
                                                ModContent.DustType<PastoralRodDust>(),
                                                randomPosition.RotatedBy(player.direction * -MathHelper.PiOver2) * speed,
                                                Scale: scale);
                dust.noGravity = true;
                dust.noLight = true;
                dust.velocity *= 0.4f;
                dust.color = color;
                dust.customData = player;
            }
        }
    }
}