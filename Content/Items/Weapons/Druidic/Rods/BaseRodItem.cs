using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

abstract class BaseRodItem<T> : NatureItem where T : BaseRodProjectile {
    protected virtual ushort ShootType() => (ushort)ProjectileID.WoodenArrowFriendly;

    protected sealed override void SafeSetDefaults2() {
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        ushort baseRodProjectileType = (ushort)ModContent.ProjectileType<T>();
        if (player.ownedProjectileCounts[baseRodProjectileType] < 1) {
            Projectile.NewProjectileDirect(source, player.Center, default, baseRodProjectileType, NatureWeaponHandler.GetNatureDamage(Item), Item.knockBack, player.whoAmI, Item.useTime * 2, ShootType());
        }

        return false;
    }
}

abstract class BaseRodItem : BaseRodItem<BaseRodProjectile> { }

abstract class BaseRodProjectile : NatureProjectile {
    private const float STARTROTATION = 1.4f;
    private const float MINROTATION = -0.24f, MAXROTATION = 0.24f;
    private const int TICKSTOREUSE = 30;

    private short _leftTimeToReuse;
    private float _maxUseTime, _maxUseTime2;
    private bool _shoot;
    private float _rotation;

    protected Player Owner => Projectile.GetOwnerAsPlayer();
    protected bool FacedLeft => Owner.direction == -1;

    protected float OffsetRotation => FacedLeft ? -0.2f : 0.2f;

    protected bool IsInUse => !Owner.CCed;
    protected float UseTime => Math.Clamp(CurrentUseTime / _maxUseTime, 0f, 1f);
    protected float Step => Math.Clamp(1f - UseTime, 0f, 1f);

    protected Item HeldItem => Owner.HeldItem;
    protected Texture2D HeldItemTexture => TextureAssets.Item[HeldItem.type].Value;

    protected Vector2 Offset => (new Vector2(0f + CorePositionOffsetFactor().X * Owner.direction, 1f + CorePositionOffsetFactor().Y) * new Vector2(HeldItemTexture.Width, HeldItemTexture.Height)).RotatedBy(Projectile.rotation + OffsetRotation + (FacedLeft ? MathHelper.PiOver2 : -MathHelper.PiOver2));
    protected Vector2 CorePosition => Projectile.Center + Offset;

    protected int CurrentUseTime { get => (int)Projectile.ai[0]; private set => Projectile.ai[0] = value < 0 ? 0 : value; }
    protected ushort ShootType => (ushort)Projectile.ai[1];
    protected bool ShouldBeActive { get => Projectile.ai[2] == 0f; private set => Projectile.ai[2] = value ? 0f : 1f; }

    public override string Texture => ResourceManager.EmptyTexture;

