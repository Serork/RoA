using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Wreath;
using RoA.Common.Projectiles;
using RoA.Content.Buffs;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;

using static RoA.Content.Items.Equipables.Accessories.RagingBoots;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class SeedOfWisdomRoot : NatureProjectile_NoTextureLoad, IRequestAssets, IPostSetupContent {
    public enum SeedOfWisdomRoot_RequstedTextureType : byte {
        Root,
        Root_Map,
        Root_Map2
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)SeedOfWisdomRoot_RequstedTextureType.Root, ResourceManager.MiscellaneousProjectileTextures + "SeedOfWisdomRoot"),
         ((byte)SeedOfWisdomRoot_RequstedTextureType.Root_Map, ResourceManager.MiscellaneousProjectileTextures + "SeedOfWisdomRoot_Map"),
         ((byte)SeedOfWisdomRoot_RequstedTextureType.Root_Map2, ResourceManager.MiscellaneousProjectileTextures + "SeedOfWisdomRoot_Map2")];

    void IPostSetupContent.PostSetupContent() {
        Main.QueueMainThreadAction(() => {
            if (!Main.dedServ && AssetInitializer.TryGetRequestedTextureAssets<SeedOfWisdomRoot>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
                Texture2D rootMap = indexedTextureAssets[(byte)SeedOfWisdomRoot_RequstedTextureType.Root_Map].Value;
                _rootMapPositions = rootMap.GetColorMap(Vector2.Zero, 0f, 1f, addOffset: false);
                Texture2D rootMap2 = indexedTextureAssets[(byte)SeedOfWisdomRoot_RequstedTextureType.Root_Map2].Value;
                _rootMapPositions2 = rootMap2.GetColorMap(Vector2.Zero, 0f, 1f, addOffset: false);
            }
        });
    }

    public struct RootPseudoTileInfo(Point16 position) {
        public Point16 Position = position;
        public Point16 FrameCoords = Point16.NegativeOne;
        public float Progress;
        public bool SpawnDusts;
    }

    public RootPseudoTileInfo[] RootPseudoTileData { get; private set; } = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float ScaleFactorValue => ref Projectile.localAI[1];
    public ref float IncreasedValue => ref Projectile.ai[0];
    public ref float RootChoice => ref Projectile.ai[1];
    public ref float SpawnedFromLandingValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Increased {
        get => IncreasedValue == 1f;
        set => IncreasedValue = value.ToInt();
    }

    public bool SpawnedFromLanding => SpawnedFromLandingValue > 0f;

    private static List<Vector2> _rootMapPositions = null!,
                                 _rootMapPositions2 = null!;
    private bool _shouldBeKilled;
    private float _growSpeed;
    private bool _reversed; // sync
    private float _scaleFactor;
    private float _allProgress;

    public override void Unload() {
        _rootMapPositions.Clear();
        _rootMapPositions = null!;
    }

    public override bool? CanDamage() => base.CanDamage();
    public override bool? CanCutTiles() => false;

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.Opacity = 0f;

        Projectile.penetrate = -1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    protected override bool ShouldAttachNatureWeaponOnSpawn() => false;

    protected override void SafeOnSpawn(IEntitySource source) {
        Item? bootsItem = Projectile.GetOwnerAsPlayer().GetModPlayer<RagingBootsAttackHandler>().Boots;
        NatureProjectileSetItem(Projectile, bootsItem);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        foreach (RootPseudoTileInfo rootPseudoTileInfo in RootPseudoTileData) {
            if (rootPseudoTileInfo.Progress < 0.01f) {
                continue;
            }
            Point16 tilePosition = rootPseudoTileInfo.Position;
            Vector2 worldPosition = tilePosition.ToWorldCoordinates() - Vector2.One * TileHelper.TileSize * 0.5f;
            if (new Rectangle((int)worldPosition.X, (int)worldPosition.Y, (int)TileHelper.TileSize, (int)TileHelper.TileSize).Intersects(targetHitbox)) {
                return true;
            }
        }

        return false;
    }

    public override void AI() {
        if (!SpawnedFromLanding) {
            Projectile.timeLeft = 2;
        }

        if (!SpawnedFromLanding && !Projectile.GetOwnerAsPlayer().GetCommon().StandingStill) {
            _shouldBeKilled = true;
        }

        if (SpawnedFromLanding && Projectile.timeLeft <= 2) {
            _shouldBeKilled = true;
            Projectile.timeLeft = 2;
        }

        void init() {
            if (!Init) {
                Init = true;

                float extra = 0.075f * SpawnedFromLandingValue / 100f;
                _growSpeed = 0.5f + extra;

                if (SpawnedFromLanding) {
                    Projectile.timeLeft = (int)SpawnedFromLandingValue;
                }

                if (Projectile.IsOwnerLocal()) {

                    RootChoice = Increased ? Main.rand.Next(6) : Main.rand.Next(5);
                    _reversed = Main.rand.NextBool();
                    Projectile.netUpdate = true;
                }

                List<Point16> positions = [];
                if (!Increased) {
                    int rootSize = 29;
                    foreach (Vector2 position in _rootMapPositions) {
                        float x = position.X;
                        int x2 = rootSize * (int)RootChoice,
                            x3 = rootSize * ((int)RootChoice + 1);
                        if (!(x >= x2 && x <= x3)) {
                            continue;
                        }
                        Vector2 position2 = Projectile.Center + new Vector2(_reversed ? (rootSize - x % rootSize) : (x % rootSize), position.Y) * TileHelper.TileSize - Vector2.UnitX * rootSize * TileHelper.TileSize * 0.5f;
                        if (_reversed) {
                            position2.X -= TileHelper.TileSize / 2f;
                            position2.Y -= TileHelper.TileSize;
                        }
                        positions.Add(position2.ToTileCoordinates16());
                    }
                    positions = [.. positions.OrderByDescending(x => MathF.Abs(x.Y - Projectile.Center.Y))];
                }
                else {
                    int rootSize = 19;
                    foreach (Vector2 position in _rootMapPositions2) {
                        float x = position.X;
                        int x2 = rootSize * (int)RootChoice,
                            x3 = rootSize * ((int)RootChoice + 1);
                        if (!(x >= x2 && x <= x3)) {
                            continue;
                        }
                        Vector2 position2 = Projectile.Center + new Vector2(_reversed ? (rootSize - x % rootSize) : (x % rootSize), position.Y) * TileHelper.TileSize - Vector2.UnitX * rootSize * TileHelper.TileSize * 0.5f;
                        if (_reversed) {
                            position2.X -= TileHelper.TileSize / 2f;
                            position2.Y -= TileHelper.TileSize;
                        }
                        position2.Y -= rootSize * TileHelper.TileSize * 0.5f;
                        positions.Add(position2.ToTileCoordinates16());
                    }
                    positions = [.. positions.OrderBy(x => x.ToWorldCoordinates().Distance(Projectile.Center))];
                }
                RootPseudoTileData = new RootPseudoTileInfo[positions.Count];
                for (int i = 0; i < RootPseudoTileData.Length; i++) {
                    RootPseudoTileData[i] = new RootPseudoTileInfo(positions.ToList()[i]);
                }
            }
        }
        void processRootTiles() {
            float allProgress = 0f;
            int count = RootPseudoTileData.Length;
            for (int i = 0; i < count; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                ref RootPseudoTileInfo currentRootTileInfo = ref RootPseudoTileData[currentSegmentIndex];
                ref RootPseudoTileInfo previousRootTileInfo = ref RootPseudoTileData[previousSegmentIndex];
                Point16 tilePosition = currentRootTileInfo.Position;
                float progress = 0f;
                if (i > 0) {
                    HashSet<Point16> allRootTilePositions = GetRootTilePositions((rootTilePosition) => TileHelper.ArePositionsAdjacent(rootTilePosition, new Point16(tilePosition.X, tilePosition.Y), 1));
                    Point16 right = tilePosition + new Point16(1, 0);
                    Point16 left = tilePosition - new Point16(1, 0);
                    Point16 bottom = tilePosition + new Point16(0, 1);
                    Point16 top = tilePosition - new Point16(0, 1);
                    bool hasRight = allRootTilePositions.Contains(right);
                    bool hasLeft = allRootTilePositions.Contains(left);
                    bool hasBottom = allRootTilePositions.Contains(bottom);
                    bool hasTop = allRootTilePositions.Contains(top);
                    if (hasLeft) {
                        RootPseudoTileInfo leftRootTileInfo = RootPseudoTileData.FirstOrDefault(x => x.Position == left);
                        progress += leftRootTileInfo.Progress;
                    }
                    if (hasRight) {
                        RootPseudoTileInfo leftRootTileInfo = RootPseudoTileData.FirstOrDefault(x => x.Position == right);
                        progress += leftRootTileInfo.Progress;
                    }
                    if (hasBottom) {
                        RootPseudoTileInfo leftRootTileInfo = RootPseudoTileData.FirstOrDefault(x => x.Position == bottom);
                        progress += leftRootTileInfo.Progress;
                    }
                    if (hasTop) {
                        RootPseudoTileInfo leftRootTileInfo = RootPseudoTileData.FirstOrDefault(x => x.Position == top);
                        progress += leftRootTileInfo.Progress;
                    }
                }
                if (currentSegmentIndex > 0 && progress < 1f) {
                    continue;
                }
                float growthSpeed = _growSpeed;
                if (currentRootTileInfo.Progress >= 0.75f && !currentRootTileInfo.SpawnDusts) {
                    if (!Main.rand.NextBool(3)) {
                        Vector2 position = currentRootTileInfo.Position.ToWorldCoordinates() - Vector2.One * 8f;
                        Tile tile = WorldGenHelper.GetTileSafely(currentRootTileInfo.Position.X, currentRootTileInfo.Position.Y);
                        if (tile.HasTile) {
                            int dust = Dust.NewDust(position, 16, 16, TileHelper.GetKillTileDust(currentRootTileInfo.Position.X, currentRootTileInfo.Position.Y, tile));
                            Vector2 pos1 = currentRootTileInfo.Position.ToWorldCoordinates();
                            Vector2 pos2 = previousRootTileInfo.Position.ToWorldCoordinates();
                            Main.dust[dust].velocity += -pos1.DirectionTo(pos2) * 1f;
                        }
                    }
                    currentRootTileInfo.SpawnDusts = true;
                }
                currentRootTileInfo.Progress = Helper.Approach(currentRootTileInfo.Progress, 1f, growthSpeed);
                allProgress += currentRootTileInfo.Progress;
            }
            allProgress /= count;

            //_allProgress = allProgress;
            _allProgress = Helper.Approach(_allProgress, allProgress, 0.1f);

            float clampedAllProgress = MathUtils.Clamp01(allProgress);
            if (!SpawnedFromLanding) {
                Player player = Projectile.GetOwnerAsPlayer();
                player.statDefense += (int)(clampedAllProgress * 25);
                player.AddBuff<WiseDefense>(2);
            }
            else {
                foreach (Player player in Main.ActivePlayers) {
                    if (!player.IsAlive()) {
                        continue;
                    }
                    if (player.Distance(Projectile.Center) > TileHelper.TileSize * 9) {
                        continue;
                    }

                    player.statDefense += (int)(clampedAllProgress * 25);
                    player.AddBuff<WiseDefense>(2);
                }
            }

            if (!_shouldBeKilled) {
                Projectile.Opacity = Ease.CubeOut(clampedAllProgress);
            }
            else {
                Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.2f);
                if (Projectile.Opacity <= 0f) {
                    Projectile.Kill();
                }
            }

            _growSpeed = Helper.Approach(_growSpeed, 0.25f, 0.0175f);
        }

        init();
        processRootTiles();

        Projectile.localAI[2] = 1f;
        _scaleFactor = 1f;

        float progress = MathHelper.Clamp(_scaleFactor, 0f, 1f);
        float opacity = 1f;
        float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 12f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
        ScaleFactorValue = MathHelper.Lerp(ScaleFactorValue, factor, ScaleFactorValue < factor ? 0.1f : 0.025f);
    }

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<SeedOfWisdomRoot>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        if (!Init) {
            return;
        }

        Texture2D texture = indexedTextureAssets[(byte)SeedOfWisdomRoot_RequstedTextureType.Root].Value;

        Texture2D circleTexture = ResourceManager.Circle,
                  circleTexture2 = ResourceManager.Circle3;

        Color getColor() {
            Color color = WreathHandler.GetCurrentColor(Projectile.GetOwnerAsPlayer());
            if (_shouldBeKilled) {
                color = Color.Lerp(color, color.ModifyRGB(0.375f), Ease.CubeOut(Projectile.Opacity));
            }
            return color;
        }

        if (SpawnedFromLanding) {
            Vector2 circlePosition = Projectile.Center - Vector2.One * TileHelper.TileSize * new Vector2(1f, 1f) + Vector2.UnitX * TileHelper.TileSize * 1f;
            Rectangle circleClip = circleTexture.Bounds;
            Vector2 circleOrigin = circleClip.Centered();
            Vector2 circleSize = Vector2.One * Ease.CubeOut(_allProgress) * 1.5f;
            Color circleColor = getColor() * MathHelper.Lerp(0.75f, 0f, Ease.CubeOut(_allProgress));
            DrawInfo circleDrawInfo = new() {
                Clip = circleClip,
                Origin = circleOrigin,
                Scale = circleSize,
                Color = circleColor
            };
            SpriteBatch batch = Main.spriteBatch;
            batch.DrawWithSnapshot(circleTexture, circlePosition, circleDrawInfo, blendState: BlendState.Additive);

            Rectangle circle2Clip = circleTexture2.Bounds;
            Vector2 circle2Origin = circle2Clip.Centered();
            Vector2 circle2Size = Vector2.One * Ease.CubeOut(_allProgress) * 2.75f;
            Color circle2Color = getColor() * MathHelper.Lerp(0.75f, 0f, Ease.CubeOut(_allProgress));
            DrawInfo circle2DrawInfo = new() {
                Clip = circle2Clip,
                Origin = circle2Origin,
                Scale = circle2Size,
                Color = circle2Color
            };
            batch.DrawWithSnapshot(circleTexture2, circlePosition, circle2DrawInfo, blendState: BlendState.Additive);
        }

        Vector2 drawOffset = -Vector2.UnitY * 8f;

        drawOffset *= 0f;

        float progress = MathHelper.Clamp(_scaleFactor, 0f, 1f);
        float opacity = 1f;

        float factor = ScaleFactorValue * Projectile.localAI[2];

        for (int i = 0; i < RootPseudoTileData.Length; i++) {
            ref RootPseudoTileInfo rootTileInfo = ref RootPseudoTileData[i];
            Point16 tilePosition = rootTileInfo.Position;
            HashSet<Point16> allRootTilePositions = GetRootTilePositions((rootTilePosition) => TileHelper.ArePositionsAdjacent(rootTilePosition, new Point16(tilePosition.X, tilePosition.Y), 1));
            Rectangle clip;
            Point16 right = tilePosition + new Point16(1, 0);
            Point16 left = tilePosition - new Point16(1, 0);
            Point16 bottom = tilePosition + new Point16(0, 1);
            Point16 top = tilePosition - new Point16(0, 1);
            bool hasRight = allRootTilePositions.Contains(right);
            bool hasLeft = allRootTilePositions.Contains(left);
            bool hasBottom = allRootTilePositions.Contains(bottom);
            bool hasTop = allRootTilePositions.Contains(top);
            if (rootTileInfo.FrameCoords == Point16.NegativeOne) {
                rootTileInfo.FrameCoords = IceBlock.GetTileFrame(hasLeft, hasRight, hasTop, hasBottom);
            }
            Point position = tilePosition.ToPoint();
            Color color = getColor();
            color = color.MultiplyRGB(Lighting.GetColor(position));
            float opacity2 = rootTileInfo.Progress;
            if (_shouldBeKilled) {
                opacity *= Projectile.Opacity;
            }
            clip = new(rootTileInfo.FrameCoords.X, rootTileInfo.FrameCoords.Y, 16, (int)(16 * (!hasTop && !hasBottom ? 1f : Ease.CubeOut(rootTileInfo.Progress))));
            DrawUtils.SingleTileDrawInfo info = new(texture, position, clip, color * rootTileInfo.Progress * opacity2, 0, false);
            DrawUtils.DrawSingleTile(info, drawOffset: drawOffset);

            color *= 1f;
            color.A = 70;
            color *= opacity;
            opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
            color *= factor * opacity * 2f;
            color *= 1.5f;
            float scale = Projectile.scale + factor * 0.035f;
            for (int i2 = 0; i2 < 3; i2++) {
                info = new(texture, position, clip, color.MultiplyAlpha(scale) * 0.75f * rootTileInfo.Progress * opacity2, 0, false);
                DrawUtils.DrawSingleTile(info, scale, drawOffset: drawOffset);
            }

            float scale2 = 1f + (float)(0.15f * Math.Sin(Main.timeForVisualEffects / 10.0));
            info = new(texture, position, clip, color.MultiplyAlpha(scale) * 0.75f * rootTileInfo.Progress * opacity2, 0, false);
            DrawUtils.DrawSingleTile(info, scale2, drawOffset: drawOffset);
        }
    }

    private static HashSet<Point16> GetRootTilePositions(Predicate<Point16>? filter = null) {
        HashSet<Point16> result = [];
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<SeedOfWisdomRoot>()) {
            var seedProj = projectile.As<SeedOfWisdomRoot>();
            if (!seedProj.Init) {
                continue;
            }
            foreach (RootPseudoTileInfo rootPseudoTileInfo in seedProj.RootPseudoTileData) {
                Point16 position = rootPseudoTileInfo.Position;
                if (filter == null || filter(position)) {
                    result.Add(position);
                }
            }
        }
        return result;
    }
}
