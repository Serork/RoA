using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Utilities;

using RoA.Common.Druid;
using RoA.Common.Druid.Forms;
using RoA.Common.GlowMasks;
using RoA.Common.Projectiles;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature;

abstract class CaneBaseItem<T> : NatureItem where T : CaneBaseProjectile {
    public override void Load() {
        On_Player.TryAllowingItemReuse += On_Player_TryAllowingItemReuse;
    }

    private void On_Player_TryAllowingItemReuse(On_Player.orig_TryAllowingItemReuse orig, Player self, Item sItem) {
        orig(self, sItem);

        bool flag = false;
        if (self.autoReuseAllWeapons && sItem.IsANatureWeapon() && sItem.useStyle == ItemUseStyleID.HiddenAnimation && sItem.shoot == ProjectileID.WoodenArrowFriendly)
            flag = true;

        if (flag)
            self.releaseUseItem = true;
    }

    protected virtual ushort ProjectileTypeToCreate() => (ushort)ProjectileID.WoodenArrowFriendly;

    protected virtual ushort TimeToCastAttack(Player player) => (ushort)(NatureWeaponHandler.GetUseSpeed(Item, player) * 2);

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
            Projectile.NewProjectileDirect(source, player.Center, default, baseRodProjectileType, NatureWeaponHandler.GetNatureDamage(Item, player), knockback, player.whoAmI, TimeToCastAttack(player), ProjectileTypeToCreate());
        }

        return false;
    }
}

abstract class CaneBaseItem : CaneBaseItem<CaneBaseProjectile> { }

abstract class CaneBaseProjectile : NatureProjectile_NoTextureLoad {
    private const float STARTROTATION = 1.4f;
    private const float MINROTATION = -0.24f, MAXROTATION = 0.24f;
    private const int BASEPENALTY = 30;

    private bool _init;
    private ushort _penaltyTime, _maxPenaltyTime;
    private float _maxUseTime;
    private float _rotation;
    private Vector2 _positionOffset;
    private bool _shouldBeActive;
    private bool _shouldDrawGlowMask;
    protected Projectile LastShotProjectile { get; private set; }

    protected bool Shot, Shot2;

    protected Player Owner => Projectile.GetOwnerAsPlayer();
    protected bool FacedLeft => Owner.direction == -1;
    protected float OffsetRotation => FacedLeft ? -0.2f : 0.2f;

    public float UseTime => _maxUseTime;

    public float AttackTimeLeftProgress => Math.Clamp(CurrentUseTime / _maxUseTime, 0f, 1f);
    public float AttackProgress01 => Math.Clamp(1f - AttackTimeLeftProgress, 0f, 1f);

    protected Vector2 GravityOffset => Owner.gravDir == -1 ? (-Vector2.UnitY * 10f) : Vector2.Zero;
    protected Vector2 Offset => GetCorePositionOffset(CorePositionOffsetFactor());
    public Vector2 CorePosition => Projectile.Center + Offset + GravityOffset;

    public Vector2 GetCorePositionOffset(Vector2 corePositionOffsetFactor) => AttachedNatureWeapon.IsEmpty() ? default : (new Vector2(0f + corePositionOffsetFactor.X * Owner.direction, 1f + corePositionOffsetFactor.Y) * new Vector2(AttachedNatureWeapon.width, AttachedNatureWeapon.height)).RotatedBy(Projectile.rotation + OffsetRotation + (FacedLeft ? MathHelper.PiOver2 : -MathHelper.PiOver2));

    public bool PreparingAttack => !Shot;

    public int CurrentUseTime { get => (int)Projectile.ai[0]; private set => Projectile.ai[0] = value < 0 ? 0 : value; }
    public bool ShouldBeActive { get => _shouldBeActive; private set => _shouldBeActive = value; }

    protected ushort CurrentPenaltyTime => _penaltyTime;
    protected float PenaltyProgress => (float)_penaltyTime / _maxPenaltyTime;
    protected ushort ShootType => (ushort)Projectile.ai[1];

    public virtual bool IsInUse => Owner.IsAliveAndFree();

    protected virtual float OffsetPositionMult { get; } = 0f;

