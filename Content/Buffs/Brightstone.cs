using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

public class Brightstone : ModBuff {
	public override void SetStaticDefaults () {
		// DisplayName.SetDefault("Brightstone Lights");
		// Description.SetDefault("You emit glowing lumps of light");
	}

	public override void Update (Player player, ref int buffIndex) => player.GetModPlayer<BrightstonePlayer>().brightstoneEffect = true;
}

sealed class BrightstonePlayer : ModPlayer {
	public bool brightstoneEffect;

	public override void ResetEffects()
		=> brightstoneEffect = false;

	public override void UpdateEquips() {
		if (!brightstoneEffect)
			return;

        //Lighting.AddLight(Player.Center, new Color(238, 225, 111).ToVector3());
        Lighting.AddLight(Player.Center, new Color(238, 225, 111).ToVector3() * 0.6f);
        if (Player.velocity.Length() > 1f && (Player.controlLeft || Player.controlRight || Player.controlJump || Player.velocity.Y > 1f) && !Player.rocketFrame) {
            if (Main.rand.NextBool(6))
				Projectile.NewProjectile(Player.GetSource_Misc("Brightstone"), Player.Center.X - 2f * Player.direction, Player.Center.Y, 0, 0, ModContent.ProjectileType<Projectiles.Friendly.Brightstone>(), 0, 0, Player.whoAmI);
		}
	}
}