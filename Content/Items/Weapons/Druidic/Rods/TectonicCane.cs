using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Common.VisualEffects;
using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class TectonicDebris : VisualEffect<TectonicDebris> {
    protected override void SetDefaults() {
        MaxTimeLeft = TimeLeft = 120;

        SetFramedTexture(3);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        float length = Velocity.Length();
        Rotation += length * 0.0314f;

        DrawColor = Lighting.GetColor(Position.ToTileCoordinates());

        if (TimeLeft <= 30) {
            Scale = Utils.GetLerpValue(0, 30, TimeLeft, true);
        }
        else {
            Scale = MathHelper.Lerp(Scale, 1f, 0.015f);
        }

        if (AI0 == 0f) {
            Velocity *= 0.95f;
            if (Velocity.Length() < 0.1f) {
                AI0 = 1f;
                Velocity *= 0.5f;
            }
        }
        else {
            Velocity.X *= 0.5f;
            Velocity.Y += 0.1f;
            Velocity.Y = Math.Min(10f, Velocity.Y);
        }

        Position += Velocity;

        if (Scale <= 0.1f || float.IsNaN(Scale) || --TimeLeft <= 0) {
            RestInPool();
        }
    }
}

sealed class TectonicCane : BaseRodItem<TectonicCane.TectonicCaneBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<TectonicCaneProjectile>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36, 38);
        Item.SetDefaultToUsable(-1, 40, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.35f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class TectonicCaneBase : BaseRodProjectile {
        protected override byte TimeAfterShootToExist(Player player) => (byte)(player.itemTimeMax * 2);

        protected override bool ShouldWaitUntilProjDespawns() => false;

        protected override void SpawnDustsOnShoot(Player player, Vector2 corePosition) {
            int count = 10;
            for (int i = 0; i < count; i++) {
                float progress = (float)i / count;
                VisualEffectSystem.New<TectonicDebris>(VisualEffectLayer.BEHINDPROJS).
                    Setup(
                    corePosition - Vector2.One * 5f + Main.rand.RandomPointInArea(10f, 10f),
                    Vector2.One.RotatedByRandom(MathHelper.TwoPi * progress).SafeNormalize(Vector2.One) * Main.rand.NextFloat(2f, 5f),
                    scale: Main.rand.NextFloat(0.9f, 1.1f) * 1.5f);
            }
        }

        protected override void SpawnCoreDustsBeforeShoot(float step, Player player, Vector2 corePosition) {
            step = Ease.CubeIn(step);
            float offset = 40f * (1f - MathHelper.Clamp(step, 0.4f, 1f));
            Vector2 randomOffset = Main.rand.RandomPointInArea(offset, offset), spawnPosition = corePosition + randomOffset;
            bool flag = !Main.rand.NextBool(3);
            int dustType = flag ? ModContent.DustType<TectonicDust>() : DustID.Torch;
            float velocityFactor = MathHelper.Clamp(Vector2.Distance(spawnPosition, corePosition) / offset, 0.25f, 1f) * 2f * Math.Max(step, 0.25f) + 0.25f;
            Dust dust = Dust.NewDustPerfect(spawnPosition, dustType,
                Scale: MathHelper.Clamp(velocityFactor * 1.4f, 1.2f, 1.75f));
            dust.velocity = (corePosition - spawnPosition).SafeNormalize(Vector2.One) * velocityFactor;
            dust.velocity *= 0.9f;
            dust.noGravity = true;

            EvilBranch.GetPos(player, out Point point, out Point point2, maxDistance: 800f);
            Vector2 position = point2.ToWorldCoordinates();
            dustType = TileHelper.GetKillTileDust((int)position.X / 16, (int)position.Y / 16, Main.tile[(int)position.X / 16, (int)position.Y / 16]);
            float progress = 1.25f * Ease.ExpoInOut(Math.Max(step, 0.25f)) + 0.25f;
            int count = (int)(4 * Math.Max(0.25f, progress));
            for (int k = 0; k < count; k++) {
                Dust.NewDust(position - new Vector2(32f, 0f), 60, 2, dustType, 0, Main.rand.NextFloat(-2f, -1f) * progress, count < 2 ? 0 : Main.rand.Next(255), default, 
                    Main.rand.NextFloat(1.5f) * MathHelper.Clamp(progress, 0.6f, 0.85f));
            }
        }
    }
}

sealed class TectonicCaneProjectile : NatureProjectile {
    public override void Load() {
        On_Collision.TileCollision += On_Collision_TileCollision;
    }

