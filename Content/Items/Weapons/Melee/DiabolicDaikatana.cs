using Humanizer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;


namespace RoA.Content.Items.Weapons.Melee;

sealed class DiabolicDaikatana : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 44; int height = 46;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 30;

        Item.autoReuse = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 18;
        Item.knockBack = 4f;

        Item.value = Item.buyPrice(gold: 1, silver: 60);
        Item.rare = ItemRarityID.Orange;
        //Item.UseSound = SoundID.Item1;

        Item.useTurn = false;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        //Item.noMelee = true;
        //Item.noUseGraphic = true;
        //Item.channel = true;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;

        Item.shoot = ModContent.ProjectileType<DiabolicDaikatanaProj>();
        Item.shootSpeed = 1f;
    }

    //public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
    //    return base.Shoot(player, source, position, velocity, type, damage, knockback);
    //}

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Muramasa)
            .AddIngredient(ItemID.HellstoneBar, 10)
            .AddTile(TileID.DemonAltar)
            .Register();
    }

    private class DaikatanaLayer : PlayerDrawLayer {
        private static Asset<Texture2D> _daikatanaTexture;
        private Vector2 _daikatanaPosition;

        public override void Load() => _daikatanaTexture = ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMeleeTextures + nameof(DiabolicDaikatana));

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].type == ModContent.ItemType<DiabolicDaikatana>();

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead) {
                return;
            }

            if (drawInfo.drawPlayer.ItemAnimationActive) {
                return;
            }

            Player player = drawInfo.drawPlayer;
            Texture2D texture = _daikatanaTexture.Value;
            if (player.direction < 0) {
                _daikatanaPosition.X = drawInfo.Position.X + (float)player.width / 2f + texture.Width / 2 * 1.35f - 7;
                _daikatanaPosition.Y = drawInfo.Position.Y + (float)player.height / 2f - texture.Height / 2.8f - 5;
            }
            else {
                _daikatanaPosition.X = drawInfo.Position.X + (float)player.width / 2f - texture.Width * 1.35f + 6;
                _daikatanaPosition.Y = drawInfo.Position.Y + (float)player.height / 2f + texture.Height / 2.8f - 5;
            }
            _daikatanaPosition += player.Size / 2f;
            _daikatanaPosition.X += player.width;
            _daikatanaPosition.X += 8 * -player.direction;
            _daikatanaPosition.Y += 8;
            Vector2 position = _daikatanaPosition - Main.screenPosition;
            SpriteEffects effects = player.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color color = Lighting.GetColor((int)((player.position.X + (float)player.width / 2f) / 16f), (int)((player.position.Y - 4f - (float)texture.Height / 2f) / 16f));
            DrawData drawData = new(texture,
                                    position,
                                    new Rectangle?(new Rectangle(0, 0, 44, 46)),
                                    color, 
                                    player.direction > 0 ? -2.3f : 2.3f,
                                    Vector2.Zero, 1f, effects);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}

// adapted aequus 
sealed class DiabolicDaikatanaProj : ModProjectile {
    private float _armRotation;
    private int _swingTime;
    private int _swingTimeMax;
    private Vector2 _angleVector;
    private int _swordHeight = 100;
    private int _swordWidth = 30;

    public Vector2 BaseAngleVector { get; private set; }
    public Vector2 AngleVector { get => _angleVector; private set => _angleVector = Vector2.Normalize(value); }

    public float Progress => Math.Clamp(1f - _swingTime / (float)_swingTimeMax, 0f, 1f);

    public override string Texture => ResourceManager.ItemsWeaponsMeleeTextures + nameof(DiabolicDaikatana);

    public override void SetDefaults() {
        Projectile.localNPCHitCooldown = 500;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.ownerHitCheck = true;
        Projectile.aiStyle = -1;
        Projectile.width = Projectile.height = 90;

        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;

        Projectile.tileCollide = false;

        Projectile.noEnchantments = true;

        _swordHeight = 60;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 center = Main.player[Projectile.owner].Center;
        bool flag = Progress > 0.375f && Progress < 0.575f;
        return flag && Helper.DeathrayHitbox(center - AngleVector * 20f, center + AngleVector * (75 * Projectile.scale/* * baseSwordScale*/), targetHitbox, _swordWidth * Projectile.scale/* * baseSwordScale*/);
    }

