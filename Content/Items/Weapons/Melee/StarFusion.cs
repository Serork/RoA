using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Melee;
using RoA.Core.Utility;
using RoA.Utilities;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

[AutoloadGlowMask]
sealed class StarFusion : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("Creates magical constellation on hit");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 46; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 34;
        Item.autoReuse = false;
        Item.useTurn = true;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 32;
        Item.knockBack = 4f;

        Item.value = Item.sellPrice(silver: 75);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item1;

        //Item.glowMask = RiseofAgesGlowMask.Get(nameof(StarFusion));
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.Next(5) == 0 && Main.rand.NextChance(0.75))
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Main.rand.NextBool(3) ? DustID.YellowStarDust : DustID.Enchanted_Gold, 0f, 0f, 150, default(Color), 1.2f);

        if (Main.rand.Next(10) == 0 && Main.rand.NextChance(0.75))
            Gore.NewGore(new EntitySource_ItemUse(player, Item), new Vector2(hitbox.X, hitbox.Y), default(Vector2), Main.rand.Next(16, 17));
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        int index = 0;
        void spawnStar() {
            index++;
            Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.5f;
            Vector2 offset = new Vector2(0f, 200f).RotatedBy(velocity.ToRotation());
            ushort type = (ushort)ModContent.ProjectileType<MeltingStar>();
            Vector2 spawnPos = target.Center + offset;
            Vector2 projectileVelocity = Helper.VelocityToPoint(spawnPos, target.Center, 1f);
            Projectile.NewProjectile(target.GetSource_OnHit(target), spawnPos.X, spawnPos.Y, projectileVelocity.X, projectileVelocity.Y, type, Item.damage / 2, Item.knockBack, player.whoAmI, 0f, (float)index, target.whoAmI);
        }
        for (int i = 0; i < 3; i++) {
            spawnStar();
        }
    }

    //public override void AddRecipes() {
    //    CreateRecipe()
    //        .AddIngredient(ItemID.HellstoneBar, 15)
    //        .AddIngredient(ItemID.FallenStar, 15)
    //        .AddIngredient<Materials.MercuriumNugget>(5)
    //        .AddTile(TileID.Anvils)
    //        .Register();
    //}
}