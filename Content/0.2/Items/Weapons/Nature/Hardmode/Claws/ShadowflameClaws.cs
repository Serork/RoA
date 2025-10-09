using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class ShadowflameClaws : ClawsBaseItem<ShadowflameClaws.ShadowflameClawsSlash> {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 0f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 0.85f;
    public override float SecondAttackSpeedModifier => 0.925f;
    public override float ThirdAttackSpeedModifier => 1f;

    public override float FirstAttackScaleModifier => 1f;
    public override float SecondAttackScaleModifier => 1.25f;
    public override float ThirdAttackScaleModifier => 1.5f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 36);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Pink;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override (Color, Color) SlashColors(Player player) => (Color.Lerp(new Color(144, 85, 240), new Color(63, 63, 163).ModifyRGB(1f), 0.75f), Color.Lerp(new Color(90, 30, 200), new Color(63, 63, 163).ModifyRGB(0.75f), 0.75f));

    public sealed class ShadowflameClawsSlash : ClawsSlash {
        protected override bool OnSlashDustSpawn(float progress) {
            float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
            float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
            Player player = Main.player[Projectile.owner];
            float offset = player.gravDir == 1 ? 0f : (-MathHelper.PiOver4 * progress);
            float f = Projectile.rotation + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
            Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
            Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
            for (int num807 = 0; (float)num807 < Projectile.scale * 10f * 5; num807++) {
                int type = 27;
                Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2 * player.gravDir), 100, default, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                dust.scale *= 0.35f;
                dust.scale *= Projectile.scale;
                dust.noGravity = true;
                Dust dust2 = dust;
                dust2 = dust;
                dust2.velocity -= Projectile.velocity * (1.3f - Projectile.scale);
                dust.fadeIn = 100 + Projectile.owner;
                dust2 = dust;
                dust2.scale += Projectile.scale * 0.75f;
            }

            return false;
        }
    }
}
