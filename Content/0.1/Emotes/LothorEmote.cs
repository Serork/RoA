using RoA.Common.NPCs;

using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace RoA.Content.Emotes;

sealed class LothorEmote : ModEmoteBubble {
    public override void SetStaticDefaults() {
        AddToCategory(EmoteID.Category.Dangers);
    }

    public override bool IsUnlocked() {
        // This emote only shows when minion boss is downed, just as vanilla do.
        return DownedBossSystem.DownedLothorBoss;
    }
}