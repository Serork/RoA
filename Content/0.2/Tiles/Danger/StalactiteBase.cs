using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Content.Tiles.Decorations;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Danger;

abstract class StalactiteProjectileBase : ModProjectile {
    private static byte FRAMECOUNT => 3;

    protected virtual float FallSpeedModifier => 1f;

    public override string Texture => ResourceManager.EnemyProjectileTextures + GetType().Name[..^10];

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

    public override void SetDefaults() {
        Projectile.SetSizeValues(16, 30);

        Projectile.hostile = true;
        Projectile.friendly = true;

        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;

        Projectile.timeLeft = 360;
        Projectile.tileCollide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.manualDirectionChange = true;

        Projectile.spriteDirection = 1;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.spriteDirection = 1;
    }

    public override bool PreDraw(ref Color lightColor) {
        Vector2 position = Projectile.position;
        Projectile.spriteDirection = 1;
        Projectile.position.Y -= 2f;
        Projectile.QuickDrawAnimated(lightColor);
        Projectile.position.Y = position.Y;

        return false;
    }

    public sealed override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        height = 20;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public sealed override void AI() {
        Projectile.direction = Projectile.spriteDirection = 1;
        if (Projectile.ai[0] != 0f) {
            Projectile.frame = (int)(Projectile.ai[0] / 16f);
            Projectile.ai[0] = 0f;
        }

        Projectile.velocity.Y += 0.35f * FallSpeedModifier;
        Projectile.velocity.Y = Math.Min(10f, Projectile.velocity.Y);
    }

    public sealed override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        modifiers.FinalDamage *= MathUtils.Clamp01(Projectile.velocity.Y / 7.5f);

        SafeModifyHitPlayer(target, ref modifiers);
    }

    protected virtual void SafeModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.FinalDamage *= MathUtils.Clamp01(Projectile.velocity.Y / 7.5f);

        SafeModifyHitNPC(target, ref modifiers);
    }

    protected virtual void SafeModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    public sealed override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, KillDustType());
        }
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
    }

    protected abstract ushort KillDustType();
}

abstract class StalactiteTE<T> : ModTileEntity where T : StalactiteProjectileBase {
    private static ushort PLACETIMEMIN => 0;
    private static ushort PLACETIMEMAX => 30;
    private static ushort DANGERAREAWIDTH => 100;
    private static ushort DANGERAREAHEIGHT => 500;
    private static float PLACEBASEVALUE => -1f;

    private float _placeTime = PLACEBASEVALUE;
    private bool _shouldFall;

    public override void Update() {
        if (WorldGen.gen) {
            return;
        }

        if (_placeTime == PLACEBASEVALUE) {
            _placeTime = Main.rand.NextFloat(PLACETIMEMIN, PLACETIMEMAX);
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            return;
        }

        Vector2 stalactitePosition = Position.ToWorldCoordinates();
        Rectangle dangerArea = GeometryUtils.TopRectangle(stalactitePosition, new Vector2(DANGERAREAWIDTH, DANGERAREAHEIGHT));
        foreach (Player player in Main.ActivePlayers) {
            if (player.getRect().Intersects(dangerArea) && player.GetCommon().Fell) {
                _shouldFall = true;
                break;
            }
        }
        if (!_shouldFall) {
            foreach (NPC npc in Main.ActiveNPCs) {
                if (npc.noGravity) {
                    continue;
                }
                if (npc.getRect().Intersects(dangerArea) && npc.GetCommon().Fell) {
                    _shouldFall = true;
                    break;
                }
            }
        }
        if (_shouldFall && _placeTime > 0) {
            _placeTime--;
            return;
        }
        if (_shouldFall) {
            WorldGen.KillTile(Position.X, Position.Y);
        }
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(_placeTime);
    }

    public override void NetReceive(BinaryReader reader) {
        _placeTime = reader.ReadSingle();
    }

    public override bool IsTileValidForEntity(int x, int y) => true;

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

    public override bool IsTileDangerous(int i, int j, Player player) => true;

    public override bool CanDrop(int i, int j) => false;

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        Projectile.NewProjectileDirect(new EntitySource_TileBreak(i, j), new Point16(i, j).ToWorldCoordinates() + Vector2.UnitY * 6f + Vector2.UnitX, Vector2.Zero, ModContent.ProjectileType<T2>(), 100, 0f, Main.myPlayer, frameX);
    }

    public sealed override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        SoundEngine.PlaySound(SoundID.Dig, new Point16(i, j).ToWorldCoordinates());

        ModContent.GetInstance<T1>().Kill(i, j);
    }

    protected virtual void SafeSetStaticDefaults() { }
}
