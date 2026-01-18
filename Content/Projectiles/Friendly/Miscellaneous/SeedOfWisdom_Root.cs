using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

[Tracked]
sealed class SeedOfWisdomRoot : ModProjectile_NoTextureLoad, IRequestAssets, IPostSetupContent {
    public enum SeedOfWisdomRoot_RequstedTextureType : byte {
        Root,
        Root_Map
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)SeedOfWisdomRoot_RequstedTextureType.Root, ResourceManager.MiscellaneousProjectileTextures + "SeedOfWisdomRoot"),
         ((byte)SeedOfWisdomRoot_RequstedTextureType.Root_Map, ResourceManager.MiscellaneousProjectileTextures + "SeedOfWisdomRoot_Map")];

    void IPostSetupContent.PostSetupContent() {
        Main.QueueMainThreadAction(() => {
            if (!Main.dedServ && AssetInitializer.TryGetRequestedTextureAssets<SeedOfWisdomRoot>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
                Texture2D rootMap = indexedTextureAssets[(byte)SeedOfWisdomRoot_RequstedTextureType.Root_Map].Value;
                _rootMapPositions = rootMap.GetColorMap(Vector2.Zero, 0f, 1f, addOffset: false);
            }
        });
    }

    public struct RootPseudoTileInfo(Point16 position) {
        public Point16 Position = position;
        public Point16 FrameCoords = Point16.NegativeOne;
    }

    public RootPseudoTileInfo[] RootPseudoTileData { get; private set; } = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float RootChoice => ref Projectile.ai[1];
    public ref float ReversedValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Reversed {
        get => ReversedValue == 1f;
        set => ReversedValue = value.ToInt();
    }

    private static List<Vector2> _rootMapPositions = null!;

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (!Projectile.GetOwnerAsPlayer().GetCommon().StandingStill) {
            Projectile.Kill();
        }

        void init() {
            if (!Init) {
                Init = true;

                if (Projectile.IsOwnerLocal()) {
                    RootChoice = Main.rand.Next(5);
                    Reversed = Main.rand.NextBool();
                    Projectile.netUpdate = true;
                }

                HashSet<Point16> positions = [];
                int rootSize = 29;
                foreach (Vector2 position in _rootMapPositions) {
                    float x = position.X;
                    int x2 = rootSize * (int)RootChoice,
                        x3 = rootSize * ((int)RootChoice + 1);
                    if (!(x >= x2 && x <= x3)) {
                        continue;
                    }
                    Vector2 position2 = Projectile.Center + new Vector2(Reversed ? (rootSize - x % rootSize) : (x % rootSize), position.Y) * TileHelper.TileSize - Vector2.UnitX * rootSize * TileHelper.TileSize * 0.5f;
                    if (Reversed) {
                        position2.X -= TileHelper.TileSize;
                    }
                    positions.Add(position2.ToTileCoordinates16());
                }
                RootPseudoTileData = new RootPseudoTileInfo[positions.Count];
                for (int i = 0; i < RootPseudoTileData.Length; i++) {
                    RootPseudoTileData[i] = new RootPseudoTileInfo(positions.ToList()[i]);
                }
            }
        }
        init();
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<SeedOfWisdomRoot>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        if (!Init) {
            return;
        }

        Texture2D texture = indexedTextureAssets[(byte)SeedOfWisdomRoot_RequstedTextureType.Root].Value;

        for (int i = 0; i < RootPseudoTileData.Length; i++) {
            ref RootPseudoTileInfo rootPseudoTileInfo = ref RootPseudoTileData[i];
            Point16 tilePosition = rootPseudoTileInfo.Position;
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
            if (rootPseudoTileInfo.FrameCoords == Point16.NegativeOne) {
                rootPseudoTileInfo.FrameCoords = IceBlock.GetTileFrame(hasLeft, hasRight, hasTop, hasBottom);
            }
            Color color = Color.White;
            clip = new(rootPseudoTileInfo.FrameCoords.X, rootPseudoTileInfo.FrameCoords.Y, 16, 16);
            Point position = tilePosition.ToPoint();
            DrawUtils.SingleTileDrawInfo info = new(texture, position, clip, color, 0, false);
            DrawUtils.DrawSingleTile(info);
        }
    }

    private static HashSet<Point16> GetRootTilePositions(Predicate<Point16>? filter = null) {
        HashSet<Point16> result = [];
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<SeedOfWisdomRoot>()) {
            foreach (RootPseudoTileInfo rootPseudoTileInfo in projectile.As<SeedOfWisdomRoot>().RootPseudoTileData) {
                Point16 position = rootPseudoTileInfo.Position;
                if (filter == null || filter(position)) {
                    result.Add(position);
                }
            }
        }
        return result;
    }
}
