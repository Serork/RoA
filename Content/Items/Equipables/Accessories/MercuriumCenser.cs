using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Waist)]
sealed class MercuriumCenser : NatureItem {
    private class MercuriumCenserHandler : ModPlayer {
        private float _spawnFumesTimer;

        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            var handler = Player.GetWreathHandler();
            if (!handler.IsFull1) {
                return;
            }

            float spawnTime = 15f;
            int spawnCount = 3;
            if (--_spawnFumesTimer <= 0f) {
                _spawnFumesTimer = spawnTime;

                ushort type = (ushort)ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.MercuriumCenserToxicFumes>();
                IEntitySource source = Player.GetSource_FromThis();
                int damage = (int)Player.GetTotalDamage(DruidClass.Nature).ApplyTo(8);
                float knockback = 0f;
                Vector2 spawnPosition = Player.Center;

                if (Player.whoAmI == Main.myPlayer) {
                    float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < spawnCount; i++) {
                        Vector2 velocity = Vector2.One.RotatedBy(MathHelper.TwoPi / i + offset * i) + Vector2.Clamp(Player.velocity, new Vector2(-2f, -2f), new Vector2(2f, 2f));
                        Projectile.NewProjectile(source, spawnPosition.X, spawnPosition.Y, velocity.X, velocity.Y, type,
                            damage, knockback, Player.whoAmI);
                    }
                }
            }
        }
    }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    protected override void SafeSetDefaults() {
        int width = 20; int height = 34;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 25, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().DischargeTimeDecreaseMultiplier -= 0.15f;
        player.GetModPlayer<MercuriumCenserHandler>().IsEffectActive = true;
    }
}