    public override void AI() {
        Projectile.extraUpdates = 6;
        Player player = Main.player[Projectile.owner];
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            _swingTimeMax = player.itemAnimationMax;

            player.itemTime = _swingTimeMax + 1;
            player.itemTimeMax = _swingTimeMax + 1;
            player.itemAnimation = _swingTimeMax + 1;
            player.itemAnimationMax = _swingTimeMax + 1;

            _swingTimeMax *= Projectile.extraUpdates + 1;
            _swingTime = _swingTimeMax;
            Projectile.timeLeft = _swingTimeMax + 2;
        }
        player.heldProj = Projectile.whoAmI;
        if (!player.frozen && !player.stoned) {
            float progress = Progress;
            Projectile.direction = player.direction;
            BaseAngleVector = new Vector2(0.88f * Projectile.direction, 0.47f);
            InterpolateSword(progress, out var angleVector, out float swingProgress, out float scale);
            SetArmRotation(player, swingProgress);
            var arm = Main.GetPlayerArmPosition(Projectile);
            AngleVector = angleVector;
            Projectile.position = arm + AngleVector * _swordHeight / 2f;
            Projectile.position.X -= Projectile.width / 2f;
            Projectile.position.Y -= Projectile.height / 2f;
            Projectile.rotation = angleVector.ToRotation();
            bool flag = Projectile.timeLeft <= _swingTimeMax * 0.6f;
            if (flag && Main.myPlayer == Projectile.owner && Projectile.ai[0] == 0f) {
                Projectile.ai[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Vector2 velocity = Helper.VelocityToPoint(player.MountedCenter, Main.MouseWorld, 12f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + velocity * 2f, velocity, ModContent.ProjectileType<JudgementCut>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                Projectile.netUpdate = true;
            }
            if (Progress > 0.375f && Progress < 0.575f && Projectile.numUpdates == -1) {
                int amt = 4;
                for (int i = 0; i < amt; i++) {
                    Vector2 velocity = AngleVector.RotatedBy(MathHelper.PiOver2 * -Projectile.direction) * Main.rand.NextFloat(2f, 8f);
                    int type = ModContent.DustType<DaikatanaDust>();
                    Dust dust = Dust.NewDustPerfect(player.Center + AngleVector * Main.rand.NextFloat(20f, 80f * Projectile.scale), type, velocity, Scale: Main.rand.NextFloat(0.45f, 0.7f) * 1.25f * Main.rand.NextFloat(1.25f, 1.75f));
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    dust.scale *= Projectile.scale;
                    dust.fadeIn = dust.scale + 0.2f;
                    dust.noGravity = true;
                }
                for (int i = 0; i < amt * 2; i++) {
                    Rectangle rectangle = Utils.CenteredRectangle(player.Center + AngleVector * Main.rand.NextFloat(20f, 80f * Projectile.scale), new Vector2(20f, 80f * Projectile.scale));
                    Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
                }
            }
        }
        else {
            Projectile.timeLeft++;
        }
        _swingTime--;
        Projectile.hide = true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Color drawColor = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
        GetSwordDrawInfo(out var texture, out var handPosition, out var rotationOffset, out var origin, out var effects);
        DrawSword(texture, handPosition - Vector2.UnitX * (Progress > 0.5f ? (Main.player[Projectile.owner].direction != 1 ? 4 : -2) : 0), null, drawColor, rotationOffset, origin, effects);

        return false;
    }

    private void GetSwordDrawInfo(out Texture2D texture, out Vector2 handPosition, out float rotationOffset, out Vector2 origin, out SpriteEffects effects) {
        texture = TextureAssets.Projectile[Type].Value;
        handPosition = Main.GetPlayerArmPosition(Projectile) - new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
        rotationOffset = 0f;
        int direction = 1;
        if (AngleVector.X < 0f) {
            direction = -1;
        }
        else if (AngleVector.X > 0f) {
            direction = 1;
        }
        if (Main.player[Projectile.owner].direction == direction * -Main.player[Projectile.owner].direction) {
            effects = SpriteEffects.None;
            origin.X = 0f;
            origin.Y = texture.Height;
            rotationOffset += MathHelper.PiOver4;
        }
        else {
            effects = SpriteEffects.FlipHorizontally;
            origin.X = texture.Width;
            origin.Y = texture.Height;
            rotationOffset += MathHelper.PiOver4 * 3f;
        }
    }

    private void DrawSword(Texture2D texture, Vector2 handPosition, Rectangle? frame, Color color, float rotationOffset, Vector2 origin, SpriteEffects effects) {
        Main.EntitySpriteDraw(
            texture,
            handPosition - Main.screenPosition + AngleVector,
            frame ?? null,
            color,
            Projectile.rotation + rotationOffset,
            origin,
            Projectile.scale,
            effects
        );
    }

    private void InterpolateSword(float progress, out Vector2 angleVector, out float swingProgress, out float scale) {
        swingProgress = progress >= 0.5f ? 0.5f + (0.5f - (float)Math.Pow(2f, 30f * (0.5f - (progress - 0.5f)) - 15f) / 2f) : (float)Math.Pow(2f, 30f * progress - 15f) / 2f;
        angleVector = BaseAngleVector.RotatedBy((swingProgress * (MathHelper.Pi * 1.75f) - MathHelper.PiOver2 * 1.75f) * -Projectile.direction * (0.9f + 0.1f));
        scale = 1f;
    }

    private void SetArmRotation(Player player, float progress) {
        var diff = Main.player[Projectile.owner].MountedCenter - Projectile.Center;
        if (Math.Sign(diff.X) == -player.direction) {
            var v = diff;
            v.X = Math.Abs(diff.X);
            _armRotation = v.ToRotation();
        }
        else if (_armRotation < 0.1f) {
            if (Projectile.direction * (progress >= 0.5f ? -1 : 1) * -player.direction == -1) {
                _armRotation = -1.11f;
            }
            else {
                _armRotation = 1.11f;
            }
        }
        if (_armRotation > 1.1f) {
            player.bodyFrame.Y = 56;
        }
        else if (_armRotation > 0.5f) {
            player.bodyFrame.Y = 56 * 2;
        }
        else if (_armRotation < -0.5f) {
            player.bodyFrame.Y = 56 * 4;
        }
        else {
            player.bodyFrame.Y = 56 * 3;
        }
    }
}