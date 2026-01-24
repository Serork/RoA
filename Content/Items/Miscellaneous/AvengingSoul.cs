using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

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

        return false;
    }
}
