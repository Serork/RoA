using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous.QuestFish;

sealed class Druidfish : ModItem {
    public override void SetDefaults() {
        Item.DefaultToQuestFish();

        Item.width = 30;
        Item.height = 38;
    }

    public override bool IsQuestFish() => true;

    public override bool IsAnglerQuestAvailable() => true;

    public override void AnglerQuestChat(ref string description, ref string catchLocation) {
        description = Language.GetTextValue($"Mods.RoA.AnglerQuests.{nameof(Druidfish)}.Description");
        catchLocation = Language.GetTextValue($"Mods.RoA.AnglerQuests.{nameof(Druidfish)}.CatchLocation");
    }
}
