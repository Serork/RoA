using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Ranged.DistilleryOfDeathGust;

namespace RoA.Content.Items.Weapons.Ranged.Hardmode;

sealed class DistilleryOfDeath : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(58, 38);
        Item.DefaultToRangedWeapon(ModContent.ProjectileType<DistilleryOfDeath_UseProjectile>(), AmmoID.None, 10, 5f);
        Item.knockBack = 6.5f;
        Item.UseSound = null;
        Item.damage = 14;
        Item.value = Item.buyPrice(0, 35);
        Item.rare = 3;

        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.channel = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }

    private class DistilleryOfDeath_UseProjectile : ModProjectile {
        public override string Texture => ItemLoader.GetItem(ModContent.ItemType<DistilleryOfDeath>()).Texture;

        private bool _killNextFrame;

        public ref float ShootValue => ref Projectile.localAI[0];
        public ref float SpawnValue => ref Projectile.localAI[1];

        public ref float GustTypeValue => ref Projectile.ai[0];
        public ref float GustTypeValue2 => ref Projectile.ai[1];
        public ref float DelayValue => ref Projectile.ai[2];

        public GustType CurrentGustType {
            get => (GustType)GustTypeValue;
            set => GustTypeValue = (float)value;
        }

        public bool CanShoot_Override => DelayValue <= 0f;

        public override void SetDefaults() {
            Projectile.SetSizeValues(10);
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI() {
            int owner = Projectile.owner;
            Player player = Main.player[owner];

            if (_killNextFrame) {
                Projectile.Kill();

                player.GetCommon().DistilleryOfDeathLastShootType = CurrentGustType;
            }

            float scale = Projectile.scale;
            ref Vector2 velocity = ref Projectile.velocity;
            Vector2 vector21 = Main.player[owner].GetPlayerCorePoint();
            if (Main.myPlayer == owner) {
                if (Main.player[owner].channel) {
                    float num178 = Main.player[owner].inventory[Main.player[owner].selectedItem].shootSpeed * scale;
                    Vector2 vector22 = vector21;
                    float num179 = (float)Main.mouseX + Main.screenPosition.X - vector22.X;
                    float num180 = (float)Main.mouseY + Main.screenPosition.Y - vector22.Y;
                    if (Main.player[owner].gravDir == -1f)
                        num180 = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector22.Y;

                    float num181 = (float)Math.Sqrt(num179 * num179 + num180 * num180);
                    num181 = (float)Math.Sqrt(num179 * num179 + num180 * num180);
                    num181 = num178 / num181;
                    num179 *= num181;
                    num180 *= num181;
                    if (num179 != velocity.X || num180 != velocity.Y)
                        Projectile.netUpdate = true;

                    velocity.X = num179;
                    velocity.Y = num180;
                }
                else {
                    _killNextFrame = true;

                    if (player.IsLocal()) {
                        var previous = CurrentGustType;
                        while (CurrentGustType == previous) {
                            CurrentGustType = Main.rand.GetRandomEnumValue<GustType>(1);
                        }
                        Projectile.netUpdate = true;
                    }
                }
            }

            if (SpawnValue == 0f) {
                SpawnValue = 1f;

                CurrentGustType = player.GetCommon().DistilleryOfDeathLastShootType;
            }

            if (velocity.X > 0f)
                Main.player[owner].ChangeDir(1);
            else if (velocity.X < 0f)
                Main.player[owner].ChangeDir(-1);

            Projectile.spriteDirection = Projectile.direction;
            Main.player[owner].ChangeDir(Projectile.direction);
            Main.player[owner].heldProj = Projectile.whoAmI;
            Main.player[owner].SetDummyItemTime(2);
            Projectile.position.X = vector21.X - (float)(Projectile.width / 2);
            Projectile.position.Y = vector21.Y - (float)(Projectile.height / 2);
            Projectile.rotation = (float)(Math.Atan2(velocity.Y, velocity.X) + 1.5700000524520874) - MathHelper.PiOver2;
            if (Main.player[owner].direction == 1)
                Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);
            else
                Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);

            velocity.X *= 1f + (float)Main.rand.Next(-3, 4) * 0.01f;

            DelayValue = Helper.Approach(DelayValue, 0f, 1f);

            player.GetCommon().DistilleryOfDeathLastShootType = CurrentGustType;

            int useTime = player.itemAnimationMax;
            if (CanShoot_Override && ShootValue++ >= useTime) {
                ShootValue = 0;

                SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);

                if (player.IsLocal()) {
                    ProjectileUtils.SpawnPlayerOwnedProjectile<DistilleryOfDeathGust>(new ProjectileUtils.SpawnProjectileArgs(player, Projectile.GetSource_ReleaseEntity()) {
                        Position = Projectile.Center,
                        Velocity = Projectile.velocity,
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack,
                        AI0 = (float)CurrentGustType
                    });
                }

                GustTypeValue2++;
                int shootCount = 5;
                if (GustTypeValue2 >= shootCount) {
                    GustTypeValue2 = 0;

                    DelayValue = useTime * 3;

                    if (player.IsLocal()) {
                        var previous = CurrentGustType;
                        while (CurrentGustType == previous) {
                            CurrentGustType = Main.rand.GetRandomEnumValue<GustType>(1);
                        }
                        Projectile.netUpdate = true;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor) {
            var texture = TextureAssets.Projectile[Type].Value;

            var pos = Projectile.Center - Main.screenPosition;
            var effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically : Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Frame().Left(), Projectile.scale, effects);

            return false;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;
        public override bool ShouldUpdatePosition() => false;
    }
}
