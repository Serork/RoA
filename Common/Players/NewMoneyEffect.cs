using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public int NewMoneyEffectByPlayerWhoAmI;

    public override void PostUpdateBuffs() {
        if (Player.FindBuff(ModContent.BuffType<NewMoneyDebuff>(), out int buffIndex) && Player.buffTime[buffIndex] >= NewMoney.DEBUFFTIMENEEDFORBUFRST) {
            Player.DelBuff<NewMoneyDebuff>();

            int damage = NewMoney.BURSTDAMAGE;
            float knockBack = NewMoney.BURSTKNOCKBACK;
            Player player = Main.player[NewMoneyEffectByPlayerWhoAmI];
            if (player.IsLocal()) {
                Projectile.NewProjectile(Player.GetSource_Misc("newmoneyburst"), Player.Center, Vector2.Zero, ModContent.ProjectileType<NewMoneyBite>(),
                    damage,
                    knockBack,
                    NewMoneyEffectByPlayerWhoAmI,
                    Player.whoAmI,
                    ai2: 1f);
            }
        }
    }
}
