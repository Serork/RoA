using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Defaults;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Tools;

// also see ItemGlowMaskHandler
sealed class RodOfTheLustrous : ModItem {
    public override Color? GetAlpha(Color lightColor) {
        return Color.Lerp(lightColor, Color.White, 1f);
    }

    public override void SetDefaults() {
        Item.SetSizeValues(34, 44);

        Item.damage = 8;
        Item.DamageType = DamageClass.Melee;

        Item.useAnimation = 10;
        Item.useTime = 3;

        Item.useStyle = ItemUseStyleID.Swing;

        Item.useTurn = true;

        Item.autoReuse = true;

        Item.knockBack = 5f;
        Item.pick = 35;
        Item.axe = 7;

        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;

        Item.value = Item.sellPrice();
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.Next(3) == 0) {
            float num8 = player.itemRotation + Main.rand.NextFloatDirection() * ((float)Math.PI / 2f) * 0.7f;
            Vector2 vector3 = num8.ToRotationVector2();
            int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<TintableDustGlow>(), vector3.X, vector3.Y, 200, Color.Lerp(Color.Gold, Color.White, Main.rand.NextFloat() * 0.3f), 1.5f + Main.rand.NextFloat() * 0.2f);
            Main.dust[dust].fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLightEmittence = true;
            if (Main.dust[dust].position.Distance(player.Center) < player.width * 1.5f) {
                Main.dust[dust].active = false;
            }
        }
    }
}
