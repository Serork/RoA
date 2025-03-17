using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class RoyalQualityHoney : NatureItem {
    private class RoyalQualityHoneyHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            if (Player.lifeRegen >= 4) {
                Player.AddBuff(BuffID.Honey, 1);
            }
        }
    }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    protected override void SafeSetDefaults() {
        int width = 22; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }

	public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<RoyalQualityHoneyHandler>().IsEffectActive = true;
}