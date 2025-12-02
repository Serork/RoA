using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Weak : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Weak");
        //Description.SetDefault("Physical abilities are decreased");
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.GetGlobalNPC<WeakNPC>().weakness = true;
        npc.damage = (int)(npc.defDamage * 0.8f);
    }
}

sealed class WeakPlayer : ModPlayer {
    public bool weakness;

    public override void ResetEffects() => weakness = false;

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (weakness && Main.rand.NextBool(1, 4))
            for (int k = 0; k < 3; k++)
                if (Main.rand.NextBool(1, 4)) {
                    var _dust = Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<WeakDust>(), Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1.4f);
                    Main.dust[_dust].noGravity = true;
                    Main.dust[_dust].velocity.Y += 5f;
                    Main.dust[_dust].velocity.Y *= 0.5f;
                }
    }
}

sealed class WeakNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool weakness;

    public override void ResetEffects(NPC npc) => weakness = false;

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (weakness && Main.rand.NextBool(1, 4))
            for (int k = 0; k < 3; k++)
                if (Main.rand.NextBool(1, 4)) {
                    var _dust = Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<WeakDust>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.4f);
                    Main.dust[_dust].noGravity = true;
                    Main.dust[_dust].velocity.Y += 5f;
                    Main.dust[_dust].velocity.Y *= 0.5f;
                }
    }
}