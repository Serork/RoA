using RoA.Content.Biomes.Backwoods;

using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class TownNPCHappiness : GlobalNPC {
    public override void SetStaticDefaults() {
        NPCHappiness.Get(NPCID.Dryad).SetBiomeAffection<BackwoodsBiome>(AffectionLevel.Hate);
        NPCHappiness.Get(NPCID.WitchDoctor).SetBiomeAffection<BackwoodsBiome>(AffectionLevel.Hate);
    }
}
