using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class OnFire : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<OnFireNPC>().onFire = true;
}

sealed class OnFireNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool onFire;

    public override void ResetEffects(NPC npc) => onFire = false;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (onFire) {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= 8;
        }
    }
}
