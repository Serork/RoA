using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Common.Druid.Forms;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

abstract class BaseRodItem<T> : NatureItem where T : BaseRodProjectile {
    public override void Load() {
        On_Player.TryAllowingItemReuse += On_Player_TryAllowingItemReuse;
    }

    private void On_Player_TryAllowingItemReuse(On_Player.orig_TryAllowingItemReuse orig, Player self, Item sItem) {
        orig(self, sItem);

        bool flag = false;
        if (self.autoReuseAllWeapons && sItem.IsADruidicWeapon() && sItem.useStyle == ItemUseStyleID.HiddenAnimation && sItem.shoot == ProjectileID.WoodenArrowFriendly)
            flag = true;

        if (flag)
            self.releaseUseItem = true;
    }

    protected virtual ushort ShootType() => (ushort)ProjectileID.WoodenArrowFriendly;

    protected virtual ushort GetUseTime(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2);

    public override void SetStaticDefaults() => CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

    protected sealed override void SafeSetDefaults2() {
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[(ushort)ModContent.ProjectileType<T>()] < 1;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        ushort baseRodProjectileType = (ushort)ModContent.ProjectileType<T>();
        if (player.ownedProjectileCounts[baseRodProjectileType] < 1) {
            Projectile.NewProjectileDirect(source, player.Center, default, baseRodProjectileType, NatureWeaponHandler.GetNatureDamage(Item, player), knockback, player.whoAmI, GetUseTime(player), ShootType());
        }

        return false;
    }
}

abstract class BaseRodItem : BaseRodItem<BaseRodProjectile> { }

abstract class BaseRodProjectile : DruidicProjectile {
    protected const float STARTROTATION = 1.4f;
    protected const float MINROTATION = -0.24f, MAXROTATION = 0.24f;
    protected const int TICKSTOREUSE = 30;

    protected short _leftTimeToReuse;
    protected float _maxUseTime, _maxUseTime2;
    protected bool _shot, _shot2;
    protected float _rotation;
    protected Vector2 _positionOffset;

    protected Player Owner => Projectile.GetOwnerAsPlayer();
    protected bool FacedLeft => Owner.direction == -1;

    protected float OffsetRotation => FacedLeft ? -0.2f : 0.2f;

    protected virtual bool IsInUse => Owner.IsAliveAndFree();
    protected float UseTime => Math.Clamp(CurrentUseTime / _maxUseTime, 0f, 1f);
    protected float Step => Math.Clamp(1f - UseTime, 0f, 1f);

    protected Item HeldItem => AttachedNatureWeapon;
    protected Texture2D HeldItemTexture => HeldItem.IsEmpty() ? null : TextureAssets.Item[HeldItem.type].Value;

    protected Vector2 GravityOffset => Owner.gravDir == -1 ? (-Vector2.UnitY * 10f) : Vector2.Zero;
    protected Vector2 Offset => HeldItemTexture == null ? Vector2.Zero : (new Vector2(0f + CorePositionOffsetFactor().X * Owner.direction, 1f + CorePositionOffsetFactor().Y) * new Vector2(HeldItemTexture.Width, HeldItemTexture.Height)).RotatedBy(Projectile.rotation + OffsetRotation + (FacedLeft ? MathHelper.PiOver2 : -MathHelper.PiOver2));
    public Vector2 CorePosition => Projectile.Center + Offset + GravityOffset;

    public bool PreparingAttack => !_shot;

    public int CurrentUseTime { get => (int)Projectile.ai[0]; private set => Projectile.ai[0] = value < 0 ? 0 : value; }
    protected ushort ShootType => (ushort)Projectile.ai[1];
    public bool ShouldBeActive { get => Projectile.ai[2] == 0f; private set => Projectile.ai[2] = 1 - value.ToInt(); }

    public override string Texture => ResourceManager.EmptyTexture;

    protected virtual float OffsetPositionMult { get; } = 0f;

