using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

public class Brightstone : ModBuff {
	public override void SetStaticDefaults() {
		// DisplayName.SetDefault("Brightstone Lights");
		// Description.SetDefault("You emit glowing lumps of light");
	}

	public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BrightstonePlayer>().brightstoneEffect = true;
}

sealed class BrightstonePlayer : ModPlayer {
    private Vector2[] _oldPositions = new Vector2[5];
    
	public bool brightstoneEffect;

    public override void Load() {
        ResetPositions();
    }

    private void ResetPositions() {
        for (int j = 0; j < _oldPositions.Length; j++) {
            _oldPositions[j] = Vector2.Zero;
        }
    }

    public override void ResetEffects()
		=> brightstoneEffect = false;

	public override void UpdateEquips() {
		if (!brightstoneEffect)
			return;

        for (int num2 = _oldPositions.Length - 1; num2 > 0; num2--) {
            _oldPositions[num2] = _oldPositions[num2 - 1];
        }
        _oldPositions[0] = Player.Center;
        //Lighting.AddLight(Player.Center, new Color(238, 225, 111).ToVector3());
        Lighting.AddLight(Player.Center, new Color(238, 225, 111).ToVector3() * 0.6f);
        if (Player.velocity.Length() >= 1f && (Player.controlLeft || Player.controlRight || Player.controlJump || Player.velocity.Y >= 1f) && !Player.rocketFrame) {
            Vector2 pos = _oldPositions[1] + Player.velocity;
            if (Vector2.Distance(_oldPositions[1], Player.Center) > Math.Sqrt(Player.width * Player.height) * 0.05f && Player.miscCounter % 2 == 0)
				Projectile.NewProjectile(Player.GetSource_Misc("Brightstone"), pos.X, pos.Y, 0, 0, ModContent.ProjectileType<Projectiles.Friendly.Brightstone>(), 0, 0, Player.whoAmI);
		}
	}
}