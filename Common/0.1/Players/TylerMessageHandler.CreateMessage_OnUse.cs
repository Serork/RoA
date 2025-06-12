using Microsoft.Xna.Framework;

using RoA.Content.Items.Consumables;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class TylerMessageHandler : ModPlayer {
    sealed class TylerItem : GlobalItem {
        public override bool InstancePerEntity => true;

        private Vector2 _messageVelocity = new(0f, -8f);

        public override bool? UseItem(Item item, Player player) {
            if (Main.rand.NextBool(5)) {
                TylerMessageHandler handler = player.GetModPlayer<TylerMessageHandler>();
                if (item.type == ModContent.ItemType<SlipperyDynamite>() || item.type == ItemID.Dynamite || item.type == ItemID.BouncyDynamite || item.type == ItemID.StickyDynamite ||
                    item.type == ModContent.ItemType<SlipperyBomb>() || item.type == ItemID.Bomb || item.type == ItemID.BouncyBomb || item.type == ItemID.StickyBomb) {
                    handler.Create(MessageSource.Explosive, player.Top, _messageVelocity);
                }
                if (item.type == ItemID.CellPhone) {
                    handler.Create(MessageSource.OnCall, player.Top, _messageVelocity);
                }
                if (item.type == ItemID.RedPotion) {
                    handler.Create(MessageSource.RedPotion, player.Top, _messageVelocity);
                }
            }
            return null;
        }
    }
}