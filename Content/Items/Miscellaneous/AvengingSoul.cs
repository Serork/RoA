using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class AvengingSoul : MagicHerb1 {
    public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 200);

    public override void SetDefaults() {
        int width = 14,
            height = width;
        Item.Size = new Vector2(width, height);
    }

    protected override bool DisappearOverTime() => false;

    public override void PostUpdate() {
        float num9 = (float)Main.rand.Next(90, 111) * 0.01f;
        num9 *= Main.essScale * 0.5f;
        Lighting.AddLight((int)((Item.position.X + (float)(Item.width / 2)) / 16f), (int)((Item.position.Y + (float)(Item.height / 2)) / 16f), 0.5f * num9, 0.1f * num9, 0.1f * num9);
    }

    public override bool OnPickup(Player player) {
        SoundEngine.PlaySound(SoundID.Grab, player.Center);
        //player.Heal(20);

        int num = 20;
        num = Main.DamageVar(num, player.luck);
        int direction = 0;
        PlayerDeathReason playerDeathReason = PlayerDeathReason.ByCustomReason(Language.GetOrRegister($"Mods.RoA.DeathReasons.AvengingSoul{Main.rand.Next(2)}").ToNetworkText(player.name));
        int result = (int)player.Hurt(playerDeathReason, num, direction, armorPenetration: 9999);

        for (float num17 = 0f; num17 < 1f; num17 += 0.05f) {
            if (Main.rand.NextBool()) {
                continue;
            }

            Vector2 vector10 = Vector2.UnitX.RotatedBy((float)Math.PI * 2f * num17);
            Vector2 center = player.GetPlayerCorePoint();
            float num18 = Main.rand.NextFloat(0.5f, 3.5f) * Main.rand.NextFloat(1f, 2f);
            int size = 4;
            Vector2 dustPosition = center + Main.rand.NextVector2CircularEdge(size, size);
            Vector2 dustVelocity = vector10 * num18 * 10f;
            dustPosition += dustVelocity;
            dustVelocity = dustPosition.DirectionTo(center) * 4f;
            Dust dust = Dust.NewDustPerfect(dustPosition, 43, dustVelocity, 254, Color.White, 0.5f);
            dust.noGravity = true;
            dust.fadeIn = Main.rand.NextFloat() * 1.2f;
            dust.velocity *= Main.rand.NextFloat(0.25f, 0.75f);
        }

        return false;
    }
}
