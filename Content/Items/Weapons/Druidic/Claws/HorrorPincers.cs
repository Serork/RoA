using Microsoft.Xna.Framework;

using RiseofAges.Content.Projectiles.Friendly.Druid;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Utilities;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class HorrorPincers : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(8, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 24);
    }

    protected override (Color, Color) SlashColors() => (new Color(132, 75, 140), new Color(160, 100, 200));

    public override void SafeOnUse(Player player, ClawsHandler clawsStats) {
        int offset = 30 * player.direction;
        var position = new Vector2(player.Center.X + offset, player.Center.Y);
        Vector2 point = Helper.VelocityToPoint(player.Center, Main.MouseWorld, 1.2f);
        clawsStats.SetSpecialAttackData<InfectedWave>(Item, new Vector2(position.X, position.Y - 14f), point, playSoundStyle: SoundID.Item95);
    }
}
