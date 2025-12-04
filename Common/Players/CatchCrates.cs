using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Miscellaneous;
using RoA.Content.Items.Placeable.Miscellaneous.Hardmode;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class CatchCrates : ModPlayer {
    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
        bool inWater = !attempt.inLava && !attempt.inHoney;
        bool inBackwoods = Player.InModBiome<BackwoodsBiome>();
        if (inWater && inBackwoods && attempt.crate) {
            if (!attempt.veryrare && !attempt.legendary && attempt.rare) {
                itemDrop = Main.hardMode ? ModContent.ItemType<HardmodeBackwoodsCrate>() : ModContent.ItemType<BackwoodsCrate>();
            }
        }
    }
}
