using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Liquids;

sealed class FishingInTar : ModPlayer {
    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
        int liquidType = Main.tile[attempt.X, attempt.Y].LiquidType;
        if (liquidType == LiquidLoader.LiquidType<Tar>()) {
            itemDrop = 0;
        }
    }
}
