using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Dusts;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class cleoMask : ModItem {
    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
        glowMaskColor = Color.White * (1f - shadow) * 0.5f;
    }

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Peegeon's Hood");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 24; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override void UpdateVanitySet(Player player) {
        if (player.controlJump && player.velocity.Y < -0.25f) {
            int num = player.height;
            if (player.gravDir == -1f)
                num = 4;

            for (int i = 0; i < 2; i++) {
                int num2 = ((i == 0) ? 2 : (-2));
                Rectangle r = ((i != 0) ? new Rectangle((int)player.position.X + player.width - 4, (int)player.position.Y + num - 10, 8, 8) : new Rectangle((int)player.position.X - 4, (int)player.position.Y + num - 10, 8, 8));
                if (player.direction == -1)
                    r.X -= 4;

                int type = 6;
                float scale = 2.5f;
                int alpha = 100;
                float num3 = 1f;
                Vector2 vector = new Vector2((float)(-num2) - player.velocity.X * 0.3f, 2f * player.gravDir - player.velocity.Y * 0.3f);
                Dust dust;
                int num4 = Main.rand.Next(6);
                r.Y += 2 * (int)player.gravDir;
                if (num4 == 0 || num4 == 1) {
                    dust = Dust.NewDustDirect(r.TopLeft(), r.Width, r.Height, ModContent.DustType<cleoDust>(), 0f, 0f, 50, Color.Lerp(Color.Blue, Color.LightBlue, Main.rand.NextFloat() * 0.5f));
                    dust.shader = GameShaders.Armor.GetSecondaryShader(player.cLegs, player);
                    dust.scale = 0.66f;
                    dust.noGravity = true;
                    dust.velocity *= 0.25f;
                    dust.velocity -= player.velocity * 0.5f;
                    dust.velocity += vector * 0.5f;
                    dust.position += dust.velocity * 4f;
                    dust.noLight = dust.noLightEmittence = true;
                    if (Main.rand.Next(5) == 0)
                        dust.fadeIn = 0.8f;
                }

                type = ModContent.DustType<cleoDust>();
                alpha = 50;
                scale = 0.7f;
                num3 = 0.5f;

                dust = Dust.NewDustDirect(r.TopLeft(), r.Width, r.Height, type, 0f, 0f, alpha, Color.Lerp(Color.Blue, Color.LightBlue, Main.rand.NextFloat() * 0.5f), scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(player.cLegs, player);
                dust.velocity += vector;
                dust.velocity *= num3;
                dust.noLight = dust.noLightEmittence = true;
                //switch (vanityRocketBoots) {
                //    case 5:
                //        dust.noGravity = true;
                //        break;
                //    case 1:
                //        dust.noGravity = true;
                //        break;
                //    case 2:
                //        dust.velocity *= 0.1f;
                //        break;
                //    case 3:
                //        dust.velocity *= 0.05f;
                //        dust.velocity.Y += 0.15f;
                //        dust.noLight = true;
                //        if (Main.rand.Next(2) == 0) {
                //            dust.noGravity = true;
                //            dust.scale = 1.75f;
                //        }
                //        break;
                //}
            }
        }
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => head == EquipLoader.GetEquipSlot(Mod, nameof(cleoMask), EquipType.Head) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(cleoChestguard), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(cleoPants), EquipType.Legs);
}
