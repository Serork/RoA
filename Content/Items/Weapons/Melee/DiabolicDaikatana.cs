using Humanizer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.GlowMasks;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly.Melee;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

[AutoloadGlowMask(0, 0, 178, 178, shouldApplyItemAlpha: true)]
sealed class DiabolicDaikatana : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        PrefixLegacy.ItemSets.SwordsHammersAxesPicks[Type] = true;
    }

    public override void SetDefaults() {
        int width = 44; int height = 46;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 38;

        Item.autoReuse = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 28;
        Item.knockBack = 5f;

        Item.crit = 6;

        Item.rare = ItemRarityID.Orange;

        Item.useTurn = false;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;

        Item.shoot = ModContent.ProjectileType<DiabolicDaikatanaProj>();
        Item.shootSpeed = 1f;

        Item.value = Item.sellPrice(0, 2, 50, 0);
    }

    //public override void AddRecipes() {
    //    CreateRecipe()
    //        .AddIngredient(ItemID.Muramasa)
    //        .AddIngredient(ItemID.HellstoneBar, 10)
    //        .AddTile(TileID.DemonAltar)
    //        .Register();
    //}

    private class DaikatanaLayer : PlayerDrawLayer {
        private static Asset<Texture2D> _daikatanaTexture, _daikatanaTextureGlow;

        public override void SetStaticDefaults() {
            if (Main.dedServ) {
                return;
            }

            string textureName = ResourceManager.ItemsWeaponsMeleeTextures + "DiabolicDaikatanaUse";
            _daikatanaTexture = ModContent.Request<Texture2D>(textureName);
            _daikatanaTextureGlow = ModContent.Request<Texture2D>(textureName + "_Glow");
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => true;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            if (drawInfo.shadow != 0f || player.dead) {
                return;
            }

            if (player.inventory[player.selectedItem].type != ModContent.ItemType<DiabolicDaikatana>()) {
                return;
            }

            Vector2 drawPosition = drawInfo.drawPlayer.RotatedRelativePoint(drawInfo.drawPlayer.MountedCenter, true) - drawInfo.drawPlayer.Size / 2f + new Vector2(0f, -2f) + new Vector2(0f, drawInfo.drawPlayer.gfxOffY);
            drawPosition.X += 12f;
            if (player.direction == 1) {
                drawPosition.X -= 4f;
            }
            if (player.sitting.isSitting) {
                drawPosition.Y -= 4f;
            }
            if (player.itemAnimation < player.itemAnimationMax - 1 && player.itemAnimation != 0) {
                return;
            }

            float scale = player.CappedMeleeOrDruidScale();
            Texture2D texture = _daikatanaTexture.Value;
            Vector2 position = new((int)(drawPosition.X), (int)(drawPosition.Y));
            Vector2 offset = new(texture.Width / 2f * player.direction, 0f);
            if (player.direction < 0) {
                offset.Y += (int)(texture.Height * 0.8f - 2f);
                offset.X += (int)(-texture.Width * (float)player.direction);
                offset.X += 1;
            }
            offset.X -= (int)(texture.Width * 0.7f - 2f);
            offset.X -= (int)(player.width * 0.75f * player.direction);
            offset.Y += (int)(player.height / 3f);
            if (player.gravDir == -1f) {
                if (player.direction > 0) {
                    offset.X += 1;
                    offset.X -= 2;
                }
                else {
                    offset.X -= 2;
                    offset.Y -= 4;
                }
                offset.Y -= 13;
            }
            if (player.gravDir == -1f) {
            }
            position += offset * scale;
            position.Y -= (texture.Height / 2f + 8f) * (scale - 1f);
            position.X -= texture.Width / 6f * (scale - 1f) * player.direction;
            SpriteEffects effects = player.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float rotation = 2.3f + (player.gravDir == -1 ? 0.1f : 0f);
            bool gravReversed = player.gravDir == -1f;
            for (int i = 0; i < 2; i++) {
                Color color = drawInfo.itemColor;
                position = position.Floor();
                if (i != 0) {
                    texture = _daikatanaTextureGlow.Value;
                    color = Color.Lerp(Color.Blue * 0.7f, Lighting.GetColor((int)position.X / 16, (int)position.Y / 16), Lighting.Brightness((int)position.X / 16, (int)position.Y / 16));
                }
                DrawData drawData = new(texture,
                                        position - Main.screenPosition,
                                        new Rectangle((player.gravDir == -1).ToInt() * 46, 0, 46, 46),
                                        player.HeldItem.GetAlpha(color) * drawInfo.stealth * (1f - drawInfo.shadow),
                                        player.direction > 0 ? -rotation : rotation,
                                        texture.Size() / 2f,
                                        scale,
                                        effects);
                drawInfo.DrawDataCache.Add(drawData);
            }
        }
    }
}

