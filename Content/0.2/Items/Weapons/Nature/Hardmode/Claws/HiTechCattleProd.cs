using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[AutoloadGlowMask]
[WeaponOverlay(WeaponType.Claws)]
sealed class HiTechCattleProd : ClawsBaseItem<HiTechCattleProd.HiTechCattleProdSlash> {
    public override bool IsHardmodeClaws => true;

    public override float BrightnessModifier => 1f;
    public override bool HasLighting => true;

    public override float FirstAttackSpeedModifier => 0.5f;
    public override float SecondAttackSpeedModifier => 0.5f;
    public override float ThirdAttackSpeedModifier => 1f;

    public override float FirstAttackScaleModifier => 1.25f;
    public override float SecondAttackScaleModifier => 1.25f;
    public override float ThirdAttackScaleModifier => 1.5f;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 34);
        Item.SetWeaponValues(40, 4.2f);

        Item.rare = ItemRarityID.Pink;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override void SetSpecialAttackData(Player player, ref ClawsHandler.AttackSpawnInfoArgs args) {
        args.ShouldReset = false;
    }

    protected override (Color, Color) SetSlashColors(Player player)
        => (new Color(97, 200, 225), new Color(98, 154, 179));

    public sealed class HiTechCattleProdSlash : ClawsSlash {
        protected override void SafeOnHitPlayer(Player target, Player.HurtInfo info) {
            target.AddBuff(BuffID.Electrified, Main.rand.Next(60, 180) * 2);
        }

        protected override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Electrified, Main.rand.Next(60, 180) * 2);

            if (Projectile.localAI[2] != 0f) {
                return;
            }

            if (!target.CanActivateOnHitEffect()) {
                return;
            }

            if (!Projectile.GetOwnerAsPlayer().GetWreathHandler().IsActualFull6) {
                return;
            }

            SpawnStar(target);
            Projectile.GetOwnerAsPlayer().GetWreathHandler().HandleOnHitNPCForNatureProjectile(Projectile, true);
            Projectile.localAI[2] = 1f;
        }

        private void SpawnStar(NPC target) {
            if (!Projectile.IsOwnerLocal()) {
                return;
            }

            ProjectileUtils.SpawnPlayerOwnedProjectile<HiTechStar>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                Position = target.Center,
                Damage = Projectile.damage,
                KnockBack = Projectile.knockBack
            });
        }
    }
}
