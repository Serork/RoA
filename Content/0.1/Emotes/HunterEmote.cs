using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace RoA.Content.Emotes;

sealed class HunterEmote : ModEmoteBubble {
    public override void SetStaticDefaults() {
        AddToCategory(EmoteID.Category.Town);
    }
}
