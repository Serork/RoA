using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class BrilliantBouquet : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Brilliant Bouquet");
        //Tooltip.SetDefault("Launches a random tulip petal\nCombines all effects while Wreath is charged");

        Item.staff[Item.type] = true;
    }

    protected override void SafeSetDefaults() {
        NatureWeaponHandler.SetPotentialDamage(Item, 18);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.15f);

        Item.damage = 7;
        Item.knockBack = 1f;
        Item.crit = 6;

        Item.width = 32;
        Item.height = 36;

        Item.useTime = Item.useAnimation = 25;

        //item.reuseDelay = 60;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item65;
        Item.autoReuse = true;
        Item.noMelee = true;

        Item.rare = ItemRarityID.Orange;

        Item.shoot = ModContent.ProjectileType<TulipPetalOld>();
        Item.shootSpeed = 8f;

        Item.value = Item.sellPrice(0, 2, 0, 0);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float offset3 = Item.width * 1.4f;
        if (!Collision.CanHit(player.Center, 0, 0, position + Vector2.Normalize(velocity) * offset3, 0, 0)) {
            return false;
        }

        int petalType = Main.rand.Next(3);
        bool flag = player.GetWreathHandler().IsFull1;
        if (flag) {
            petalType = 3;
        }
        Vector2 velocity2 = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        int petalType2 = petalType;
        if (flag) {
            petalType2 = 1;
        }
        //petalType2 = 1;
        float rot = velocity2.ToRotation();
        //if (player.direction == 1) {
        //    rot -= MathHelper.PiOver4 / 4f;
        //}
        float size = 0.15f * petalType2;
        Vector2 offset = new(size, -size);
        offset *= player.direction;
        position += (Item.Size * offset).RotatedBy(rot);
        //position += new Vector2(6f, 0f * player.direction).RotatedBy(rot);
        int amount = 27;
        if (petalType == 1) {
            amount = 32;
        }
        if (petalType == 2) {
            amount = player.direction == 1 ? 30 : 37;
        }
        position += velocity2 * amount;
        velocity = position.DirectionTo(player.GetWorldMousePosition()) * velocity.Length();
        Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, petalType);

        if (Main.rand.NextChance(0.7)) {
            float offset2 = 6f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                    spawnPosition = position + randomOffset;

            //ushort dustType = CoreDustType();
            Dust dust = Dust.NewDustPerfect(spawnPosition,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f),
                                            Alpha: flag ? Main.rand.Next(3) : petalType);
            dust.customData = Main.rand.NextFloatRange(50f);
        }

        return false;
    }

    public override Vector2? HoldoutOffset() => new(-14, 10);
}


