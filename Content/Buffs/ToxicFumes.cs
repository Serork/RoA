using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class ToxicFumesNoTimeDisplay : ToxicFumes {
    public override string Texture => ResourceManager.BuffTextures + nameof(ToxicFumes);

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Main.buffNoTimeDisplay[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}

class ToxicFumes : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Toxic Fumes");
        // Description.SetDefault("Loosing Life, movement speed decreased");

        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.velocity.X *= 0.95f;
        player.GetModPlayer<ToxicFumesPlayer>().toxicFumes = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.GetGlobalNPC<ToxicFumesNPC>().toxicFumes = true;
        if (npc.boss) return;
        npc.velocity.X *= 0.95f;

    }
}

sealed class ToxicFumesPlayer : ModPlayer {
    public bool toxicFumes;

    public override void ResetEffects()
        => toxicFumes = false;

    public override void UpdateBadLifeRegen() {
        if (toxicFumes) {
            if (Player.lifeRegen > 0) Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
            Player.lifeRegen -= 6;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
            return;
        }
        if (toxicFumes) {
            if (Main.rand.Next(4) == 0) {
                int dust = Dust.NewDust(drawInfo.Position - new Vector2(0f, -2f) - Player.Size / 2f, Player.width, Player.height, ModContent.DustType<Dusts.ToxicFumes>(), Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 150, new Color(), 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.6f;
                Main.dust[dust].velocity.Y = 1.1f;
            }
            r *= 0.6f;
            g *= 0.9f;
            b *= 0.6f;
        }
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) {
        if (toxicFumes && (int)damage == 10) {
            damageSource = PlayerDeathReason.ByCustomReason(Language.GetOrRegister($"Mods.RoA.DeathReasons.ToxicFumesDeathMessage{Main.rand.Next(4)}").ToNetworkText(Player.name));
            //damageSource = PlayerDeathReason.ByCustomReason(Player.name + Language.GetOrRegister($"Mods.RoA.DeathReasons.ToxicFumesDeathMessage{Main.rand.Next(4)}").Value);
        }

        return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
    }
}

sealed class ToxicFumesNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool toxicFumes;

    public override void ResetEffects(NPC npc) => toxicFumes = false;


    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (toxicFumes) {
            if (npc.lifeRegen > 0) npc.lifeRegen = 0;
            npc.lifeRegen -= 9;
            if (damage < 3) damage = 3;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (toxicFumes && npc.active) {
            drawColor = Color.Lerp(drawColor, new Color(130, 230, 130).MultiplyRGB(drawColor), 0.25f);
            if (Main.rand.Next(4) == 0) {
                int dust = Dust.NewDust(npc.position - new Vector2(0f, -2f) - npc.Size / 2f, npc.width, npc.height, ModContent.DustType<Dusts.ToxicFumes>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.6f;
                Main.dust[dust].velocity.Y = 1.1f;
            }
            //Lighting.AddLight(npc.position, 0.6f, 0.9f, 0.6f);
        }
    }
}