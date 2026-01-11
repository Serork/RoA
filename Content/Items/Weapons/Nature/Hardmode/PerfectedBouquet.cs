using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask]
sealed class PerfectedBouquet : NatureItem {
    private float _lerpColorProgress;
    private Color _lerpColor;
    private int _nextTulipTypeIndex;

    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32, 36);
        Item.SetWeaponValues(36, 2f, 6);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 25, autoReuse: true, useSound: SoundID.Item65);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<TulipPetalSoul>(), 8f);
        Item.SetShopValues(ItemRarityColor.Lime7, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        float num6 = (float)Main.rand.Next(90, 111) * 0.01f;
        num6 *= Main.essScale;
        Color color = ColorUtils.GetLerpColor(ref _lerpColorProgress, ref _lerpColor, [new Color(0.1f, 0.1f, 0.6f), new Color(0.5f, 0.3f, 0.05f), new Color(0.1f, 0.5f, 0.2f)]);
        Lighting.AddLight((int)((player.itemLocation.X - 4) / 16f), (int)((player.itemLocation.Y) / 16f), color.R * num6 / 255f, color.G * num6 / 255f, color.B * num6 / 255f);
    }

    public override void PostUpdate() {
        float num6 = (float)Main.rand.Next(90, 111) * 0.01f;
        num6 *= Main.essScale;
        Color color = ColorUtils.GetLerpColor(ref _lerpColorProgress, ref _lerpColor, [new Color(0.1f, 0.1f, 0.6f), new Color(0.5f, 0.3f, 0.05f), new Color(0.1f, 0.5f, 0.2f)]);
        Lighting.AddLight((int)((Item.Center.X - 4) / 16f), (int)((Item.Center.Y) / 16f), color.R * num6 / 255f, color.G * num6 / 255f, color.B * num6 / 255f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float collisionCheckSize = Item.width * 1.4f;
        Vector2 collisionCheckPosition = position + Vector2.Normalize(velocity) * collisionCheckSize;
        if (!Collision.CanHit(player.Center, 0, 0, collisionCheckPosition, 0, 0)) {
            return false;
        }

        TulipPetalSoul.PetalType petalType = (TulipPetalSoul.PetalType)_nextTulipTypeIndex++;
        if (_nextTulipTypeIndex > 2) {
            _nextTulipTypeIndex = 0;
        }
        Vector2 shootVelocityNormalized = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        float itemRotation = shootVelocityNormalized.ToRotation();
        Vector2 itemSizeOffset = shootVelocityNormalized * Item.width;
        position += itemSizeOffset;
        Vector2 petalOffset;
        bool isWreathCharged = WreathHandler.IsWreathCharged(player);
        if (isWreathCharged) {
            petalOffset = new Vector2(10f, -5f * player.direction).RotatedBy(itemRotation);
        }
        else {
            if (petalType == TulipPetalSoul.PetalType.SkeletronPrime) {
                petalOffset = new Vector2(10f, -5f * player.direction).RotatedBy(itemRotation);
            }
            else if (petalType == TulipPetalSoul.PetalType.Destroyer) {
                petalOffset = new Vector2(4f, -12f * player.direction).RotatedBy(itemRotation);
            }
            else {
                petalOffset = new Vector2(9f, 3f * player.direction).RotatedBy(itemRotation);
            }
        }
        petalOffset -= new Vector2(2f, 2f * player.direction).RotatedBy(itemRotation);
        position += petalOffset;
        velocity = position.DirectionTo(player.GetWorldMousePosition());
        int whoAmI = ProjectileUtils.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, beforeNetSend: (projectile) => {
            _ = new TulipPetalSoul.TulipPetalSoulValues(projectile) {
                CurrentType = petalType
            };
        }, centered: true);


        for (int i = 0; i < 2; i++) {
            float offset2 = 6f;
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                    spawnPosition = position + randomOffset;
            Dust dust = Dust.NewDustPerfect(spawnPosition,
                                            ModContent.DustType<Dusts.Tulip>(),
                                            (spawnPosition - position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f),
                                            Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f) * 1.5f,
                                            Alpha: Dusts.Tulip.SOULORANGE + (isWreathCharged ? Main.rand.Next(3) : (byte)petalType));
            dust.customData = Main.rand.NextFloatRange(50f);
        }

        return false;
    }

    public override Vector2? HoldoutOffset() => new(-14, 10);
}
