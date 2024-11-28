using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

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
    private Vector2 _twigPosition;
    private float _extra;

    internal bool Crimson;

    public override string Texture => ResourceManager.FriendlyProjectileTextures + $"Druidic/{nameof(EvilLeaf)}";

    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 200;
        Projectile.penetrate = -1;
        Projectile.hide = true;
    }

    internal void SetUpPositionOnTwig(Vector2 position) => _twigPosition = position;

    public override void AI() {
        Projectile.direction = (int)Projectile.ai[0];
        Projectile parent = Main.projectile[(int)Projectile.ai[1]];
        if (parent != null && parent.active) {
            Vector2 parentScale = parent.As<EvilBranch>().Scale;
            Projectile.position = parent.position + _twigPosition * parentScale;
            Projectile.position -= Vector2.One * 2f;
            if (Projectile.localAI[0] == 0f) {
                Projectile.ai[2] = 1f;
                Projectile.localAI[2] = 1.25f;
                Projectile.localAI[1] = Projectile.localAI[2];
            }
            float scaleY = parentScale.Y * Projectile.ai[2];
            float rotation = MathHelper.Pi * Projectile.direction * (1f - scaleY);
            if (Projectile.localAI[0] == 0f) {
                Projectile.localAI[0] = 1f;
                Projectile.rotation = rotation;
                _extra = 1f;
            }
            if (Projectile.ai[0] == 1f) {
                Projectile.position -= new Vector2(10f, 0f);
            }
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, rotation, 0.2f);
            if (scaleY < 0.5f) {
                //Projectile.localAI[2] = 1.25f;
                //Projectile.localAI[1] = Projectile.localAI[2];
            }
            else {
                Projectile.localAI[2] = MathHelper.SmoothStep(Projectile.localAI[2], 1f + 0.1f * _extra, Projectile.localAI[1] * 0.25f);
                Projectile.localAI[1] *= 0.999f;
                if (scaleY >= 0.9f) {
                    _extra = MathHelper.Lerp(_extra, 0f, Projectile.localAI[1] * 0.175f);
                }
            }
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.localAI[2], Projectile.ai[2] * 0.25f);
            //Projectile.rotation += 0.1f;
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        SpriteEffects spriteEffects = (SpriteEffects)((int)Projectile.ai[0] != 1).ToInt();
        Color color = Color.White;
        Rectangle sourceRectangle = new(Crimson ? 14 : 0, 0, 14, 14);
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, Projectile.ai[0] == 1 ? new Vector2(2, 12) : new Vector2(12, 12), Projectile.scale, spriteEffects);

        return false;
    }
}

sealed class EvilBranch : NatureProjectile {
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
        Projectile.timeLeft = 200;
        Projectile.penetrate = -1;
        Projectile.hide = true;
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
        while (!WorldGen.SolidTile(point)) {
            if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point.X, point.Y).TileType]) {
                break;
            }

            point.Y++;
        }
        Projectile.Center = point.ToWorldCoordinates();

        SetUpLeafPoints();
        foreach (LeafInfo leafInfo in _leavesInfo) {
            Vector2 leafPosition = leafInfo.Position;
            int direction = leafInfo.FacedRight.ToDirectionInt();
            leafPosition.X = 42f - leafPosition.X;
            direction *= -Projectile.direction;
            if (Projectile.direction == -1) {
                direction *= -1;
            }
            Vector2 position = Projectile.position;
            Vector2 leafTwigPosition = -new Vector2(14, 122) + leafPosition;
            position += leafTwigPosition;
            int projectile = CreateNatureProjectile(Projectile.GetSource_NaturalSpawn(), Item, position, Vector2.Zero, ModContent.ProjectileType<EvilLeaf>(), Projectile.damage, Projectile.knockBack, Projectile.owner, direction, Projectile.identity);
            Main.projectile[projectile].As<EvilLeaf>().SetUpPositionOnTwig(leafTwigPosition);
            Main.projectile[projectile].As<EvilLeaf>().Crimson = Projectile.ai[2] == 1f;
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
                position.Y -= sourceRectangle.Height;
                position.Y += 2;
                position.Y += sourceRectangle.Height * (1f - _scale.Y);
            }
            position.Y -= 6;
            SpriteEffects spriteEffects = (SpriteEffects)(Projectile.direction == 1).ToInt();
            Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, sourceRectangle.BottomCenter(), _scale, spriteEffects);
        }
        draw(Color.White, true);
        draw(Color.White, false);

        return false;
    }
}