using System.Reflection;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class DryadBlood : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dryad Blood");
        //Description.SetDefault("Damaging debuffs deal 75% less damage");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<DryadBloodPlayer>().dryadBlood = true;
}

sealed class DryadBloodPlayer : ModPlayer {
    private static object Hook_PlayerLoader_UpdateBadRegen;

    public bool dryadBlood;

    public override void ResetEffects() => dryadBlood = false;

    public override void Load() {
        Hook_PlayerLoader_UpdateBadRegen = RoA.Detour(typeof(PlayerLoader).GetMethod(nameof(PlayerLoader.UpdateBadLifeRegen), BindingFlags.Public | BindingFlags.Static),
                typeof(DryadBloodPlayer).GetMethod(nameof(PlayerLoader_UpdateBadLifeRegen), BindingFlags.NonPublic | BindingFlags.Static));
    }

    public override void Unload() {
        Hook_PlayerLoader_UpdateBadRegen = null;
    }

    private delegate void PlayerLoader_CatchFish_orig(Player player);
    private static void PlayerLoader_UpdateBadLifeRegen(PlayerLoader_CatchFish_orig self, Player player) {
        self(player);

        if (player.GetModPlayer<DryadBloodPlayer>().dryadBlood) {
            player.lifeRegen += (int)(-player.lifeRegen * 0.75f);
        }
    }
}