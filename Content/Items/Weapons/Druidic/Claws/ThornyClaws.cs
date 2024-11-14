using Microsoft.Xna.Framework;

using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druid;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Utilities;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class ThornyClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(14, 4f);
    }

    protected override (Color, Color) SlashColors() => (new Color(75, 167, 85), new Color(100, 200, 110));

    //protected override bool ShouldModifyShootStats => false;

    public override void SafeOnUse(Player player, ClawsHandler clawsStats) {
        ushort type = (ushort)ModContent.ProjectileType<Snatcher>();
        bool shouldReset = true;
        foreach (Projectile projectile in  Main.ActiveProjectiles) {
            if (projectile.type == type && projectile.owner == player.whoAmI) {
                if (projectile.timeLeft < 30) {
                    shouldReset = false;
                }
                break;
            }
        }
        clawsStats.SetSpecialAttackData(new ClawsHandler.AttackSpawnInfoArgs() {
            Owner = Item,
            SpawnPosition = player.Center,
            StartVelocity = Helper.VelocityToPoint(player.Center, Main.MouseWorld, 1f).SafeNormalize(Vector2.Zero),
            ProjectileTypeToSpawn = type,
            ShouldReset = shouldReset,
            ShouldSpawn = player.ownedProjectileCounts[type] < 1
        });
    }
}