    protected abstract void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition);

    protected virtual void SpawnCoreDustsWhileShotProjectileIsActive(float step, Player player, Vector2 corePosition) { }

    protected abstract void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2);
    
    protected virtual void SpawnDustsOnShoot(Player player, Vector2 corePosition) { }

    protected virtual Vector2 CorePositionOffsetFactor() => Vector2.Zero;

    protected virtual void ShootProjectile() {
        SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        Vector2 spawnPosition = CorePosition;
        Vector2 velocity = Vector2.Zero;
        ushort count = 1;
        float ai0 = 0f, ai1 = 0f, ai2 = 0f;
        if (Projectile.IsOwnerMyPlayer(Owner)) {
            for (int i = 0; i < count; i++) {
                SetSpawnProjectileSettings(Owner, ref spawnPosition, ref velocity, ref count, ref ai0, ref ai1, ref ai2);
                Projectile.NewProjectileDirect(Projectile.GetSource_NaturalSpawn(), spawnPosition, velocity, ShootType, Projectile.damage, Projectile.knockBack, Owner.whoAmI, ai0, ai1, ai2);
                if (Main.netMode != NetmodeID.Server) {
                    SpawnDustsOnShoot(Owner, CorePosition);
                }
            }
        }
    }

    protected sealed override void SafeSetDefaults() {
        Projectile.Size = 10 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
    }

    protected sealed override void SafeSetDefaults2() { }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_leftTimeToReuse);
        writer.Write(_maxUseTime);
        writer.Write(_shoot);
        writer.Write(_rotation);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _leftTimeToReuse = reader.ReadInt16();
        _maxUseTime = reader.ReadSingle();
        _shoot = reader.ReadBoolean();
        _rotation = reader.ReadSingle();
    }

    public sealed override bool PreDraw(ref Color lightColor) {
        Texture2D texture = HeldItemTexture;
        Rectangle sourceRectangle = texture.Bounds;
        Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY - Owner.gfxOffY);
        float offsetY = 5f;
        Vector2 origin = new(texture.Width * 0.5f * (1f - Owner.direction), (Owner.gravDir == -1f) ? 0 : texture.Height);
        Color color = lightColor;
        float extraRotation = MathHelper.PiOver4 * Owner.direction;
        if (Owner.gravDir == -1f) {
            extraRotation -= MathHelper.PiOver2 * Owner.direction;
        }
        float rotation = Projectile.rotation + extraRotation;
        float scale = 1f;
        SpriteEffects effects = (SpriteEffects)(Owner.direction != 1).ToInt();
        Main.EntitySpriteDraw(texture, position + Vector2.UnitY * offsetY, sourceRectangle, color, rotation, origin, scale, effects);

        return false;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.IsOwnerMyPlayer(Owner)) {
            _rotation = FacedLeft ? STARTROTATION : -STARTROTATION;
            Projectile.spriteDirection = Owner.direction;
            _maxUseTime = _maxUseTime2 = CurrentUseTime;

            Projectile.netUpdate = true;
        }
    }

    public sealed override void AI() {
        ActiveCheck();
        SetPosition();
        SetDirection();
        SetRotation();

        if (!IsInUse) {
            ShouldBeActive = false;
        }
        else if (CurrentUseTime > 0) {
            CurrentUseTime--;

            if (Main.netMode != NetmodeID.Server) {
                SpawnCoreDustsBeforeShoot(Step, Owner, CorePosition);
            }
        }
        else if (!_shoot) {
            ShootProjectile();

            _shoot = true;
            ShouldBeActive = false;
            _leftTimeToReuse /= 2;
        }

        Projectile.rotation = _rotation;
    }

    private void ActiveCheck() {
        if (!Owner.active) {
            Projectile.Kill();
        }
        if (!ShouldBeActive) {
            if (Owner.ownedProjectileCounts[ShootType] < 1) {
                _leftTimeToReuse--;
            }
            else {
                if (Main.netMode != NetmodeID.Server) {
                    SpawnCoreDustsWhileShotProjectileIsActive(Step, Owner, CorePosition);
                }
            }

            if (IsInUse && !_shoot) {
                ShouldBeActive = true;
            }
        }
        else {
            _leftTimeToReuse = TICKSTOREUSE;
        }
        if (_leftTimeToReuse > 2) {
            Owner.itemAnimation = Owner.itemTime = 2;
            Projectile.timeLeft = 2;
            Owner.heldProj = Projectile.whoAmI;
        }
    }

    private void SetPosition() {
        Vector2 center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
        center.X = (int)center.X;
        center.Y = (int)center.Y;
        Projectile.Center = center;
        if (Projectile.IsOwnerMyPlayer(Owner)) {
            Vector2 pointPosition = Owner.GetViableMousePosition();
            Projectile.velocity = (pointPosition - center).SafeNormalize(Vector2.One);
            Projectile.netUpdate = true;
        }
        Projectile.Center += Projectile.velocity;
    }

    private void SetDirection() {
        Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
        Owner.direction = Projectile.spriteDirection;
        float armRotation = Projectile.rotation - MathHelper.PiOver4 * Owner.direction;
        Owner.SetCompositeBothArms(armRotation);
    }

    private void SetRotation() {
        float rotation = Projectile.velocity.ToRotation() + OffsetRotation + (FacedLeft ? MathHelper.Pi : 0f);
        float rotationLerp = Math.Clamp(Math.Abs(rotation - _rotation), 0.16f, 0.24f) * 0.75f;
        float mouseRotation = Helper.SmoothAngleLerp(_rotation, rotation, rotationLerp);
        Helper.SmoothClamp(ref mouseRotation, FacedLeft ? MINROTATION : -MAXROTATION, FacedLeft ? MAXROTATION : -MINROTATION, rotationLerp);
        _rotation = mouseRotation;
    }
}