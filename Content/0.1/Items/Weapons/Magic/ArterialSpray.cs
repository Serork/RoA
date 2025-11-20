using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Common.Recipes;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Common.Players.DruidPlayerShouldersFix;
using static Terraria.Player;

namespace RoA.Content.Items.Weapons.Magic;

sealed class ArterialSpray : ModItem, IRecipeDuplicatorItem {
    ushort[] IRecipeDuplicatorItem.SourceItemTypes => [(ushort)ItemID.CrimsonRod];

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(42);

        Item.DefaultToMagicWeapon(ModContent.ProjectileType<ArterialSprayProjectile3>(), 0, 1f);

        Item.SetUsableValues(ItemUseStyleID.Swing, 20);

        Item.damage = 16;
        Item.crit = 4;

        Item.mana = 14;
        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 1, 50, 0);

        Item.noUseGraphic = true;

        Item.UseSound = new SoundStyle(ResourceManager.ItemSounds + "RazorThree") { Volume = 0.3f, Pitch = 0.1f, PitchVariance = 0.1f };
    }
}

sealed class ArterialSprayProjectile3 : ModProjectile, ProjectileHooks.IDrawLikeHeldItem, IProjectileFixShoulderWhileActive {
    private int _direction, _useTimeMax;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + "Magic/ArterialSpray_Small";

    public override void SetDefaults() {
        int width = 2; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;

        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        Projectile.aiStyle = -1;

        Projectile.ignoreWater = true;
    }

    void ProjectileHooks.IDrawLikeHeldItem.Draw(ref Color lightColor, PlayerDrawSet drawinfo) { }

    public override bool PreDraw(ref Color lightColor) {
        Player player = Main.player[Projectile.owner];
        Item heldItem = player.HeldItem;
        bool flag = _direction != 1;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 origin = new(texture.Width * 0.5f * (1 - _direction), (player.gravDir == -1f) ? 0 : texture.Height);
        int x = -(int)origin.X;
        ItemLoader.HoldoutOrigin(player, ref origin);
        Vector2 offset = new(origin.X + x, 0);
        float rotOffset = 0.785f * _direction;
        if (player.gravDir == -1f) {
            rotOffset += 1.57f * _direction;
        }
        SpriteEffects effects = flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (player.gravDir == -1f) {
            if (_direction == 1) {
                effects = SpriteEffects.FlipVertically;
            }
            else {
                effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
        }
        float progress = (float)Projectile.timeLeft / _useTimeMax;
        float f = progress < 0.5f ? progress : (1f - progress);
        Vector2 offset2 = new Vector2(0f, 5f - 3f * f).RotatedBy(Projectile.ai[1]);
        if (progress > 0.5f && Main.rand.NextChance(0.65f)) {
            Dust obj13 = Main.dust[Dust.NewDust(Projectile.position, 2, 2, ModContent.DustType<Dusts.Blood>(), Projectile.velocity.X, Projectile.velocity.Y, 100)];
            obj13.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f;
            obj13.scale = 0.9f;
            obj13.fadeIn = 1.1f;
            obj13.velocity *= 0.25f;
            obj13.position = Projectile.position - offset2 - Vector2.UnitX * _direction * 5f + Vector2.UnitX * _direction * 50f * f;
        }
        Main.spriteBatch.Draw(texture, Projectile.position + offset2 - Main.screenPosition + offset, texture.Bounds, lightColor, Projectile.ai[1] - rotOffset, origin, heldItem.scale, effects, 0);

        return false;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_direction);
        writer.Write(Projectile.timeLeft);
        writer.Write(_useTimeMax);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _direction = reader.ReadInt32();
        Projectile.timeLeft = reader.ReadInt32();
        _useTimeMax = reader.ReadInt32();
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        Projectile.Center = player.MountedCenter;
        Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * player.gfxOffY;
        if (Projectile.owner == Main.myPlayer) {
            if (Projectile.ai[0] == 0f) {
                Projectile.ai[0] = 1f;
                Projectile.timeLeft = player.itemTime;
                _direction = player.GetViableMousePosition().X > player.Center.X ? 1 : -1;
                Projectile.ai[1] = MathHelper.PiOver2 * _direction;
                _useTimeMax = player.itemTimeMax;
                Projectile.direction = Projectile.spriteDirection = player.direction = _direction;
                Projectile.netUpdate = true;
            }
        }
        Projectile.direction = Projectile.spriteDirection = player.direction = _direction;
        player.heldProj = Projectile.whoAmI;
        player.bodyFrame.Y = 168;
        //player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        float num36 = (float)Projectile.timeLeft / _useTimeMax;
        float f = num36 < 0.5f ? num36 : (1f - num36);
        num36 = f;
        float num37 = -MathHelper.PiOver2 * _direction;
        CompositeArmStretchAmount compositeArmStretchAmount3 = CompositeArmStretchAmount.Full;
        //if (num36 < 0.16f)
        //    compositeArmStretchAmount3 = CompositeArmStretchAmount.None;
        //else if (num36 <= 0.32f)
        //    compositeArmStretchAmount3 = CompositeArmStretchAmount.Quarter;
        //else if (num36 <= 0.5f)
        //    compositeArmStretchAmount3 = CompositeArmStretchAmount.ThreeQuarters;

        if (num36 < 0.25f)
            compositeArmStretchAmount3 = CompositeArmStretchAmount.None;
        else
            compositeArmStretchAmount3 = CompositeArmStretchAmount.Quarter;

        player.SetCompositeArmFront(enabled: true, compositeArmStretchAmount3, num37);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "RazorThree") { Volume = 0.3f, Pitch = 0.1f, PitchVariance = 0.1f }, player.Center);

            if (player.whoAmI == Main.myPlayer) {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, Projectile.velocity, ModContent.ProjectileType<ArterialSprayProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }
    }
}

