using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class IceBlock : NatureProjectile {
    private struct IceBlockInfo() {
        public float Opacity = 0f;
        public Point16 FrameCoords = Point16.Zero;
    }

    private readonly HashSet<Point16> _iceBlockPositions = [];
    private readonly IceBlockInfo[] _iceBlockData = new IceBlockInfo[5];

    protected override void SafeSetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        //Projectile.magic = true;
        Projectile.tileCollide = false;
        //Projectile.light = 0.5f;
        Projectile.coldDamage = true;
    }

    private bool IceBlockNearby() {
        int num184 = (int)(Projectile.position.X / 16f) - 2;
        int num185 = (int)((Projectile.position.X + (float)Projectile.width) / 16f) + 3;
        int num186 = (int)(Projectile.position.Y / 16f) - 2;
        int num187 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f) + 3;
        if (num184 < 0)
            num184 = 0;

        if (num185 > Main.maxTilesX)
            num185 = Main.maxTilesX;

        if (num186 < 0)
            num186 = 0;

        if (num187 > Main.maxTilesY)
            num187 = Main.maxTilesY;
        Vector2 vector23 = default(Vector2);
        bool onIceBlock = false;
        for (int num190 = num184; num190 < num185; num190++) {
            if (onIceBlock) {
                break;
            }
            for (int num191 = num186; num191 < num187; num191++) {
                if (Main.tile[num190, num191] != null && Main.tile[num190, num191].HasTile && Main.tile[num190, num191].TileType == 127) {
                    vector23.X = num190 * 16;
                    vector23.Y = num191 * 16;
                    onIceBlock = true;
                    break;
                }
            }
        }
        return onIceBlock;
    }

    public override bool PreDraw(ref Color lightColor) {
        if (Projectile.ai[0] < 0f) {
            for (byte i = 0; i < _iceBlockData.Length; i++) {
                byte currentSegmentIndex = i;
                ref IceBlockInfo currentSegmentData = ref _iceBlockData[currentSegmentIndex];
                Point16 tilePosition = _iceBlockPositions.ElementAt(i);
                Color tileColor = Color.White * MathUtils.Clamp01(currentSegmentData.Opacity);
                Rectangle clip = new(0, 0, 18, 18);
                if (currentSegmentData.FrameCoords == Point16.Zero) {
                    Point16 right = tilePosition + new Point16(1, 0);
                    Point16 left = tilePosition - new Point16(1, 0);
                    Point16 bottom = tilePosition + new Point16(0, 1);
                    Point16 top = tilePosition - new Point16(0, 1);
                    bool hasRight = _iceBlockPositions.Contains(right);
                    bool hasLeft = _iceBlockPositions.Contains(left);
                    bool hasBottom = _iceBlockPositions.Contains(bottom);
                    bool hasTop = _iceBlockPositions.Contains(top);
                    if (hasRight && !hasLeft && !hasTop && !hasBottom) {
                        currentSegmentData.FrameCoords = new Point16(162, Main.rand.Next(3) * 18);
                    }
                    else if (hasLeft && !hasRight && !hasTop && !hasBottom) {
                        currentSegmentData.FrameCoords = new Point16(216, Main.rand.Next(3) * 18);
                    }
                    else if (hasBottom && !hasRight && !hasTop && !hasLeft) {
                        currentSegmentData.FrameCoords = new Point16(108 + Main.rand.Next(3) * 18, 0);
                    }
                    else if (hasTop && !hasRight && !hasBottom && !hasLeft) {
                        currentSegmentData.FrameCoords = new Point16(108 + Main.rand.Next(3) * 18, 54);
                    }
                    else {
                        currentSegmentData.FrameCoords = new Point16(18 + Main.rand.Next(3) * 18, 18);
                    }
                }
                clip = new Rectangle(currentSegmentData.FrameCoords.X, currentSegmentData.FrameCoords.Y, 16, 16);
                var info = new DrawUtils.SingleTileDrawInfo(TextureAssets.Tile[TileID.MagicalIceBlock].Value, tilePosition.ToPoint(), clip, tileColor, 0, false);
                DrawUtils.DrawSingleTile(info);
                DrawUtils.DrawSingleTile(info with { Color = Color.White.MultiplyAlpha(Utils.Remap(currentSegmentData.Opacity, 0f, 1f, 1f, 0f)) * Utils.Remap(currentSegmentData.Opacity, 0f, 1f, 0f, 1f) * Utils.Remap(currentSegmentData.Opacity, 1f, 1.25f, 1f, 0f) });
            }

            return false;
        }

        return base.PreDraw(ref lightColor);
    }

    public override void AI() {
        if (Projectile.ai[0] < 0f) {
            for (byte i = 0; i < _iceBlockData.Length; i++) {
                byte currentSegmentIndex = i,
                     previousSegmentIndex = (byte)Math.Max(0, i - 1);
                var previousSegmentData = _iceBlockData[previousSegmentIndex];
                if (currentSegmentIndex > 0 && previousSegmentData.Opacity < 0.5f) {
                    continue;
                }
                _iceBlockData[currentSegmentIndex].Opacity += 0.1f;
                _iceBlockData[currentSegmentIndex].Opacity = MathF.Min(1.25f, _iceBlockData[currentSegmentIndex].Opacity);
            }
        }

        if (Projectile.ai[2] == 0f && Projectile.localAI[2] == 0f) {
            Projectile.ai[2] = 5;
            Projectile.localAI[2] = 1f;
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
                foreach (Point16 iceBlockPosition in _iceBlockPositions) {
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
                foreach (Point16 iceBlockPosition in _iceBlockPositions) {
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
            if (WorldGen.InWorld(num209, num210) && _iceBlockPositions.Contains(new Point16(num209, num210))) {
                if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
                    if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !_iceBlockPositions.Contains(new Point16(num209, num210 - 1)))
                        num210--;
                    else if (WorldGen.InWorld(num209, num210 + 1) && !_iceBlockPositions.Contains(new Point16(num209, num210 + 1)))
                        num210++;
                    else if (WorldGen.InWorld(num209, num210 - 1) && !_iceBlockPositions.Contains(new Point16(num209, num210 - 1)))
                        num210--;
                    else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !_iceBlockPositions.Contains(new Point16(num209 - 1, num210)))
                        num209--;
                    else if (WorldGen.InWorld(num209 + 1, num210) && !_iceBlockPositions.Contains(new Point16(num209 + 1, num210)))
                        num209++;
                    else if (WorldGen.InWorld(num209 - 1, num210) && !_iceBlockPositions.Contains(new Point16(num209 - 1, num210)))
                        num209--;
                }
                else if (Projectile.Center.X < (float)(num209 * 16 + 8) && WorldGen.InWorld(num209 - 1, num210) && !_iceBlockPositions.Contains(new Point16(num209 - 1, num210))) {
                    num209--;
                }
                else if (WorldGen.InWorld(num209 + 1, num210) && !_iceBlockPositions.Contains(new Point16(num209 + 1, num210))) {
                    num209++;
                }
                else if (WorldGen.InWorld(num209 - 1, num210) && !_iceBlockPositions.Contains(new Point16(num209 - 1, num210))) {
                    num209--;
                }
                else if (Projectile.Center.Y < (float)(num210 * 16 + 8) && WorldGen.InWorld(num209, num210 - 1) && !_iceBlockPositions.Contains(new Point16(num209, num210 - 1))) {
                    num210--;
                }
                else if (WorldGen.InWorld(num209, num210 + 1) && !_iceBlockPositions.Contains(new Point16(num209, num210 + 1))) {
                    num210++;
                }
                else if (WorldGen.InWorld(num209, num210 - 1) && !_iceBlockPositions.Contains(new Point16(num209, num210 - 1))) {
                    num210--;
                }
            }
            bool placedAll = attempt <= 0;
            if (/*!WorldGen.SolidTile(num209, num210) && */!_iceBlockPositions.Contains(new Point16(num209, num210)) /*WorldGen.PlaceTile(num209, num210, 127, mute: false, forced: false, Projectile.owner)*/) {
                _iceBlockPositions.Add(new Point16(num209, num210));
                _iceBlockData[(byte)(5 - Projectile.ai[2])] = new IceBlockInfo();

                if (Main.netMode == 1)
                    NetMessage.SendData(17, -1, -1, null, 1, num209, num210, 127f);

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

        if (_iceBlockPositions.Count != 0) {
            foreach (Point16 iceBlockPosition in _iceBlockPositions) {
                int num625 = iceBlockPosition.X;
                int num626 = iceBlockPosition.Y;
                if (Main.tile[num625, num626].TileType == 127 && Main.tile[num625, num626].HasTile)
                    WorldGen.KillTile(num625, num626);
            }
        }
    }
}