    private static Dictionary<Point, (Projectile, Vector2)> GeneratePosition() {
        Dictionary<Point, (Projectile, Vector2)> tectonicPlatesPositions = [];
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.type == ModContent.ProjectileType<TectonicCaneProjectile>()) {
                List<Vector2> positions = [];
                for (int i = -2; i < 3; i++) {
                    Vector2 position = projectile.position - Vector2.UnitY * 120f * projectile.ai[0];
                    position.X += i * 10f;
                    positions.Add(position);
                }
                foreach (Vector2 position in positions) {
                    tectonicPlatesPositions.TryAdd(position.ToTileCoordinates(), (projectile, position));
                }
            }
        }
        return tectonicPlatesPositions;
    }

    private Vector2 On_Collision_TileCollision(On_Collision.orig_TileCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough, bool fall2, int gravDir) {
        Collision.up = false;
        Collision.down = false;
        Vector2 result = Velocity;
        Vector2 vector = Velocity;
        Vector2 vector2 = Position + Velocity;
        Vector2 vector3 = Position;
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = -1;
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;
        int num5 = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        Dictionary<Point, (Projectile, Vector2)> tectonicPlatesPositions = GeneratePosition();
        float num6 = (value4 + 3) * 16;
        Vector2 vector4 = default(Vector2);
        bool flag12 = false;
        for (int i = num5; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                int num1451 = TETrainingDummy.Find(i, j);
                if (num1451 != -1) {
                    flag12 = true;
                }
            }
        }
        for (int i = num5; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                bool flag3 = false;
                if (tectonicPlatesPositions.TryGetValue(new Point(i, j), out (Projectile, Vector2) turple)) {
                    flag3 = true;
                }
                if (flag12) {
                    flag3 = false;
                }
                if (!flag3 && (Main.tile[i, j] == null || !Main.tile[i, j].HasTile || Main.tile[i, j].IsActuated || (!Main.tileSolid[Main.tile[i, j].TileType] && (!Main.tileSolidTop[Main.tile[i, j].TileType] || Main.tile[i, j].TileFrameY != 0))))
                    continue;
                vector4.X = i * 16;
                vector4.Y = j * 16;
                if (flag3) {
                    vector4 = turple.Item2;
                    vector4.Y -= 2f;
                }
                int num7 = 16;
                if (Main.tile[i, j].IsHalfBlock) {
                    vector4.Y += 8f;
                    num7 -= 8;
                }

                bool flag4 = !flag3 || turple.Item1.ai[0] == 1f;
                if (flag4) {
                    if (!(vector2.X + (float)Width > vector4.X) ||
                        !(vector2.X < vector4.X + 16f) || 
                        !(vector2.Y + (float)Height > vector4.Y) ||
                        !(vector2.Y < vector4.Y + (float)num7))
                        continue;
                }

                bool flag = false;
                bool flag2 = false;
                if (Main.tile[i, j].Slope > (SlopeType)2) {
                    if (Main.tile[i, j].Slope == (SlopeType)3 && vector3.Y + Math.Abs(Velocity.X) >= vector4.Y && vector3.X >= vector4.X)
                        flag2 = true;

                    if (Main.tile[i, j].Slope == (SlopeType)4 && vector3.Y + Math.Abs(Velocity.X) >= vector4.Y && vector3.X + (float)Width <= vector4.X + 16f)
                        flag2 = true;
                }
                else if (Main.tile[i, j].Slope > 0) {
                    flag = true;
                    if (Main.tile[i, j].Slope == (SlopeType)1 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X >= vector4.X)
                        flag2 = true;

                    if (Main.tile[i, j].Slope == (SlopeType)2 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X + (float)Width <= vector4.X + 16f)
                        flag2 = true;
                }

                if (flag2)
                    continue;

                if (vector3.Y + (float)Height <= vector4.Y) {
                    Collision.down = true;
                    bool flag10 = !Main.tileSolidTop[Main.tile[i, j].TileType] || flag3 || !fallThrough;
                    bool flag11 = flag10 || !(Velocity.Y <= 1f || fall2);
                    if (flag11 && num6 > vector4.Y) {
                        num3 = i;
                        num4 = j;
                        if (num7 < 16)
                            num4++;

                        if (num3 != num && !flag) {
                            result.Y = vector4.Y - (vector3.Y + (float)Height) + ((gravDir == -1) ? (-0.01f) : 0f);
                            num6 = vector4.Y;
                            if (flag3 && fallThrough) {
                                result.Y += 0.01f;
                            }
                            if (!flag4) {
                                result.Y -= 20f;
                            }
                        }
                    }
                }
                else if (vector3.X + (float)Width <= vector4.X && !Main.tileSolidTop[Main.tile[i, j].TileType] && !flag3) {
                    if (i < 1 || (Main.tile[i - 1, j].Slope != (SlopeType)2 && Main.tile[i - 1, j].Slope != (SlopeType)4)) {
                        num = i;
                        num2 = j;
                        if (num2 != num4)
                            result.X = vector4.X - (vector3.X + (float)Width);

                        if (num3 == num)
                            result.Y = vector.Y;
                    }
                }
                else if (vector3.X >= vector4.X + 16f && !Main.tileSolidTop[Main.tile[i, j].TileType] && !flag3) {
                    if (Main.tile[i + 1, j].Slope != (SlopeType)1 && Main.tile[i + 1, j].Slope != (SlopeType)3) {
                        num = i;
                        num2 = j;
                        if (num2 != num4)
                            result.X = vector4.X + 16f - vector3.X;

                        if (num3 == num)
                            result.Y = vector.Y;
                    }
                }
                else if (vector3.Y >= vector4.Y + (float)num7 && !Main.tileSolidTop[Main.tile[i, j].TileType] && !flag3) {
                    Collision.up = true;
                    num3 = i;
                    num4 = j;
                    result.Y = vector4.Y + (float)num7 - vector3.Y + ((gravDir == 1) ? 0.01f : 0f);
                    if (num4 == num2)
                        result.X = vector.X;
                }
            }
        }

        return result;
    }


    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;

        Projectile.tileCollide = false;

        Projectile.netImportant = true;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Projectile.spriteDirection = Main.rand.NextBool().ToDirectionInt();

        EvilBranch.GetPos(Main.player[Projectile.owner], out Point point, out Point point2, maxDistance: 800f);
        Projectile.Center = point2.ToWorldCoordinates();

        Projectile.netUpdate = true;
    }

    private void SpawnDebris(Vector2 position, Vector2 center, float endPositionY) {
        if (Projectile.owner == Main.myPlayer) {
            Vector2 velocity;
            int type = ModContent.ProjectileType<TectonicCaneProjectile2>();
            float length = Main.rand.NextFloat(30f, 60f) * 0.02f;
            Vector2 speed = (position - center).SafeNormalize(Vector2.One) * length;
            speed = speed.RotatedBy(Main.rand.NextFloatRange(MathHelper.PiOver4 * 0.75f));
            velocity = speed;
            position = position + Main.rand.RandomPointInArea(8, 16);
            if (position.Y > endPositionY) {
                position.Y = endPositionY;
            }
            for (int i = 0; i < 4; i++) {
                bool flag = !Main.rand.NextBool(3);
                Dust dust = Dust.NewDustPerfect(position - Vector2.One * 2f + Main.rand.RandomPointInArea(4f, 4f),
                    flag ? ModContent.DustType<TectonicDust>() : DustID.Torch,
                    Scale: Main.rand.NextFloat(1.5f, 2f) * (flag ? Main.rand.NextFloat(0.5f, 0.75f) : 1f));
                dust.customData = 1;
            }
            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), position, velocity, type, Projectile.damage, Projectile.knockBack, Projectile.owner, speed.X.GetDirection());
        }
    }

    public override void AI() {
        if (Projectile.ai[0] < 1f) {
            Projectile.ai[0] += TimeSystem.LogicDeltaTime * 2.5f;
            int dustType = TileHelper.GetKillTileDust((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16, Main.tile[(int)Projectile.position.X / 16, (int)Projectile.position.Y / 16]);
            for (int k = 0; k < 9; k++) {
                Dust.NewDust(Projectile.position - new Vector2(32f, 0f), 60, 2, dustType, 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));
            }
        }
        else {
            Projectile.ai[0] = 1f;
        }
        Projectile.ai[1] = Ease.SineOut(Projectile.ai[0]);
        float lerpAmount = 1f - Projectile.ai[0] * 0.5f;
        Projectile.localAI[0] = MathHelper.SmoothStep(Projectile.localAI[0], Main.rand.NextFloatRange(35f) * Main.rand.NextFloatDirection(), lerpAmount);
        Projectile.localAI[1] = MathHelper.SmoothStep(Projectile.localAI[1], Main.rand.NextFloatRange(50f) * Main.rand.NextFloatDirection(), lerpAmount);

        Vector2 size = new(60f, 90f * Projectile.ai[0]);
        Vector2 offset = new(0f, size.Y / 3f);
        Vector2 startPosition = Projectile.position - new Vector2(size.X / 2f, size.Y) - offset;
        Vector2 endPosition = startPosition + size;
        Vector2 center = startPosition + size / 2f;
        center += Vector2.UnitY * 10f;
        center.X -= 4f;
        Vector2 length = endPosition - startPosition;
        float num56 = 0.45f;
        if (num56 > 0.6f)
            num56 = 0.6f;
        DelegateMethods.v3_1 = new Vector3(num56, num56 * 0.65f, num56 * 0.4f);
        Utils.PlotTileLine(center - size / 2f - Vector2.One * 4f, center + size / 2f - Vector2.One * 4f, (float)8f * Projectile.scale, DelegateMethods.CastLight);

        //if (Projectile.timeLeft < 180 && Projectile.Opacity > 0.05f) {
        //    Projectile.localAI[2] += 1f + 0.01f * Projectile.timeLeft;
        //    if (Projectile.localAI[2] >= 30f) {
        //        //Projectile.Opacity -= 0.1f;
        //        if (Projectile.Opacity < 0f) {
        //            Projectile.Opacity = 0f;
        //        }
        //        Projectile.localAI[2] = 0f;

        //        SpawnDebris();
        //    }
        //}
        if (Projectile.timeLeft < 100) {
            Projectile.timeLeft = 100;
            float max = length.X / 20;
            float max2 = length.Y / 14;
            for (int index2 = 0; (double)index2 < max2; ++index2) {
                float value = index2 / (max2 * 1.5f);
                value = MathHelper.Clamp(value, 0.2f, 0.35f);
                for (int index1 = (int)(max * (value)); (double)index1 < max * (1f - value); ++index1) {
                    Vector2 position = startPosition + new Vector2((float)(10 + index1 * 20), (float)(7 + index2 * 14));
                    float length2 = Math.Abs(position.Length());
                    float length3 = Math.Abs(center.Length());
                    if (length2 > length3 || length2 < length3) {
                        SpawnDebris(position, center, endPosition.Y);
                    }
                }
            }
            Projectile.Kill();
        }
    }

    public override void PostAI() {
        if (++Projectile.frameCounter > 8) {
            Projectile.frameCounter = 0;
            if (++Projectile.frame > 3) {
                Projectile.frame = 0;
            }
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.005f;
        target.AddBuff(ModContent.BuffType<Burning>(), (int)(60f * num2 * 2f));
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.005f;
        target.AddBuff(ModContent.BuffType<Burning>(), (int)(60f * num2 * 2f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return Collision.CheckAABBvAABBCollision(
            targetHitbox.Location.ToVector2(), 
            targetHitbox.Size(), 
            Projectile.position - new Vector2(30f, 120f),
            new Vector2(60f, 120f));
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

    public override bool PreDraw(ref Color lightColor) {
        Vector2 size = new(60f, 90f);
        Vector2 offset = new(0f, size.Y / 3f);
        Vector2 startPosition = Projectile.position - new Vector2(size.X / 2f, size.Y) - offset;
        Vector2 endPosition = startPosition + size;
        Vector2 center = startPosition + size / 2f;
        SpriteFrame frame = new(1, 4);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Color drawColor = Lighting.GetColor(center.ToTileCoordinates());
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection == 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        frame = frame.With(0, (byte)Projectile.frame);
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        int height = (int)(sourceRectangle.Height * Projectile.ai[1]);
        sourceRectangle.Height = Math.Clamp(height, 2, sourceRectangle.Height);
        bool flag = Projectile.ai[0] != 1f;
        if (flag) {
            position += new Vector2(Main.rand.NextFloatRange(0.5f) * Projectile.localAI[0], Main.rand.NextFloatRange(1f) * Projectile.localAI[1]) * 0.75f * (1f - Projectile.ai[0]);
        }
        if (position.Y > Projectile.position.Y) {
            position.Y = Projectile.position.Y;
        }
        //texture = ModContent.Request<Texture2D>(Texture + "3").Value;
        //Main.EntitySpriteDraw(texture, position + Vector2.One, null, drawColor, Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);
        texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, drawColor * Projectile.Opacity, Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);
        texture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.Lerp(drawColor, Color.White, 0.5f) * (drawColor.A / 255f) * Ease.QuartIn(Projectile.Opacity), Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);

        return false;
    }
}

sealed class TectonicCaneProjectile2 : NatureProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 2;
    }

    protected override void SafeSetDefaults() {
        Projectile.Size = new Vector2(18f, 16f);
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Projectile.frame = Main.rand.NextBool().ToInt();
    }

    public override void AI() {
        float value = Projectile.velocity.Length();
        Projectile.direction = (int)Projectile.ai[0];
        Projectile.rotation += value * 0.05f * Projectile.direction;
        Projectile.velocity.Y += 0.1f;
        Projectile.velocity.Y = Math.Min(10f, Projectile.velocity.Y);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.ai[1] != 1f) {
            float max = Math.Max(oldVelocity.X, oldVelocity.Y);
            if (oldVelocity.X == max) {
                oldVelocity.X = 0f;
            }
            else {
                oldVelocity.Y = 0f;
            }
            Projectile.ai[1] = 1f;
        }
        else {
            Projectile.velocity *= 0.97f;
        }

        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 6; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<TectonicDust>(),
                Scale: Main.rand.NextFloat(0.95f, 1.05f) * 1.2f);
            dust.velocity *= Main.rand.NextFloat();
            dust.velocity *= 0.7f;
            dust.noGravity = true;
        }
    }
}