using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed partial class NPCCommon : GlobalNPC {
    public int NewMoneyEffectByPlayerWhoAmI;

    public partial void NewMoneyPostAI(NPC npc) {
        if (npc.FindBuff(ModContent.BuffType<NewMoneyDebuff>(), out int buffIndex) && npc.buffTime[buffIndex] >= NewMoney.DEBUFFTIMENEEDFORBUFRST) {
            npc.DelBuff<NewMoneyDebuff>();

            int damage = NewMoney.BURSTDAMAGE;
            float knockBack = NewMoney.BURSTKNOCKBACK;
            Player player = Main.player[NewMoneyEffectByPlayerWhoAmI];
            if (player.IsLocal()) {
                Projectile.NewProjectile(npc.GetSource_Misc("newmoneyburst"), npc.Center, Vector2.Zero, ModContent.ProjectileType<NewMoneyBite>(), 
                    damage,
                    knockBack,
                    NewMoneyEffectByPlayerWhoAmI,
                    npc.whoAmI);
            }
        }
    }
}
