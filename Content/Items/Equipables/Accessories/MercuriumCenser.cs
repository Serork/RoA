using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class MercuriumCenser : NatureItem {
    private class MercuriumCenserHandler : ModPlayer {
        private float _spawnFumesTimer;

        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            var handler = Player.GetModPlayer<WreathHandler>();
            if (!handler.IsFull1) {
                return;
            }

            float spawnTime = 15f;
            int spawnCount = 5;
            if (--_spawnFumesTimer <= 0f) {
                _spawnFumesTimer = spawnTime;

                if (Player.whoAmI != Main.myPlayer) {
                    return;
                }

                ushort type = (ushort)ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.MercuriumCenserToxicFumes>();
                IEntitySource source = Player.GetSource_FromThis();
                int damage = 15;
                float knockback = 3f;
                Vector2 spawnPosition = Player.Center;
                float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < spawnCount; i++) {
                    Vector2 velocity = Vector2.One.RotatedBy(MathHelper.TwoPi / i + offset * i) + Player.velocity;
                    Projectile.NewProjectile(source, spawnPosition.X, spawnPosition.Y, velocity.X, velocity.Y, type,
                        damage, knockback, Player.whoAmI);
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

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<MercuriumCenserHandler>().IsEffectActive = true;
}