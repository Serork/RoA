using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class RoyalQualityHoney : ModItem {
    public sealed class RoyalQualityHoneyHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            if (!Player.IsAlive()) {
                return;
            }

            if (Player.HasBuff(BuffID.Honey)) {
                return;
            }

            if (Player.lifeRegen >= 4) {
                Player.AddBuff(ModContent.BuffType<Honey>(), 1);
                Player.honey = true;
            }
        }
    }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 22; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 70, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<RoyalQualityHoneyHandler>().IsEffectActive = true;
}