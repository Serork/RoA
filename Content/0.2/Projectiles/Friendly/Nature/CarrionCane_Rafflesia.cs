using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Items.Weapons.Nature.Hardmode.Canes;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Rafflesia : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 360;
    private static float MAXLENGTH => 150f;
    private static float MINLENGTH => 100f;
    public static byte STEMFRAMECOUNT => 4;
    private static byte TULIPCOUNT => 4;
    private static byte FLYCOUNTTOSPAWN => 5;

    public enum RafflesiaRequstedTextureType : byte {
        Tulip,
        Stem,
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)RafflesiaRequstedTextureType.Tulip, ResourceManager.NatureProjectileTextures + "Rafflesia"),
         ((byte)RafflesiaRequstedTextureType.Stem, ResourceManager.NatureProjectileTextures + "RafflesiaStem")];

    public ref struct RafflesiaValues(Projectile projectile) {
        public ref float InitOnSpawnValue = ref projectile.localAI[0];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }
    }


    private Vector2 _spawnPosition;
    private int _flySpawnedCount;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.NeedsUUID[Type] = true;
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.Size = new Vector2(30);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 45;
        Projectile.netImportant = true;
        Projectile.Opacity = 0f;

        Projectile.timeLeft = TIMELEFT;
        Projectile.manualDirectionChange = true;
    }

    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => false;

    public override void AI() {
        float attackSpeedModifier = 0.15f;

        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, attackSpeedModifier);

        if (Projectile.Opacity >= 0.9f) {
            Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], 0f, attackSpeedModifier);
        }

        //Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], 1f, 0.05f);
        if (Projectile.frameCounter++ > Main.rand.Next(3, 6)) {
            Projectile.frameCounter = 0;

            Dust dust = Dust.NewDustPerfect(Projectile.Center - (Vector2.UnitY * 10f + Main.rand.RandomPointInArea(5f)).RotatedBy(Projectile.rotation), ModContent.DustType<Dusts.Rot>());
            dust.velocity = new Vector2((0.5f + Main.WindForVisuals) * Main.rand.NextFloatRange(1f), 1f).RotatedBy(Projectile.rotation) * -Main.rand.NextFloat(0.5f, 1f);
            dust.scale *= Main.rand.NextFloat(0.8f, 1.2f);
        }

        void setLengthAndSpawnPosition() {
            RafflesiaValues rafflesiaValues = new(Projectile);
            if (!rafflesiaValues.Init) {
                rafflesiaValues.Init = true;

                Projectile.localAI[2] = 0.75f;

                if (Projectile.IsOwnerLocal()) {
                    Player owner = Projectile.GetOwnerAsPlayer();
                    Vector2 mousePosition = owner.GetCappedWorldMousePosition(CarrionCane.CarrionCaneBase.CAPPEDMOUSEPOSITIONWIDTH, CarrionCane.CarrionCaneBase.CAPPEDMOUSEPOSITIONHEIGHT);
                    Vector2 position = CarrionCane.CarrionCaneBase.GetTilePosition(owner, mousePosition, false, 0, (int)CarrionCane.CarrionCaneBase.CAPPEDMOUSEPOSITIONWIDTH, (int)CarrionCane.CarrionCaneBase.CAPPEDMOUSEPOSITIONHEIGHT).ToWorldCoordinates() - Vector2.UnitY * 4f;
                    _spawnPosition = position;

                    Projectile.Center = GetMoveTowardsPosition();

                    Projectile.ai[1] = Main.rand.NextFloat(1f, MAXLENGTH / MINLENGTH);

                    Projectile.ai[2] = Main.rand.NextFloatRange(MathHelper.TwoPi);

                    Projectile.netUpdate = true;
                }
            }
        }

        Player owner = Projectile.GetOwnerAsPlayer();
        bool flag = _flySpawnedCount >= FLYCOUNTTOSPAWN;
        Vector2 spawnPosition = owner.GetWorldMousePosition();
        if (!flag) {
            owner.SyncMousePosition();
            Vector2 areaSize = Projectile.Size / 2f;
            Vector2 areaCenter = owner.GetWorldMousePosition() - areaSize / 2f;
            if (Main.rand.NextChance(0.1f)) {
                Fly.CreateFlyDust(areaCenter, areaCenter.DirectionTo(Projectile.Center) * Main.rand.NextFloat(0.5f, 1f) * 5f, (int)areaSize.X, (int)areaSize.Y);
            }
        }
        if (Projectile.localAI[1]++ >= 20f && !flag) {
            owner.SyncMousePosition();
            if (Projectile.IsOwnerLocal()) {
                Vector2 velocity = spawnPosition.DirectionTo(Projectile.Center) * 5f;
                velocity = velocity.RotatedByRandom(MathHelper.PiOver2);
                spawnPosition += velocity;
                int uuid = Projectile.GetByUUID(Projectile.owner, Projectile.whoAmI);
                ProjectileUtils.SpawnPlayerOwnedProjectile<Fly>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_FromAI()) with {
                    Damage = Projectile.damage,
                    KnockBack = Projectile.knockBack,
                    Position = spawnPosition,
                    Velocity = velocity,
                    AI0 = uuid
                });
            }
            Vector2 areaSize = Projectile.Size / 2f;
            Vector2 areaCenter = owner.GetWorldMousePosition() - areaSize / 2f;
            for (int i = 0; i < 2; i++) {
                Fly.CreateFlyDust(areaCenter, areaCenter.DirectionTo(Projectile.Center) * Main.rand.NextFloat(0.5f, 1f) * 5f, (int)areaSize.X, (int)areaSize.Y);
            }

            Projectile.localAI[1] = 0f;
            _flySpawnedCount++;
        }

        Projectile.ai[0] = MathHelper.SmoothStep(Projectile.ai[0], 1f, attackSpeedModifier * 1.25f);

        setLengthAndSpawnPosition();

        Projectile.velocity *= 0f;

        if (Projectile.localAI[2] <= 0.01f) {
            return;
        }
        Vector2 goToPosition = GetMoveTowardsPosition();
        float lerpValue = 0.05f * Projectile.localAI[2];
        Projectile.Center = Vector2.Lerp(Projectile.Center, goToPosition, lerpValue);
        if (Projectile.Opacity < 0.5f) {
            Projectile.direction = (goToPosition.X - _spawnPosition.X).GetDirection();
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Rafflesia>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D tulipTexture = indexedTextureAssets[(byte)RafflesiaRequstedTextureType.Tulip].Value,
                  stemTexture = indexedTextureAssets[(byte)RafflesiaRequstedTextureType.Stem].Value;

        Vector2 center = Projectile.Center;
        DrawStem(stemTexture, center, _spawnPosition);

        float angle = MathHelper.PiOver4;
        float angleOffsetLength = 30f;
        Vector2 angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(angle) * angleOffsetLength;
        float lengthOffsetAmount = 20f * Projectile.ai[0];
        Vector2 lengthOffset = center.DirectionTo(_spawnPosition) * lengthOffsetAmount;
        DrawStem(stemTexture, center + angleOffset + lengthOffset, _spawnPosition, 0.1f, 0.85f);
        angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(-angle) * angleOffsetLength;
        DrawStem(stemTexture, center + angleOffset + lengthOffset, _spawnPosition, -0.1f, 0.85f);
        angleOffsetLength = 50f;
        lengthOffsetAmount = 30f * Projectile.ai[0];
        angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(angle) * angleOffsetLength;
        lengthOffset = center.DirectionTo(_spawnPosition) * lengthOffsetAmount;
        DrawStem(stemTexture, center + angleOffset + lengthOffset, _spawnPosition, 0.2f, 0.75f);
        angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(-angle) * angleOffsetLength;
        DrawStem(stemTexture, center + angleOffset + lengthOffset, _spawnPosition, -0.2f, 0.75f);

        //CreateTulipDusts();

        center += center.DirectionFrom(_spawnPosition) * 4f;
        float tulipCount = TULIPCOUNT;
        for (int i = 0; i < tulipCount + 1; i++) {
            Vector2 position = center;
            Rectangle clip = tulipTexture.Bounds;
            Color color = lightColor;
            float progress = (float)i / MathHelper.Lerp(tulipCount, tulipCount + 1, Projectile.ai[0]);
            float progress3 = (float)(i + 1) / MathHelper.Lerp(tulipCount, tulipCount + 1, Projectile.ai[0]);
            float desiredRotation = (MathHelper.Pi * progress + Projectile.rotation - MathHelper.PiOver2) * Projectile.direction;
            float rotation = desiredRotation;
            rotation = Utils.AngleLerp(rotation, (MathHelper.TwoPi * progress + Projectile.rotation - MathHelper.PiOver2) * Projectile.direction, Projectile.ai[0]);
            if (Projectile.direction < 0) {
                rotation -= MathHelper.PiOver4 / 4f;
            }
            float mid = tulipCount / 2f;
            float progress2 = MathF.Abs(i - mid) / mid;
            Vector2 scale = Vector2.One * MathHelper.Lerp(Utils.Remap(1f - progress2, 0f, 1f, 0.5f, 1f, true) * 1.25f, 1f, Projectile.ai[0]);
            Main.spriteBatch.Draw(tulipTexture, position, DrawInfo.Default with {
                Clip = clip,
                Color = color,
                Rotation = rotation,
                Origin = Utils.Bottom(clip),
                Scale = scale * Projectile.Opacity
            });
        }
    }

    public override void OnKill(int timeLeft) {
        CreateTulipDusts();

        Vector2 center = Projectile.Center;
        CreateStemDusts(center, _spawnPosition);

        float angle = MathHelper.PiOver4;
        float angleOffsetLength = 30f;
        Vector2 angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(angle) * angleOffsetLength;
        float lengthOffsetAmount = 20f * Projectile.ai[0];
        Vector2 lengthOffset = center.DirectionTo(_spawnPosition) * lengthOffsetAmount;
        CreateStemDusts(center + angleOffset + lengthOffset, _spawnPosition, 0.1f, 0.85f);
        angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(-angle) * angleOffsetLength;
        CreateStemDusts(center + angleOffset + lengthOffset, _spawnPosition, -0.1f, 0.85f);
        angleOffsetLength = 50f;
        lengthOffsetAmount = 30f * Projectile.ai[0];
        angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(angle) * angleOffsetLength;
        lengthOffset = center.DirectionTo(_spawnPosition) * lengthOffsetAmount;
        CreateStemDusts(center + angleOffset + lengthOffset, _spawnPosition, 0.2f, 0.75f);
        angleOffset = _spawnPosition.DirectionFrom(center).RotatedBy(-angle) * angleOffsetLength;
        CreateStemDusts(center + angleOffset + lengthOffset, _spawnPosition, -0.2f, 0.75f);
    }
    
    private void CreateTulipDusts() {
        float tulipCount = TULIPCOUNT * 1 + TULIPCOUNT / 2;
        Vector2 center = Projectile.Center;
        for (int i = 0; i < tulipCount + 1; i++) {
            Vector2 position = center;
            float progress = (float)i / MathHelper.Lerp(tulipCount, tulipCount + 1, Projectile.ai[0]);
            float progress3 = (float)(i + 1) / MathHelper.Lerp(tulipCount, tulipCount + 1, Projectile.ai[0]);
            float desiredRotation = (MathHelper.Pi * progress + Projectile.rotation - MathHelper.PiOver2) * Projectile.direction;
            float rotation = desiredRotation;
            rotation = Utils.AngleLerp(rotation, (MathHelper.TwoPi * progress + Projectile.rotation - MathHelper.PiOver2) * Projectile.direction, Projectile.ai[0]);
            if (Projectile.direction < 0) {
                rotation -= MathHelper.PiOver4 / 4f;
            }
            rotation += MathHelper.PiOver4 * 0.75f;
            position = position - Vector2.UnitY * 10f + Vector2.UnitY.RotatedBy(rotation) * Main.rand.NextFloat(20f);
            Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<Dusts.CarrionCane2>());
            //dust.customData = Main.rand.NextFloat(1f, 2.5f);
        }
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_spawnPosition);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _spawnPosition = reader.ReadVector2();
    }

    private Vector2 GetMoveTowardsPosition() {
        Player owner = Projectile.GetOwnerAsPlayer();
        Vector2 mousePosition = owner.GetWorldMousePosition();
        Vector2 destination = new(_spawnPosition.X + _spawnPosition.DirectionTo(mousePosition).X * 50f, _spawnPosition.Y - 100f);
        float length = (destination - _spawnPosition).Length() * Projectile.ai[1];
        length = Utils.Clamp(length, MINLENGTH, MAXLENGTH);
        length *= MathUtils.Clamp01(Projectile.Opacity * 2f);
        float rotation = Projectile.Center.DirectionTo(destination).ToRotation() + MathHelper.PiOver2;
        float maxRotation = MathHelper.PiOver4 / 2f;
        rotation = Utils.Clamp(rotation, -maxRotation, maxRotation);
        float lerpValue = 0.05f * Projectile.localAI[2];
        if (Projectile.Center.Y > destination.Y) {
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rotation, lerpValue);
        }
        owner.SyncMousePosition();
        return _spawnPosition + _spawnPosition.DirectionTo(destination) * length;
    }

    private void DrawStem(Texture2D textureToDraw, Vector2 currentPosition, Vector2 endPosition, float sinOffset = 0f, float extraScale = 1f) {
        float progress = 0f;
        int count = 500;
        float maxLength = (endPosition - currentPosition).Length();
        float sinOffsetX = 50f;
        for (int i = 0; i < count; i++) {
            Vector2 between = endPosition - currentPosition;
            float length = between.Length();
            float currentProgress = length / maxLength;
            float scaleProgress = Utils.Clamp(currentProgress, sinOffset == 0f ? 0.35f : 0f, 1f) * extraScale;
            int height = 8;
            Vector2 velocity = (endPosition - currentPosition).SafeNormalize(Vector2.UnitY) * height * scaleProgress;
            Vector2 velocityToAdd = velocity;
            velocityToAdd = velocityToAdd.RotatedBy(Math.Sin(i * sinOffsetX + Projectile.ai[2] + sinOffset) * scaleProgress) * 0.85f;
            progress += Main.rand.NextFloat(0.0001f, 0.00033f);

            if (length <= 1f) {
                break;
            }

            Vector2 position = endPosition;

            //if (WorldGen.SolidTile(position.ToTileCoordinates())) {
            //    continue;
            //}

            Vector2 scale = Vector2.One * scaleProgress;
            ulong seedForRandomness = (ulong)i;
            int frameToUse = i == 0 ? STEMFRAMECOUNT - 1 : Utils.RandomInt(ref seedForRandomness, STEMFRAMECOUNT);
            Main.EntitySpriteDraw(textureToDraw,
                                  position - Main.screenPosition,
                                  new Rectangle(0, 10 * frameToUse, 18, height),
                                  Lighting.GetColor(position.ToTileCoordinates()),
                                  velocityToAdd.ToRotation() + MathHelper.PiOver2,
                                  new Vector2(9, height / 2),
                                  scale,
                                  SpriteEffects.None);

            endPosition -= velocityToAdd;
        }
    }

    private void CreateStemDusts(Vector2 currentPosition, Vector2 endPosition, float sinOffset = 0f, float extraScale = 1f) {
        float progress = 0f;
        int count = 500;
        float maxLength = (endPosition - currentPosition).Length();
        float sinOffsetX = 50f;
        for (int i = 0; i < count; i++) {
            Vector2 between = endPosition - currentPosition;
            float length = between.Length();
            float currentProgress = length / maxLength;
            float scaleProgress = Utils.Clamp(currentProgress, sinOffset == 0f ? 0.35f : 0f, 1f) * extraScale;
            int height = 8;
            Vector2 velocity = (endPosition - currentPosition).SafeNormalize(Vector2.UnitY) * height * scaleProgress;
            Vector2 velocityToAdd = velocity;
            velocityToAdd = velocityToAdd.RotatedBy(Math.Sin(i * sinOffsetX + Projectile.ai[2] + sinOffset) * scaleProgress) * 0.85f;
            progress += Main.rand.NextFloat(0.0001f, 0.00033f);

            if (length <= height / 4) {
                break;
            }

            Vector2 position = endPosition;

            //if (WorldGen.SolidTile(position.ToTileCoordinates())) {
            //    continue;
            //}

            if (!Main.rand.NextBool(4)) {
                Dust obj2 = Dust.NewDustPerfect(position + Main.rand.RandomPointInArea(4f), ModContent.DustType<Dusts.CarrionCane3>(), Vector2.Zero, 
                    Scale: 1.5f * Utils.Clamp(scaleProgress, 0.45f, 1f) * extraScale + 0.15f * Main.rand.NextFloat());
                obj2.noGravity = true;
                obj2.fadeIn = 0.5f;
            }

            //Vector2 scale = Vector2.One * scaleProgress;
            //ulong seedForRandomness = (ulong)i;
            //int frameToUse = i == 0 ? STEMFRAMECOUNT - 1 : Utils.RandomInt(ref seedForRandomness, STEMFRAMECOUNT);
            //Main.EntitySpriteDraw(textureToDraw,
            //                      position - Main.screenPosition,
            //                      new Rectangle(0, 10 * frameToUse, 18, height),
            //                      Lighting.GetColor(position.ToTileCoordinates()),
            //                      velocityToAdd.ToRotation() + MathHelper.PiOver2,
            //                      new Vector2(9, height / 2),
            //                      scale,
            //                      SpriteEffects.None);

            endPosition -= velocityToAdd;
        }
    }
}
