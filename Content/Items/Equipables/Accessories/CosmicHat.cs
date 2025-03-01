using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Miscellaneous;

using System;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Face)]
sealed class CosmicHat : ModItem {
	private class CosmisHatHandler : ModPlayer {
        private float timer = 0f;

		public bool IsEffectActive;

        public override void ResetEffects() {
			IsEffectActive = false;
        }

        public override void PostUpdateEquips() {
			if (!IsEffectActive) {
				return;
			}

            if (timer > 0f) timer -= 1f;
			Player player = Player;
            if ((float)player.statMana < (float)player.statManaMax2 * 0.35f && timer <= 0.0 && player.whoAmI == Main.myPlayer) {
                float x = player.position.X + (float)Main.rand.Next(-600, 450);
                float y = player.position.Y - (float)Main.rand.Next(750, 1000);
                Vector2 vector2 = new Vector2(x, y);
                float playerX = player.position.X + (float)(player.width / 2) - vector2.X;
                float playerY = player.position.Y + (float)(player.height / 2) - vector2.Y;
                float speed = 22f / (float)Math.Sqrt((double)playerX * (double)playerX + (double)playerY * (double)playerY);
                float speedX = playerX * speed;
                float speedY = playerY * speed;
                Projectile.NewProjectile(player.GetSource_Misc("cosmichat"), x, y, speedX, speedY, ModContent.ProjectileType<CosmicMana>(), 0, 0f, player.whoAmI, 0f, 0f);
                timer = 7f;
            }
        }
    }

	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Cosmic Hat");
		//Tooltip.SetDefault("Mana stars fall, when your will is too low\n'Great for a walk in a shadowy forest'");
		ArmorIDs.Face.Sets.OverrideHelmet[Item.faceSlot] = true;
        ArmorIDs.Head.Sets.DrawFullHair[Item.faceSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 34; int height = 22;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

	public override void UpdateAccessory(Player player, bool hideVisual) {
		player.GetModPlayer<CosmisHatHandler>().IsEffectActive = true;
	}
}
