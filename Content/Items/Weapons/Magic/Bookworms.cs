using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace RoA.Content.Items.Weapons.Magic;

sealed class Bookworms : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 20;
        Item.autoReuse = false;
        Item.useTurn = false;

        Item.DamageType = DamageClass.Magic;
        Item.damage = 14;
        Item.knockBack = 2f;

        Item.noMelee = true;
        Item.mana = 10;

        Item.shoot = ModContent.ProjectileType<BookwormsProjectile>();
        Item.shootSpeed = 6f;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelo = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position.X -= 4f;
        position += new Vector2(-newVelo.Y, newVelo.X) * ((4f - (player.direction == 1 ? 4f : 0f)) * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        //Vector2 funnyOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 35f;
        //if (!Collision.CanHit(position, 0, 0, position + funnyOffset, 0, 0)) {
        //    return false;
        //}
        return true;
    }
}

sealed class BookwormsProjectile : ModProjectile {
    private int _direction;
    private float _length;

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.NeedsUUID[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.Size = Vector2.One * 10f;

        Projectile.aiStyle = -1;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Magic;

        Projectile.netImportant = true;

        Projectile.tileCollide = false;

        Projectile.timeLeft = 90;

        Projectile.penetrate = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.FriendlyProjectileTextures + $"Magic/Bookworms{(int)Projectile.ai[0] + 1}").Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 size = new(20, 26);
        Vector2 origin = size / 2f;
        Main.EntitySpriteDraw(texture, position + origin / 2f + new Vector2(8f * _direction, 0f).RotatedBy(Projectile.rotation), null, lightColor * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, effects);

        return false;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_direction);
        writer.Write(_length);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _direction = reader.ReadInt32();
        _length = reader.ReadSingle();
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 6; i++) {
            Vector2 vector39 = Projectile.position;
            Point size = new(20, 26);
            Dust obj2 = Main.dust[Dust.NewDust(vector39, size.X, size.Y, DustID.CorruptGibs, Projectile.oldVelocity.X, -2f, 0, default, 1.1f + 0.15f * Main.rand.NextFloat())];
            obj2.noGravity = true;
        }
        if (Projectile.ai[0] == 0f) {
            for (int i = 0; i < 6; i++) {
                Vector2 vector39 = Projectile.position;
                Point size = new(20, 26);
                Dust obj2 = Main.dust[Dust.NewDust(vector39, size.X, size.Y, DustID.CorruptGibs, 0f, -2f, 0, default, 1.1f + 0.15f * Main.rand.NextFloat())];
                obj2.noGravity = true;
            }
        }
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        Projectile.Opacity = Utils.GetLerpValue(90, 80, Projectile.timeLeft, true);
        //Projectile.tileCollide = Projectile.Opacity >= 0.5f;
        if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
            Projectile.timeLeft -= 1;
        }
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;
            if (Projectile.owner == Main.myPlayer) {
                _direction = (player.Center - player.GetViableMousePosition()).X.GetDirection();
                _length = Main.rand.NextFloat(0.25f, 0.3f);
                Projectile.netUpdate = true;
            }
            Projectile.direction = Projectile.spriteDirection = _direction;
        }
        if (Projectile.ai[0] == 0f && Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            if (Projectile.owner == Main.myPlayer) {
                int count = Main.rand.Next(6, 9);
                int latest = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
                for (int i = 0; i < count; i++) {
                    int oldLatest = latest;
                    bool tail = i == count - 1;
                    Vector2 position = Projectile.position + new Vector2(20, 26) / 2f;
                    latest = Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, Type, Projectile.damage, Projectile.knockBack, Projectile.owner,
                        tail ? 2f : 1f);
                    Main.projectile[latest].ai[1] = oldLatest;
                    Main.projectile[latest].ai[2] = i + 1;
                }
            }
        }
        if (Projectile.ai[0] == 0f) {
            if (Projectile.localAI[1] == 0f) {
                if (_direction == -1) {
                    Projectile.localAI[1] = 3f;
                }
                else {
                    Projectile.localAI[1] = 3f;
                }
            }
            Projectile.localAI[1] += 0.5f;
            float length = _length * -_direction;
            float magnitude = 5f * length * Projectile.Opacity;
            Vector2 lastOffset = Projectile.oldVelocity.RotatedBy(MathHelper.PiOver2) * (float)Math.Cos((Projectile.localAI[1] - 1f) * length);
            Vector2 offset = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * (float)Math.Cos(Projectile.localAI[1] * length);
            Vector2 velocity = (offset - lastOffset) * magnitude;
            Vector2 center = Projectile.Center - Projectile.velocity;
            Projectile.Center += velocity;

            Projectile.rotation = (Projectile.Center - center).ToRotation() + MathHelper.PiOver2;

            return;
        }
        int byUUID = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[1]);
        if (byUUID == -1) {
            Projectile.Kill();
            return;
        }
        if (Main.projectile.IndexInRange(byUUID)) {
            Projectile following = Main.projectile[byUUID];
            Projectile.velocity = Vector2.Zero;
            Vector2 dif = following.Center - Projectile.Center;
            if (dif.LengthSquared() < 200f) {
                Projectile.Opacity = 0f;
                if (Main.rand.NextBool(3)) {
                    Vector2 vector39 = Projectile.position;
                    Point size = new(20, 26);
                    Dust obj2 = Main.dust[Dust.NewDust(vector39 - Vector2.UnitY * size.Y / 3f, size.X, size.Y, DustID.CorruptGibs, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1.1f + 0.15f * Main.rand.NextFloat())];
                    obj2.noGravity = true;
                }
            }
            float length = 20f * (Projectile.ai[2] <= 1 ? 0.95f : 0.5f);
            Projectile.rotation = dif.ToRotation() + (float)Math.PI / 2f;
            if (dif != Vector2.Zero) {
                Projectile.Center = following.Center - Vector2.Normalize(dif) * length;
            }
        }
    }
}