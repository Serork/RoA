using Microsoft.Xna.Framework;

using RoA.Common.Sets;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class FlametrackerDebuff : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Hemorrhage");
        //Description.SetDefault("Losing life");
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;

        BuffSets.Debuffs[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<FlametrackerDebuffNPC>().flametrackerDebuff = true;
}

sealed class FlametrackerDebuffNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool flametrackerDebuff;

    public override void ResetEffects(NPC npc) => flametrackerDebuff = false;

    public override void PostAI(NPC npc) {
        if (flametrackerDebuff) {
            npc.defense = npc.defDefense / 2;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        //      if (!npc.active) {
        //          return;
        //      }
        //      if (flametrackerDebuff) {
        //	if (Main.rand.Next(4) == 0) {
        //		int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, ModContent.DustType<Hemorrhage2>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 0, default, 1.1f);
        //		Main.dust[dust].noGravity = true;
        //		Main.dust[dust].velocity *= 1.8f;
        //		Main.dust[dust].velocity.Y = 0.5f;
        //	}
        //}
    }
}