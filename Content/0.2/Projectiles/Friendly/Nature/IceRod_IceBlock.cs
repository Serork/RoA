using Microsoft.Xna.Framework;

using RoA.Common.Projectiles;
using RoA.Content.Items.Placeable.Seeds;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

// TODO: separate block mechanic (and collision)
sealed class HittingIceBlocksWithPickaxeSupport : ModPlayer {
    public override void Load() {
        On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += On_Player_ItemCheck_UseMiningTools_ActuallyUseMiningTool;
        On_SmartCursorHelper.Step_Pickaxe_MineSolids += On_SmartCursorHelper_Step_Pickaxe_MineSolids; ;
    }

    private void On_SmartCursorHelper_Step_Pickaxe_MineSolids(On_SmartCursorHelper.orig_Step_Pickaxe_MineSolids orig, Player player, object providedInfo, List<Tuple<int, int>> grappleTargets, ref int focusedX, ref int focusedY) {
        var SmartCursorUsageInfo = typeof(SmartCursorHelper).GetNestedType("SmartCursorUsageInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        Item item = (Item)SmartCursorUsageInfo.GetField("item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableStartX = (int)SmartCursorUsageInfo.GetField("reachableStartX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableEndX = (int)SmartCursorUsageInfo.GetField("reachableEndX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableStartY = (int)SmartCursorUsageInfo.GetField("reachableStartY", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableEndY = (int)SmartCursorUsageInfo.GetField("reachableEndY", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        Vector2 mouse = (Vector2)SmartCursorUsageInfo.GetField("mouse", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        List<Tuple<int, int>> _targets = (List<Tuple<int, int>>)typeof(SmartCursorHelper).GetField("_targets", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(null);

        if (!(focusedX > -1 || focusedY > -1)) {
            int type = item.type;
            if (!(type < 0 || item.pick <= 0)) {
                _targets.Clear();
                for (int i = reachableStartX; i <= reachableEndX; i++) {
                    for (int j = reachableStartY; j <= reachableEndY; j++) {
                        foreach (Point16 iceBlockPosition in EnumerateIceBlockPositions()) {
                            if (iceBlockPosition.X == i && iceBlockPosition.Y == j) {
                                _targets.Add(new Tuple<int, int>(i, j));
                            }
                        }
                    }
                }

                if (_targets.Count > 0) {
                    float num = -1f;
                    Tuple<int, int> tuple = _targets[0];
                    for (int k = 0; k < _targets.Count; k++) {
                        float num2 = Vector2.Distance(new Vector2(_targets[k].Item1, _targets[k].Item2) * 16f + Vector2.One * 8f, mouse);
                        if (num == -1f || num2 < num) {
                            num = num2;
                            tuple = _targets[k];
                        }
                    }

                    if (Collision.InTileBounds(tuple.Item1, tuple.Item2, reachableStartX, reachableStartY, reachableEndX, reachableEndY)) {
                        focusedX = tuple.Item1;
                        focusedY = tuple.Item2;
                    }
                }

                _targets.Clear();
            }
        }

        orig(player, providedInfo, grappleTargets, ref focusedX, ref focusedY);
    }

    private void On_Player_ItemCheck_UseMiningTools_ActuallyUseMiningTool(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y) {
        if (sItem.pick > 0) {
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<IceBlock>()) {
                var iceBlock = projectile.As<IceBlock>();
                for (int i = 0; i < iceBlock.IceBlockPositions.Count; i++) {
                    if (iceBlock.IceBlockPositions[i] == Point16.Zero) {
                        continue;
                    }
                    if (iceBlock.IceBlockPositions[i].X == x && iceBlock.IceBlockPositions[i].Y == y) {
                        self.ApplyItemTime(sItem, self.pickSpeed);

                        iceBlock.ResetAdjIceBlocks(iceBlock.IceBlockPositions[i]);
                        var worldPos = iceBlock.IceBlockPositions[i].ToWorldCoordinates();
                        SoundEngine.PlaySound(SoundID.Item27, worldPos);
                        for (int num624 = 0; num624 < 10; num624++) {
                            Dust.NewDust(new Vector2(worldPos.X, worldPos.Y), 18, 18, DustID.IceRod);
                        }
                        iceBlock.IceBlockPositions[i] = Point16.Zero;

                        canHitWalls = true;
                        return;
                    }
                }
            }
        }

        orig(self, sItem, out canHitWalls, x, y);
    }

    private IEnumerable<Point16> EnumerateIceBlockPositions() {
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<IceBlock>()) {
            var iceBlock = projectile.As<IceBlock>();
            for (int i = 0; i < iceBlock.IceBlockPositions.Count; i++) {
                if (iceBlock.IceBlockPositions[i] == Point16.Zero) {
                    continue;
                }
                yield return iceBlock.IceBlockPositions[i];
            }
        }
    }
}

[Tracked]
sealed class IceBlock : NatureProjectile {
    public struct IceBlockInfo() {
        public Point16 FrameCoords = Point16.Zero;
        public float Opacity = 0f;
        public byte Penetrate = 2;
        public bool PixieSoundPlayed;

        public readonly bool CanDamage => Penetrate > 0;
    }

    private ushort[][]? _immunityFramesPerNPC;

    public readonly List<Point16> IceBlockPositions = [];
    public readonly IceBlockInfo[] IceBlockData = new IceBlockInfo[5];

    private ref ushort GetImmuneTime(byte index, int npcId) => ref _immunityFramesPerNPC![index][npcId];

    protected override void SafeSetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        //Projectile.magic = true;
        Projectile.tileCollide = false;
        //Projectile.light = 0.5f;
        Projectile.coldDamage = true;
        Projectile.penetrate = -1;
    }

    private bool IceBlockNearby() {
        return false;
        //int num184 = (int)(Projectile.position.X / 16f) - 2;
        //int num185 = (int)((Projectile.position.X + (float)Projectile.width) / 16f) + 3;
        //int num186 = (int)(Projectile.position.Y / 16f) - 2;
        //int num187 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f) + 3;
        //if (num184 < 0)
        //    num184 = 0;

        //if (num185 > Main.maxTilesX)
        //    num185 = Main.maxTilesX;

        //if (num186 < 0)
        //    num186 = 0;

        //if (num187 > Main.maxTilesY)
        //    num187 = Main.maxTilesY;
        //Vector2 vector23 = default(Vector2);
        //bool onIceBlock = false;
        //for (int num190 = num184; num190 < num185; num190++) {
        //    if (onIceBlock) {
        //        break;
        //    }
        //    for (int num191 = num186; num191 < num187; num191++) {
        //        if (Main.tile[num190, num191] != null && Main.tile[num190, num191].HasTile && Main.tile[num190, num191].TileType == 127) {
        //            vector23.X = num190 * 16;
        //            vector23.Y = num191 * 16;
        //            onIceBlock = true;
        //            break;
        //        }
        //    }
        //}
        //return onIceBlock;
    }

    public override bool? CanDamage() => false;

    [Flags]
    public enum NeighborSides : byte {
        None = 0,
        Left = 1 << 0,    // 1
        Right = 1 << 1,   // 2
        Top = 1 << 2,     // 4
        Bottom = 1 << 3,  // 8
        TopLeft = 1 << 4,     // 16
        TopRight = 1 << 5,    // 32
        BottomLeft = 1 << 6,  // 64
        BottomRight = 1 << 7  // 128
    }

    public int GetSideMask(bool hasLeft, bool hasRight, bool hasTop, bool hasBottom) {
        int mask = 0;

        if (hasLeft) mask |= (int)NeighborSides.Left;
        if (hasRight) mask |= (int)NeighborSides.Right;
        if (hasTop) mask |= (int)NeighborSides.Top;
        if (hasBottom) mask |= (int)NeighborSides.Bottom;

        if (hasTop && hasLeft) mask |= (int)NeighborSides.TopLeft;
        if (hasTop && hasRight) mask |= (int)NeighborSides.TopRight;
        if (hasBottom && hasLeft) mask |= (int)NeighborSides.BottomLeft;
        if (hasBottom && hasRight) mask |= (int)NeighborSides.BottomRight;

        return mask;
    }

    public Point16 GetTileFrame(bool hasLeft, bool hasRight, bool hasTop, bool hasBottom) => GetFrameByMask(GetSideMask(hasLeft, hasRight, hasTop, hasBottom));

    private Point16 GetFrameByMask(int mask) {
        return mask switch {
            (int)(NeighborSides.Top | NeighborSides.Left | NeighborSides.Bottom |
                  NeighborSides.TopLeft | NeighborSides.BottomLeft)
                => new Point16(72, Main.rand.Next(3) * 18),

            (int)(NeighborSides.Top | NeighborSides.Right | NeighborSides.Bottom |
                  NeighborSides.TopRight | NeighborSides.BottomRight)
                => new Point16(0, Main.rand.Next(3) * 18),

            (int)(NeighborSides.Left | NeighborSides.Right | NeighborSides.Top |
                  NeighborSides.TopLeft | NeighborSides.TopRight)
                => new Point16(18 + Main.rand.Next(3) * 18, 36),

            (int)(NeighborSides.Left | NeighborSides.Right | NeighborSides.Bottom |
                  NeighborSides.BottomLeft | NeighborSides.BottomRight)
                => new Point16(18 + Main.rand.Next(3) * 18, 0),

            (int)(NeighborSides.Bottom | NeighborSides.Left | NeighborSides.BottomLeft)
                 => new Point16(18 + Main.rand.Next(3) * 36, 54),

            (int)(NeighborSides.Bottom | NeighborSides.Right | NeighborSides.BottomRight)
                => new Point16(0 + Main.rand.Next(3) * 36, 54),

            (int)(NeighborSides.Top | NeighborSides.Left | NeighborSides.TopLeft)
                => new Point16(18 + Main.rand.Next(3) * 36, 72),

            (int)(NeighborSides.Top | NeighborSides.Right | NeighborSides.TopRight)
                => new Point16(0 + Main.rand.Next(3) * 36, 72),

            (int)(NeighborSides.Top | NeighborSides.Bottom) => new Point16(90, Main.rand.Next(3) * 18),
            (int)(NeighborSides.Left | NeighborSides.Right) => new Point16(108 + Main.rand.Next(3) * 18, 72),

            (int)NeighborSides.Top => new Point16(108 + Main.rand.Next(3) * 18, 54),
            (int)NeighborSides.Right => new Point16(162, Main.rand.Next(3) * 18),
            (int)NeighborSides.Bottom => new Point16(108 + Main.rand.Next(3) * 18, 0),
            (int)NeighborSides.Left => new Point16(216, Main.rand.Next(3) * 18),

            (int)(NeighborSides.Top | NeighborSides.Right) => new Point16(0 + Main.rand.Next(3) * 36, 72),
            (int)(NeighborSides.Top | NeighborSides.Left) => new Point16(18 + Main.rand.Next(3) * 36, 72),
            (int)(NeighborSides.Bottom | NeighborSides.Right) => new Point16(18 + Main.rand.Next(3) * 36, 54),
            (int)(NeighborSides.Bottom | NeighborSides.Left) => new Point16(0 + Main.rand.Next(3) * 36, 54),

            (int)(NeighborSides.Top | NeighborSides.Left | NeighborSides.Right) => new Point16(36 + Main.rand.Next(3) * 18, 18),
            (int)(NeighborSides.Right | NeighborSides.Top | NeighborSides.Bottom) => new Point16(0, Main.rand.Next(3) * 18),
            (int)(NeighborSides.Bottom | NeighborSides.Left | NeighborSides.Right) => new Point16(18 + Main.rand.Next(3) * 18, 0),
            (int)(NeighborSides.Left | NeighborSides.Top | NeighborSides.Bottom) => new Point16(72, Main.rand.Next(3) * 18),

            (int)(NeighborSides.Top | NeighborSides.Right | NeighborSides.Bottom | NeighborSides.Left)
                => new Point16(108 + Main.rand.Next(3) * 18, 18),

            (int)(NeighborSides.Top | NeighborSides.Right | NeighborSides.Bottom | NeighborSides.Left |
                  NeighborSides.TopLeft | NeighborSides.TopRight | NeighborSides.BottomLeft | NeighborSides.BottomRight)
                => new Point16(18 + Main.rand.Next(3) * 18, 18),

            _ => new Point16(162 + Main.rand.Next(3) * 18, 54)
        };
    }

    public override bool PreDraw(ref Color lightColor) {
        if (Projectile.ai[0] < 0f) {
            for (byte i = 0; i < IceBlockData.Length; i++) {
                byte currentSegmentIndex = i;
                ref IceBlockInfo iceBlockInfo = ref IceBlockData[currentSegmentIndex];
                if (!iceBlockInfo.CanDamage) {
                    continue;
                }
                HashSet<Point16> allIceBlockPositions = [];
                foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<IceBlock>()) {
                    foreach (Point16 iceBlockPosition in projectile.As<IceBlock>().IceBlockPositions) {
                        allIceBlockPositions.Add(iceBlockPosition);
                    }
                }
                Point16 tilePosition = IceBlockPositions[i];
                Color tileColor = Color.White * MathUtils.Clamp01(iceBlockInfo.Opacity);
                Rectangle clip;
                Point16 right = tilePosition + new Point16(1, 0);
                Point16 left = tilePosition - new Point16(1, 0);
                Point16 bottom = tilePosition + new Point16(0, 1);
                Point16 top = tilePosition - new Point16(0, 1);
                bool hasRight = allIceBlockPositions.Contains(right);
                bool hasLeft = allIceBlockPositions.Contains(left);
                bool hasBottom = allIceBlockPositions.Contains(bottom);
                bool hasTop = allIceBlockPositions.Contains(top);
                if (iceBlockInfo.FrameCoords == Point16.Zero) {
                    iceBlockInfo.FrameCoords = GetTileFrame(hasLeft, hasRight, hasTop, hasBottom);
                }
                //if (tilePosition.X == Player.tileTargetX && tilePosition.Y == Player.tileTargetY) {
                //    Main.NewText((NeighborSides)GetSideMask(hasLeft, hasRight, hasTop, hasBottom));
                //}
                clip = new Rectangle(iceBlockInfo.FrameCoords.X, iceBlockInfo.FrameCoords.Y, 16, 16);
                var info = new DrawUtils.SingleTileDrawInfo(TextureAssets.Tile[TileID.MagicalIceBlock].Value, tilePosition.ToPoint(), clip, tileColor, 0, false);
                DrawUtils.DrawSingleTile(info);
                DrawUtils.DrawSingleTile(info with { Color = Color.White.MultiplyAlpha(Utils.Remap(iceBlockInfo.Opacity, 0f, 1f, 1f, 0f)) * Utils.Remap(iceBlockInfo.Opacity, 0f, 1f, 0f, 1f) * Utils.Remap(iceBlockInfo.Opacity, 1f, 1.25f, 1f, 0f) });
            }

            return false;
        }

        return base.PreDraw(ref lightColor);
    }

    public override void AI() {
        void damageNPCs() {
            if (!Projectile.IsOwnerLocal()) {
                return;
            }

            for (byte i = 0; i < IceBlockData.Length; i++) {
                var iceBlockInfo = IceBlockData[i];
                if (!iceBlockInfo.CanDamage || IceBlockPositions[i] == Point16.Zero) {
                    continue;
                }
                Vector2 worldPosition = IceBlockPositions[i].ToWorldCoordinates();
                foreach (NPC npcForCollisionCheck in Main.ActiveNPCs) {
                    Rectangle hitBox = GeometryUtils.CenteredSquare(worldPosition, 18);
                    if (!NPCUtils.DamageNPCWithPlayerOwnedProjectile(npcForCollisionCheck, Projectile,
                                                                     ref GetImmuneTime(i, npcForCollisionCheck.whoAmI),
                                                                     damageSourceHitbox: hitBox,
                                                                     direction: MathF.Sign(hitBox.Center.X - npcForCollisionCheck.Center.X))) {
                        continue;
                    }
                }
            }
        }
        void resetDamageInfo() {
            for (byte i = 0; i < IceBlockData.Length; i++) {
                var iceBlockInfo = IceBlockData[i];
                if (!iceBlockInfo.CanDamage) {
                    if (i < IceBlockPositions.Count) {
                        if (IceBlockPositions[i] != Point16.Zero) {
                            for (byte i2 = 0; i2 < IceBlockData.Length; i2++) {
                                IceBlockData[i2].Opacity = 0.25f;
                                IceBlockData[i2].FrameCoords = Point16.Zero;
                            }
                        }
                        IceBlockPositions[i] = Point16.Zero;
                    }
                    continue;
                }
                for (int npcId = 0; npcId < Main.npc.Length; npcId++) {
                    ref ushort immuneTime = ref GetImmuneTime(i, npcId);
                    if (immuneTime > 0) {
                        if (immuneTime == 1) {
                            IceBlockData[i].Penetrate--;
                        }
                        immuneTime--;
                    }
                }
            }
        }

        if (Projectile.ai[0] < 0f) {
            damageNPCs();
            resetDamageInfo();

            for (byte i = 0; i < IceBlockData.Length; i++) {
                byte currentSegmentIndex = i,
                     previousSegmentIndex = (byte)Math.Max(0, i - 1);
                var previousSegmentInfo = IceBlockData[previousSegmentIndex];
                ref IceBlockInfo iceBlockData = ref IceBlockData[currentSegmentIndex];
                if (iceBlockData.Opacity >= 0.75f && !iceBlockData.PixieSoundPlayed) {
                    SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
                    iceBlockData.PixieSoundPlayed = true;
                }
                if (iceBlockData.Opacity < 0.75f && iceBlockData.PixieSoundPlayed) {
                    iceBlockData.PixieSoundPlayed = false;
                }
                if (currentSegmentIndex > 0 && previousSegmentInfo.Opacity < 0.5f) {
                    continue;
                }
                iceBlockData.Opacity += 0.1f;
                iceBlockData.Opacity = MathF.Min(1.25f, iceBlockData.Opacity);
            }
        }

        if (Projectile.ai[2] == 0f && Projectile.localAI[2] == 0f) {
            Projectile.ai[2] = 5;
            Projectile.localAI[2] = 1f;

            _immunityFramesPerNPC = new ushort[5][];
            for (int i = 0; i < _immunityFramesPerNPC.Length; i++) {
                _immunityFramesPerNPC[i] = new ushort[Main.npc.Length];
            }
        }

        if (Projectile.velocity.X == 0f && Projectile.velocity.Y == 0f)
            Projectile.alpha = 255;

        Dust dust2;

        // placed a few tiles
        if (Projectile.ai[1] < 0f) {
            if (Projectile.timeLeft > 60)
                Projectile.timeLeft = 60;

            if (Projectile.velocity.X > 0f)
                Projectile.rotation += 0.3f;
            else
                Projectile.rotation -= 0.3f;

            checkForSolids();

            if (!IceBlockNearby()) {
                Projectile.ai[0] = (int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f);
                Projectile.ai[1] = (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f);
                PlaceIceBlocks();
            }

            int num192 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 67);
            Main.dust[num192].noGravity = true;
            dust2 = Main.dust[num192];
            dust2.velocity *= 0.3f;
            return;
        }

        // placed all tiles
        if (Projectile.ai[0] < 0f) {
            if (Projectile.ai[0] == -1f) {
                // first time placed all blocks
                foreach (Point16 iceBlockPosition in IceBlockPositions) {
                    Vector2 worldIceBlockPosition = iceBlockPosition.ToWorldCoordinates();
                    for (int num193 = 0; num193 < 10; num193++) {
                        int num194 = Dust.NewDust(new Vector2(worldIceBlockPosition.X, worldIceBlockPosition.Y) - Projectile.Size / 2f, Projectile.width, Projectile.height, 67, 0f, 0f, 0, default(Color), 1.1f);
                        Main.dust[num194].noGravity = true;
                        dust2 = Main.dust[num194];
                        dust2.velocity *= 1.3f;
                    }
                }
            }
            else {
                foreach (Point16 iceBlockPosition in IceBlockPositions) {
                    if (Main.rand.Next(30) == 0) {
                        Vector2 worldIceBlockPosition = iceBlockPosition.ToWorldCoordinates();
                        int num195 = Dust.NewDust(new Vector2(worldIceBlockPosition.X, worldIceBlockPosition.Y), Projectile.width, Projectile.height, 67, 0f, 0f, 100);
                        dust2 = Main.dust[num195];
                        dust2.velocity *= 0.2f;
                    }
                }
            }

            Projectile.ai[0] -= 1f;
            if (Projectile.ai[0] <= -900f) {
                Projectile.Kill();
            }

            return;
        }
        
        void checkForSolids() {
            int num198 = (int)(Projectile.position.X / 16f) - 1;
            int num199 = (int)((Projectile.position.X + (float)Projectile.width) / 16f) + 2;
            int num200 = (int)(Projectile.position.Y / 16f) - 1;
            int num201 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f) + 2;
            if (num198 < 0)
                num198 = 0;

            if (num199 > Main.maxTilesX)
                num199 = Main.maxTilesX;

            if (num200 < 0)
                num200 = 0;

            if (num201 > Main.maxTilesY)
                num201 = Main.maxTilesY;

            int num202 = (int)Projectile.position.X + 4;
            int num203 = (int)Projectile.position.Y + 4;
            Vector2 vector24 = default(Vector2);
            for (int num204 = num198; num204 < num199; num204++) {
                for (int num205 = num200; num205 < num201; num205++) {
                    if (Main.tile[num204, num205] != null && Main.tile[num204, num205].HasUnactuatedTile && Main.tile[num204, num205].TileType != 127 && Main.tileSolid[Main.tile[num204, num205].TileType] && !Main.tileSolidTop[Main.tile[num204, num205].TileType]) {
                        vector24.X = num204 * 16;
                        vector24.Y = num205 * 16;
                        if ((float)(num202 + 8) > vector24.X && (float)num202 < vector24.X + 16f && (float)(num203 + 8) > vector24.Y && (float)num203 < vector24.Y + 16f)
                            StartCharging(num204, num205);
                    }
                }
            }
        }

        checkForSolids();

        int num206 = (int)(Projectile.Center.X / 16f);
        int num207 = (int)(Projectile.Center.Y / 16f);
        if (Projectile.lavaWet)
            StartCharging(num206, num207);

        if (WorldGen.InWorld(num206, num207) && Main.tile[num206, num207] != null && Main.tile[num206, num207].LiquidAmount > 0 && TileHelper.IsShimmer(num206, num207))
            StartCharging(num206, num207);

        if (!Projectile.active)
            return;

        int num208 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 67);
        Main.dust[num208].noGravity = true;
        dust2 = Main.dust[num208];
        dust2.velocity *= 0.3f;

