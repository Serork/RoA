using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;

using RoA.Content.Liquids;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class TarDebuff : ModBuff {
    public override void Load() {
        On_Main.TryGetBuffTime += On_Main_TryGetBuffTime;
    }

    private bool On_Main_TryGetBuffTime(On_Main.orig_TryGetBuffTime orig, int buffSlotOnPlayer, out int buffTimeValue) {
        int num = Main.LocalPlayer.buffType[buffSlotOnPlayer];
        Player self = Main.LocalPlayer;
        if (self.GetModdedWetArray()[LiquidLoader.LiquidType<Tar>() - LiquidID.Count] && num == ModContent.BuffType<TarDebuff>()) {
            buffTimeValue = 0;
            return false;
        }
        bool result = orig(buffSlotOnPlayer, out buffTimeValue);
        return result;
    }

    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;

        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.GetModPlayer<TarDebuff_Player>().IsEffectActive = true;

        player.moveSpeed *= 0.333f;
    }

    private class TarDebuff_NPC : GlobalNPC {
        public bool IsEffectActive;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc) => IsEffectActive = false;

        public override void UpdateLifeRegen(NPC npc, ref int damage) {
            if (!IsEffectActive) {
                return;
            }

            if (npc.onFire || npc.onFire2 || npc.onFire3 || npc.onFrostBurn || npc.onFrostBurn2 || npc.shadowFlame) {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 50;
                if (damage < 10)
                    damage = 10;
            }
        }
    }

    private class TarDebuff_Player : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PreUpdateMovement() {
            //if (!IsEffectActive) {
            //    return;
            //}

            //Player.moveSpeed *= 0.333f;
        }

        public override void UpdateBadLifeRegen() {
            if (!IsEffectActive) {
                return;
            }

            if (Player.onFire || Player.onFire2 || Player.onFire3 || Player.onFrostBurn || Player.onFrostBurn2) {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegen -= 50;
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
            if (!IsEffectActive) {
                return;
            }

            Player drawPlayer = drawInfo.drawPlayer;
            if (!Player.GetModdedWetArray()[LiquidLoader.LiquidType<Tar>() - LiquidID.Count]) {
                int alpha2 = Main.rand.Next(50, 100);
                ushort tarDustType = (ushort)ModContent.DustType<Dusts.TarDebuff>();
                if (Main.rand.Next(5) <= 2 && drawInfo.shadow == 0f) {
                    Vector2 position3 = drawInfo.Position;
                    position3.X -= 2f;
                    position3.Y -= 2f;
                    if (Main.rand.Next(4) == 0) {
                        //DrawColor newColor2 = new DrawColor(88 / 255f + 0.1f * Main.rand.NextFloat(), 74 / 255f + 0.1f * Main.rand.NextFloat(), 91 / 255f + 0.1f * Main.rand.NextFloat());
                        //newColor2.A = (byte)(newColor2.A * 0.85f);
                        Dust dust11 = Dust.NewDustDirect(position3, drawPlayer.width + 4, drawPlayer.height + 2, tarDustType, 0f, 0f, alpha2, default, 1f);
                        if (Main.rand.Next(2) == 0)
                            dust11.alpha += 25;

                        if (Main.rand.Next(2) == 0)
                            dust11.alpha += 25;

                        dust11.noLight = true;
                        dust11.velocity *= 0.2f;
                        dust11.velocity += drawPlayer.velocity * 0.7f;
                        dust11.fadeIn = 0.8f;
                        drawInfo.DustCache.Add(dust11.dustIndex);
                    }

                    //if (Main.rand.Next(30) == 0) {
                    //    DrawColor color2 = Main.hslToRgb(88 / 255f, 74 / 255f, 91 / 255f);
                    //    color2.A = (byte)(color2.A * 0.75f);
                    //    Dust dust12 = Dust.NewDustDirect(position3, drawPlayer.width + 4, drawPlayer.height + 2, DustID.TintableDustLighted, 0f, 0f, 254, new DrawColor(88, 74, 91, 0), 0.45f);
                    //    dust12.noLight = true;
                    //    dust12.velocity.X *= 0f;
                    //    dust12.velocity *= 0.03f;
                    //    dust12.fadeIn = 0.6f;
                    //    //drawInfo.DustCache.Add(dust12.dustIndex);
                    //}
                }
            }

            r *= 0.8f;
            g *= 0.8f;
            b *= 0.8f;
        }
    }
}
