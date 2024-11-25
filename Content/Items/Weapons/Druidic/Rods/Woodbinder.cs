using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class Woodbinder : BaseRodItem<Woodbinder.WoodbinderBase> {
    protected override void SafeSetDefaults() {
        Item.SetSize(36, 42);
        Item.SetDefaultToUsable(-1, 60, useSound: SoundID.Item80);
        Item.SetWeaponValues(10, 2f);

        //NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.45f);
        NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class WoodbinderBase : BaseRodProjectile {
        protected override Vector2 CorePositionOffsetFactor() => new(0.275f, 0f);

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            float offset = 10f;
            if (step > 0f) {
                Vector2 spawnPosition = corePosition + (Vector2.UnitY * offset).RotatedBy(step * MathHelper.TwoPi * Utils.Remap(step, 0f, 1f, 2f, 5f) * -player.direction);

                for (int i = 0; i < 3; i++) {
                    Dust dust = Dust.NewDustPerfect(spawnPosition,
                                                    Main.rand.NextBool(4) ? ModContent.DustType<Dusts.Woodbinder>() : ModContent.DustType<WoodTrash>(),
                                                    Vector2.Zero,
                                                    Scale: Main.rand.NextFloat(1.25f, 1.5f) * Utils.Remap(step, 0f, 1f, 0.5f, 1f));
                    dust.noGravity = true;
                }
            }
        }
    }
}