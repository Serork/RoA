using RoA.Common.NPCs;
using RoA.Content.Emotes;

using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class EmotePicker : ILoadable {
    public void Load(Mod mod) {
        On_EmoteBubble.ProbeBosses += On_EmoteBubble_ProbeBosses;
    }

    private void On_EmoteBubble_ProbeBosses(On_EmoteBubble.orig_ProbeBosses orig, EmoteBubble self, System.Collections.Generic.List<int> list) {
        orig(self, list);

        if (DownedBossSystem.downedLothorBoss) {
            list.Add(ModContent.EmoteBubbleType<LothorEmote>());
        }
    }

    public void Unload() { }
}