    protected virtual void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) { }

    protected virtual void SpawnCoreDustsWhileShotProjectileIsActive(float step, Player player, Vector2 corePosition) { }

    protected virtual void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2) { }
    protected virtual void SetSpawnProjectileSettings2(Player player, ref int damage, ref float knockBack) { }


    protected virtual void SpawnDustsWhenReady(Player player, Vector2 corePosition) { }
    protected virtual void SpawnDustsOnShoot(Player player, Vector2 corePosition) { }

    protected virtual Vector2 CorePositionOffsetFactor() => Vector2.Zero;

    protected virtual ushort TimeAfterShootToExist(Player player) => 0;

    protected virtual bool ShouldWaitUntilProjDespawns() => true;

    protected virtual float MinUseTimeToShootFactor() => 0f;

    protected virtual bool DespawnWithProj() => false;

    protected virtual bool ShouldPlayShootSound() => true;

    protected virtual bool ShouldBeActiveAfterShoot() => false;

    protected virtual byte ProjActiveCount() => 1;

    //protected virtual bool ShouldStopUpdatingRotationAndDirection() => Shot && _penaltyTime > 0;

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
        if (Projectile.IsOwnerLocal()) {
            for (int i = 0; i < count; i++) {
                SetSpawnProjectileSettings(Owner, ref spawnPosition, ref velocity, ref count, ref ai0, ref ai1, ref ai2);
                SetSpawnProjectileSettings2(Owner, ref damage, ref knockBack);
                LastShotProjectile = Projectile.NewProjectileDirect(Owner.GetSource_ItemUse(Owner.GetSelectedItem()), spawnPosition, velocity, ShootType, damage, knockBack, Owner.whoAmI, ai0, ai1, ai2);
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
        writer.Write(_penaltyTime);
        writer.Write(_maxUseTime);
        writer.Write(Shot);
        writer.Write(Shot2);
        writer.Write(_rotation);
        writer.WriteVector2(_positionOffset);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _penaltyTime = reader.ReadUInt16();
        _maxUseTime = reader.ReadSingle();
        Shot = reader.ReadBoolean();
        Shot2 = reader.ReadBoolean();
        _rotation = reader.ReadSingle();
        _positionOffset = reader.ReadVector2();
    }

    protected virtual void SafePreDraw() { }

    protected sealed override void Draw(ref Color lightColor) {
        if (AttachedNatureWeapon.IsEmpty()) {
            return;
        }

        SafePreDraw();

        Texture2D heldItemTexture = TextureAssets.Item[AttachedNatureWeapon.type].Value;

        Rectangle sourceRectangle = heldItemTexture.Bounds;
        Vector2 position = Projectile.Center;
        float offsetY = 5f * Owner.gravDir;
        Vector2 origin = new(heldItemTexture.Width * 0.5f * (1f - Owner.direction), Owner.gravDir == -1f ? 0 : heldItemTexture.Height);
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

        Vector2 drawPosition = position + Vector2.UnitY * offsetY - Main.screenPosition;
        Main.EntitySpriteDraw(heldItemTexture, drawPosition, sourceRectangle, color, rotation, origin, scale, effects);

        if (_shouldDrawGlowMask) {
            var glowMaskInfo = ItemGlowMaskHandler.GlowMasks[AttachedNatureWeapon.type];
            Texture2D heldItemGlowMaskTexture = glowMaskInfo.Texture.Value;
            float brightnessFactor = Lighting.Brightness((int)drawPosition.X / 16, (int)drawPosition.Y / 16);
            color = Color.Lerp(glowMaskInfo.Color, lightColor, brightnessFactor);
            Color glowMaskColor = glowMaskInfo.ShouldApplyItemAlpha ? color * (1f - Projectile.alpha / 255f) : glowMaskInfo.Color;
            Main.EntitySpriteDraw(heldItemGlowMaskTexture, drawPosition, sourceRectangle, glowMaskColor, rotation, origin, scale, effects);
        }
    }

    protected sealed override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.IsOwnerLocal()) {
            _rotation = FacedLeft ? STARTROTATION : -STARTROTATION;
            if (Owner.gravDir == -1) {
                _rotation -= MathHelper.Pi;
            }
            Projectile.spriteDirection = Owner.direction;
            _maxUseTime = CurrentUseTime;
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

    protected virtual void Initialize() { }

    public sealed override void AI() {
        if (AttachedNatureWeapon.IsEmpty()) {
            Projectile.Kill();
            return;
        }

        SetPosition();

        if (!_init) {
            _init = true;

            Initialize();

            _shouldDrawGlowMask = AttachedNatureWeapon.ModItem.GetType().GetAttribute<AutoloadGlowMaskAttribute>() != null;
        }

        if (Owner.whoAmI != Main.myPlayer && Projectile.localAI[2] == 0f && AttachedNatureWeapon != null) {
            Projectile.localAI[2] = 1f;

            SoundEngine.PlaySound(AttachedNatureWeapon.UseSound, Owner.Center);
        }

        ActiveCheck();
        SetDirection();
        SetRotation();

        Owner.bodyFrame.Y = 56;

        if (!IsInUse) {
            ShouldBeActive = false;
        }
        else if (CurrentUseTime > 0) {
            CurrentUseTime--;

            SpawnCoreDustsBeforeShoot(AttackProgress01, Owner, CorePosition);
        }
        else if (!Shot2) {
            Shot2 = true;
            if (ShouldPlayShootSound()) {
                PlayAttackSound();
            }
            SpawnDustsWhenReady(Owner, CorePosition);
        }
        if (ShouldShoot() && !Shot) {
            ShootProjectile();
            SpawnDustsOnShoot(Owner, CorePosition);
            ReleaseCane();
        }

        Projectile.rotation = Owner.fullRotation + _rotation;

        AfterProcessingCane();
    }

    protected virtual void AfterProcessingCane() { }

    public virtual void ReleaseCane() {
        if (Shot) {
            return;
        }

        Shot = true;

        if (!ShouldBeActiveAfterShoot()) {
            ShouldBeActive = false;
        }
        ApplyWeaponPenalty();
        Projectile.netUpdate = true;
    }

    protected void ApplyWeaponPenalty(ushort? timeAfterShootToExist = null) {
        timeAfterShootToExist ??= GetTimeAfterShootToExist();
        if (ShouldWaitUntilProjDespawns()) {
            if (timeAfterShootToExist != 0) {
                _penaltyTime = timeAfterShootToExist.Value;
                _maxPenaltyTime = _penaltyTime;
            }
            else {
                _penaltyTime /= 2;
                _maxPenaltyTime = _penaltyTime;
            }
        }
        else {
            if (timeAfterShootToExist != 0) {
                _penaltyTime = timeAfterShootToExist.Value;
                _maxPenaltyTime = _penaltyTime;
            }
        }
    }

    protected byte GetTimeAfterShootToExist() => (byte)(AttachedNatureWeapon!.IsEmpty() ? 0 : TimeAfterShootToExist(Owner));

    private void ActiveCheck() {
        if (!Owner.IsAliveAndFree()) {
            Projectile.Kill();
        }

        if (Owner.GetFormHandler().IsInADruidicForm) {
            Projectile.Kill();
        }
        bool haveProjsActive = Owner.ownedProjectileCounts[ShootType] >= ProjActiveCount();
        if (!ShouldBeActive) {
            if (!haveProjsActive || !ShouldWaitUntilProjDespawns()) {
                if (IsActive()) {
                    _penaltyTime--;
                }
            }
            else {
                SpawnCoreDustsWhileShotProjectileIsActive(AttackProgress01, Owner, CorePosition);
            }

            if (IsInUse && !Shot) {
                ShouldBeActive = true;
            }
        }
        else {
            _penaltyTime = BASEPENALTY;
        }
        if (_penaltyTime > 2) {
            if (Owner.itemTime < 2) {
                Owner.itemTime = Owner.itemAnimation = 10;
            }
            Owner.bodyFrame.Y = Owner.legFrame.Y;
            Projectile.timeLeft = 2;
            Owner.heldProj = Projectile.whoAmI;
        }
        if (Shot && DespawnWithProj() && !haveProjsActive) {
            Projectile.Kill();
        }

        if (Owner.whoAmI == Main.myPlayer && Main.mouseLeft && Main.mouseLeftRelease &&
            AttackTimeLeftProgress > 0.25f && AttackTimeLeftProgress < 0.75f) {
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
        Vector2 center = Owner.MountedCenter;
        Projectile.Center = center;
        Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * Owner.gfxOffY;
        //bool flag = !ShouldStopUpdatingRotationAndDirection();
        bool flag = true;
        if (Projectile.IsOwnerLocal() && flag) {
            Vector2 pointPosition = Owner.GetViableMousePosition();
            Projectile.velocity = (pointPosition - Projectile.Center).SafeNormalize(Vector2.One);
            _positionOffset = (pointPosition - Projectile.Center).SafeNormalize(Vector2.One) * OffsetPositionMult;
            Projectile.netUpdate = true;
        }
        Projectile.Center += Projectile.velocity + _positionOffset;
    }

    private void SetDirection() {
        //bool flag = !ShouldStopUpdatingRotationAndDirection();
        bool flag = true;
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
        //bool flag = !ShouldStopUpdatingRotationAndDirection();
        bool flag = true;
        if (flag) {
            float rotation = Projectile.velocity.ToRotation() + OffsetRotation + (FacedLeft ? MathHelper.Pi : 0f);
            float rotationLerp = Math.Clamp(Math.Abs(rotation - _rotation), 0.16f, 0.24f) * 0.5f;
            if (!FacedLeft && rotation >= MathHelper.PiOver2) {
                rotation = MathHelper.PiOver2;
            }
            if (FacedLeft && rotation <= 4.54f && rotation >= 4.51f) {
                rotation = 4.55f;
            }
            float mouseRotation = Helper.SmoothAngleLerp(_rotation, rotation, rotationLerp, MathHelper.Lerp);
            float min = MINROTATION;
            float max = MAXROTATION;
            Helper.SmoothClamp(ref mouseRotation, FacedLeft ? min : -max * 0.7f, FacedLeft ? max : -min, rotationLerp);
            _rotation = mouseRotation;
        }
    }
}