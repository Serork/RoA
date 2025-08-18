using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Rafflesia : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 1000;
    private static float MAXLENGTH => 150f;
    private static float MINLENGTH => 100f;
    private static byte STEMFRAMECOUNT => 4;
    private static byte TULIPCOUNT => 6;
    private static byte FLYCOUNTTOSPAWN => 10;

    private enum RafflesiaRequstedTextureType : byte {
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
    }

    public override void AI() {
        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f);

        if (Projectile.Opacity >= 0.9f) {
            Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], 0f, 0.1f);
        }

        //Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], 1f, 0.05f);
        if (Projectile.frameCounter++ > Main.rand.Next(3, 6)) {
            Projectile.frameCounter = 0;

            Dust dust = Dust.NewDustPerfect(Projectile.Center - (Vector2.UnitY * 10f + Main.rand.RandomPointInArea(5f)).RotatedBy(Projectile.rotation), ModContent.DustType<Rot>());
            dust.velocity = new Vector2(0.5f * Main.rand.NextFloatRange(1f), 1f).RotatedBy(Projectile.rotation) * -Main.rand.NextFloat(0.5f, 1f);
        }

        void setLengthAndSpawnPosition() {
            RafflesiaValues rafflesiaValues = new(Projectile);
            if (!rafflesiaValues.Init) {
                rafflesiaValues.Init = true;

                Projectile.localAI[2] = 0.75f;

                if (Projectile.IsOwnerLocal()) {
                    Player owner = Projectile.GetOwnerAsPlayer();
                    Vector2 mousePosition = owner.GetWorldMousePosition();
                    _spawnPosition = TulipBase.GetTilePosition(owner, mousePosition, false).ToWorldCoordinates();

                    Projectile.Center = GetMoveTowardsPosition();

                    Projectile.ai[1] = Main.rand.NextFloat(1f, MAXLENGTH / MINLENGTH);

                    Projectile.ai[2] = Main.rand.NextFloatRange(MathHelper.TwoPi);

                    Projectile.netUpdate = true;
                }
            }
        }

        if (Projectile.localAI[1]++ >= 10f && _flySpawnedCount < FLYCOUNTTOSPAWN) {
            if (Projectile.IsOwnerLocal()) {
                Player owner = Projectile.GetOwnerAsPlayer();
                Vector2 spawnPosition = owner.GetWorldMousePosition();
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
            Projectile.localAI[1] = 0f;
            _flySpawnedCount++;
        }

        setLengthAndSpawnPosition();

        Projectile.velocity *= 0f;

        if (Projectile.localAI[2] <= 0.01f) {
            return;
        }
        Vector2 goToPosition = GetMoveTowardsPosition();
        float lerpValue = 0.05f * Projectile.localAI[2];
        Projectile.Center = Vector2.Lerp(Projectile.Center, goToPosition, lerpValue);
    }

    private Vector2 GetMoveTowardsPosition() {
        Player owner = Projectile.GetOwnerAsPlayer();
        Vector2 mousePosition = owner.GetWorldMousePosition();
        Vector2 destination = new(_spawnPosition.X + _spawnPosition.DirectionTo(mousePosition).X * 50f, _spawnPosition.Y - 100f);
        float length = (destination - _spawnPosition).Length() * Projectile.ai[1];
        length = Utils.Clamp(length, MINLENGTH, MAXLENGTH) ;
        length *= MathUtils.Clamp01(Projectile.Opacity * 2f);
        Projectile.ai[0] = length;
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

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Rafflesia>(out Dictionary<byte, Asset<Texture2D>?>? indexedTextureAssets)) {
            return;
        }

        Texture2D tulipTexture = indexedTextureAssets![(byte)RafflesiaRequstedTextureType.Tulip]!.Value,
                  stemTexture = indexedTextureAssets![(byte)RafflesiaRequstedTextureType.Stem]!.Value;

        Vector2 center = Projectile.Center;
        DrawStem(stemTexture, center, _spawnPosition);

        center += center.DirectionFrom(_spawnPosition) * 4f;
        float tulipCount = TULIPCOUNT;
        for (int i = 0; i < tulipCount + 1; i++) {
            Vector2 position = center;
            Rectangle clip = tulipTexture.Bounds;
            Color color = lightColor;
            float progress = (float)i / tulipCount;
            float rotation = MathHelper.Pi * progress + Projectile.rotation - MathHelper.PiOver2;
            float mid = tulipCount / 2f;
            float progress2 = MathF.Abs(i - mid) / mid;
            Vector2 scale = Vector2.One * Utils.Remap(1f - progress2, 0f, 1f, 0.5f, 1f, true) * 1.25f;
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

    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_spawnPosition);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _spawnPosition = reader.ReadVector2();
    }

    private void DrawStem(Texture2D textureToDraw, Vector2 currentPosition, Vector2 endPosition) {
        float progress = 0f;
        int count = 500;
        float maxLength = (endPosition - currentPosition).Length();
        float lengthProgress = Projectile.ai[0] / MAXLENGTH;
        float sinOffsetX = 50f;
        for (int i = 0; i < count; i++) {
            Vector2 between = endPosition - currentPosition;
            float length = between.Length();
            float currentProgress = length / maxLength;
            float scaleProgress = Utils.Clamp(currentProgress, 0.35f, 1f);
            int height = 8;
            Vector2 velocity = (endPosition - currentPosition).SafeNormalize(Vector2.UnitY) * height * scaleProgress;
            Vector2 velocityToAdd = velocity;
            velocityToAdd = velocityToAdd.RotatedBy(Math.Sin(i * sinOffsetX + Projectile.ai[2]) * scaleProgress) * 0.75f;
            progress += Main.rand.NextFloat(0.0001f, 0.00033f);

            if (length <= 0.1f) {
                break;
            }

            Vector2 position = endPosition;

            //if (WorldGen.SolidTile(position.ToTileCoordinates())) {
            //    continue;
            //}

            Vector2 scale = Vector2.One * scaleProgress;
            ulong seedForRandomness = (ulong)i;
            Main.EntitySpriteDraw(textureToDraw,
                                  position - Main.screenPosition,
                                  new Rectangle(0, 10 * Utils.RandomInt(ref seedForRandomness, STEMFRAMECOUNT), 18, height),
                                  Lighting.GetColor(position.ToTileCoordinates()),
                                  velocityToAdd.ToRotation() + MathHelper.PiOver2,
                                  new Vector2(9, height / 2),
                                  scale,
                                  SpriteEffects.None);

            endPosition -= velocityToAdd;
        }
    }
}
