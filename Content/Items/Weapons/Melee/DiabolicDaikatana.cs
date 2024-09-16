using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

using static tModPorter.ProgressUpdate;

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
        //Item.holdStyle = ItemHoldStyleID.HoldHeavy;

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
}

sealed class DiabolicDaikatanaProj : ModProjectile {
    private float _armRotation;
    private int _swingTime;
    private int _swingTimeMax;
    private Vector2 _angleVector;

    public int swordHeight = 100;
    public int swordWidth = 30;

    public Vector2 BaseAngleVector { get; private set; }
    public Vector2 AngleVector { get => _angleVector; set => _angleVector = Vector2.Normalize(value); }

    public float AnimProgress => Math.Clamp(1f - _swingTime / (float)_swingTimeMax, 0f, 1f);

    public override string Texture => ResourceManager.ItemsWeaponsMeleeTextures + nameof(DiabolicDaikatana);

    public override void SetDefaults() {
        Projectile.localNPCHitCooldown = 500;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.ownerHitCheck = true;
        Projectile.aiStyle = -1;
        Projectile.width = 90;
        Projectile.height = 90;

        Projectile.tileCollide = false;

        swordHeight = 60;
    }

    public override void AI() {
        Projectile.extraUpdates = Projectile.timeLeft <= _swingTimeMax * 0.6f ? 8 : 10;
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
            float progress = AnimProgress;
            Projectile.direction = player.direction;
            BaseAngleVector = new Vector2(0.88f * Projectile.direction, 0.47f);
            InterpolateSword(progress, out var angleVector, out float swingProgress, out float scale);
            SetArmRotation(player, swingProgress);
            var arm = Main.GetPlayerArmPosition(Projectile);
            AngleVector = angleVector;
            Projectile.position = arm + AngleVector * swordHeight / 2f;
            Projectile.position.X -= Projectile.width / 2f;
            Projectile.position.Y -= Projectile.height / 2f;
            Projectile.rotation = angleVector.ToRotation();
            if (Projectile.timeLeft <= _swingTimeMax * 0.6f && Main.myPlayer == Projectile.owner && Projectile.ai[0] == 0f) {
                Projectile.ai[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Vector2 velocity = Helper.VelocityToPoint(player.MountedCenter, Main.MouseWorld, 12f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + velocity * 2f, velocity, ModContent.ProjectileType<JudgementCut>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                Projectile.netUpdate = true;
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
        DrawSword(texture, handPosition, null, drawColor, rotationOffset, origin, effects);

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
        swingProgress = progress >= 0.5f ? 0.5f + (0.5f - MathF.Pow(2f, 20f * (0.5f - (progress - 0.5f)) - 10f) / 2f) : MathF.Pow(2f, 20f * progress - 10f) / 2f;
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