sealed class ArterialSprayProjectile : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.aiStyle = 1;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 25;

        Projectile.DamageType = DamageClass.Magic;

        AIType = ProjectileID.Bullet;
    }

    public override bool? CanDamage() => false;

    private void SummonSlash(Vector2 target) {
        Vector2 vec = Projectile.velocity;
        int direction = Main.player[Projectile.owner].direction;
        Vector2 v = new Vector2(-vec.Y * 1.5f, vec.X * 1.5f) * direction;
        Vector2 v2 = v.SafeNormalize(Vector2.Zero) * 6f;
        float offset = 15f;
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target - v2 * offset, v2.RotatedByRandom(MathHelper.PiOver4 * -direction), ModContent.ProjectileType<ArterialSprayProjectile2>(), Projectile.damage, 0f, Projectile.owner, 0f, target.Y);
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
        }
        Projectile.localAI[0] += 1f;
        bool flag = Projectile.localAI[0] >= 16f;
        Projectile.tileCollide = Projectile.localAI[0] > 8f;
        if (flag) {
            Projectile.Kill();
        }
        if (Projectile.localAI[0] > 4f && Projectile.localAI[0] % 4f == 0f) {
            if (Projectile.owner == Main.myPlayer) {
                SummonSlash(Projectile.Center);
            }
        }
    }
}