// adapted aequus 
sealed class DiabolicDaikatanaProj : ModProjectile {
    private float _armRotation;
    private int _swingTime;
    private int _swingTimeMax;
    private Vector2 _angleVector;
    private int _swordHeight = 60;
    private int _swordWidth = 30;

    public Vector2 BaseAngleVector { get; private set; }
    public Vector2 AngleVector { get => _angleVector; private set => _angleVector = Vector2.Normalize(value); }

    public float Progress => Math.Clamp(1f - _swingTime / (float)_swingTimeMax, 0f, 1f);

    public override string Texture => ResourceManager.ItemsWeaponsMeleeTextures + "DiabolicDaikatanaUse";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
    }

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
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Vector2 center = Main.player[Projectile.owner].Center;
        bool flag = Progress > 0.375f && Progress < 0.575f;
        return flag && Helper.DeathrayHitbox(center - AngleVector * 20f, center + AngleVector * (75 * Projectile.scale/* * baseSwordScale*/), targetHitbox, _swordWidth * Projectile.scale/* * baseSwordScale*/);
    }

    public override bool? CanCutTiles() => Progress > 0.375f && Progress < 0.575f;

    public static Vector2 GetPlayerArmPosition(Projectile proj) {
        Player player = Main.player[proj.owner];
        Vector2 vector = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
        if (player.direction != 1)
            vector.X = (float)player.bodyFrame.Width - vector.X;

        if (player.gravDir != 1f)
            vector.Y = (float)player.bodyFrame.Height - vector.Y;

        vector -= new Vector2(player.bodyFrame.Width - player.width, player.bodyFrame.Height - 42) / 2f;
        Vector2 pos = player.MountedCenter - new Vector2(20f, 42f) / 2f + vector + Vector2.UnitY * player.gfxOffY;
        if (player.mount.Active && player.mount.Type == 52) {
            pos.Y -= player.mount.PlayerOffsetHitbox;
            pos += new Vector2(12 * player.direction, -12f);
        }

        return player.RotatedRelativePoint(pos, true);
    }

    public override void AI() {
        Projectile.extraUpdates = 5;
        Player player = Main.player[Projectile.owner];
        Projectile.scale = Projectile.localAI[2];
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

            if (Projectile.localAI[2] == 0f) {
                Projectile.localAI[2] = 1f;
                float scale = Main.player[Projectile.owner].CappedMeleeOrDruidScale();
                if (scale != 1f) {
                    Projectile.localAI[2] *= scale;
                    Projectile.scale *= scale;
                }
            }
        }
        player.heldProj = Projectile.whoAmI;
        if (!player.frozen && !player.stoned) {
            float progress = Progress;
            Projectile.direction = player.direction;
            BaseAngleVector = new Vector2(0.88f * Projectile.direction, 0.47f);
            InterpolateSword(progress, out var angleVector, out float swingProgress, out float scale);
            SetArmRotation(player, swingProgress);
            var arm = player.RotatedRelativePoint(player.MountedCenter, true);
            AngleVector = angleVector;
            Projectile.position = arm + AngleVector * _swordHeight / 2f;
            Projectile.position.X -= Projectile.width / 2f;
            Projectile.position.Y -= Projectile.height / 2f;
            Projectile.rotation = angleVector.ToRotation() * player.gravDir;
            bool flag = Projectile.timeLeft <= _swingTimeMax * 0.6f;
            if (flag && Main.myPlayer == Projectile.owner && Projectile.ai[0] == 0f) {
                Projectile.ai[0] = 1f;
                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "SwordSliceMagic") { Volume = 1.5f }, Projectile.Center);
                Vector2 velocity = Helper.VelocityToPoint(player.MountedCenter, Main.MouseWorld, 12f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + velocity * 2f, velocity, ModContent.ProjectileType<JudgementCut>(), Projectile.damage / 2, Projectile.knockBack / 2.5f, Projectile.owner);
                Projectile.netUpdate = true;
            }
            if (Progress > 0.375f && Progress < 0.575f && Projectile.numUpdates == -1) {
                float offset = player.gravDir == 1 ? 0f : (-MathHelper.PiOver2 * player.direction);
                angleVector = BaseAngleVector.RotatedBy((swingProgress * (MathHelper.Pi * 1.75f) - MathHelper.PiOver2 * 1.75f) * -Projectile.direction * (0.9f + 0.1f) + offset);
                for (int i = 0; i < 4; i++) {
                    Vector2 velocity = angleVector.RotatedBy(MathHelper.PiOver2 * -Projectile.direction * player.gravDir) * Main.rand.NextFloat(2f, 8f);
                    int type = ModContent.DustType<DaikatanaDust>();
                    Dust dust = Dust.NewDustPerfect(player.Center + angleVector * Main.rand.NextFloat(20f, 80f * Projectile.scale), type, velocity, Scale: Main.rand.NextFloat(0.45f, 0.7f) * 1.25f * Main.rand.NextFloat(1.25f, 1.75f));
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    dust.scale *= Projectile.scale;
                    dust.fadeIn = dust.scale + 0.2f;
                    dust.noGravity = true;
                }
                for (int i = 0; i < 8; i++) {
                    Rectangle rectangle = Utils.CenteredRectangle(player.Center + angleVector * Main.rand.NextFloat(20f, 80f * Projectile.scale), new Vector2(20f, 80f * Projectile.scale));
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
        GetSwordDrawInfo(out var texture, out var handPosition, out var rotationOffset, out var origin, out var effects);
        handPosition += AngleVector;
        Player player = Main.player[Projectile.owner];
        handPosition += -Vector2.UnitX * (Progress > 0.5f ? (player.direction != 1 ? 4 : -2) : 0);
        for (int i = 0; i < 2; i++) {
            Color drawColor = Lighting.GetColor((int)((double)player.position.X + (double)player.width * 0.5) / 16, (int)(((double)player.position.Y + (double)player.height * 0.5) / 16.0));
            if (i != 0) {
                texture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
                drawColor = Color.Lerp(Color.Blue * 0.7f, Lighting.GetColor((int)handPosition.X / 16, (int)handPosition.Y / 16), Lighting.Brightness((int)handPosition.X / 16, (int)handPosition.Y / 16) * 0.75f);
            }
            DrawSword(texture, handPosition,
                  new Rectangle((player.gravDir == -1).ToInt() * 46, 0, 46, 46), drawColor * Projectile.Opacity, rotationOffset, origin, effects);
        }

        return false;
    }

    private void GetSwordDrawInfo(out Texture2D texture, out Vector2 handPosition, out float rotationOffset, out Vector2 origin, out SpriteEffects effects) {
        texture = TextureAssets.Projectile[Type].Value;
        Player player = Main.player[Projectile.owner];
        handPosition = GetPlayerArmPosition(Projectile) - new Vector2(0f, player.gfxOffY);
        if (player.gravDir == -1f) {
            handPosition.Y -= 1;
        }
        if (Progress <= 0.5f) {
            handPosition += new Vector2(11f * player.direction, 1f).Floor();
            if (player.direction > 0) {
                handPosition.X += 1;
            }
            else {
                handPosition.Y -= 1;
            }
        }
        if (player.gravDir == -1) {
            if (player.direction == -1) {
                handPosition.Y -= 3;
                //handPosition.X -= 2;
            }
            else {
            }
            //handPosition.X -= 4;
            //handPosition -26;
        }
        rotationOffset = 0f;
        int direction = 1;
        if (AngleVector.X < 0f) {
            direction = -1;
        }
        else if (AngleVector.X > 0f) {
            direction = 1;
        }
        if (player.direction == direction * -player.direction) {
            effects = SpriteEffects.None;
            origin.X = 0f;
            origin.Y = 46;
            rotationOffset += MathHelper.PiOver4;
        }
        else {
            effects = SpriteEffects.FlipHorizontally;
            origin.X = 44;
            origin.Y = 46;
            rotationOffset += MathHelper.PiOver4 * 3f;
        }
    }

    private void DrawSword(Texture2D texture, Vector2 handPosition, Rectangle? frame, Color color, float rotationOffset, Vector2 origin, SpriteEffects effects) {
        Main.EntitySpriteDraw(
            texture,
            handPosition - Main.screenPosition,
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
        Player player = Main.player[Projectile.owner];
        angleVector = BaseAngleVector.RotatedBy((swingProgress * (MathHelper.Pi * 1.75f) - MathHelper.PiOver2 * 1.75f) * -Projectile.direction * (0.9f + 0.1f));
        scale = 1f;
    }

    private void SetArmRotation(Player player, float progress) {
        Vector2 diff = player.MountedCenter - Projectile.Center;
        if (Math.Sign(diff.X) == -player.direction) {
            Vector2 velocity = diff;
            velocity.X = Math.Abs(diff.X);
            _armRotation = velocity.ToRotation();
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