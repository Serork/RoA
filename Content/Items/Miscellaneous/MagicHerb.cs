using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

class MagicHerb3 : MagicHerb1 { }
class MagicHerb2 : MagicHerb1 { }
class MagicHerb1 : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Magic Herb");
		ItemID.Sets.ItemIconPulse[Item.type] = true;
	}

	public override Color? GetAlpha(Color lightColor) => Color.White;

	public override void SetDefaults() {
		int width = 20; int height = width;
		Item.Size = new Vector2(width, height);
	}

	public override bool ItemSpace(Player player) => true;

	public override bool CanPickup(Player player) => true;

	public override void PostUpdate() {
		_ = Main.rand.Next(90, 111) * 0.01f * (Main.essScale * 0.5f);
		Lighting.AddLight(Item.Center, Color.LightGreen.ToVector3() * 0.5f * Main.essScale);
	}

	public override bool OnPickup(Player player) {
		player.statLife += 40;
		if (Main.myPlayer == player.whoAmI) {
			player.HealEffect(40);
		}
		SoundEngine.PlaySound(SoundID.Item60, player.position);
		return false;
	}
}
