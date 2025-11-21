using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Items;
using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System.Collections.Generic;

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
        if (!handler.IsEffectActive) {
            handler.Boots = Item;
        }
        handler.IsEffectActive = true;

        player.moveSpeed += 0.05f;
        player.runAcceleration += 0.05f;

        if ((player.gravDir == 1 && player.velocity.Y > 0) || (player.gravDir == -1 && player.velocity.Y < 0)) {
            player.gravity *= 1.5f;
        }
    }

    private class RagingBootsAttackHandler : ModPlayer {
        private bool _onGround;
        private Vector2 _speedBeforeGround;
        private int _fallLength;

        public bool IsEffectActive;
        public Item Boots;

        public override void ResetEffects() {
            if (!IsEffectActive) {
                Boots = null;
            }
            IsEffectActive = false;
        }

        public override void PostUpdateEquips() {
            if (!IsEffectActive) {
                return;
            }

            Item item = Boots;
            if (item == null || item == default) {
                return;
            }

            bool enoughSpeed = _fallLength > 2;
            bool land = ((Player.velocity.Y == 0f || Player.sliding) && enoughSpeed) && !_onGround;
            int count = (int)_speedBeforeGround.Length();
            int count2 = (int)MathHelper.Clamp(_fallLength, 0, 20);
            bool onIceBlock = false;
            if (TileHelper.CustomSolidCollision_CheckForIceBlocks(Player, Player.position - Vector2.One * 3, Player.width + 6, Player.height + 6, TileID.Sets.Platforms, land, 
                onDestroyingIceBlock: (player) => {
                    if (player.whoAmI == Main.myPlayer) {
                        float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / count2) {
                            Projectile.NewProjectile(Player.GetSource_Accessory(item), Player.Bottom, _speedBeforeGround.RotatedBy(i + startAngle) * Main.rand.NextFloat(0.25f, 0.75f), ModContent.ProjectileType<IceShard>(), NatureWeaponHandler.GetNatureDamage(item, Player) * 2, Player.GetTotalKnockback(DruidClass.Nature).ApplyTo(item.knockBack) * 0.5f);
                        }
                    }
                    onIceBlock = true;
                })) {
                if (land) {
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

                    if (!onIceBlock) {
                        int dustType = TileHelper.GetKillTileDust((int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16, Main.tile[(int)Player.Bottom.X / 16, (int)Player.Bottom.Y / 16]);
                        for (int k = 0; k < count2 * 2; k++) {
                            Dust.NewDust(new Vector2(Player.position.X, Player.Bottom.Y), Player.width, 2, dustType, SpeedX: -velocity.X * 0.4f, SpeedY: -velocity.Y * 0.4f, Alpha: Main.rand.Next(255), Scale: Main.rand.NextFloat(1.5f) * 0.85f);
                        }
                    }

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
                                NatureWeaponHandler.GetNatureDamage(item, Player),
                                Player.GetTotalKnockback(DruidClass.Nature).ApplyTo(item.knockBack),
                                Player.whoAmI,
                                (int)(18 + count2 * 2f));
                        }
                    }

                    _onGround = true;
                }

                return;
            }

            _speedBeforeGround = Player.velocity;
            if ((Player.gravDir == 1 && Player.velocity.Y > 9.5f) || (Player.gravDir == -1 && Player.velocity.Y < -9.5f))
                _fallLength++;
            else _fallLength = 0;
            _onGround = false;
        }
    }
}