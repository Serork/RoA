using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Projectiles.Friendly.Miscellaneous;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Face)]
sealed class CosmicHat : ModItem {
	private sealed class CosmicHatHandler : ModPlayer {
        private float _timer, _timer2;
        private int _lastMana;

		public bool IsEffectActive;
        public bool IsEffectActive2;

        public override void Load() {
            On_Player.GetManaCost += On_Player_GetManaCost;
            On_Player.UpdateManaRegen += On_Player_UpdateManaRegen;
        }

        private void On_Player_UpdateManaRegen(On_Player.orig_UpdateManaRegen orig, Player self) {
            if (self.GetModPlayer<CosmicHatHandler>()._timer > 0f) {
                return;
            }

            orig(self);
        }

        private int On_Player_GetManaCost(On_Player.orig_GetManaCost orig, Player self, Item item) {
            if (self.GetModPlayer<CosmicHatHandler>()._timer > 0f) {
                return 0;
            }

            return orig(self, item);
        }

        public override void ResetEffects() {
			IsEffectActive = false;
        }

        public override void PostUpdateEquips() {
			if (!IsEffectActive) {
				return;
			}

			Player player = Player;
            float min = 0.4f;
            if (_timer > 0f) {
                _timer--;
                player.manaRegen = 0;
                player.manaRegenDelay = 0;
            }
            if (!IsEffectActive2 && _timer <= 0f) {
                if ((float)player.statMana < (float)player.statManaMax2 * min) {
                    if (!player.HasBuff(BuffID.ManaSickness)) {
                        IsEffectActive2 = true;
                        _timer = _timer2 = 180f;
                        _lastMana = player.statMana;

                        if (player.whoAmI == Main.myPlayer) {
                            for (int i = 0; i < 3; i++) {
                                Projectile.NewProjectile(player.GetSource_Misc("cosmichat"),
                                    Player.Center.X, Player.Center.Y, 0f, 0f, ModContent.ProjectileType<CosmicMana>(), 0, 0f, player.whoAmI, i, 0f);
                            }
                        }
                    }
                }
            }
            else if (player.statMana > (float)player.statManaMax2 * min) {
                IsEffectActive2 = false;
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
		player.GetModPlayer<CosmicHatHandler>().IsEffectActive = true;
	}
}
