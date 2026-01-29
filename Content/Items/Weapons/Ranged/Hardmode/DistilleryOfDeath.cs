using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core;
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

using static RoA.Common.ShaderLoader;
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

    //public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
    //    position -= velocity.TurnLeft().SafeNormalize() * 9f * -player.direction;
    //    velocity = position.DirectionTo(player.GetViableMousePosition()) * velocity.Length();
    //}

    //public override Vector2? HoldoutOffset() => new Vector2(2f, -4f);

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }

    private class DistilleryOfDeath_UseProjectile : ModProjectile {
        private static byte SHOOTCOUNTPERTYPE => 15;

        public override string Texture => ItemLoader.GetItem(ModContent.ItemType<DistilleryOfDeath>()).Texture;

        private static Asset<Texture2D> _fillTexture1 = null!,
                                        _fillTexture2 = null!;

        private bool _killNextFrame;
        private float _extraRotation;
        private float _gustTypeValue3;
        private float _delay = 1f;

        public ref float ShootValue => ref Projectile.localAI[0];
        public ref float SpawnValue => ref Projectile.localAI[1];
        public ref float ShootCount => ref Projectile.localAI[2];

        public ref float GustTypeValue => ref Projectile.ai[0];
        public ref float GustTypeValue2 => ref Projectile.ai[1];
        public ref float DelayValue => ref Projectile.ai[2];

        public GustType CurrentGustType {
            get => (GustType)GustTypeValue;
            set => GustTypeValue = (float)value;
        }

        public GustType NextGustType {
            get => (GustType)GustTypeValue2;
            set => GustTypeValue2 = (float)value;
        }

        public GustType NextNextGustType {
            get => (GustType)_gustTypeValue3;
            set => _gustTypeValue3 = (float)value;
        }

        public bool CanShoot_Override => DelayValue <= 0f;
        public float ShootProgress => !CanShoot_Override ? 1f : ShootValue / SHOOTCOUNTPERTYPE;
        public float ShootProgress2 => !CanShoot_Override ? 1f : (float)Projectile.GetOwnerAsPlayer().GetCommon().DistilleryOfDeathShootCount / SHOOTCOUNTPERTYPE;
        public float ShootProgress3 => (float)Projectile.GetOwnerAsPlayer().GetCommon().DistilleryOfDeathShootCount / SHOOTCOUNTPERTYPE;
        public float DelayProgress => 1f - Utils.GetLerpValue(0.5f, 0f, DelayValue / _delay, true);

        public override void SetStaticDefaults() {
            if (Main.dedServ) {
                return;
            }

            _fillTexture1 = ModContent.Request<Texture2D>(Texture + "_Fill1");
            _fillTexture2 = ModContent.Request<Texture2D>(Texture + "_Fill2");
        }

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

                player.GetCommon().DistilleryOfDeathLastShootType_Current = CurrentGustType;
                player.GetCommon().DistilleryOfDeathLastShootType_Next = NextGustType;
                player.GetCommon().DistilleryOfDeathLastShootType_Next_Next = NextNextGustType;
            }

            int shootCount = SHOOTCOUNTPERTYPE;

            float scale = Projectile.scale;
            ref Vector2 velocity = ref Projectile.velocity;
            Vector2 vector21 = Main.player[owner].GetPlayerCorePoint();
            if (Main.myPlayer == owner) {
                if (Main.player[owner].channel) {

                }
                else if (CanShoot_Override) {
                    _killNextFrame = true;

                    if (player.GetCommon().DistilleryOfDeathShootCount >= shootCount) {
                        ChangeType();
                    }
                }
            }

            if (SpawnValue == 0f) {
                SpawnValue = 1f;

                CurrentGustType = player.GetCommon().DistilleryOfDeathLastShootType_Current;
                NextGustType = player.GetCommon().DistilleryOfDeathLastShootType_Next;
                NextNextGustType = player.GetCommon().DistilleryOfDeathLastShootType_Next_Next;

                if (CurrentGustType == NextGustType) {
                    ChangeType();
                }

                Direct();
            }

            velocity.X *= 1f + (float)Main.rand.Next(-3, 4) * 0.01f;

            DelayValue = Helper.Approach(DelayValue, 0f, 1f);

            player.GetCommon().DistilleryOfDeathLastShootType_Current = CurrentGustType;

            if (velocity.X > 0f)
                Main.player[owner].ChangeDir(1);
            else if (velocity.X < 0f)
                Main.player[owner].ChangeDir(-1);

            int useTime = player.itemAnimationMax;

            if (ShootCount > 0 && CanShoot_Override) {
                float shootProgress = 1f - ShootProgress2;
                shootProgress = Ease.QuartOut(shootProgress);
                float maxRotation = 0.025f * shootProgress;
                float extraRotation = Helper.Wave(-maxRotation, maxRotation, 10f, Projectile.whoAmI);
                _extraRotation = Utils.AngleLerp(_extraRotation, extraRotation, 0.25f);
            }

            if (CanShoot_Override && ShootValue++ >= useTime) {
                ShootValue = 0;

                Direct();

                SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);

                if (player.IsLocal()) {
                    Vector2 velocity2 = Projectile.velocity;
                    Vector2 position2 = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + _extraRotation) * 5f;
                    position2 -= velocity.TurnLeft().SafeNormalize() * 2f * -player.direction * player.gravDir;
                    velocity2 = velocity2.RotatedBy(MathHelper.PiOver4 * 0.075f * Main.rand.NextFloatDirection());
                    ProjectileUtils.SpawnPlayerOwnedProjectile<DistilleryOfDeathGust>(new ProjectileUtils.SpawnProjectileArgs(player, Projectile.GetSource_ReleaseEntity()) {
                        Position = position2,
                        Velocity = velocity2,
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack,
                        AI0 = (float)CurrentGustType
                    });
                }

                ShootCount++;
                player.GetCommon().DistilleryOfDeathShootCount++;
                if (ShootCount >= shootCount || player.GetCommon().DistilleryOfDeathShootCount >= shootCount) {
                    ShootCount = 0;

                    player.GetCommon().DistilleryOfDeathShootCount = 0;

                    DelayValue = _delay = useTime * 5;

                    ChangeType();
                }
            }

            player.SetDummyItemTime(2);

            Projectile.spriteDirection = Projectile.direction;
            Main.player[owner].ChangeDir(Projectile.direction);
            Main.player[owner].heldProj = Projectile.whoAmI;
            Main.player[owner].SetDummyItemTime(2);
            Projectile.position.X = vector21.X - (float)(Projectile.width / 2);
            Projectile.position.Y = vector21.Y - (float)(Projectile.height / 2);
            Projectile.Center = Utils.Floor(Projectile.Center);
            Projectile.position -= velocity.SafeNormalize() * 12f;
            Projectile.position -= velocity.TurnLeft().SafeNormalize() * 4f * -player.direction * player.gravDir;

            Vector2 position = Projectile.Center + Projectile.velocity.SafeNormalize() * 60f - velocity.TurnLeft().SafeNormalize() * 2f * -player.direction * player.gravDir;
            if (Main.rand.NextChance(DelayProgress)) {
                if (Main.rand.NextBool(5)) {
                    Dust.NewDustPerfect(position + Main.rand.RandomPointInArea(4f), ModContent.DustType<Dusts.Smoke2>(),
                        Vector2.UnitY * -2f + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(1f, 2f),
                        0,
                        Color.Lerp(Color.Black, Color.Brown, 0.5f * Main.rand.NextFloat()) * 0.125f,
                        Main.rand.NextFloat(0.4f, 0.6f) * 1f);
                }
            }
        }

        private void ChangeType() {
            int owner = Projectile.owner;
            Player player = Main.player[owner];
            if (player.IsLocal()) {
                CurrentGustType = NextGustType;
                NextGustType = NextNextGustType;
                NextNextGustType = Main.rand.GetRandomEnumValue<GustType>(1);
                while (NextNextGustType == NextGustType ||
                       NextNextGustType == CurrentGustType) {
                    NextNextGustType = Main.rand.GetRandomEnumValue<GustType>(1);
                }
                Projectile.netUpdate = true;
            }
        }

        private void Direct() {
            int owner = Projectile.owner;
            Player player = Main.player[owner];

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
            }

            Projectile.rotation = (float)(Math.Atan2(velocity.Y, velocity.X) + 1.5700000524520874) - MathHelper.PiOver2;
            if (Main.player[owner].direction == 1)
                Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);
            else
                Main.player[owner].itemRotation = (float)Math.Atan2(velocity.Y * (float)Projectile.direction, velocity.X * (float)Projectile.direction);
        }

        public override bool PreDraw(ref Color lightColor) {
            if (SpawnValue == 0f) {
                return false;
            }

            var texture = TextureAssets.Projectile[Type].Value;

            Player player = Projectile.GetOwnerAsPlayer();

            var pos = Projectile.Center - Main.screenPosition;
            var effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically : Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            if (player.gravDir < 0) {
                effects = (Projectile.spriteDirection == -1) ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            }

            float rotation = Projectile.rotation + _extraRotation;
            Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor), rotation, texture.Frame().Left(), Projectile.scale, effects);

            if (_fillTexture1.IsLoaded && _fillTexture2.IsLoaded) {
                SpriteBatch batch = Main.spriteBatch;
                void drawPart(Color baseColorBASE, bool current = true) {
                    SpriteBatchSnapshot snapshot = batch.CaptureSnapshot();

                    texture = current ? _fillTexture1.Value : _fillTexture2.Value;
                    Vector2 position = Projectile.Center - Main.screenPosition;
                    if (current) {
                        position += new Vector2(30, 10 * -player.direction * player.gravDir).RotatedBy(rotation);
                    }
                    else {
                        position += new Vector2(20, 6 * -player.direction * player.gravDir).RotatedBy(rotation);
                    }
                    Rectangle clip = texture.Bounds;
                    Vector2 origin = texture.Frame().Left();
                    SpriteEffects flip = effects;
                    Color baseColor = baseColorBASE;
                    float scale = Projectile.scale;
                    float shootOpacity = 1f - ShootProgress2;
                    if (!current) {
                        shootOpacity = 1f;
                    }
                    float opacity = 0.5f * Projectile.Opacity;

                    if (current) {
                        VerticalAppearanceShader.Reset();
                        VerticalAppearanceShader.Progress = Ease.QuadInOut(ShootProgress2);
                        if (DelayProgress > 0f) {
                            VerticalAppearanceShader.Progress = DelayProgress;
                        }
                        VerticalAppearanceShader.FromDown = false;
                    }

                    for (float num11 = 0f; num11 < 1f; num11 += 1f / 3f) {
                        float num12 = (TimeSystem.TimeForVisualEffects + Projectile.whoAmI) % 2f / 1f * Projectile.direction;
                        Color color = Main.hslToRgb((num12 + num11) % 1f, 1f, 0.5f).MultiplyRGB(baseColor);
                        color.A = 0;
                        color *= 0.5f;
                        for (int j = 0; j < 2; j++) {
                            for (int k = 0; k < 2; k++) {
                                Vector2 drawPosition = position + ((num12 + num11) * ((float)Math.PI * 2f)).ToRotationVector2() * 1f;

                                Color drawColor = Color.Lerp(baseColor, color, 0.5f) * opacity;
                                if (current) {
                                    batch.Begin(snapshot with { sortMode = SpriteSortMode.Immediate }, true);
                                    VerticalAppearanceShader.DrawColor = drawColor;
                                    VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
                                }
                                batch.Draw(texture, drawPosition, clip, drawColor, rotation, origin, scale, flip, 0f);
                                if (current) {
                                    batch.Begin(snapshot, true);
                                }
                            }
                        }
                    }
                    baseColor.A = 100;
                    baseColor *= 0.75f;

                    Color drawColor2 = baseColor * opacity;
                    if (current) {
                        batch.Begin(snapshot with { sortMode = SpriteSortMode.Immediate }, true);
                        VerticalAppearanceShader.DrawColor = drawColor2;
                        VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
                    }
                    batch.Draw(texture, position, clip, drawColor2, rotation, origin, scale, flip, 0f);
                    if (current) {
                        batch.Begin(snapshot, true);
                    }
                }

                Color lightColor2 = Projectile.GetAlpha(lightColor);
                drawPart(lightColor2.MultiplyRGB(GetColorPerType(CurrentGustType)), true);
                Color color2 = Color.Lerp(lightColor2.MultiplyRGB(GetColorPerType(NextGustType)), lightColor2.MultiplyRGB(GetColorPerType(NextNextGustType)), Ease.QuadIn(ShootProgress3));
                drawPart(color2, false);
            }

            return false;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;
        public override bool ShouldUpdatePosition() => false;
    }
}
