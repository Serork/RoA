using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Decorations;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Danger;

abstract class StalactiteProjectileBase : ModProjectile {
    private static byte FRAMECOUNT => 3;

    public override string Texture => ResourceManager.EnemyProjectileTextures + GetType().Name[..^10];

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

    public override void SetDefaults() {
        Projectile.SetSizeValues(16, 30);

        Projectile.hostile = true;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;

        Projectile.timeLeft = 360;
        Projectile.tileCollide = true;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.35f;
        Projectile.velocity.Y = Math.Min(10f, Projectile.velocity.Y);
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, KillDustType());
        }
    }

    protected abstract ushort KillDustType();
}

abstract class StalactiteTE<T> : ModTileEntity where T : StalactiteProjectileBase {
    private int _placeTime = -1;

    public override void Update() {
        if (_placeTime == -1) {
            _placeTime = 300;
            return;
        }
        else if (_placeTime > 0) {
            _placeTime--;
            return;
        }

        Vector2 stalactitePosition = Position.ToWorldCoordinates();
        Rectangle dangerArea = GeometryUtils.TopRectangle(stalactitePosition, new Vector2(300, 300));
        bool shouldFall = false;
        foreach (Player player in Main.ActivePlayers) {
            if (player.getRect().Intersects(dangerArea)) {
                shouldFall = true;
                break;
            }
        }
        if (shouldFall) {
            WorldGen.KillTile(Position.X, Position.Y);

            Projectile.NewProjectileDirect(new EntitySource_TileEntity(this), Position.ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<T>(), 100, 0f, Main.myPlayer);
        }
    }

    public override bool IsTileValidForEntity(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        return tile.HasTile && TileLoader.GetTile(tile.TileType) is StalactiteBase<StalactiteTE<T>, T>;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        int id = Place(i, j);
        return id;
    }
}

abstract class StalactiteBase<T1, T2> : ModTile where T1 : StalactiteTE<T2> where T2 : StalactiteProjectileBase {
    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.CoordinateHeights = [18, 18];
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.DrawYOffset = -2;
        TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<T1>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);

        SafeSetStaticDefaults();

        HitSound = null;
        DustType = -1;
    }

    public sealed override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<T1>().Kill(i, j);

    protected virtual void SafeSetStaticDefaults() { }
}
