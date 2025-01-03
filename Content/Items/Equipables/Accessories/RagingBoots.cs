using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Shoes)]
sealed class RagingBoots : NatureItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    protected override void SafeSetDefaults() {
        Item.SetSize(26, 30);

        Item.damage = 34;
        Item.knockBack = 5f;
        Item.value = Item.buyPrice(gold: 3);
        Item.defense = 2;
        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 45);
        NatureWeaponHandler.SetFillingRate(Item, 0.25f);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<RagingBootsAttackHandler>().IsEffectActive = true;

        player.moveSpeed += 0.05f;
        player.runAcceleration += 0.05f;
    }

    private sealed class RagingBootsAttackHandler : ModPlayer {
        private bool _onGround;
        private Vector2 _speedBeforeGround;

        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PreUpdateMovement() {
            if (!IsEffectActive) {
                return;
            }
        }

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            Item item = Player.armor.FirstOrDefault(item => item.type == ModContent.ItemType<RagingBoots>());
            if (item == null) {
                return;
            }

            if (WorldGenHelper.CustomSolidCollision(Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms)) {
                if ((Player.velocity.Y == 0f || Player.sliding) && !_onGround) {
                    Vector2 velocity = _speedBeforeGround * 0.35f;
                    List<Color> colors = [new Color(147, 177, 253), new Color(50, 107, 197), new Color(9, 61, 191)];
                    for (int i = 0; i < 40; i++) {
                        if (Main.rand.Next(3) != 0) {
                            int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height - 2f), Player.width + 2, 6, DustID.Snow, 0f, 0f, 50, Main.rand.NextFromList([.. colors]));
                            if (Player.gravDir == -1f)
                                Main.dust[num6].position.Y -= Player.height + 4;
                            Main.dust[num6].noGravity = true;

                            Main.dust[num6].scale *= 2f;
                            Main.dust[num6].position.X -= _speedBeforeGround.X * 1f;
                            if (Player.gravDir == -1f)
                                Main.dust[num6].velocity.Y *= -1f;

                            Main.dust[num6].velocity -= _speedBeforeGround * 0.6f * Main.rand.NextFloat();
                        }
                    }

                    var center = Player.Bottom;
                    float radius = Player.Size.Length() / 2f * 0.55f;
                    if (Main.myPlayer == Player.whoAmI) {
                        var velo = velocity;
                        center += velocity;
                        int count = (int)_speedBeforeGround.Length();
                        for (int i = 0; i < count; i++) {
                            var shootTo = velo.RotatedBy(MathHelper.PiOver2 * (i < count / 2).ToDirectionInt() + MathHelper.PiOver4 * 0.75f * Main.rand.NextFloatDirection());
                            var shootLocation = center + Vector2.Normalize(shootTo) * 10f;
                            NatureProjectile.CreateNatureProjectile(Player.GetSource_Misc("ragingboots"), item, shootLocation, shootTo, 
                                ModContent.ProjectileType<RagingBootsWave>(),
                                NatureWeaponHandler.GetNatureDamage(item, Player),
                                Player.GetTotalKnockback(DruidClass.NatureDamage).ApplyTo(item.knockBack), 
                                Player.whoAmI,
                                (int)(18 + _speedBeforeGround.Length() * 2f));
                        }
                    }

                    _onGround = true;
                }

                return;
            }

            _speedBeforeGround = Player.velocity;
            _onGround = false;
        }
    }
}