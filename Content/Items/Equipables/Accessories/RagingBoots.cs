using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
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

        if (player.velocity.Y > 0f) {
            player.gravity *= 1.5f;
        }
    }

    private sealed class RagingBootsAttackHandler : ModPlayer {
        private bool _onGround;
        private Vector2 _speedBeforeGround;

        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            Item item = Player.armor.FirstOrDefault(item => item.type == ModContent.ItemType<RagingBoots>());
            if (item == null || item == default) {
                return;
            }

            if (WorldGenHelper.CustomSolidCollision(Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms)) {
                if (((Player.velocity.Y == 0f || Player.sliding) && _speedBeforeGround.Length() > 7.5f) && !_onGround) {
                    SoundEngine.PlaySound(SoundID.Item167 with { PitchVariance = 0.1f }, Player.Bottom);

                    Vector2 velocity = _speedBeforeGround * 0.35f;
                    List<Color> colors = [new Color(147, 177, 253), new Color(50, 107, 197), new Color(9, 61, 191)];
                    for (int i = 0; i < 30; i++) {
                        if (Main.rand.Next(3) != 0) {
                            int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height - 2f), Player.width + 2, 6, DustID.Snow, 0f, 0f, 50, Main.rand.NextFromList([.. colors]));
                            if (Player.gravDir == -1f)
                                Main.dust[num6].position.Y -= Player.height + 4;
                            Main.dust[num6].noGravity = true;

                            Main.dust[num6].scale *= 2f;
                            Main.dust[num6].position.X -= _speedBeforeGround.X * 1f;
                            if (Player.gravDir == -1f)
                                Main.dust[num6].velocity.Y *= -1f;

                            Main.dust[num6].velocity -= velocity * 1.25f * Main.rand.NextFloat();
                        }
                    }

                    int count = (int)_speedBeforeGround.Length();
                    int dustType = TileHelper.GetKillTileDust((int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16, Main.tile[(int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16]);
                    for (int k = 0; k < count * 2; k++) {
                        Dust.NewDust(new Vector2(Player.position.X, Player.Bottom.Y), Player.width, 2, dustType, SpeedX: -velocity.X * 0.4f, SpeedY: -velocity.Y * 0.4f, Alpha: Main.rand.Next(255), Scale: Main.rand.NextFloat(1.5f) * 0.85f);
                    }

                    var center = Player.Bottom;
                    float radius = Player.Size.Length() / 2f * 0.55f;
                    if (Main.myPlayer == Player.whoAmI) {
                        var velo = velocity;
                        center += velocity;
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