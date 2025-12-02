using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.WorldEvents;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class DruidSoul : ModItem {
    public override void Load() {
        On_NPC.ReleaseNPC += On_NPC_ReleaseNPC;
    }

    private int On_NPC_ReleaseNPC(On_NPC.orig_ReleaseNPC orig, int x, int y, int Type, int Style, int who) {
        if (Type == ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Summon.DruidSoul>()) {
            int num = -1;
            if (Main.netMode == 1) {
                NetMessage.SendData(71, -1, -1, null, x, y, Type, Style);
            }
            else {
                num = NPC.NewNPC(Main.player[who].GetSource_ReleaseEntity(), x, y, Type);
                Main.npc[num].netUpdate = true;
            }

            return num;
        }

        return orig(x, y, Type, Style, who);
    }

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White * 0.9f, 0.5f);

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.DefaultToCapturedCritter(ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Summon.DruidSoul>());
        Item.SetShopValues(ItemRarityColor.Orange3, Item.sellPrice(0, 1));
        Item.width = 26;
        Item.height = 30;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Summon.DruidSoul>()) && AltarHandler.GetAltarStrength() <= 0f;

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        Color color = alphaColor;
        int length = 6;
        for (int index = 0; index < length; index++) {
            float factor = (length - (float)index) / length;
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                Texture2D text = TextureAssets.Item[Type].Value;
                Rectangle frame = new(0, 0, text.Width, text.Height);
                Vector2 position = Item.Center - Main.screenPosition - Vector2.UnitY * 2f;
                spriteBatch.Draw(text, position + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) + Vector2.UnitY * 2f,
                    frame, lightColor.MultiplyAlpha((float)i / length), rotation, Item.Size / 2f, Helper.Wave(scale + 0.05f, scale + 0.15f, 1f, 0f) * factor, SpriteEffects.None, 0f);
            }
        }

        return true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        => itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
}
