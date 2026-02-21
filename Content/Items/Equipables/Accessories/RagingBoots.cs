using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Content.Dusts;
using RoA.Content.Items.Equipables.Accessories.Hardmode;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Shoes)]
sealed class RagingBoots : NatureItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        //foreach (TooltipLine tooltip in tooltips) {
        //    if (tooltip.Mod == "Terraria" && tooltip.Name == "Speed") {
        //        tooltip.Hide();
        //    }
        //}
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 30);

        Item.damage = 34;
        Item.knockBack = 5f;
        Item.defense = 2;
        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;

        NatureWeaponHandler.SetPotentialDamage(Item, 45);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.25f);

        Item.value = Item.sellPrice(0, 1, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var handler = player.GetModPlayer<RagingBootsAttackHandler>();
        if (!handler.IsRagingBootsEffectActive) {
            handler.RagingBoots = Item;
        }
        handler.IsRagingBootsEffectActive = true;

        player.moveSpeed += 0.05f;
        player.runAcceleration += 0.05f;

        if ((player.gravDir == 1 && player.velocity.Y > 0) || (player.gravDir == -1 && player.velocity.Y < 0)) {
            player.gravity *= 1.5f;
        }
    }

    public class RagingBootsAttackHandler : ModPlayer {
        private bool _onGround, _onGround2;
        private Vector2 _speedBeforeGround;
        private int _fallLength;

        public bool IsRagingBootsEffectActive,
                    IsWoodSandalsEffectActive;
        public Item RagingBoots = null!,
                    WoodSandals = null!;

        public override void ResetEffects() {
            if (!IsRagingBootsEffectActive) {
                RagingBoots = null!;
            }
            if (!IsWoodSandalsEffectActive) {
                WoodSandals = null!;
            }
            IsRagingBootsEffectActive = false;
            IsWoodSandalsEffectActive = false;
        }

        public override void PostUpdateEquips() {
            bool calculateFallLength = true;

            void handleRagingBoots() {
                if (!IsRagingBootsEffectActive) {
                    return;
                }

                Item ragingBootsItem = RagingBoots;
                if (ragingBootsItem.IsEmpty()) {
                    return;
                }

                bool ragingBoots = ragingBootsItem.type == ModContent.ItemType<RagingBoots>();

                bool enoughSpeed = _fallLength > 2;
                bool land = Player.IsGrounded() && enoughSpeed && !_onGround;
                int count = (int)_speedBeforeGround.Length();
                int count2 = (int)MathHelper.Clamp(_fallLength, 0, 20);
                bool onIceBlock = false;
                if (TileHelper.CustomSolidCollision_CheckForIceBlocks(Player, Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms, land,
                    onDestroyingIceBlock: (player) => {
                        if (ragingBoots) {
                            if (player.whoAmI == Main.myPlayer) {
                                float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                                for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / count2) {
                                    Projectile.NewProjectile(Player.GetSource_Accessory(ragingBootsItem), Player.Bottom, _speedBeforeGround.RotatedBy(i + startAngle) * Main.rand.NextFloat(0.25f, 0.75f), ModContent.ProjectileType<IceShard>(), NatureWeaponHandler.GetNatureDamage(ragingBootsItem, Player) * 2, Player.GetTotalKnockback(DruidClass.Nature).ApplyTo(ragingBootsItem.knockBack) * 0.5f);
                                }
                            }
                            onIceBlock = true;
                        }
                    })) {
                    if (land) {
                        SoundEngine.PlaySound(SoundID.Item167 with { PitchVariance = 0.1f }, Player.Bottom);

                        Vector2 velocity = _speedBeforeGround * 0.35f;
                        List<Color> colors = [new Color(147, 177, 253), new Color(50, 107, 197), new Color(9, 61, 191)];
                        for (int i = 0; i < 15; i++) {
                            if (Main.rand.Next(3) != 0) {
                                int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height - 2f), Player.width + 2, 6, ModContent.DustType<SnowDust3>(), 0f, 0f, 50, Main.rand.NextFromList([.. colors]));
                                if (Player.gravDir == -1f)
                                    Main.dust[num6].position.Y -= Player.height + 4;
                                Main.dust[num6].noGravity = true;

                                Main.dust[num6].scale *= 1.5f;
                                Main.dust[num6].position.X -= _speedBeforeGround.X * 1f;
                                if (Player.gravDir == -1f)
                                    Main.dust[num6].velocity.Y *= -1f;

                                Main.dust[num6].velocity -= velocity * 1.25f * Main.rand.NextFloat();
                            }
                        }

                        if (!onIceBlock) {
                            int dustType = TileHelper.GetKillTileDust((int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16, Main.tile[(int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16]);
                            for (int k = 0; k < count2 * 2; k++) {
                                Dust.NewDust(new Vector2(Player.position.X, Player.Bottom.Y), Player.width, 2, dustType, SpeedX: -velocity.X * 0.4f, SpeedY: -velocity.Y * 0.4f, Alpha: Main.rand.Next(255), Scale: Main.rand.NextFloat(1.5f) * 0.85f);
                            }
                        }

                        if (ragingBoots) {
                            var center = Player.Bottom;
                            float radius = Player.Size.Length() / 2f * 0.55f;
                            if (Main.myPlayer == Player.whoAmI) {
                                var velo = velocity;
                                center += velocity;
                                for (int i = 0; i < count; i++) {
                                    var shootTo = velo.RotatedBy(MathHelper.PiOver2 * (i < count / 2).ToDirectionInt() + MathHelper.PiOver4 * 0.75f * Main.rand.NextFloatDirection());
                                    var shootLocation = center + Vector2.Normalize(shootTo) * 10f;
                                    Projectile.NewProjectile(Player.GetSource_Misc("ragingboots"), shootLocation, shootTo,
                                        ModContent.ProjectileType<RagingBootsWave>(),
                                        NatureWeaponHandler.GetNatureDamage(ragingBootsItem, Player),
                                        Player.GetTotalKnockback(DruidClass.Nature).ApplyTo(ragingBootsItem.knockBack),
                                        Player.whoAmI,
                                        (int)(18 + count2 * 2f));
                                }
                            }
                        }

                        _onGround = true;
                    }

                    calculateFallLength = false;
                }
            }
            void handleWoodBoots() {
                if (!IsWoodSandalsEffectActive) {
                    return;
                }

                Item woodBootsItem = WoodSandals;
                if (woodBootsItem.IsEmpty()) {
                    return;
                }

                bool woodBoots = !(woodBootsItem.type == ModContent.ItemType<SandalwoodStompers>());

                bool enoughSpeed = _fallLength > 2;
                bool land = Player.IsGrounded() && enoughSpeed && !_onGround2;
                int count = (int)_speedBeforeGround.Length();
                int count2 = (int)MathHelper.Clamp(_fallLength, 0, 20);
                bool onIceBlock = false;
                if (TileHelper.CustomSolidCollision_CheckForIceBlocks(Player, Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms, land,
                    onDestroyingIceBlock: (player) => {
                        if (!woodBoots) {
                            if (player.whoAmI == Main.myPlayer) {
                                float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                                for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / count2) {
                                    Projectile.NewProjectile(Player.GetSource_Accessory(woodBootsItem), Player.Bottom, _speedBeforeGround.RotatedBy(i + startAngle) * Main.rand.NextFloat(0.25f, 0.75f), ModContent.ProjectileType<IceShard>(), NatureWeaponHandler.GetNatureDamage(woodBootsItem, Player) * 2, Player.GetTotalKnockback(DruidClass.Nature).ApplyTo(woodBootsItem.knockBack) * 0.5f);
                                }
                            }
                            onIceBlock = true;
                        }
                    })) {
                    if (land) {
                        if (!_onGround) {
                            SoundEngine.PlaySound(SoundID.Item167 with { PitchVariance = 0.1f }, Player.Bottom);
                        }

                        Vector2 velocity = _speedBeforeGround * 0.35f;
                        List<Color> colors = [new Color(147, 177, 253), new Color(50, 107, 197), new Color(9, 61, 191)];
                        if (!woodBoots) {
                            colors.Clear();
                            colors.Add(WreathHandler.GetCurrentColor(Player));
                            colors.Add(WreathHandler.GetCurrentColor(Player).ModifyRGB(1.3f));
                            colors.Add(WreathHandler.GetCurrentColor(Player).ModifyRGB(0.7f));
                        }
                        for (int i = 0; i < 15; i++) {
                            if (Main.rand.Next(3) != 0) {
                                int num6 = Dust.NewDust(new Vector2(Player.position.X - 4f, Player.position.Y + (float)Player.height - 2f), Player.width + 2, 6, ModContent.DustType<SnowDust3>(), 0f, 0f, 50, Main.rand.NextFromList([.. colors]));
                                if (Player.gravDir == -1f)
                                    Main.dust[num6].position.Y -= Player.height + 4;
                                Main.dust[num6].noGravity = true;

                                Main.dust[num6].scale *= 1.5f;
                                Main.dust[num6].position.X -= _speedBeforeGround.X * 1f;
                                if (Player.gravDir == -1f)
                                    Main.dust[num6].velocity.Y *= -1f;

                                Main.dust[num6].velocity -= velocity * 1.25f * Main.rand.NextFloat();
                            }
                        }

                        if (!onIceBlock && !_onGround) {
                            int dustType = TileHelper.GetKillTileDust((int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16, Main.tile[(int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16]);
                            for (int k = 0; k < count2 * 2; k++) {
                                Dust.NewDust(new Vector2(Player.position.X, Player.Bottom.Y), Player.width, 2, dustType, SpeedX: -velocity.X * 0.4f, SpeedY: -velocity.Y * 0.4f, Alpha: Main.rand.Next(255), Scale: Main.rand.NextFloat(1.5f) * 0.85f);
                            }
                        }

                        if (!woodBoots) {
                            Vector2 position = Player.Bottom;
                            for (int i = 0; i < count2; i++) {
                                if (Main.rand.Next(3) == 0) {
                                    Dust dust2 = Dust.NewDustDirect(position, 0, 0, DustID.TintableDustLighted, velocity.X, velocity.Y, 254, Main.rand.NextFromList([.. colors]), 0.5f);
                                    Vector2 vector3 = Main.rand.NextVector2Circular(1f, 1f) * 4f;
                                    dust2.position = position + vector3 * 10f;
                                    dust2.velocity = vector3;
                                }
                            }

                            if (Main.netMode != NetmodeID.Server && Player.IsLocal()) {
                                string tag = "Sandalwood Stompers Stomp";
                                float strength = count2 / 20f * 1.5f;
                                PunchCameraModifier punchCameraModifier = new PunchCameraModifier(position.ToTileCoordinates().ToWorldCoordinates(), MathHelper.PiOver2.ToRotationVector2(), strength, 6f, (int)(Ease.CircOut(count2 / 20f) * 10), 1000f, tag);
                                Main.instance.CameraModifiers.Add(punchCameraModifier);
                            }

                            if (Player.IsLocal()) {
                                ProjectileUtils.SpawnPlayerOwnedProjectile<SeedOfWisdomRoot>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_Accessory(woodBootsItem)) {
                                    Position = position,
                                    AI0 = 1f,
                                    AI2 = count2 * 20f,
                                    Damage = NatureWeaponHandler.GetNatureDamage(woodBootsItem, Player),
                                    KnockBack = Player.GetTotalKnockback(DruidClass.Nature).ApplyTo(woodBootsItem.knockBack)
                                });
                            }
                        }

                        _onGround = true;

                        _onGround2 = true;
                    }

                    calculateFallLength = false;
                }
            }

            handleRagingBoots();
            handleWoodBoots();

            if (_onGround && !_onGround2) {
                _onGround2 = true;
            }

            if (!calculateFallLength) {
                return;
            }

            _speedBeforeGround = Player.velocity;
            if ((Player.gravDir == 1 && Player.velocity.Y > 9.5f) || (Player.gravDir == -1 && Player.velocity.Y < -9.5f))
                _fallLength++;
            else _fallLength = 0;
            _onGround = false;
            _onGround2 = false;
        }
    }
}