using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using System;

using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class BloodbathLocket : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bloodbath Locket");
        // Tooltip.SetDefault("Increases damage by 1% for each enemy and 5% for each boss nearby, up to 25%");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = width;
        Item.Size = new Vector2(width, height);

        Item.value = Item.sellPrice(gold: 2);
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
        Item.expert = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<BloodbathLocketPlayer>().bloodbathLocket = true;
        if (!hideVisual) player.GetModPlayer<BloodbathLocketPlayer>().theWildEye = true;
    }

    private sealed class BloodbathLocketPlayer : ModPlayer {
        public bool bloodbathLocket, theWildEye;
        private float bloodbathDamage;
        private const float bloodbathDamageMax = 0.25f;

        public override void ResetEffects() {
            bloodbathLocket = false;
            theWildEye = false;
            bloodbathDamage = 0;
        }

        public override void UpdateEquips() {
            float locketRadius = 1000;
            if (bloodbathLocket) {
                for (int i = 0; i < Main.npc.Length; i++) {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && !npc.friendly && !npc.CountsAsACritter && Vector2.Distance(Player.Center, npc.Center) < locketRadius) {
                        if (npc.boss) bloodbathDamage += 0.05f;
                        else bloodbathDamage += 0.01f;
                        if (theWildEye) TheWildEye();
                    }
                }
                if (bloodbathDamage > bloodbathDamageMax) bloodbathDamage = bloodbathDamageMax;
                Player.GetDamage(DamageClass.Generic) += bloodbathDamage;
            }
        }

        public void TheWildEye() {
            //int index = 0 + Player.bodyFrame.Y / 56;
            //if (index >= Main.OffsetsPlayerHeadgear.Length) index = 0;

            //Vector2 vector2_1 = Vector2.Zero + Player.velocity;
            //if (Player.mount.Active && Player.mount.Cart) {
            //    int num = Math.Sign(Player.velocity.X);
            //    if (num == 0) num = Player.direction;
            //    vector2_1 = new Vector2(MathHelper.Lerp(0.0f, -8f, Player.fullRotation / 0.78f), MathHelper.Lerp(0.0f, 2f, Math.Abs(Player.fullRotation / 0.78f))).RotatedBy(Player.fullRotation, new Vector2());
            //    if (num == Math.Sign(Player.fullRotation))
            //        vector2_1 *= MathHelper.Lerp(1f, 0.6f, Math.Abs(Player.fullRotation / 0.78f));
            //}

            //Vector2 spinningpoint1 = new Vector2(3 * Player.direction - (Player.direction == 1 ? 1 : 0), -11.5f * Player.gravDir) + Vector2.UnitY * Player.gfxOffY + Player.Size / 2f + Main.OffsetsPlayerHeadgear[index];
            //Vector2 spinningpoint2 = new Vector2(3 * Player.shadowDirection[1] - (Player.direction == 1 ? 1 : 0), -11.5f * Player.gravDir) + Player.Size / 2f + Main.OffsetsPlayerHeadgear[index];
            //if (Player.fullRotation != 0.0) {
            //    spinningpoint1 = spinningpoint1.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
            //    spinningpoint2 = spinningpoint2.RotatedBy(Player.fullRotation, Player.fullRotationOrigin);
            //}

            //float num1 = 0.0f;
            //if (Player.mount.Active) num1 = Player.mount.PlayerOffset;

            //Vector2 vector2_2 = Player.position + spinningpoint1 + vector2_1;
            //vector2_2.Y -= num1 / 2f;

            //Vector2 vector2_3 = Player.oldPosition + spinningpoint2 + vector2_1;
            //vector2_3.Y -= num1 / 2f;

            //float num2 = 1f;
            //int num3 = (int)Vector2.Distance(vector2_2, vector2_3) / 3 + 1;
            //if (Vector2.Distance(vector2_2, vector2_3) % 3.0 != 0.0) ++num3;

            //DelegateMethods.v3_1 = Main.hslToRgb(Main.rgbToHsl(Player.eyeColor).X, 1f, 0.5f).ToVector3() * 0.5f * num2;
            //if (Player.velocity != Vector2.Zero)
            //    Utils.PlotTileLine(Player.Center, Player.Center + Player.velocity * 2f, 4f, DelegateMethods.CastLightOpen);
            //else
            //    Utils.PlotTileLine(Player.Left, Player.Right, 4f, DelegateMethods.CastLightOpen);

            //for (float num4 = 1f; num4 <= (double)num3; ++num4) {
            //    Dust dust = Main.dust[Dust.NewDust(Player.Center, 0, 0, ModContent.DustType<RedLineDust>(), 0.0f, 0.0f, 0, default, 1f)];
            //    dust.position = Vector2.Lerp(vector2_3, vector2_2, num4 / num3);
            //    dust.noGravity = true;
            //    dust.velocity = Vector2.Zero;
            //    dust.customData = Player;
            //    dust.scale = num2;
            //    dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cYorai, Player);
            //}
        }
    }
}