    protected virtual void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) { }

    protected virtual void SpawnCoreDustsWhileShotProjectileIsActive(float step, Player player, Vector2 corePosition) { }

    protected virtual void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) { }
    protected virtual void SetSpawnProjectileSettings2(Player player, ref int damage, ref float knockBack) { }


    protected virtual void SpawnDustsWhenReady(Player player, Vector2 corePosition) { }
    protected virtual void SpawnDustsOnShoot(Player player, Vector2 corePosition) { }

    protected virtual Vector2 CorePositionOffsetFactor() => Vector2.Zero;

    protected virtual byte TimeAfterShootToExist(Player player) => 0;

    protected virtual bool ShouldWaitUntilProjDespawns() => true;

    protected virtual float MinUseTimeToShootFactor() => 0f;

    protected virtual bool DespawnWithProj() => false;

    protected virtual bool ShouldPlayShootSound() => true;

    protected virtual bool ShouldBeActiveAfterShoot() => false;

    protected virtual byte ProjActiveCount() => 1;

    protected virtual bool ShouldntUpdateRotationAndDirection() => _shot && _leftTimeToReuse > 0;

    protected bool ShouldShootInternal() => CurrentUseTime <= _maxUseTime * MinUseTimeToShootFactor();
    protected virtual bool ShouldShoot() => ShouldShootInternal() || !IsInUse;

    protected virtual bool IsActive() => true;

    protected virtual void ShootProjectile() {
        Owner.ApplyItemAnimation(Owner.GetSelectedItem());

        Vector2 spawnPosition = CorePosition;
        Vector2 velocity = Vector2.Zero;
        ushort count = 1;
        int damage = Projectile.damage;
        float knockBack = Projectile.knockBack;
        float ai0 = 0f, ai1 = 0f, ai2 = 0f;
        if (Projectile.IsOwnerMyPlayer(Owner)) {
            for (int i = 0; i < count; i++) {
                SetSpawnProjectileSettings(Owner, ref spawnPosition, ref velocity, ref count, ref ai0, ref ai1, ref ai2);
                SetSpawnProjectileSettings2(Owner, ref damage, ref knockBack);
                Projectile.NewProjectileDirect(Owner.GetSource_ItemUse(Owner.GetSelectedItem()), spawnPosition, velocity, ShootType, damage, knockBack, Owner.whoAmI, ai0, ai1, ai2);
            }
        }
    }

    protected sealed override void SafeSetDefaults() {
        Projectile.Size = 10 * Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;

        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    protected sealed override void SafeSetDefaults2() { }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.Write(_leftTimeToReuse);
        writer.Write(_maxUseTime);
        writer.Write(_shot);
        writer.Write(_shot2);
        writer.Write(_rotation);
        writer.WriteVector2(_positionOffset);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _leftTimeToReuse = reader.ReadInt16();
        _maxUseTime = reader.ReadSingle();
        _shot = reader.ReadBoolean();
        _shot2 = reader.ReadBoolean();
        _rotation = reader.ReadSingle();
        _positionOffset = reader.ReadVector2();
    }

    public sealed override bool PreDraw(ref Color lightColor) {
        Texture2D texture = HeldItemTexture;
        if (texture == null) {
            return false;
        }

        Rectangle sourceRectangle = texture.Bounds;
        Vector2 position = Projectile.Center - Main.screenPosition;
        float offsetY = 5f * Owner.gravDir;
        Vector2 origin = new(texture.Width * 0.5f * (1f - Owner.direction), Owner.gravDir == -1f ? 0 : texture.Height);
        Color color = lightColor;
        float rotation = Projectile.rotation;
        float scale = 1f;
        float rotationOffset = MathHelper.PiOver4 * Owner.direction;
        if (Owner.gravDir == -1f) {
            rotationOffset -= MathHelper.PiOver2 * Owner.direction;
        }
        if (Owner.direction == 1) {
            position += -Vector2.UnitX * 2f;
        }
        SpriteEffects effects = Owner.direction != 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (Owner.gravDir == -1f) {
            if (Owner.direction == 1) {
                effects = SpriteEffects.FlipVertically;
            }
            else {
                effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
        }
        rotation += rotationOffset;
        Main.EntitySpriteDraw(texture, position + Vector2.UnitY * offsetY, sourceRectangle, color, rotation, origin, scale, effects);

        return false;
    }

    protected sealed override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.IsOwnerMyPlayer(Owner)) {
            _rotation = FacedLeft ? STARTROTATION : -STARTROTATION;
            if (Owner.gravDir == -1) {
                _rotation -= MathHelper.Pi;
            }
            Projectile.spriteDirection = Owner.direction;
            _maxUseTime = _maxUseTime2 = CurrentUseTime;
            SafestOnSpawn(source);

            Projectile.netUpdate = true;
        }
    }

    protected virtual void SafestOnSpawn(IEntitySource source) { }

    protected virtual void SetAttackSound(ref SoundStyle attackSound) { }

    protected void PlayAttackSound() {
        SoundStyle attackSound = SoundID.Item20;
        SetAttackSound(ref attackSound);
        SoundEngine.PlaySound(attackSound, CorePosition);
    }

    public sealed override void AI() {
        if (Owner.whoAmI != Main.myPlayer && Projectile.localAI[2] == 0f && AttachedNatureWeapon != null) {
            Projectile.localAI[2] = 1f;

            SoundEngine.PlaySound(AttachedNatureWeapon.UseSound, Owner.Center);
        }
        //Main.NewText(NatureWeaponHandler.GetUseSpeed(Item, Owner));

        ActiveCheck();
        SetPosition();
        SetDirection();
        SetRotation();

        Owner.bodyFrame.Y = 56;

        if (!IsInUse) {
            ShouldBeActive = false;
        }
        else if (CurrentUseTime > 0) {
            CurrentUseTime--;

            SpawnCoreDustsBeforeShoot(Step, Owner, CorePosition);
        }
        else if (!_shot2) {
            _shot2 = true;
            if (ShouldPlayShootSound()) {
                PlayAttackSound();
            }
            SpawnDustsWhenReady(Owner, CorePosition);
        }
        if (ShouldShoot() && !_shot) {
            ShootProjectile();
            SpawnDustsOnShoot(Owner, CorePosition);
            _shot = true;
            if (!ShouldBeActiveAfterShoot()) {
                ShouldBeActive = false;
            }
            byte timeAfterShootToExist = (byte)(AttachedNatureWeapon.IsEmpty() ? 0 : TimeAfterShootToExist(Owner));
            if (ShouldWaitUntilProjDespawns()) {
                if (timeAfterShootToExist != 0) {
                    _leftTimeToReuse = timeAfterShootToExist;
                }
                else {
                    _leftTimeToReuse /= 2;
                }
            }
            else {
                if (timeAfterShootToExist != 0) {
                    _leftTimeToReuse = timeAfterShootToExist;
                }
            }
        }

        Projectile.rotation = Owner.fullRotation + _rotation;
    }

    private void ActiveCheck() {
        if (!Owner.IsAliveAndFree()) {
            Projectile.Kill();
        }

        if (Owner.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
            Projectile.Kill();
        }
        bool haveProjsActive = Owner.ownedProjectileCounts[ShootType] >= ProjActiveCount();
        if (!ShouldBeActive) {
            if (!haveProjsActive || !ShouldWaitUntilProjDespawns()) {
                if (IsActive()) {
                    _leftTimeToReuse--;
                }
            }
            else {
                SpawnCoreDustsWhileShotProjectileIsActive(Step, Owner, CorePosition);
            }

            if (IsInUse && !_shot) {
                ShouldBeActive = true;
            }
        }
        else {
            _leftTimeToReuse = TICKSTOREUSE;
        }
        if (_leftTimeToReuse > 2) {
            if (Owner.itemTime < 2) {
                Owner.itemTime = Owner.itemAnimation = 10;
            }
            Owner.bodyFrame.Y = Owner.legFrame.Y;
            Projectile.timeLeft = 2;
            Owner.heldProj = Projectile.whoAmI;
        }
        if (_shot && DespawnWithProj() && !haveProjsActive) {
            Projectile.Kill();
        }

        if (Owner.whoAmI == Main.myPlayer && Main.mouseLeft && Main.mouseLeftRelease &&
            UseTime > 0.25f && UseTime < 0.75f) {
            if (DespawnWithProj()) {
                foreach (Projectile projectile in Main.ActiveProjectiles) {
                    if (projectile.owner == Owner.whoAmI && projectile.type == ShootType) {
                        projectile.Kill();
                    }
                }
            }
            Main.mouseLeftRelease = false;
            Owner.controlUseItem = false;
            Owner.itemAnimation = Owner.itemTime = NatureWeaponHandler.GetUseSpeed(AttachedNatureWeapon, Owner);
            Projectile.Kill();
        }
    }

    private void SetPosition() {
        Vector2 center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
        Projectile.Center = center;
        Projectile.Center = Utils.Floor(Projectile.Center);
        bool flag = !AttachedNatureWeapon.IsEmpty() && !ShouldntUpdateRotationAndDirection();
        if (Projectile.IsOwnerMyPlayer(Owner) && flag) {
            Vector2 pointPosition = Owner.GetViableMousePosition();
            Projectile.velocity = (pointPosition - Projectile.Center).SafeNormalize(Vector2.One);
            _positionOffset = (pointPosition - Projectile.Center).SafeNormalize(Vector2.One) * OffsetPositionMult;
            Projectile.netUpdate = true;
        }
        Projectile.Center += Projectile.velocity + _positionOffset;
    }

    private void SetDirection() {
        bool flag = !AttachedNatureWeapon.IsEmpty() && !ShouldntUpdateRotationAndDirection();
        if (flag) {
            if (Projectile.velocity.X != 0f) {
                Projectile.spriteDirection = Owner.direction;
                Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            }
        }
        float armRotation = Projectile.rotation - MathHelper.PiOver4 * Owner.direction;
        if (Owner.gravDir == -1) {
            armRotation = MathHelper.Pi - armRotation + MathHelper.PiOver2 * Owner.direction;
        }
        Owner.SetCompositeBothArms(armRotation, Player.CompositeArmStretchAmount.Full);
    }

    private void SetRotation() {
        bool flag = !AttachedNatureWeapon.IsEmpty() && !ShouldntUpdateRotationAndDirection();
        if (flag) {
            float rotation = Projectile.velocity.ToRotation() + OffsetRotation + (FacedLeft ? MathHelper.Pi : 0f);
            float rotationLerp = Math.Clamp(Math.Abs(rotation - _rotation), 0.16f, 0.24f) * 0.75f;
            if (!FacedLeft && rotation >= MathHelper.PiOver2) {
                rotation = MathHelper.PiOver2;
            }
            if (FacedLeft && rotation <= 4.54f && rotation >= 4.51f) {
                rotation = 4.55f;
            }
            float mouseRotation = Helper.SmoothAngleLerp(_rotation, rotation, rotationLerp);
            float min = MINROTATION;
            float max = MAXROTATION;
            Helper.SmoothClamp(ref mouseRotation, FacedLeft ? min : -max * 0.7f, FacedLeft ? max : -min, rotationLerp);
            _rotation = mouseRotation;
        }
    }
}