using RoA.Common.Players;
using RoA.Content.Biomes.Backwoods;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous.QuestFish;

sealed class SwimmingStone : ModItem {
    public override void SetDefaults() {
        Item.DefaultToQuestFish();

        Item.width = 32;
        Item.height = 22;
    }

    public override bool IsQuestFish() => true;

    public override bool IsAnglerQuestAvailable() => Main.hardMode; 

    public override void AnglerQuestChat(ref string description, ref string catchLocation) {
        description = Language.GetTextValue($"Mods.RoA.AnglerQuests.{nameof(SwimmingStone)}.Description");
        catchLocation = Language.GetTextValue($"Mods.RoA.AnglerQuests.{nameof(SwimmingStone)}.CatchLocation");
    }
}
