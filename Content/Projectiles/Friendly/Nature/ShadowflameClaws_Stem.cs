using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class ShadowflameStem : NatureProjectile_NoTextureLoad, IRequestAssets, IPostSetupContent {
    private static ushort TIMELEFT => 300;

    public enum ShadowflameStemRequstedTextureType : byte {
        Stem,
        Leaf1,
        Leaf2,
        Leaf3,
        SmallStemMap,
        MediumStemMap,
        LargeStemMap
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)ShadowflameStemRequstedTextureType.Stem, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_Stem"),
         ((byte)ShadowflameStemRequstedTextureType.Leaf1, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_Leaf1"),
         ((byte)ShadowflameStemRequstedTextureType.Leaf2, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_Leaf2"),
         ((byte)ShadowflameStemRequstedTextureType.Leaf3, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_Leaf3"),
         ((byte)ShadowflameStemRequstedTextureType.SmallStemMap, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_SmallStemMap"),
         ((byte)ShadowflameStemRequstedTextureType.MediumStemMap, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_MediumStemMap"),
         ((byte)ShadowflameStemRequstedTextureType.LargeStemMap, ResourceManager.NatureProjectileTextures + "ShadowflameClaws_LargeStemMap")];

    public struct ShadowflameLeafInfo(Vector2 position, ShadowflameLeafInfo.LeafType type, ushort apprearanceTime, bool flip) {
        public enum LeafType : byte {
            Small,
            Medium, 
            Large
        }

        public Vector2 Position = position;
        public LeafType Type = type;
        public ushort AppearanceTime = apprearanceTime;
        public bool Flip = flip;

        public readonly Vector2 GetSize() {
            switch (Type) {
                default:
                    return new Vector2(20, 14);
                case LeafType.Medium:
                    return new Vector2(24);
                case LeafType.Large:
                    return new Vector2(36);
            }
        }
    }

    public ref struct ShadowflameStemValues(Projectile projectile) {
        public enum StemType : byte {
            Small,
            Medium,
            Large,
            Count
        }

        public ref float InitOnSpawnValue = ref projectile.localAI[0];
        public ref float TargetWhoAmIValue = ref projectile.ai[0];
        public ref float StemTypeValue = ref projectile.ai[1];
        public ref float DirectionValue = ref projectile.ai[2];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public StemType Type {
            readonly get => (StemType)StemTypeValue;
            set {
                byte frameToSet = Utils.Clamp((byte)value, (byte)StemType.Small, (byte)(StemType.Count - 1));
                StemTypeValue = frameToSet;
            }
        }

        public bool FacedRight {
            readonly get => DirectionValue != 0f;
            set => DirectionValue = value.ToInt();
        }

        public readonly NPC Target => Main.npc[(int)TargetWhoAmIValue];
        public readonly bool IsTargetActive => Target.active;
    }

    public ushort StemDrawTimeLeftStart => (ushort)(TIMELEFT - 30);
    public bool ShouldDrawMark => Projectile.timeLeft > StemDrawTimeLeftStart;

    private static List<Vector2>? _smallStemLeafPositions, _mediumStemLeafPositions, _largeStemLeafPositions;
    private static Vector2 _smallStemSize, _mediumStemSize, _largeStemSize;

    private readonly List<ShadowflameLeafInfo> _leafData = [];

    void IPostSetupContent.PostSetupContent() {
        Main.QueueMainThreadAction(() => {
            if (!Main.dedServ && AssetInitializer.TryGetRequestedTextureAssets<ShadowflameStem>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
                Texture2D smallStemTextureMap = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.SmallStemMap].Value;
                _smallStemLeafPositions = smallStemTextureMap.GetColorMap(Vector2.Zero, 0f, 1f, distanceToPreviousToBeAdded: 10f);
                _smallStemSize = smallStemTextureMap.Size();
                Texture2D mediumStemTextureMap = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.MediumStemMap].Value;
                _mediumStemLeafPositions = mediumStemTextureMap.GetColorMap(Vector2.Zero, 0f, 1f, distanceToPreviousToBeAdded: 10f);
                _mediumStemSize = mediumStemTextureMap.Size();
                Texture2D largeStemTextureMap = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.LargeStemMap].Value;
                _largeStemLeafPositions = largeStemTextureMap.GetColorMap(Vector2.Zero, 0f, 1f, distanceToPreviousToBeAdded: 10f);
                _largeStemSize = largeStemTextureMap.Size();
            }
        });
    }

    public override void Unload() {
        _smallStemLeafPositions?.Clear(); _smallStemLeafPositions = null;
        _mediumStemLeafPositions?.Clear(); _mediumStemLeafPositions = null;
        _largeStemLeafPositions?.Clear(); _largeStemLeafPositions = null;
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: false, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(0);

        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.aiStyle = -1;

        Projectile.penetrate = -1;

        Projectile.manualDirectionChange = true;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 20;

        Projectile.netImportant = true;
    }
    
    private Vector2 GetLeafPositionByType(float progress) {
        progress = Utils.Clamp(progress, 0.05f, 0.85f);
        ShadowflameStemValues shadowflameStemValues = new(Projectile);
        bool facedRight = !shadowflameStemValues.FacedRight;
        Vector2 result = Vector2.Zero;
        switch (shadowflameStemValues.Type) {
            case ShadowflameStemValues.StemType.Small:
                foreach (Vector2 position in _smallStemLeafPositions!) {
                    Vector2 position2 = position;
                    if (facedRight) {
                        position2.X = _smallStemSize.X - position.X;
                        position2.X -= _smallStemSize.X;
                        position2.X -= 4f;
                    }
                    position2.X += 2f;
                    position2.Y -= _smallStemSize.Y / 3f;
                    float max = _smallStemSize.Y * 0.88f;
                    if (max - MathF.Abs(position2.Y) < max * progress) {
                        continue;
                    }
                    return position2;
                }
                return result;
            case ShadowflameStemValues.StemType.Medium:
                foreach (Vector2 position in _mediumStemLeafPositions!) {
                    Vector2 position2 = position;
                    if (facedRight) {
                        position2.X = _mediumStemSize.X - position.X;
                        position2.X -= _mediumStemSize.X;
                        position2.X -= 4f;
                    }
                    position2.X += 2f;
                    position2.Y -= _mediumStemSize.Y / 3f;
                    position2.Y -= 8f;
                    float max = _mediumStemSize.Y * 0.88f;
                    if (max - MathF.Abs(position2.Y) < max * progress) {
                        continue;
                    }
                    return position2;
                }
                return result;
            case ShadowflameStemValues.StemType.Large:
                foreach (Vector2 position in _largeStemLeafPositions!) {
                    Vector2 position2 = position;
                    if (facedRight) {
                        position2.X = _largeStemSize.X - position.X;
                        position2.X -= _largeStemSize.X;
                        position2.X -= 4f;
                    }
                    position2.X += 2f;
                    position2.Y -= _largeStemSize.Y / 3f;
                    position2.Y -= 10f;
                    float max = _largeStemSize.Y * 0.88f;
                    if (max - MathF.Abs(position2.Y) < max * progress) {
                        continue;
                    }
                    return position2;
                }
                return result;
        }

        return result;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        ushort count = (ushort)(_leafData.Count - 1);
        writer.Write(count);
        for (int i = 0; i < count; i++) {
            var leafData = _leafData[i];
            writer.WriteVector2(leafData.Position);
            writer.Write((byte)leafData.Type);
            writer.Write(leafData.AppearanceTime);
            writer.Write(leafData.Flip);
        }
        _leafData.Clear();
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        ushort count = reader.ReadUInt16();
        for (int i = 0; i < count; i++) {
            Vector2 position = reader.ReadVector2();
            ShadowflameLeafInfo.LeafType type = (ShadowflameLeafInfo.LeafType)reader.ReadByte();
            ushort appearanceTime = reader.ReadUInt16();
            bool flip = reader.ReadBoolean();
            _leafData.Add(new ShadowflameLeafInfo(position, type, appearanceTime, flip));
        }
    }

    public override void AI() {
        void init() {
            ShadowflameStemValues shadowflameStemValues = new(Projectile);
            if (!shadowflameStemValues.Init) {
                shadowflameStemValues.Init = true;

                if (Projectile.IsOwnerLocal()) {
                    shadowflameStemValues.Type = Main.rand.GetRandomEnumValue<ShadowflameStemValues.StemType>(1);
                    shadowflameStemValues.FacedRight = Main.rand.NextBool();

                    for (float i = 1f; i > 0f;) {
                        float lerp = 0.125f;
                        var type = Main.rand.GetRandomEnumValue<ShadowflameLeafInfo.LeafType>();
                        switch (shadowflameStemValues.Type) {
                            case ShadowflameStemValues.StemType.Small:
                                type = Main.rand.GetRandomEnumValue<ShadowflameLeafInfo.LeafType>(1);
                                break;
                            case ShadowflameStemValues.StemType.Medium:
                                lerp = 0.0875f;
                                break;
                            case ShadowflameStemValues.StemType.Large:
                                lerp = 0.05f;
                                break;
                        }
                        i -= lerp;
                        if (!Main.rand.NextChance(0.75f)) {
                            continue;
                        }
                        float offset = 10f;
                        switch (type) {
                            case ShadowflameLeafInfo.LeafType.Medium:
                                offset = 12.5f;
                                break;
                            case ShadowflameLeafInfo.LeafType.Large:
                                offset = 15f;
                                break;
                        }
                        _leafData.Add(new ShadowflameLeafInfo(GetLeafPositionByType(i + Main.rand.NextFloatRange(0.05f)) + Main.rand.Random2(offset), type, (ushort)Main.rand.Next(20), Main.rand.NextBool()));
                    }

                    Projectile.netUpdate = true;
                }
            }
        }
        void sitOnTarget() {
            ShadowflameStemValues shadowflameStemValues = new(Projectile);
            if (!shadowflameStemValues.IsTargetActive) {
                Projectile.Kill();
                return;
            }

            NPC target = shadowflameStemValues.Target;
            Projectile.Center = target.Center;
            Projectile.SetDirection((int)shadowflameStemValues.DirectionValue);
        }

        init();
        sitOnTarget();
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(153, Main.rand.Next(60, 181));
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(153, Main.rand.Next(60, 181));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (ShouldDrawMark) {
            return false;
        }

        for (float i = 0; i < 1f; i += 0.1f) {
            if (GeometryUtils.CenteredSquare(Projectile.Center + GetLeafPositionByType(i), 6).Intersects(targetHitbox)) {
                return true;
            }
        }
        foreach (var leafInfo in _leafData) {
            if (GeometryUtils.CenteredSquare(Projectile.Center + leafInfo.Position, leafInfo.GetSize()).Intersects(targetHitbox)) {
                return true;
            }
        }

        if (GeometryUtils.CenteredSquare(Projectile.Center, 10).Intersects(targetHitbox)) {
            return true;
        }

        return false;
    }

    public override void OnKill(int timeLeft) {
        for (float i = 0; i < 1f; i += 0.1f) {
            for (int num389 = 0; num389 < 3; num389++) {
                int num390 = Dust.NewDust(Projectile.Center - Vector2.UnitY * 12.5f + GetLeafPositionByType(i) + Main.rand.RandomPointInArea(6) / 2f, 0, 0, DustID.Shadowflame, Alpha: 125);
                Main.dust[num390].noGravity = true;
                Main.dust[num390].fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                Main.dust[num390].velocity *= 0.85f + Main.rand.NextFloatRange(0.1f);
                Main.dust[num390].scale *= 1.1f;
                Dust dust2 = Main.dust[num390];
                dust2 = Main.dust[num390];
                dust2 = Main.dust[num390];
                dust2.scale += (float)Main.rand.Next(150) * 0.001f;
            }
        }
        foreach (var leafInfo in _leafData) {
            for (int num389 = 0; num389 < 8; num389++) {
                int num390 = Dust.NewDust(Projectile.Center - Vector2.UnitY * 12.5f + leafInfo.Position + Main.rand.RandomPointInArea(leafInfo.GetSize()) * 0.625f, 0, 0, DustID.Shadowflame, Alpha: 125);
                Main.dust[num390].noGravity = true;
                Main.dust[num390].fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                Main.dust[num390].velocity *= 0.85f + Main.rand.NextFloatRange(0.1f);
                Main.dust[num390].scale *= 1.1f;
                Dust dust2 = Main.dust[num390];
                dust2 = Main.dust[num390];
                dust2 = Main.dust[num390];
                dust2.scale += (float)Main.rand.Next(150) * 0.001f;
            }
        }
        for (int num389 = 0; num389 < 9; num389++) {
            int num390 = Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(10), 0, 0, DustID.Shadowflame, Alpha: 125);
            Main.dust[num390].noGravity = true;
            Main.dust[num390].fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
            Main.dust[num390].velocity *= 0.85f + Main.rand.NextFloatRange(0.1f);
            Main.dust[num390].scale *= 1.1f;
            Dust dust2 = Main.dust[num390];
            dust2 = Main.dust[num390];
            dust2 = Main.dust[num390];
            dust2.scale += (float)Main.rand.Next(150) * 0.001f;
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<ShadowflameStem>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        float idleOffset = Helper.Wave(-1f, 1f, 2.5f, Projectile.whoAmI) * 0.35f;

        SpriteBatch batch = Main.spriteBatch;
        Color color = lightColor;
        int timeLeft = Projectile.timeLeft;

        const float STEAMAPPEARANCETIME = 10;
        void drawMark() {
            Projectile proj = Projectile;
            Microsoft.Xna.Framework.Color color35 = new Color(123, 89, 234);

            float num165 = (1f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 30f / 0.5f * ((float)Math.PI * 2f) * 3f)) * 0.8f;
            float scaleFactor = 1f - Utils.GetLerpValue(StemDrawTimeLeftStart + STEAMAPPEARANCETIME, StemDrawTimeLeftStart - STEAMAPPEARANCETIME, timeLeft, true);
            //scaleFactor = Utils.Remap(scaleFactor, 0f, 1f, 0.5f, 1f, true);
            Vector2 scale = Vector2.One * scaleFactor * (0.25f + num165);
            batch.DrawWithSnapshot(() => {
                Texture2D texture = ResourceManager.Circle5;
                Vector2 position = Projectile.Center;
                Rectangle clip = texture.Bounds;
                Vector2 origin = clip.Centered();
                for (int i = 0; i < 2; i++) {
                    batch.Draw(texture, position, DrawInfo.Default with {
                        Clip = clip,
                        Color = color35,
                        Origin = origin,
                        Scale = scale
                    });
                }
            }, blendState: BlendState.Additive);

            color35.A = 0;
            Vector2 vector30 = proj.Center - Main.screenPosition;
            Texture2D value16 = ResourceManager.DefaultSparkle;
            Microsoft.Xna.Framework.Color color36 = color35;
            Vector2 origin7 = value16.Size() / 2f;
            Microsoft.Xna.Framework.Color color37 = color35 * 0.5f;
            Vector2 vector31 = new Vector2(0.5f, 2f);
            Vector2 vector32 = new Vector2(0.5f, 1f);
            color36 *= num165;
            color37 *= num165;

            int num166 = 0;
            Vector2 position4 = vector30;
            var dir = SpriteEffects.None;
            vector31 *= scaleFactor;
            vector32 *= scaleFactor;
            float rotation = Helper.Wave(-MathHelper.PiOver4 * 0.05f, MathHelper.PiOver4 * 0.05f, 20f, Projectile.whoAmI);
            Main.EntitySpriteDraw(value16, position4, null, color36, (float)Math.PI / 2f + rotation, origin7, vector31, dir);
            Main.EntitySpriteDraw(value16, position4, null, color36, 0f + rotation, origin7, vector32, dir);
            Main.EntitySpriteDraw(value16, position4, null, color37, (float)Math.PI / 2f + rotation, origin7, vector31 * 0.6f, dir);
            Main.EntitySpriteDraw(value16, position4, null, color37, 0f + rotation, origin7, vector32 * 0.6f, dir);
        }
        void drawStemWithLeafs() {
            if (ShouldDrawMark) {
                return;
            }

            float opacity = 0.9f;

            Projectile proj = Projectile;
            Microsoft.Xna.Framework.Color color35 = new Color(123, 89, 234);
            color35.A = 0;
            Vector2 vector30 = proj.Center - Main.screenPosition;
            Texture2D value16 = ResourceManager.DefaultSparkle;
            Microsoft.Xna.Framework.Color color36 = color35;
            Vector2 origin7 = value16.Size() / 2f;
            Microsoft.Xna.Framework.Color color37 = color35 * 1f;
            float num165 = (1f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 30f / 0.5f * ((float)Math.PI * 2f) * 3f)) * 0.8f;
            Vector2 vector31 = new Vector2(0.5f, 2f);
            Vector2 vector32 = new Vector2(0.5f, 1f);
            color36 *= num165 * 1.5f;
            color37 *= num165 * 1.5f;
            int num166 = 0;

            float scaleFactor = Utils.GetLerpValue(StemDrawTimeLeftStart, StemDrawTimeLeftStart - STEAMAPPEARANCETIME, timeLeft, true);
            float appearanceScaleXFactor = Ease.CubeOut(scaleFactor),
                  appearanceScaleYFactor = Ease.CubeOut(scaleFactor);

            float colorFactor = Ease.CubeOut(Utils.GetLerpValue(StemDrawTimeLeftStart, StemDrawTimeLeftStart - STEAMAPPEARANCETIME * 3, timeLeft, true));
            color = Color.Lerp(color36, color, colorFactor);
            Color color2 = Color.Lerp(color37, color, colorFactor);

            ShadowflameStemValues shadowflameStemValues = new(Projectile);
            Texture2D texture = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.Stem].Value;
            SpriteFrame spriteFrame = new(3, 1, (byte)shadowflameStemValues.Type, 0);
            Rectangle sourceRectangle = spriteFrame.GetSourceRectangle(texture);
            Vector2 origin = sourceRectangle.BottomCenter();
            Vector2 position = Projectile.Center + Vector2.UnitY * 15f;
            Vector2 scale = Vector2.One;
            scale.X *= appearanceScaleXFactor;
            scale.Y *= appearanceScaleYFactor;
            SpriteEffects flip = Projectile.direction.ToSpriteEffects();

            Vector2 idleExtra = Vector2.One.RotatedBy(MathHelper.PiOver4 * idleOffset - MathHelper.Pi * 0.75f) * 2f;
            float idleRotation = idleOffset * 0.05f;
            float rotation = idleRotation;

            batch.Draw(texture, position + idleExtra, DrawInfo.Default with {
                Clip = sourceRectangle,
                Color = color * opacity,
                Origin = origin,
                Scale = scale,
                ImageFlip = flip,
                Rotation = rotation
            });
            batch.Draw(texture, position + idleExtra, DrawInfo.Default with {
                Clip = sourceRectangle,
                Color = color2 * opacity,
                Origin = origin,
                Scale = scale,
                ImageFlip = flip,
                Rotation = rotation
            });

            for (int i = _leafData.Count - 1; i > 0; i--) {
                var leafData = _leafData[i];
                Vector2 position2 = Projectile.Center + leafData.Position;
                color = Lighting.GetColor(position2.ToTileCoordinates());
                colorFactor = Ease.CubeOut(Utils.GetLerpValue(StemDrawTimeLeftStart - STEAMAPPEARANCETIME - leafData.AppearanceTime, StemDrawTimeLeftStart - STEAMAPPEARANCETIME * 2 - STEAMAPPEARANCETIME - leafData.AppearanceTime, timeLeft, true));
                color = Color.Lerp(color36, color, colorFactor);
                color2 = Color.Lerp(color37, color, colorFactor);

                Texture2D leafTexture = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.Leaf3].Value;
                if (leafData.Type == ShadowflameLeafInfo.LeafType.Medium) {
                    leafTexture = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.Leaf2].Value;
                }
                else if (leafData.Type == ShadowflameLeafInfo.LeafType.Large) {
                    leafTexture = indexedTextureAssets[(byte)ShadowflameStemRequstedTextureType.Leaf1].Value;
                }
                Rectangle clip = leafTexture.Bounds;
                Vector2 origin2 = clip.Size() / 2f;
                if (leafData.Flip) {
                    flip = flip == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                }

                //Dust.NewDustPerfect(Projectile.Center + leafData.Position, DustID.Adamantite, Vector2.Zero).noGravity = true;
                batch.Draw(leafTexture, position2 + idleExtra * 5f, DrawInfo.Default with {
                    Clip = clip,
                    Color = color * opacity,
                    Origin = origin2,
                    Scale = scale * colorFactor,
                    ImageFlip = flip,
                    Rotation = rotation
                });
            }
        }

        drawMark();
        drawStemWithLeafs();
    }
}