sealed class ArterialSprayProjectile2 : ModProjectile {
    public override string Texture => ResourceManager.FriendlyProjectileTextures + "Magic/ArterialSpray2";

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = -1;
        Projectile.scale = 1f + (float)Main.rand.Next(30) * 0.01f;
        Projectile.extraUpdates = 1;
        Projectile.timeLeft = 15 * Projectile.MaxUpdates;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;
    }

    public override bool PreDraw(ref Color lightColor) {
        if (Projectile.ai[0] > 1f) {
            Color baseColor = Color.Red;
            lightColor = new Color(baseColor.R, baseColor.G, baseColor.B, 0).MultiplyRGB(lightColor) * Projectile.Opacity * Utils.GetLerpValue(0f, 15f, Projectile.timeLeft, true);

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.position - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = SpriteEffects.None;

            int num147 = 8;
            int num148 = 2;
            int num149 = 1;
            //num149 = (int)(ProjectileID.Sets.TrailCacheLength[Projectile.type] * 0.5f);
            num149 = (int)(ProjectileID.Sets.TrailCacheLength[Projectile.type] * 0.85f);
            num147 = 0;
            num148 = -2;
            Texture2D texture2 = ModContent.Request<Texture2D>(ResourceManager.FriendlyProjectileTextures + "Magic/ArterialSpray4").Value;
            for (int num152 = num149; (num148 > 0 && num152 < num147) || (num148 < 0 && num152 > num147); num152 += num148) {
                if (num152 >= Projectile.oldPos.Length) {
                    continue;
                }

                Vector2 vector29 = Projectile.oldPos[num152] - Main.screenPosition;
                if (vector29 == Vector2.Zero) {
                    continue;
                }
                Main.EntitySpriteDraw(texture, vector29 + Projectile.Size / 2f, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects);
                Main.EntitySpriteDraw(texture2, vector29 + Projectile.Size / 2f, null, lightColor * 0.25f, Projectile.rotation, origin, Projectile.scale, effects);
            }
            Main.EntitySpriteDraw(texture2, drawPosition + Projectile.Size / 2f, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects);
            Main.EntitySpriteDraw(texture, drawPosition + Projectile.Size / 2f, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects);
            texture2 = ModContent.Request<Texture2D>(ResourceManager.FriendlyProjectileTextures + "Magic/ArterialSpray3").Value;
            //Main.EntitySpriteDraw(texture2, drawPosition + Projectile.Size / 2f, null, lightColor, Projectile._rotation, origin, Projectile.scale, effects);
        }

        return false;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            //SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
            Projectile.localAI[0] = 1f;
            for (int num163 = 0; num163 < 10; num163++) {
                Dust obj13 = Main.dust[Dust.NewDust(Projectile.position, 2, 2, 5, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj13.velocity = (Main.rand.NextFloatDirection() * (float)Math.PI).ToRotationVector2() * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
                obj13.scale = 0.9f;
                obj13.fadeIn = 1.1f;
                obj13.position = Projectile.Center - Projectile.velocity * 2.5f;
                obj13.velocity *= 0.75f;
                obj13.velocity += Projectile.velocity / 2f;
            }
        }


        for (int num164 = 0; num164 < 3; num164++) {
            if (Main.rand.Next(5) == 0) {
                Dust obj14 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 5, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj14.velocity = obj14.velocity / 4f + Projectile.velocity / 2f;
                obj14.scale = 1.2f;
                obj14.position = Projectile.Center + Main.rand.NextFloat() * Projectile.velocity * 2f;
            }
        }

        for (int num165 = 1; num165 < Projectile.oldPos.Length && !(Projectile.oldPos[num165] == Vector2.Zero); num165++) {
            if (Main.rand.Next(7) == 0) {
                Dust obj15 = Main.dust[Dust.NewDust(Projectile.oldPos[num165], Projectile.width, Projectile.height, 5, Projectile.velocity.X, Projectile.velocity.Y, 100)];
                obj15.velocity = obj15.velocity / 4f + Projectile.velocity / 2f;
                obj15.scale = 1.2f;
                obj15.position = Projectile.oldPos[num165] + Projectile.Size / 2f + Main.rand.NextFloat() * Projectile.velocity * 2f;
            }
        }

        Projectile.ai[0] += 1f;
        float num = (float)Math.PI / 2f;
        bool flag = Projectile.ai[0] > 1f;

        if (flag) {
            Projectile.alpha -= 10;
            int num2 = 100;
            if (Projectile.alpha < num2)
                Projectile.alpha = num2;
        }

        //int num3 = 10 * Projectile.MaxUpdates;
        //Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] / (float)num3);

        Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(0.1f * Main.rand.NextFloatDirection()) * 0.1f;

        Projectile.rotation = Projectile.velocity.ToRotation() + num;
        Projectile.tileCollide = false;
    }
}