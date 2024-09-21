using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheShock : Rod {
    protected override Color? LightingColor => new(86, 173, 177);

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 40; int height = 40;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 8;
        Item.autoReuse = true;

        Item.damage = 20;

        Item.mana = 10;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item81;

        Item.channel = true;

        Item.shoot = ModContent.ProjectileType<ShockLightning>();
        Item.shootSpeed = 22f;
    }
}