        if (Projectile.velocity.X > 0f)
            Projectile.rotation += 0.3f;
        else
            Projectile.rotation -= 0.3f;

        Vector2 center = Projectile.GetOwnerAsPlayer().Center;
        if (IceBlockNearby() && Projectile.Distance(center) > new Point16((int)Projectile.ai[0], (int)Projectile.ai[1]).ToWorldCoordinates().Distance(center)) {
            Projectile.ai[0] = num206;
            Projectile.ai[1] = num207;

        }
        else {
            PlaceIceBlocks();
        }

        float num = 0.5f;
        float num2 = 0.5f;
        float num3 = 0.5f;
        num *= 0f;
        num2 *= 0.8f;
        num3 *= 1f;
        Lighting.AddLight((int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f), (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f), num, num2, num3);

        if (Projectile.lavaWet) {
            Projectile.wet = false;
            if (Projectile.ai[0] >= 0f) {
                StartCharging(num206, num207);
            }
        }
    }

    private void PlaceIceBlocks(bool kill = false) {
        int num206 = (int)(Projectile.Center.X / 16f);
        int num207 = (int)(Projectile.Center.Y / 16f);
        int num209 = kill ? num206 : (int)Projectile.ai[0];
        int num210 = kill ? num207 : (int)Projectile.ai[1];
        if (num209 == -1 || num209 == 0) {
            num209 = num206;
        }
        if (num210 == -1 || num210 == 0) {
            num210 = num207;
        }
        if (WorldGen.InWorld(num209, num210) && WorldGen.SolidTile(num209, num210)) {
            if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
                if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1))
                    num210--;
                else if (WorldGen.InWorld(num209, num210 + 1) && !WorldGen.SolidTile(num209, num210 + 1))
                    num210++;
                else if (WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1))
                    num210--;
                else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210))
                    num209--;
                else if (WorldGen.InWorld(num209 + 1, num210) && !WorldGen.SolidTile(num209 + 1, num210))
                    num209++;
                else if (WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210))
                    num209--;
            }
            else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210)) {
                num209--;
            }
            else if (WorldGen.InWorld(num209 + 1, num210) && !WorldGen.SolidTile(num209 + 1, num210)) {
                num209++;
            }
            else if (WorldGen.InWorld(num209 - 1, num210) && !WorldGen.SolidTile(num209 - 1, num210)) {
                num209--;
            }
            else if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1)) {
                num210--;
            }
            else if (WorldGen.InWorld(num209, num210 + 1) && !WorldGen.SolidTile(num209, num210 + 1)) {
                num210++;
            }
            else if (WorldGen.InWorld(num209, num210 - 1) && !WorldGen.SolidTile(num209, num210 - 1)) {
                num210--;
            }
        }

        if (Main.myPlayer != Projectile.owner)
            return;

        if (!kill) {
            int num211 = (int)((Projectile.position.X + (float)(Projectile.width / 2)) / 16f);
            int num212 = (int)((Projectile.position.Y + (float)(Projectile.height / 2)) / 16f);

            bool flag8 = false;
            if (num211 == num209 && num212 == num210)
                flag8 = true;

            if (((Projectile.velocity.X <= 0f && num211 <= num209) || (Projectile.velocity.X >= 0f && num211 >= num209)) && ((Projectile.velocity.Y <= 0f && num212 <= num210) || (Projectile.velocity.Y >= 0f && num212 >= num210)))
                flag8 = true;

            if (!flag8)
                return;
        }

        int maxCount = (int)Projectile.ai[2];
        int attempt = maxCount;
        while (attempt-- > 0) {
            num209 = num206;
            num210 = num207;
            HashSet<Point16> allIceBlockPositions = [];
            foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<IceBlock>()) {
                foreach (Point16 iceBlockPosition in projectile.As<IceBlock>().IceBlockPositions) {
                    allIceBlockPositions.Add(iceBlockPosition);
                }
            }
            if (WorldGen.InWorld(num209, num210) && allIceBlockPositions.Contains(new Point16(num209, num210))) {
                if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
                    if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !allIceBlockPositions.Contains(new Point16(num209, num210 - 1)))
                        num210--;
                    else if (WorldGen.InWorld(num209, num210 + 1) && !allIceBlockPositions.Contains(new Point16(num209, num210 + 1)))
                        num210++;
                    else if (WorldGen.InWorld(num209, num210 - 1) && !allIceBlockPositions.Contains(new Point16(num209, num210 - 1)))
                        num210--;
                    else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !allIceBlockPositions.Contains(new Point16(num209 - 1, num210)))
                        num209--;
                    else if (WorldGen.InWorld(num209 + 1, num210) && !allIceBlockPositions.Contains(new Point16(num209 + 1, num210)))
                        num209++;
                    else if (WorldGen.InWorld(num209 - 1, num210) && !allIceBlockPositions.Contains(new Point16(num209 - 1, num210)))
                        num209--;
                }
                else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !allIceBlockPositions.Contains(new Point16(num209 - 1, num210))) {
                    num209--;
                }
                else if (WorldGen.InWorld(num209 + 1, num210) && !allIceBlockPositions.Contains(new Point16(num209 + 1, num210))) {
                    num209++;
                }
                else if (WorldGen.InWorld(num209 - 1, num210) && !allIceBlockPositions.Contains(new Point16(num209 - 1, num210))) {
                    num209--;
                }
                else if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !allIceBlockPositions.Contains(new Point16(num209, num210 - 1))) {
                    num210--;
                }
                else if (WorldGen.InWorld(num209, num210 + 1) && !allIceBlockPositions.Contains(new Point16(num209, num210 + 1))) {
                    num210++;
                }
                else if (WorldGen.InWorld(num209, num210 - 1) && !allIceBlockPositions.Contains(new Point16(num209, num210 - 1))) {
                    num210--;
                }
            }
            bool placedAll = attempt <= 0;
            if (/*!WorldGen.SolidTile(num209, num210) && */!allIceBlockPositions.Contains(new Point16(num209, num210)) /*WorldGen.PlaceTile(num209, num210, 127, mute: false, forced: false, Projectile.owner)*/) {
                Point16 iceBlockPosition = new(num209, num210);
                IceBlockPositions.Add(iceBlockPosition);
                IceBlockData[(byte)(5 - Projectile.ai[2])] = new IceBlockInfo();

                ResetAdjIceBlocks(iceBlockPosition);

                //if (Main.netMode == 1)
                //    NetMessage.SendData(17, -1, -1, null, 1, num209, num210, 127f);

                Projectile.ai[2]--;
            }

            if (placedAll) {
                StartCharging(num209, num210);
                Projectile.netUpdate = true;
            }

            //else if (placedAll) {
            //    Projectile.ai[1] = -1f;
            //}
        }
    }

    public void ResetAdjIceBlocks(Point16 iceBlockPosition) {
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<IceBlock>()) {
            var iceBlock = projectile.As<IceBlock>();
            bool[] check = new bool[5];
            for (int i = 0; i < iceBlock.IceBlockPositions.Count; i++) {
                if (TileHelper.ArePositionsAdjacent(iceBlock.IceBlockPositions[i], iceBlockPosition)) {
                    check[i] = true;
                }
            }
            for (int i = 0; i < iceBlock.IceBlockData.Length; i++) {
                if (!check[i]) {
                    continue;
                }
                iceBlock.IceBlockData[i].FrameCoords = Point16.Zero;
            }
        }
    }

    private void StartCharging(int num209, int num210) {
        Projectile.damage = 0;
        Projectile.ai[0] = -1f;
        Projectile.velocity *= 0f;
        Projectile.alpha = 255;
        Projectile.position.X = num209 * 16;
        Projectile.position.Y = num210 * 16;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.ai[0] >= 0f) {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            for (int num624 = 0; num624 < 10; num624++) {
                Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.IceRod);
            }
        }

        foreach (var iceBlockPosition in IceBlockPositions) {
            ResetAdjIceBlocks(iceBlockPosition);
            Vector2 iceBlockPositionWorld = iceBlockPosition.ToWorldCoordinates();
            SoundEngine.PlaySound(SoundID.Item27, iceBlockPositionWorld);
            for (int num624 = 0; num624 < 10; num624++) {
                Dust.NewDust(new Vector2(iceBlockPositionWorld.X, iceBlockPositionWorld.Y), 18, 18, DustID.IceRod);
            }
        }

        if (IceBlockPositions.Count != 0) {
            IceBlockPositions.Clear();
        }
    }
}
