using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class GodDescent : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.GetGlobalNPC<GodDescent_NPCHandler>().IsEffectActive = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.GetModPlayer<GodDescent_PlayerHandler>().IsEffectActive = true;
    }

    public sealed class GodDescent_ShaderApplier : IInitializer {
        void ILoadable.Load(Mod mod) {
            On_Main.DrawNPCDirect += On_Main_DrawNPCDirect;
            On_Main.DrawProjDirect += On_Main_DrawProjDirect;
            On_PlayerDrawHelper.SetShaderForData += On_PlayerDrawHelper_SetShaderForData;
        }

        private void On_PlayerDrawHelper_SetShaderForData(On_PlayerDrawHelper.orig_SetShaderForData orig, Player player, int cHead, ref DrawData cdd) {
            if (player.GetModPlayer<GodDescent_PlayerHandler>().IsEffectActive) {
                orig(player, cHead, ref cdd);

                Effect godDescentShader = ShaderLoader.GodDescent.Value;
                float width = MathF.Max(100, player.width * 2f);
                float height = MathF.Max(100, player.height * 2f);
                Vector4 sourceRectangle = new(-width / 2f, -height / 2f, width, height);
                Vector2 size = new(width, height);
                godDescentShader.Parameters["uSourceRect"].SetValue(sourceRectangle);
                godDescentShader.Parameters["uLegacyArmorSourceRect"].SetValue(sourceRectangle);
                godDescentShader.Parameters["uImageSize0"].SetValue(size);
                godDescentShader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                godDescentShader.CurrentTechnique.Passes[0].Apply();

                return;
            }

            orig(player, cHead, ref cdd);
        }

        private void On_Main_DrawProjDirect(On_Main.orig_DrawProjDirect orig, Main self, Projectile proj) {
            if (proj.GetGlobalProjectile<GodDescent_ProjectileHandler>().IsEffectActive) {
                SpriteBatch mySpriteBatch = Main.spriteBatch;
                mySpriteBatch.DrawWithSnapshot(() => {
                    Effect godDescentShader = ShaderLoader.GodDescent.Value;
                    float width = MathF.Max(100, proj.width * 2f);
                    float height = MathF.Max(100, proj.height * 2f);
                    Vector4 sourceRectangle = new(-width / 2f, -height / 2f, width, height);
                    Vector2 size = new(width, height);
                    godDescentShader.Parameters["uSourceRect"].SetValue(sourceRectangle);
                    godDescentShader.Parameters["uLegacyArmorSourceRect"].SetValue(sourceRectangle);
                    godDescentShader.Parameters["uImageSize0"].SetValue(size);
                    godDescentShader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                    godDescentShader.CurrentTechnique.Passes[0].Apply();

                    orig(self, proj);
                }, sortMode: SpriteSortMode.Immediate);
                return;
            }

            orig(self, proj);
        }

        private void On_Main_DrawNPCDirect(On_Main.orig_DrawNPCDirect orig, Main self, SpriteBatch mySpriteBatch, NPC rCurrentNPC, bool behindTiles, Vector2 screenPos) {
            if (rCurrentNPC.GetGlobalNPC<GodDescent_NPCHandler>().IsEffectActive) {
                mySpriteBatch.DrawWithSnapshot(() => {
                    Effect godDescentShader = ShaderLoader.GodDescent.Value;
                    float width = MathF.Max(100, rCurrentNPC.width * 2f);
                    float height = MathF.Max(100, rCurrentNPC.height * 2f);
                    Vector4 sourceRectangle = new(-width / 2f, -height / 2f, width, height);
                    Vector2 size = new(width, height);
                    godDescentShader.Parameters["uSourceRect"].SetValue(sourceRectangle);
                    godDescentShader.Parameters["uLegacyArmorSourceRect"].SetValue(sourceRectangle);
                    godDescentShader.Parameters["uImageSize0"].SetValue(size);
                    godDescentShader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                    godDescentShader.CurrentTechnique.Passes[0].Apply();

                    orig(self, mySpriteBatch, rCurrentNPC, behindTiles, screenPos);
                }, sortMode: SpriteSortMode.Immediate);
                return;
            }

            orig(self, mySpriteBatch, rCurrentNPC, behindTiles, screenPos);
        }
    }

    public sealed class GodDescent_PlayerHandler : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() {
            IsEffectActive = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }
    }

    public sealed class GodDescent_ProjectileHandler : GlobalProjectile {
        public ushort IsEffectActiveFor;

        public bool IsEffectActive {
            get => IsEffectActiveFor > 0;
            set => IsEffectActiveFor = GodFeather.DEBUFFTIME;
        }

        public override bool InstancePerEntity => true;

        public override void PostAI(Projectile projectile) {
            if (IsEffectActiveFor > 0) {
                IsEffectActiveFor--;
            }
        }

        public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }
    }

    public sealed class GodDescent_NPCHandler : GlobalNPC {
        public bool IsEffectActive;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc) {
            IsEffectActive = false;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }

        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) {
            if (IsEffectActive) {
                modifiers.FinalDamage /= 2;
                modifiers.Knockback /= 2;
            }
        }
    }
}
