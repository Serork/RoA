using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Friendly.Ranged;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged;

[AutoloadGlowMask(127, 127, 127)]
sealed class ChemicalPrisoner : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("");
        Item.ResearchUnlockCount = 1;
        //ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SphereOfAspiration>();
    }

    public override void SetDefaults() {
        int width = 44; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 14;
        Item.autoReuse = true;

        Item.DamageType = DamageClass.Ranged;
        Item.noMelee = true;
        Item.damage = 27;
        Item.knockBack = 3f;

        Item.rare = ItemRarityID.Orange;

        Item.useAmmo = AmmoID.Bullet;
        Item.shootSpeed = 8f;
        Item.shoot = ProjectileID.PurificationPowder;

        Item.value = Item.sellPrice(0, 2, 25, 0);

        Item.scale = 0.85f;
    }

    public override Vector2? HoldoutOffset()
        => new Vector2(2f, 0);

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        if (Collision.CanHit(position, 0, 0, position + newVelocity, 0, 0))
            position += newVelocity * 28f;
        position -= new Vector2(-newVelocity.Y, newVelocity.X) * (4f * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        ++player.GetModPlayer<ChemicalPrisonerPlayer>().count;
        int flaskType = ModContent.ProjectileType<ChemicalFlask>();
        if (player.GetModPlayer<ChemicalPrisonerPlayer>().count > 3) {
            SoundEngine.PlaySound(SoundID.Item5, player.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 6, player.Center));
            }

            Projectile.NewProjectile(source, position.X, position.Y - 2, velocity.X * 0.8f, velocity.Y * 0.8f, flaskType, damage, knockback, player.whoAmI);
        }
        else {
            SoundEngine.PlaySound(SoundID.Item36, player.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 7, player.Center));
            }

            Projectile.NewProjectile(source, position.X, position.Y - 2, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, 1f);
        }
        return false;
    }

    private class ChemicalPrisonerPlayer : ModPlayer {
        public int count;

        public override void PostUpdateMiscEffects() {
            if (count > 3) count = 0;
        }
    }
}
