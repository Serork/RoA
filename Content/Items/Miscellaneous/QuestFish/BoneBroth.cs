using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous.QuestFish;

sealed class BoneBroth : ModItem {
    public override void SetDefaults() {
        Item.DefaultToQuestFish();

        Item.width = 38;
        Item.height = 36;
    }

    public override bool IsQuestFish() => true;

    public override bool IsAnglerQuestAvailable() => true;

    public override void AnglerQuestChat(ref string description, ref string catchLocation) {
        description = Language.GetTextValue($"Mods.RoA.AnglerQuests.{nameof(BoneBroth)}.Description");
        catchLocation = Language.GetTextValue($"Mods.RoA.AnglerQuests.{nameof(BoneBroth)}.CatchLocation");
    }
}
