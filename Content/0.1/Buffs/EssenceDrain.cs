using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class EssenceDrain : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Essence Drain");
        // Description.SetDefault("Fast losing life");

        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
        => player.GetModPlayer<EssenceDrainPlayer>().essenceDrain = true;

    public override void Update(NPC npc, ref int buffIndex)
        => npc.GetGlobalNPC<EssenceDrainNPC>().essenceDrain = true;
}

internal class EssenceDrainPlayer : ModPlayer {
    public bool essenceDrain;
    public override void ResetEffects()
        => essenceDrain = false;


    public override void UpdateBadLifeRegen() {
        if (essenceDrain) {
            if (Player.lifeRegen > 0) Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
            Player.lifeRegen -= 40;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (essenceDrain) {
            if (Main.rand.NextBool(4) && drawInfo.shadow == 0.0) {
                int dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, DustID.Shadowflame, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, Color.DarkViolet, 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.8f;
                Main.dust[dust].velocity.Y -= 0.5f;
            }
            Lighting.AddLight(Player.Center, 0.5f, 0f, 0.5f);
        }
    }
}

internal class EssenceDrainNPC : GlobalNPC {
    private static Asset<Texture2D> _baneRuneTexture = null!;

    public float fadeMult = 0.1f;
    public Color color;

    public bool essenceDrain;
    public int Source;

    private float _essenceDrainTimer = 0;

    public override bool InstancePerEntity => true;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _baneRuneTexture = ModContent.Request<Texture2D>(ResourceManager.MagicProjectileTextures + "BaneRune");
    }

    public override void ResetEffects(NPC npc)
        => essenceDrain = false;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        int bonusDamage = 4;
        if (essenceDrain) {
            if (npc.lifeRegen > 0) npc.lifeRegen = 0;
            npc.lifeRegen -= 40;
            if (damage < bonusDamage) damage = bonusDamage;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (essenceDrain) {
            Vector2 vector = Vector2.UnitX * 0f;
            vector += -Vector2.UnitY.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi), default) * new Vector2(npc.width, npc.height) * 0.05f;
            vector = vector.RotatedBy(npc.velocity.ToRotation(), default);
            if (Main.rand.NextBool(3)) {
                int dust = Dust.NewDust(npc.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 0, color, 1.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].position = npc.Center + vector;
                Main.dust[dust].velocity = vector;
            }
            if (_essenceDrainTimer % 10 == 0) {
                float dustCountMax = 6;
                int dustCount = 0;
                while (dustCount < dustCountMax) {
                    if (Main.rand.NextBool(1)) {
                        vector = Vector2.UnitX * 0f;
                        vector += -Vector2.UnitY.RotatedBy(dustCount * (7f / dustCountMax), default) * new Vector2(npc.width, npc.height) * 0.5f;
                        vector = vector.RotatedBy(npc.velocity.ToRotation(), default);
                        int dust2 = Dust.NewDust(npc.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 0, color, 1.4f);
                        Main.dust[dust2].noGravity = true;
                        Main.dust[dust2].position = npc.Center + vector;
                        Main.dust[dust2].velocity = npc.velocity * 0f + vector.SafeNormalize(Vector2.UnitY) * 0.8f * Main.rand.NextFloat(-1f, 1f);
                        Main.dust[dust2].velocity.Y -= 1f;
                    }
                    int dustCountMax2 = dustCount;
                    dustCount = dustCountMax2 + 1;
                }
            }
            _essenceDrainTimer++;
            Lighting.AddLight(npc.position, 0.5f, 0f, 0.5f);
        }
    }
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (essenceDrain) {
            if (fadeMult < 0.8f) fadeMult += 0.1f;
            Vector2 offset = new Vector2(-MathHelper.Lerp(-3.5f, 5f, (float)Math.Sin(_essenceDrainTimer * 0.05f)));
            Texture2D texture = _baneRuneTexture.Value;
            color = drawColor.MultiplyRGB(Color.DarkViolet) * fadeMult;
            color = new Color(color.R, color.G, color.B * MathHelper.Lerp(1f, 2f, (float)Math.Sin(_essenceDrainTimer * 0.25f)), MathHelper.Lerp(0.75f, 1f, (float)Math.Sin(_essenceDrainTimer * 0.25f)));
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            spriteBatch.Draw(texture, npc.Top + Vector2.UnitY * offset.Y * 0.5f - Vector2.UnitY * texture.Height * 0.6f - Main.screenPosition, new Rectangle?(),
                color, 0f, origin, fadeMult / 0.8f, SpriteEffects.None, 0);
        }
        else fadeMult = 0f;
        return true;
    }
}