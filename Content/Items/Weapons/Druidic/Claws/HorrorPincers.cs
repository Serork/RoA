using Microsoft.Xna.Framework;

using RiseofAges.Content.Projectiles.Friendly.Druid;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Utilities;

using Terraria;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class HorrorPincers : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        NatureWeaponStats.SetPotentialDamage(Item, 24);

        Item.SetWeaponValues(8, 4f);
    }

    protected override (Color, Color) SlashColors() => (new Color(132, 75, 140), new Color(160, 100, 200));

    public override void SafeOnUse(Player player, ClawsStats clawsStats) {
        int offset = 30 * player.direction;
        var position = new Vector2(player.Center.X + offset, player.Center.Y);
        Vector2 point = Helper.VelocityToPoint(player.Center, Main.MouseWorld, 1.2f);
        clawsStats.SetSpecialAttackData<InfectedWave>(Item, new Vector2(position.X, position.Y - 14f), point);
    }
}
