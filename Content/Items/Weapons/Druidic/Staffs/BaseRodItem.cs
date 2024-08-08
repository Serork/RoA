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
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace RoA.Content.Items.Weapons.Druidic.Staffs;

abstract class BaseRodItem<T> : NatureItem where T : BaseRodProjectile {
    protected abstract ushort ShootType();

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

    private Player Owner => Projectile.GetOwnerAsPlayer();
    private bool FacedLeft => Owner.direction == -1;

    private float OffsetRotation => FacedLeft ? -0.2f : 0.2f;

    private bool IsInUse => !Owner.CCed;
    private float UseTime => Math.Clamp(CurrentUseTime / _maxUseTime, 0f, 1f);

    private Item HeldItem => Owner.HeldItem;
    private Texture2D HeldItemTexture => TextureAssets.Item[HeldItem.type].Value;

    private Vector2 Offset => (new Vector2(0f, 1f) * HeldItemTexture.Width).RotatedBy(Projectile.rotation + OffsetRotation + (FacedLeft ? MathHelper.PiOver2 : -MathHelper.PiOver2));
    private Vector2 CorePosition => Projectile.Center + Offset;

    public int CurrentUseTime { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value < 0 ? 0 : value; }
    public ushort ShootType => (ushort)Projectile.ai[1];
    public bool ShouldBeActive { get => Projectile.ai[2] == 0f; set => Projectile.ai[2] = value ? 0f : 1f; }

    public override string Texture => ResourceManager.EmptyTexture;

    protected abstract void SpawnCoreDusts(float step, Player player, Vector2 corePosition);

    protected abstract void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count);

    protected virtual void ShootProjectile() {
        SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        Vector2 spawnPosition = CorePosition;
        Vector2 velocity = Vector2.Zero;
        ushort count = 1;
        if (Projectile.IsOwnerMyPlayer(Owner)) {
            for (int i = 0; i < count; i++) {
                SetSpawnProjectileSettings(Owner, ref spawnPosition, ref velocity, ref count);
                Projectile.NewProjectileDirect(Projectile.GetSource_NaturalSpawn(), spawnPosition, velocity, ShootType, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
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

    public sealed override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_leftTimeToReuse);
        writer.Write(_maxUseTime);
        writer.Write(_shoot);
        writer.Write(_rotation);
    }

    public sealed override void ReceiveExtraAI(BinaryReader reader) {
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

    public sealed override void AI() {
        if (_leftTimeToReuse <= 0f && Projectile.IsOwnerMyPlayer(Owner)) {
            _rotation = FacedLeft ? STARTROTATION : -STARTROTATION;
            Projectile.spriteDirection = Owner.direction;
            _maxUseTime = _maxUseTime2 = CurrentUseTime;
            Projectile.netUpdate = true;
        }

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
                SpawnCoreDusts(Math.Clamp(1f - UseTime, 0f, 1f), Owner, CorePosition);
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
            Vector2 pointPoisition = Main.MouseWorld;
            Owner.LimitPointToPlayerReachableArea(ref pointPoisition);
            Projectile.velocity = (pointPoisition - center).SafeNormalize(Vector2.One);
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
        float rotationLerp = Math.Clamp(Math.Abs(_rotation) * 0.25f, 0.14f, 0.3f);
        float mouseRotation = Helper.SmoothAngleLerp(_rotation, rotation, rotationLerp);
        Helper.SmoothClamp(ref mouseRotation, FacedLeft ? MINROTATION : -MAXROTATION, FacedLeft ? MAXROTATION : -MINROTATION, rotationLerp);
        _rotation = mouseRotation;
    }
}