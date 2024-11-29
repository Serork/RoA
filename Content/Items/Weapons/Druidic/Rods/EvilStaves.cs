using Humanizer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class EbonwoodStaff : BaseRodItem<EbonwoodStaff.EbonwoodStaffBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<EvilBranch>();

    protected override void SafeSetDefaults() {
        Item.SetSize(44);
        Item.SetDefaultToUsable(-1, 22, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.25f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class EbonwoodStaffBase : BaseRodProjectile {
        protected override bool ShouldWaitUntilProjDespawns() => false;
    }
}

sealed class ShadewoodStaff : BaseRodItem<ShadewoodStaff.ShadewoodStaffBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<EvilBranch>();

    protected override void SafeSetDefaults() {
        Item.SetSize(44);
        Item.SetDefaultToUsable(-1, 22, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.25f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class ShadewoodStaffBase : BaseRodProjectile {
        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override void SetSpawnProjectileSettings(Player player, ref Vector2 spawnPosition, ref Vector2 velocity, ref ushort count, ref float ai0, ref float ai1, ref float ai2)
            => ai2 = 1f;
    }
}

sealed class EvilLeaf : NatureProjectile {
    private const int TIMELEFT = 400;

    private Vector2 _twigPosition;
    private float _ai5, _ai4;
    private float _angle, _angle2;
    private Vector2 _to;

    private float[] _oldRotations = new float[18];

    internal bool Crimson;
    internal int Index;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + $"Druidic/{nameof(EvilLeaf)}";

    public override void SetStaticDefaults() => Projectile.SetTrail(2, 18);

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;
        Projectile.penetrate = 1;
        Projectile.hide = true;
        Projectile.tileCollide = false;
    }

    internal void SetUpPositionOnTwig(Vector2 position) => _twigPosition = position;

    public override bool ShouldUpdatePosition() => false;

    private void SetPosition() {
        Projectile parent = Main.projectile[(int)Projectile.ai[1]];
        Vector2 parentScale = parent.As<EvilBranch>().Scale;
        Vector2 position = parent.position;
        Vector2 myPosition = _twigPosition * parentScale - Vector2.One * 2f - (Projectile.ai[0] == 1f ? new Vector2(10f, 0f) : Vector2.Zero);
        Projectile.position = position + myPosition.RotatedBy(parent.rotation);
    }

    public override void AI() {
        Projectile.direction = (int)Projectile.ai[0];
        Projectile parent = Main.projectile[(int)Projectile.ai[1]];
        Player player = Main.player[Projectile.owner];
        int num176 = (int)(MathHelper.Pi * 20f);
        if (parent != null && parent.active && Projectile.timeLeft > TIMELEFT - 30 * (Index + 1) - 10) {
            Projectile.velocity = Vector2.Zero;
            Vector2 parentScale = parent.As<EvilBranch>().Scale;
            SetPosition();
            if (Projectile.localAI[0] == 0f) {
                Projectile.ai[2] = 1f;
                Projectile.localAI[2] = 1.25f;
                Projectile.localAI[1] = Projectile.localAI[2];
                _ai4 = -1f;
                float num175 = ((float)Math.PI + (float)Math.PI * 2f * Main.rand.NextFloat() * 1.5f) * ((float)(-(int)Projectile.ai[0]));
                float num177 = num175 / (float)num176;
                if (Math.Abs(num177) >= 0.17f)
                    num177 *= 0.7f;
                _angle = num177;
            }
            float scaleY = parentScale.Y * Projectile.ai[2];
            float rotation = parent.rotation + MathHelper.Pi * Projectile.direction * (1f - scaleY);
            if (Projectile.localAI[0] == 0f) {
                Projectile.localAI[0] = 1f;
                Projectile.rotation = rotation;
                for (int j = 0; j < _oldRotations.Length; j++) {
                    _oldRotations[j] = Projectile.rotation;
                }
            }
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rotation, 0.2f);
            if (scaleY < 0.5f) {
            }
            else {
                float angle = Math.Sign(_ai5) == Math.Sign(_ai4) ? 4f : 3f;
                if (Math.Abs(_ai5) < 0.5f) {
                    angle *= 0.5f;
                }
                if (Math.Abs(_ai5) < 0.25f) {
                    angle *= 0.25f;
                }
                _ai4 += (float)-Math.Sign(_ai5) * angle;
                _ai4 *= 0.95f;
                _ai5 += _ai4 * TimeSystem.LogicDeltaTime;
                Projectile.localAI[2] = MathHelper.Lerp(Projectile.localAI[2], 1f + _ai5, Projectile.localAI[1] * 0.175f);
                Projectile.localAI[1] *= 0.995f;
            }
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.localAI[2], Projectile.ai[2] * 0.215f);
        }
        else {
            if (Projectile.velocity == Vector2.Zero) {
                _ai4 = 0f;
                Projectile.velocity = Helper.VelocityToPoint(Projectile.position, player.GetViableMousePosition(), 0.1f);
                for (int i = 0; i < 3; i++) {
                    Vector2 dustVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(Main.rand.NextFloat() * ((float)Math.PI * 2f) * 0.25f) * (Main.rand.NextFloat() * 3f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Crimson ? DustID.CrimsonPlants : DustID.CorruptPlants);
                    dust.alpha = 150;
                    dust.velocity += dustVelocity;
                    dust.velocity *= 0.6f + Main.rand.NextFloatRange(0.1f);
                    dust.noGravity = true;
                }
                float num177 = _angle / (float)num176;
                float num178 = 16f;
                Vector2 vector57 = Vector2.UnitX * num178;
                Vector2 v7 = vector57;
                Vector2 vector55 = player.GetViableMousePosition();
                Vector2 vector56 = Projectile.Center + new Vector2(Main.rand.NextFloatDirection() * (float)Projectile.width / 2f, Projectile.height / 2);
                Vector2 v6 = vector55 - vector56;
                float num179 = v6.Length();
                int num180 = 0;
                while (v7.Length() < num179 && num180 < num176) {
                    num180++;
                    v7 += vector57;
                    vector57 = vector57.RotatedBy(num177);
                }
                _angle2 = num180;
                if (Projectile.owner == Main.myPlayer) {
                    _to = player.GetViableMousePosition();
                }
            }
            else {
                if (Main.rand.NextBool(8)) {
                    Dust dust2 = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, Crimson ? DustID.CrimsonPlants : DustID.CorruptPlants, 0f, 0f, 150, default(Color), 1f);
                    dust2.noGravity = true;
                    dust2.velocity = Projectile.velocity * 0.5f;
                }
            }
            _ai4 = MathHelper.SmoothStep(_ai4, 1f, 0.1f);
            num176 = (int)(MathHelper.Pi * 20f * _ai4);
            float angle = _angle;
            float angle2 = _angle2;
            Projectile.velocity = Projectile.velocity.RotatedBy(angle);
            float fromValue = num176 - Projectile.timeLeft;
            float fromMax = angle2 + num176 / 3;
            float num4 = Utils.Remap(fromValue, angle2, fromMax, 0f, 1f) * Utils.Remap(fromValue, angle2, angle2 + num176, 1f, 0f);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * (4f + 12f * (1f - num4) * 0.1f);
            for (int num2 = _oldRotations.Length - 1; num2 > 0; num2--) {
                _oldRotations[num2] = _oldRotations[num2 - 1];
            }
            _oldRotations[0] = Projectile.rotation;
            Projectile.Opacity = Utils.GetLerpValue(Projectile.timeLeft, 0f, 10f, true);
            Projectile.velocity *= Math.Min(1f, Utils.Remap(_ai4, 0f, 1f, 0.1f, 1f) * 2f);
            Vector2 velocity = Projectile.velocity/* * (int)Projectile.ai[0]*/;
            Projectile.rotation = Helper.SmoothAngleLerp(Projectile.rotation, Helper.VelocityAngle(velocity) - MathHelper.PiOver2 * Projectile.ai[0], MathHelper.Min(1f, 1f * _ai4));
            Projectile.position += velocity;
            Projectile.position += Helper.VelocityToPoint(Projectile.position, _to, 1f);
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if (_to == Vector2.Zero) {
            behindNPCsAndTiles.Add(index);
            return;
        }
        Projectile.hide = false;
        behindNPCsAndTiles.Remove(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        SpriteEffects spriteEffects = (SpriteEffects)((int)Projectile.ai[0] != 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new(Crimson ? 14 : 0, 0, 14, 14);
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Vector2 origin = Projectile.ai[0] == 1 ? new Vector2(2, 12) : new Vector2(12, 12);
        if (_to != Vector2.Zero) {
            int num149 = Projectile.oldPos.Length;
            int num147 = 0;
            int num148 = -3;
            for (int num152 = num149; (num148 > 0 && num152 < num147) || (num148 < 0 && num152 > num147); num152 += num148) {
                if (num152 >= Projectile.oldPos.Length)
                    continue;
                float num157 = num147 - num152;
                if (num148 < 0)
                    num157 = num149 - num152;
                Color color32 = color;
                Vector2 vector29 = Projectile.oldPos[num152] + Projectile.Size / 2f;
                color32 *= Lighting.GetColor((vector29).ToTileCoordinates()).ToVector3().Length() / 1.74f;
                color32 *= 1.15f;
                color32 *= num157 / ((float)Projectile.oldPos.Length * 3f);
                Vector2 position3 = vector29 - Main.screenPosition;
                Main.EntitySpriteDraw(texture, position3, sourceRectangle, color32, _oldRotations[num152], origin, Projectile.scale, spriteEffects);
            }
        }
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        return false;
    }
}

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
            int gore = Gore.NewGore(Projectile.GetSource_Death(),
                position,
                Vector2.Zero, ModContent.Find<ModGore>(RoA.ModName + $"/EvilBranchGore{(isCrimson ? 2 : 1)}{Main.rand.Next(3) + 1}").Type, 1f);
            Main.gore[gore].velocity.Y *= 0.5f;

            for (int i2 = 0; i2 < Main.rand.Next(9, 16); i2++) {
                if (Main.rand.NextBool(6)) {
                    Dust.NewDustPerfect(position, isCrimson ? DustID.Shadewood : DustID.Ebonwood);
                }
            }
        }
    }

    private readonly struct TwigPartInfo {
        public readonly int Variant1, Variant2;

        public TwigPartInfo() {
            Variant1 = Main.rand.NextBool().ToInt();
            Variant2 = Main.rand.NextBool().ToInt();
        }
    }

    private readonly struct LeafInfo(Vector2 position, bool facedRight) {
        public readonly Vector2 Position = position;
        public readonly bool FacedRight = facedRight;
    }

    private readonly TwigPartInfo _part1Info = new(), _part2Info = new();

    private Vector2 _scale = Vector2.One;
    private List<LeafInfo> _leavesInfo = [];

    public Vector2 Scale => _scale;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + $"Druidic/{nameof(EvilBranch)}";

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.netImportant = true;
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

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Projectile.direction = Main.rand.NextBool().ToDirectionInt();

        _scale.X = 1.6f;
        _scale.Y = 0.4f;

        Point point = Main.player[Projectile.owner].GetViableMousePosition().ToTileCoordinates();
        Point point2 = point;
        if (Main.rand.NextChance(0.75)) {
            point2.X += Main.rand.Next(-2, 3);
        }
        while (!WorldGen.SolidTile(point2)) {
            if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point2.X, point2.Y).TileType]) {
                break;
            }

            point2.Y++;
        }
        Projectile.Center = point2.ToWorldCoordinates();
        Vector2 velocity = (Projectile.Center - point.ToWorldCoordinates()).SafeNormalize(-Vector2.UnitY) * 16f;
        float maxRadians = 0.45f;
        Projectile.rotation = MathHelper.Clamp(velocity.ToRotation() - MathHelper.PiOver2, -maxRadians, maxRadians);

        SetUpLeafPoints();
        int index = 0;
        foreach (LeafInfo leafInfo in _leavesInfo) {
            Vector2 leafPosition = leafInfo.Position;
            int direction = leafInfo.FacedRight.ToDirectionInt();
            leafPosition.X = 42f - leafPosition.X;
            direction *= -Projectile.direction;
            if (Projectile.direction == -1) {
                direction *= -1;
            }
            Vector2 leafTwigPosition = -new Vector2(14, 122) + leafPosition;
            int projectile = CreateNatureProjectile(Projectile.GetSource_NaturalSpawn(), Item, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<EvilLeaf>(), Projectile.damage, Projectile.knockBack, Projectile.owner, direction, Projectile.identity);
            Main.projectile[projectile].As<EvilLeaf>().SetUpPositionOnTwig(leafTwigPosition);
            Main.projectile[projectile].As<EvilLeaf>().Crimson = Projectile.ai[2] == 1f;
            Main.projectile[projectile].As<EvilLeaf>().Index = index;
            index++;
        }

        Projectile.netUpdate = true;
    }

    public override void AI() {
        _scale.X = MathHelper.SmoothStep(_scale.X, 1f, 0.5f);
        _scale.Y = MathHelper.SmoothStep(_scale.Y, 1f, 0.5f);
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
                position.Y += sourceRectangle.Height * (1f - _scale.Y);
            }
            position.Y += 2;
            SpriteEffects spriteEffects = (SpriteEffects)(Projectile.direction == 1).ToInt();
            Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, sourceRectangle.BottomCenter(), _scale, spriteEffects);
        }
        Color drawColor = Lighting.GetColor((Projectile.Center - Vector2.UnitY * 20f).ToTileCoordinates());
        draw(drawColor, true);
        draw(drawColor, false);

        return false;
    }
}