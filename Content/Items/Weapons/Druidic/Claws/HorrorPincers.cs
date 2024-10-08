﻿using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druid;
using RoA.Core;
using RoA.Core.Utility;
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
        Vector2 pointPosition = player.GetViableMousePosition();
        clawsStats.SetSpecialAttackData<InfectedWave>(Item, new(player.Center.X + 30f * player.direction, player.Center.Y - 14f), Helper.VelocityToPoint(player.Center, pointPosition, 1.2f), playSoundStyle: SoundID.Item95);
    }
}
