using RoA.Common;
using RoA.Content.Items.Weapons.Summon.Hardmode;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

class EnduranceCloud1 : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.endurance += 0.08f;

        AerialConcussion.SpawnCloud(player, 1);
    }
}

class EnduranceCloud2 : EnduranceCloud1 {
    public override void Update(Player player, ref int buffIndex) {
        player.endurance += 0.16f;

        AerialConcussion.SpawnCloud(player, 2);
    }
}

class EnduranceCloud3 : EnduranceCloud1 {
    public override void Update(Player player, ref int buffIndex) {
        player.endurance += 0.24f;

        AerialConcussion.SpawnCloud(player, 3);
    }
}
