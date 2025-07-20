using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class ExtraDruidQuote : GlobalNPC {
    private byte _currentQuoteIndex;

    private const byte MAXAWAKEQUOTES = 5;

    public override bool InstancePerEntity => true;

    public override void GetChat(NPC npc, ref string chat) {
        if (npc.type != NPCID.Dryad) {
            return;
        }
        if (!DryadAwakeHandler.DryadAwake) {
            bool hasHome = npc.homeTileX != -1 && npc.homeTileY != -1 && !npc.homeless;
            if (!hasHome) {
                string getMessage() {
                    ref byte index = ref npc.GetGlobalNPC<ExtraDruidQuote>()._currentQuoteIndex;
                    string result = Language.GetTextValue($"Mods.RoA.NPCs.Town.Dryad.AwakeQuote{index + 1}");
                    if (++index > MAXAWAKEQUOTES - 1) {
                        index = 0;
                    }
                    return result;
                }

                chat = getMessage();
            }
            return;
        }

        chat = Language.GetTextValue($"Mods.RoA.NPCs.Town.Dryad.IntroQuote");
        DryadAwakeHandler.DryadAwake = false;
        DryadAwakeHandler.DryadAwake2 = true;
    }
}