using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class EvilBranch : NatureProjectile {
    public override void OnKill(int timeLeft) {
        int max = 60;
        int y = -max;
        int count = 6;
        bool isCrimson = Projectile.ai[2] == 1f;
        for (int i = 0; i < count; i++) {
            y += max / count * 2;
            y = Math.Min(max / 2, y);
            int maxX = 15;
            if (y < max / 2) {
                maxX = 8;
            }
            int x = Main.rand.Next(-maxX, maxX);
            Vector2 position = Projectile.Center - Vector2.UnitY * max - Vector2.UnitY * max / 3f + new Vector2(x, y).RotatedBy(Projectile.rotation);
            if (Main.netMode != NetmodeID.Server) {
                int gore = Gore.NewGore(Projectile.GetSource_Death(),
                position,
                Vector2.Zero, ModContent.Find<ModGore>(RoA.ModName + $"/EvilBranchGore{(isCrimson ? 2 : 1)}{Main.rand.Next(3) + 1}").Type, 1f);
                Main.gore[gore].velocity.Y *= 0.5f;
            }

            for (int i2 = 0; i2 < Main.rand.Next(9, 16); i2++) {
                if (Main.rand.NextBool(4)) {
                    Dust.NewDustPerfect(position, isCrimson ? DustID.Shadewood : DustID.Ebonwood);
                }
            }
        }
    }

    private readonly struct TwigPartInfo {
        public readonly int Variant1, Variant2;
        public TwigPartInfo(int variant1, int variant2) {
            Variant1 = variant1;
            Variant2 = variant2;
        }

        public TwigPartInfo() {
            Variant1 = Main.rand.NextBool().ToInt();
            Variant2 = Main.rand.NextBool().ToInt();
        }
    }

    private readonly struct LeafInfo(Vector2 position, bool facedRight) {
        public readonly Vector2 Position = position;
        public readonly bool FacedRight = facedRight;
    }

    private TwigPartInfo _part1Info, _part2Info;
    private int _direction;
    private bool _init;

    private List<LeafInfo> _leavesInfo = [];

    public ref float ScaleX => ref Projectile.ai[0];
    public ref float ScaleY => ref Projectile.ai[1];

    public override string Texture => ResourceManager.FriendlyProjectileTextures + $"Nature/{nameof(EvilBranch)}";

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.Write(Projectile.rotation);
        writer.Write(_part1Info.Variant1);
        writer.Write(_part1Info.Variant2);
        writer.Write(_part2Info.Variant1);
        writer.Write(_part2Info.Variant2);
        writer.Write(_direction);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        Projectile.rotation = reader.ReadSingle();
        _part1Info = new(reader.ReadInt32(), reader.ReadInt32());
        _part2Info = new(reader.ReadInt32(), reader.ReadInt32());
        _direction = reader.ReadInt32();
    }

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, Projectile.Center - (Vector2.UnitY * 130f).RotatedBy(Projectile.rotation));
    }

    private void SetUpLeafPoints() {
        if (_part1Info.Variant1 == 0) {
            _leavesInfo.Add(new LeafInfo(new(6f, 34f), false));
            _leavesInfo.Add(new LeafInfo(new(0f, 40f), false));
            _leavesInfo.Add(new LeafInfo(new(40f, 26f), true));
        }
        else {
            _leavesInfo.Add(new LeafInfo(new(6f, 26f), false));
            _leavesInfo.Add(new LeafInfo(new(40f, 40f), true));
            _leavesInfo.Add(new LeafInfo(new(42f, 50f), true));
        }

        if (_part2Info.Variant2 == 0) {
            _leavesInfo.Add(new LeafInfo(new(-2f, 82f), false));
            _leavesInfo.Add(new LeafInfo(new(44f, 66f), true));
            _leavesInfo.Add(new LeafInfo(new(40f, 82f), true));
        }
        else {
            _leavesInfo.Add(new LeafInfo(new(-2f, 66f), false));
            _leavesInfo.Add(new LeafInfo(new(0f, 84f), false));
            _leavesInfo.Add(new LeafInfo(new(46f, 76f), true));
        }
    }

    internal static void GetPos(Player player, out Point point, out Point point2, bool random = true, float maxDistance = 400f) {
        Vector2 targetSpot = Helper.GetLimitedPosition(player.Center, player.GetViableMousePosition(), maxDistance);
        Vector2 center = player.GetPlayerCorePoint();
        Vector2 endPoint = targetSpot;
        int samplesToTake = 3;
        float samplingWidth = 4f;
        Collision.AimingLaserScan(center, endPoint, samplingWidth, samplesToTake, out var vectorTowardsTarget, out var samples);
        float num = float.PositiveInfinity;
        for (int i = 0; i < samples.Length; i++) {
            if (samples[i] < num)
                num = samples[i];
        }

        targetSpot = center + vectorTowardsTarget.SafeNormalize(Vector2.Zero) * num;
        point = targetSpot.ToTileCoordinates();
        point2 = point;
        if (random) {
            if (Main.rand.NextChance(0.75)) {
                point2.X += Main.rand.Next(-2, 3);
            }
        }
        while (!WorldGen.SolidTile(point2)) {
            if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point2.X, point2.Y).TileType]) {
                break;
            }

            point2.Y++;
        }
    }

    public override void AI() {
        if (!_init) {
            ScaleX = 1.6f;
            ScaleY = 0.4f;

            Player player = Main.player[Projectile.owner];
            if (player.whoAmI == Main.myPlayer) {
                _direction = Main.rand.NextBool().ToDirectionInt();

                GetPos(player, out Point point, out Point point2);
                Projectile.Center = point2.ToWorldCoordinates();
                Vector2 velocity = (Projectile.Center - point.ToWorldCoordinates()).SafeNormalize(-Vector2.UnitY) * 16f;
                float maxRadians = 0.375f;
                Projectile.rotation = MathHelper.Clamp(velocity.ToRotation() - MathHelper.PiOver2, -maxRadians, maxRadians);

                _part1Info = new();
                _part2Info = new();
                SetUpLeafPoints();

                Projectile.netUpdate = true;
            }

            _init = true;
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WoodCreak") { Volume = 0.6f, PitchVariance = 0.2f, Pitch = 0.8f });

            byte index = 0;
            foreach (LeafInfo leafInfo in _leavesInfo) {
                Vector2 leafPosition = leafInfo.Position;
                int direction = leafInfo.FacedRight.ToDirectionInt();
                if (_direction == 1) {
                    leafPosition.X = 42f - leafPosition.X;
                }
                direction *= -_direction;
                Vector2 leafTwigPosition = -new Vector2(14, 122) + leafPosition;
                if (!AttachedNatureWeapon.IsEmpty()) {
                    int projectile = Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<EvilLeaf>(), NatureWeaponHandler.GetNatureDamage(AttachedNatureWeapon, Main.player[Projectile.owner]), Projectile.knockBack,
                        Projectile.owner, direction, Projectile.identity, index);
                    Main.projectile[projectile].As<EvilLeaf>().SetUpTwigPosition(leafTwigPosition);
                }
                index++;
            }

            Point tileCoords = Projectile.Center.ToTileCoordinates();
            for (int i = 0; i < Main.rand.Next(5, 8); i++) {
                Dust dust = Dust.NewDustPerfect(Projectile.Center - new Vector2(0f, -2f + Main.rand.NextFloat() * 3f),
                    TileHelper.GetKillTileDust(tileCoords.X, tileCoords.Y, WorldGenHelper.GetTileSafely(tileCoords)));
                dust.velocity *= 0.5f + Main.rand.NextFloatRange(0.1f);
                dust.velocity -= new Vector2(0f, 3f * Main.rand.NextFloat(0.5f, 1f)).RotatedBy(Projectile.rotation);
                dust.scale *= 1f + Main.rand.NextFloatRange(0.1f);
            }
        }

        ScaleX = MathHelper.SmoothStep(ScaleX, 1f, 0.5f);
        ScaleY = MathHelper.SmoothStep(ScaleY, 1f, 0.5f);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

    public override bool PreDraw(ref Color lightColor) {
        SpriteFrame frame = new(4, 2);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        void draw(Color color, bool top) {
            TwigPartInfo twigPartInfo = top ? _part1Info : _part2Info;
            byte variant = (byte)(top ? twigPartInfo.Variant1 : twigPartInfo.Variant2);
            frame = frame.With((byte)(variant + Projectile.ai[2] * 2f), (byte)(!top).ToInt());
            Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
            sourceRectangle.Height += 2;
            Vector2 position = Projectile.Center - Main.screenPosition;
            if (top) {
                float value = Math.Abs(Projectile.rotation) / MathHelper.TwoPi;
                position.X += (sourceRectangle.Width - 4) * MathHelper.PiOver2 * Projectile.rotation;
                position.Y += sourceRectangle.Height * value;
                position.Y -= sourceRectangle.Height;
                position.Y += 2;
                position.Y += sourceRectangle.Height * (1f - ScaleY);
            }
            position.Y += 2;
            SpriteEffects spriteEffects = (SpriteEffects)(_direction == 1).ToInt();
            Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, sourceRectangle.BottomCenter(), new Vector2(ScaleX, ScaleY), spriteEffects);
        }
        Color drawColor = Lighting.GetColor((Projectile.Center - Vector2.UnitY * 20f).ToTileCoordinates());
        draw(drawColor, true);
        draw(drawColor, false);

        return false;
    }
}
