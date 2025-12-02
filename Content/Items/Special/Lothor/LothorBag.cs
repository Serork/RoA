using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Equipables.Miscellaneous;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

sealed class LothorBag : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.BossBag[Type] = true;
        ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;

        Item.ResearchUnlockCount = 3;
    }

    public override void SetDefaults() {
        int width = 34; int height = width;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;

        Item.rare = ItemRarityID.LightRed;
        Item.expert = true;
    }

    public override bool CanRightClick() => true;

    public override void ModifyItemLoot(ItemLoot itemLoot) {
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<LothorMask>(), 7));

        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Equipables.Accessories.BloodbathLocket>()));

        itemLoot.Add(new OneFromRulesRule(1, ItemDropRule.Common(ModContent.ItemType<SphereOfAspiration>()),
                                             ItemDropRule.Common(ModContent.ItemType<Weapons.Ranged.ChemicalPrisoner>()),
                                             ItemDropRule.Common(ModContent.ItemType<Weapons.Summon.FlederStaff>()),
                                             ItemDropRule.Common(ModContent.ItemType<Weapons.Melee.BloodshedAxe>()),
                                             ItemDropRule.Common(ModContent.ItemType<Equipables.Accessories.SoulOfTheWoods>())));

        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodCursor>(), 20));

        itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Lothor>()));
    }

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White * 0.9f, 0.4f);

    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.White.ToVector3() * 0.4f);

        if (Item.timeSinceItemSpawned % 12 == 0) {
            Vector2 center = Item.Center + new Vector2(0f, Item.height * -0.1f);
            Vector2 direction = Main.rand.NextVector2CircularEdge(Item.width * 0.6f, Item.height * 0.6f);
            float distance = 0.3f + Main.rand.NextFloat() * 0.5f;
            Vector2 velocity = new(0f, -Main.rand.NextFloat() * 0.3f - 1.5f);

            Dust dust = Dust.NewDustPerfect(center + direction * distance, DustID.SilverFlame, velocity);
            dust.scale = 0.5f;
            dust.fadeIn = 1.1f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.alpha = 0;
        }
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        Texture2D texture = TextureAssets.Item[Item.type].Value;
        Rectangle frame;

        if (Main.itemAnimations[Item.type] != null) frame = Main.itemAnimations[Item.type].GetFrame(texture, Main.itemFrameCounter[whoAmI]);
        else frame = texture.Frame();

        Vector2 frameOrigin = frame.Size() / 2f;
        Vector2 offset = new Vector2(Item.width / 2 - frameOrigin.X, Item.height - frame.Height);
        Vector2 drawPos = Item.position - Main.screenPosition + frameOrigin + offset;

        float time = Main.GlobalTimeWrappedHourly;
        float timer = Item.timeSinceItemSpawned / 240f + time * 0.04f;

        float num4 = (float)Item.timeSinceItemSpawned / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 4f;
        globalTimeWrappedHourly /= 2f;
        if (globalTimeWrappedHourly >= 1f)
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;


        globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
        for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
            spriteBatch.Draw(texture, drawPos + new Vector2(0f, 8f).RotatedBy((num5 + num4) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(150, 50, 50, 50), rotation, frameOrigin, scale, SpriteEffects.None, 0f);
        }
        for (float num6 = 0f; num6 < 1f; num6 += 0.34f) {
            spriteBatch.Draw(texture, drawPos + new Vector2(0f, 4f).RotatedBy((num6 + num4) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(200, 60, 60, 77), rotation, frameOrigin, scale, SpriteEffects.None, 0f);
        }
        spriteBatch.Draw(texture, drawPos, frame, lightColor, rotation, frameOrigin, scale, SpriteEffects.None, 0f);
        return false;
    }
}