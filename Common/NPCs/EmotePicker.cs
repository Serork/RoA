using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Emotes;
using RoA.Content.NPCs.Friendly;

using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class EmotePicker : ILoadable {
    public void Load(Mod mod) {
        On_EmoteBubble.ProbeBosses += On_EmoteBubble_ProbeBosses;
        On_EmoteBubble.ProbeBiomes += On_EmoteBubble_ProbeBiomes;
        On_EmoteBubble.ProbeWeather += On_EmoteBubble_ProbeWeather;
        On_EmoteBubble.ProbeExceptions += On_EmoteBubble_ProbeExceptions;
    }

    private void On_EmoteBubble_ProbeExceptions(On_EmoteBubble.orig_ProbeExceptions orig, EmoteBubble self, System.Collections.Generic.List<int> list, Player plr, WorldUIAnchor other) {
        orig(self, list, plr, other);

        NPC nPC = (NPC)self.anchor.entity;
        if (/*nPC.type == NPCID.WitchDoctor ||*/ nPC.type == NPCID.Dryad) {
            list.Add(ModContent.EmoteBubbleType<BackwoodsEmote>());
        }

        int hunterType = ModContent.NPCType<Hunter>();
        bool flag = other != null && ((NPC)other.entity).type == hunterType;
        if (flag || plr.InModBiome<BackwoodsBiome>() && Main.rand.NextBool(5)) {
            list.Add(ModContent.EmoteBubbleType<HunterEmote>());
            list.Add(ModContent.EmoteBubbleType<BackwoodsEmote>());
            list.Add(ModContent.EmoteBubbleType<LothorEmote>());
        }

        if (NPC.AnyNPCs(hunterType)) {
            list.Add(ModContent.EmoteBubbleType<HunterEmote>());
            list.Add(ModContent.EmoteBubbleType<BackwoodsEmote>());
        }
    }

    private void On_EmoteBubble_ProbeWeather(On_EmoteBubble.orig_ProbeWeather orig, EmoteBubble self, System.Collections.Generic.List<int> list, Player plr) {
        orig(self, list, plr);

        if (plr.InModBiome<BackwoodsBiome>() && BackwoodsFogHandler.IsFogActive) {
            list.Add(ModContent.EmoteBubbleType<BackwoodsFogEmote>());
        }
    }

    private void On_EmoteBubble_ProbeBiomes(On_EmoteBubble.orig_ProbeBiomes orig, EmoteBubble self, System.Collections.Generic.List<int> list, Player plr) {
        if ((double)(plr.position.Y / 16f) < Main.worldSurface * 0.45)
            list.Add(22);
        else if ((double)(plr.position.Y / 16f) > Main.rockLayer + Main.maxTilesY / 2 - 100.0)
            list.Add(31);
        else if ((double)(plr.position.Y / 16f) > Main.rockLayer)
            list.Add(30);
        else if (plr.ZoneHallow)
            list.Add(27);
        else if (plr.ZoneCorrupt)
            list.Add(26);
        else if (plr.ZoneCrimson)
            list.Add(25);
        else if (plr.ZoneJungle)
            list.Add(24);
        else if (plr.ZoneSnow)
            list.Add(32);
        else if (plr.InModBiome<BackwoodsBiome>())
            list.Add(ModContent.EmoteBubbleType<BackwoodsEmote>());
        else if ((double)(plr.position.Y / 16f) < Main.worldSurface && (plr.position.X < 4000f || plr.position.X > 16 * (Main.maxTilesX - 250)))
            list.Add(29);
        else if (plr.ZoneDesert)
            list.Add(28);
        else if (plr.ZoneForest)
            list.Add(23);
    }

    private void On_EmoteBubble_ProbeBosses(On_EmoteBubble.orig_ProbeBosses orig, EmoteBubble self, System.Collections.Generic.List<int> list) {
        orig(self, list);

        if (DownedBossSystem.DownedLothorBoss) {
            list.Add(ModContent.EmoteBubbleType<LothorEmote>());
        }
    }

    public void Unload() { }
}
