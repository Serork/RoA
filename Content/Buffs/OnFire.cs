using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class OnFire : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;

        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        if (npc.HasBuff(BuffID.OnFire)) {
            npc.ClearBuff(BuffID.OnFire);
        }
        npc.GetGlobalNPC<OnFireNPC>().onFire = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if (player.HasBuff(BuffID.OnFire)) {
            player.ClearBuff(BuffID.OnFire);
        }
        player.onFire = true;
    }
}

sealed class OnFireNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool onFire;

    public override void ResetEffects(NPC npc) => onFire = false;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (onFire) {
            bool flag = false;
            if (npc.HasBuff(ModContent.BuffType<TarDebuff>())) {
                flag = true;
            }
            if (npc.oiled || flag) {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 50;
                if (damage < 10)
                    damage = 10;
                return;
            }

            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= 8;
        }
    }
}
