using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility.Vanilla;

static class ProjectileUtils {
    public static void QuickDraw(this Projectile projectile, Color lightColor, float exRot = 0f, Texture2D? texture = null, SpriteEffects? spriteEffects = null) {
        Texture2D mainTex = projectile.GetTexture();

        SpriteEffects effects = spriteEffects ?? projectile.spriteDirection.ToSpriteEffects();

        Main.spriteBatch.Draw(texture ?? mainTex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation + exRot,
            mainTex.Size() / 2, projectile.scale, effects, 0);
    }

    public static void QuickDraw(this Projectile projectile, Color lightColor, float overrideRot, int EmmmItIsAStupidValue) {
        Texture2D mainTex = projectile.GetTexture();

        Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, null, lightColor, overrideRot,
            mainTex.Size() / 2, projectile.scale, 0, 0);
    }

    public static void QuickDraw(this Projectile projectile, Rectangle frameBox, Color lightColor, float exRot) {
        Texture2D mainTex = projectile.GetTexture();

        Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, frameBox, lightColor, projectile.rotation + exRot,
            frameBox.Size() / 2, projectile.scale, 0, 0);
    }

    public static void QuickDrawAnimated(this Projectile projectile, Color lightColor, float exRot = 0f, Texture2D? texture = null, byte maxFrames = 0, Vector2? scale = null, Vector2? origin = null, Vector2? originScale = null, SpriteEffects? spriteEffects = null) {
        Texture2D mainTex = texture ?? projectile.GetTexture();

        int frameSize = mainTex.Height / (maxFrames != 0 ? maxFrames : Main.projFrames[projectile.type]);
        Rectangle frameBox = new(0, frameSize * projectile.frame, mainTex.Width, frameSize);
        SpriteEffects effects = spriteEffects ?? projectile.spriteDirection.ToSpriteEffects();
        origin ??= frameBox.Size() / 2;
        if (originScale != null) {
            origin *= originScale;
        }
        if (scale != null) {
            Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, frameBox, lightColor, projectile.rotation + exRot,
                origin.Value, scale.Value, effects, 0);
        }
        else {
            Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, frameBox, lightColor, projectile.rotation + exRot,
                origin.Value, projectile.scale, effects, 0);
        }
    }

    public static void QuickDrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, int start, float extraRot = 0, float scale = -1, Texture2D? texture = null) {
        int howMany = ProjectileID.Sets.TrailCacheLength[projectile.type];
        projectile.DrawShadowTrails(drawColor, maxAlpha, maxAlpha / howMany, start, howMany, 1, extraRot, scale, texture);
    }

    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, float extraRot = 0, float scale = -1, Texture2D? texture = null) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(texture ?? mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, null,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, mainTex.Size() / 2, scale == -1 ? projectile.scale : scale, 0, 0);
    }

    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, float scaleStep, float extraRot = 0, float scale = -1) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, null,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, mainTex.Size() / 2, (scale == -1 ? projectile.scale : scale) * (1 - (i * scaleStep)), 0, 0);
    }

    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, float scaleStep, Rectangle frameBox, float extraRot = 0, float scale = -1) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);
        var origin = frameBox.Size() / 2;

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, frameBox,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, origin, (scale == -1 ? projectile.scale : scale) * (1 - (i * scaleStep)), 0, 0);
    }

    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, Vector2 scale, float extraRot = 0) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, null,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, mainTex.Size() / 2, scale, 0, 0);
    }

    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, Vector2 scale, float scaleStep, float extraRot = 0) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, null,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, mainTex.Size() / 2, scale * (1 - (i * scaleStep)), 0, 0);
    }

    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, float scale, Rectangle frameBox, float extraRot) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, frameBox,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, frameBox.Size() / 2, scale, 0, 0);
    }

    public static void DrawShadowTrailsSacleStep(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, float scaleStep, Rectangle frameBox, float extraRot = 0, float scale = -1) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, frameBox,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, frameBox.Size() / 2, (scale == -1 ? projectile.scale : scale) * (1 - (i * scaleStep)), 0, 0);
    }

    public static void DrawShadowTrailsSacleStep(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, float scaleStep, Rectangle? frameBox, SpriteEffects effect, float extraRot = 0, float scale = -1) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);
        Vector2 origin = frameBox.HasValue ? frameBox.Value.Size() / 2 : mainTex.Size() / 2;

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, frameBox,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, origin, (scale == -1 ? projectile.scale : scale) * (1 - (i * scaleStep)), effect, 0);
    }


    public static void DrawShadowTrails(this Projectile projectile, Color drawColor, float maxAlpha, float alphaStep, int start, int howMany, int step, Vector2 scale, Rectangle frameBox, float extraRot = 0) {
        Texture2D mainTex = TextureAssets.Projectile[projectile.type].Value;
        Vector2 toCenter = new(projectile.width / 2, projectile.height / 2);

        for (int i = start; i < howMany; i += step)
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + toCenter - Main.screenPosition, frameBox,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + extraRot, frameBox.Size() / 2, scale, 0, 0);
    }

    public static void Animate(this Projectile projectile, byte frameCounter, byte maxFrames = 0) {
        if (++projectile.frameCounter >= frameCounter) {
            projectile.frameCounter = 0;
            if (++projectile.frame >= (maxFrames > 0 ? maxFrames : Main.projFrames[projectile.type])) {
                projectile.frame = 0;
            }
        }
    }

    public readonly ref struct SpawnProjectileArgs(Player player, IEntitySource source) {
        public Player Player { get; } = player;
        public IEntitySource Source { get; } = source;
        public Vector2? Position { get; init; }
        public Vector2? Velocity { get; init; }
        public int Damage { get; init; }
        public float KnockBack { get; init; }
        public float AI0 { get; init; }
        public float AI1 { get; init; }
        public float AI2 { get; init; }
        public ushort? ProjectileType { get; init; }
    }

    public readonly ref struct SpawnHostileProjectileArgs(Entity entity, IEntitySource source) {
        public Entity Entity { get; } = entity;
        public IEntitySource Source { get; } = source;
        public Vector2? Position { get; init; }
        public Vector2? Velocity { get; init; }
        public int Damage { get; init; }
        public float KnockBack { get; init; }
        public float AI0 { get; init; }
        public float AI1 { get; init; }
        public float AI2 { get; init; }
        public ushort? ProjectileType { get; init; }
    }

    public ref struct SpawnCopyArgs(Projectile projectile, IEntitySource source) {
        public Projectile Projectile = projectile;
        public IEntitySource Source = source;
        public Vector2? Velocity = null;
        public float AI0 = 0f;
        public float AI1 = 0f;
        public float AI2 = 0f;
    }

    public static Projectile SpawnPlayerOwnedProjectile<T>(in SpawnProjectileArgs spawnProjectileArgs, Action<Projectile>? beforeNetSend = null, bool centered = false) where T : ModProjectile {
        Player player = spawnProjectileArgs.Player;
        return Main.projectile[NewProjectile(spawnProjectileArgs.Source, spawnProjectileArgs.Position ?? player.Center, spawnProjectileArgs.Velocity ?? Vector2.Zero, spawnProjectileArgs.ProjectileType ?? ModContent.ProjectileType<T>(), spawnProjectileArgs.Damage, spawnProjectileArgs.KnockBack, player.whoAmI, spawnProjectileArgs.AI0, spawnProjectileArgs.AI1, spawnProjectileArgs.AI2, beforeNetSend, centered)];
    }

    public static Projectile SpawnHostileProjectile<T>(in SpawnHostileProjectileArgs spawnProjectileArgs, Action<Projectile>? beforeNetSend = null, bool centered = false) where T : ModProjectile {
        Entity entity = spawnProjectileArgs.Entity;
        return Main.projectile[NewProjectile(spawnProjectileArgs.Source, spawnProjectileArgs.Position ?? entity.Center, spawnProjectileArgs.Velocity ?? Vector2.Zero, spawnProjectileArgs.ProjectileType ?? ModContent.ProjectileType<T>(), spawnProjectileArgs.Damage, spawnProjectileArgs.KnockBack, Main.myPlayer, spawnProjectileArgs.AI0, spawnProjectileArgs.AI1, spawnProjectileArgs.AI2, beforeNetSend, centered)];
    }

    public static Projectile SpawnPlayerOwnedProjectileCopy<T>(in SpawnCopyArgs spawnCopyArgs, Action<Projectile>? beforeNetSend = null, bool centered = false) where T : ModProjectile {
        Projectile projectile = spawnCopyArgs.Projectile;
        return SpawnPlayerOwnedProjectile<T>(new SpawnProjectileArgs(projectile.GetOwnerAsPlayer(), spawnCopyArgs.Source) {
            Position = projectile.position,
            Velocity = spawnCopyArgs.Velocity ?? projectile.velocity,
            Damage = projectile.damage,
            KnockBack = projectile.knockBack,
            AI0 = spawnCopyArgs.AI0,
            AI1 = spawnCopyArgs.AI1,
            AI2 = spawnCopyArgs.AI2
        }, beforeNetSend, centered);
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FindBannerToAssociateTo")]
    public extern static void Projectile_FindBannerToAssociateTo(Projectile self, IEntitySource spawnSource, Projectile next);

    public static int NewProjectile(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, Action<Projectile>? beforeNetSend = null, bool centered = false) => NewProjectile(spawnSource, position.X, position.Y, velocity.X, velocity.Y, Type, Damage, KnockBack, Owner, ai0, ai1, ai2, beforeNetSend, centered);

    /// <summary>
    /// PROBLEM?: <c>Projectile.OnSpawn</c> is called through reflection
    /// </summary>
    public static int NewProjectile(IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, Action<Projectile>? beforeNetSend = null, bool centered = false) {
        if (Owner == -1)
            Owner = Main.myPlayer;

        int num = 1000;
        for (int i = 0; i < 1000; i++) {
            if (!Main.projectile[i].active) {
                num = i;
                break;
            }
        }

        if (num == 1000)
            num = Projectile.FindOldestProjectile();

        Projectile projectile = Main.projectile[num];
        projectile.SetDefaults(Type);
        projectile.position.X = X - (float)projectile.width * 0.5f;
        projectile.position.Y = Y - (float)projectile.height * 0.5f;
        projectile.owner = Owner;
        projectile.velocity.X = SpeedX;
        projectile.velocity.Y = SpeedY;
        projectile.damage = Damage;
        projectile.knockBack = KnockBack;
        projectile.identity = num;
        projectile.gfxOffY = 0f;
        projectile.stepSpeed = 1f;
        projectile.wet = Collision.WetCollision(projectile.position, projectile.width, projectile.height);
        if (projectile.ignoreWater)
            projectile.wet = false;

        projectile.honeyWet = Collision.honey;
        projectile.shimmerWet = Collision.shimmer;
        Main.projectileIdentity[Owner, num] = num;
        Projectile_FindBannerToAssociateTo(projectile, spawnSource, projectile);


        if (projectile.aiStyle == 1) {
            while (projectile.velocity.X >= 16f || projectile.velocity.X <= -16f || projectile.velocity.Y >= 16f || projectile.velocity.Y < -16f) {
                projectile.velocity.X *= 0.97f;
                projectile.velocity.Y *= 0.97f;
            }
        }

        if (Owner == Main.myPlayer) {
            switch (Type) {
                case 206:
                    projectile.ai[0] = (float)Main.rand.Next(-100, 101) * 0.0005f;
                    projectile.ai[1] = (float)Main.rand.Next(-100, 101) * 0.0005f;
                    break;
                case 335:
                    projectile.ai[1] = Main.rand.Next(4);
                    break;
                case 358:
                    projectile.ai[1] = (float)Main.rand.Next(10, 31) * 0.1f;
                    break;
                case 406:
                    projectile.ai[1] = (float)Main.rand.Next(10, 21) * 0.1f;
                    break;
                default:
                    projectile.ai[0] = ai0;
                    projectile.ai[1] = ai1;
                    projectile.ai[2] = ai2;
                    break;
            }
        }

        if (Type == 434) {
            projectile.ai[0] = projectile.position.X;
            projectile.ai[1] = projectile.position.Y;
        }

        if (Type > 0) {
            if (ProjectileID.Sets.NeedsUUID[Type])
                projectile.projUUID = projectile.identity;

            if (ProjectileID.Sets.StardustDragon[Type]) {
                int num2 = Main.projectile[(int)projectile.ai[0]].projUUID;
                if (num2 >= 0)
                    projectile.ai[0] = num2;
            }
        }

        // Copied at the bottom. Gotos would be messy.
        /*
		if (Main.netMode != 0 && Owner == Main.myPlayer)
			NetMessage.SendData(27, -1, -1, null, num);
		*/

        if (Owner == Main.myPlayer) {
            if (ProjectileID.Sets.IsAGolfBall[Type] && Damage <= 0) {
                int num3 = 0;
                int num4 = 0;
                int num5 = 99999999;
                for (int j = 0; j < 1000; j++) {
                    if (Main.projectile[j].active && ProjectileID.Sets.IsAGolfBall[Main.projectile[j].type] && Main.projectile[j].owner == Owner && Main.projectile[j].damage <= 0) {
                        num3++;
                        if (num5 > Main.projectile[j].timeLeft) {
                            num4 = j;
                            num5 = Main.projectile[j].timeLeft;
                        }
                    }
                }

                if (num3 > 10)
                    Main.projectile[num4].Kill();
            }

            if (Type == 28)
                projectile.timeLeft = 180;

            if (Type == 516)
                projectile.timeLeft = 180;

            if (Type == 519)
                projectile.timeLeft = 180;

            if (Type == 29)
                projectile.timeLeft = 300;

            if (Type == 470)
                projectile.timeLeft = 300;

            if (Type == 637)
                projectile.timeLeft = 300;

            if (Type == 30)
                projectile.timeLeft = 180;

            if (Type == 517)
                projectile.timeLeft = 180;

            if (Type == 37)
                projectile.timeLeft = 180;

            if (Type == 773)
                projectile.timeLeft = 180;

            if (Type == 75)
                projectile.timeLeft = 180;

            if (Type == 133)
                projectile.timeLeft = 180;

            if (Type == 136)
                projectile.timeLeft = 180;

            if (Type == 139)
                projectile.timeLeft = 180;

            if (Type == 142)
                projectile.timeLeft = 180;

            if (Type == 397)
                projectile.timeLeft = 180;

            if (Type == 419)
                projectile.timeLeft = 600;

            if (Type == 420)
                projectile.timeLeft = 600;

            if (Type == 421)
                projectile.timeLeft = 600;

            if (Type == 422)
                projectile.timeLeft = 600;

            if (Type == 588)
                projectile.timeLeft = 180;

            if (Type == 779)
                projectile.timeLeft = 60;

            if (Type == 783)
                projectile.timeLeft = 60;

            if (Type == 862 || Type == 863)
                projectile.timeLeft = 60;

            if (Type == 443)
                projectile.timeLeft = 300;

            if (Type == 681)
                projectile.timeLeft = 600;

            if (Type == 684)
                projectile.timeLeft = 60;

            if (Type == 706)
                projectile.timeLeft = 120;

            if (Type == 680 && Main.player[projectile.owner].setSquireT2)
                projectile.penetrate = 7;

            if (Type == 777 || Type == 781 || Type == 794 || Type == 797 || Type == 800 || Type == 785 || Type == 788 || Type == 791 || Type == 903 || Type == 904 || Type == 905 || Type == 906 || Type == 910 || Type == 911)
                projectile.timeLeft = 180;

            // Copied from 1.3, moved from Shoot context to OnSpawn with matching logic
            if (Main.netMode != NetmodeID.Server) {
                Player throwingPlayer = Main.player[Owner];
                if (throwingPlayer.AnyThrownCostReduction && throwingPlayer.HeldItem.CountsAsClass(DamageClass.Throwing) && spawnSource is EntitySource_ItemUse_WithAmmo)
                    projectile.noDropItem = true;
            }
        }

        if (Type == 249)
            projectile.frame = Main.rand.Next(5);

        if (Owner == Main.myPlayer)
            Main.player[Owner].TryUpdateChannel(projectile);

        projectile.ApplyStatsFromSource(spawnSource);
        //ProjectileLoader.OnSpawn(projectile, spawnSource);

        var HookOnSpawn = typeof(ProjectileLoader).GetField("HookOnSpawn",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);

        projectile.ModProjectile?.OnSpawn(spawnSource);

        foreach (var g in (HookOnSpawn.GetValue(null) as Terraria.ModLoader.Core.GlobalHookList<Terraria.ModLoader.GlobalProjectile>).Enumerate(projectile)) {
            g.OnSpawn(projectile, spawnSource);
        }

        beforeNetSend?.Invoke(projectile);

        if (centered) {
            projectile.position += projectile.Size / 2f;
        }

        if (Main.netMode != 0 && Owner == Main.myPlayer)
            NetMessage.SendData(27, -1, -1, null, num);

        TrackedEntitiesSystem.RegisterTrackedProjectile(projectile);

        return num;
    }

    public static bool CanProjectileCutTiles(Projectile checkProjectile) {
        if (ProjectileLoader.CanCutTiles(checkProjectile) is bool modResult)
            return modResult;

        if (checkProjectile.aiStyle != 45 && checkProjectile.aiStyle != 137 && checkProjectile.aiStyle != 92 && checkProjectile.aiStyle != 105 && checkProjectile.aiStyle != 106 && !ProjectileID.Sets.IsAGolfBall[checkProjectile.type] && checkProjectile.type != 463 && checkProjectile.type != 69 && checkProjectile.type != 70 && checkProjectile.type != 621 && checkProjectile.type != 10 && checkProjectile.type != 11 && checkProjectile.type != 379 && checkProjectile.type != 407 && checkProjectile.type != 476 && checkProjectile.type != 623 && (checkProjectile.type < 625 || checkProjectile.type > 628) && checkProjectile.type != 833 && checkProjectile.type != 834 && checkProjectile.type != 835 && checkProjectile.type != 818 && checkProjectile.type != 831 && checkProjectile.type != 820 && checkProjectile.type != 864 && checkProjectile.type != 970 && checkProjectile.type != 995 && checkProjectile.type != 908)
            return checkProjectile.type != 1020;

        return false;
    }

    public static void CutTiles(Projectile projectileThatCuts) {
        if (CanProjectileCutTiles(projectileThatCuts))
            return;

        int owner = projectileThatCuts.owner;
        AchievementsHelper.CurrentlyMining = true;
        bool[] tileCutIgnorance = Main.player[owner].GetTileCutIgnorance(allowRegrowth: false, projectileThatCuts.trap);
        DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        DelegateMethods.tileCutIgnore = tileCutIgnorance;

        CutTilesAt(projectileThatCuts, projectileThatCuts.position, projectileThatCuts.width, projectileThatCuts.height);

        AchievementsHelper.CurrentlyMining = false;
    }

    public static void CutTilesAt(Projectile projectileThatCuts, Vector2 checkBoxPosition, int checkBoxWidth, int checkBoxHeight) {
        int num = (int)(checkBoxPosition.X / 16f);
        int num2 = (int)((checkBoxPosition.X + (float)checkBoxWidth) / 16f) + 1;
        int num3 = (int)(checkBoxPosition.Y / 16f);
        int num4 = (int)((checkBoxPosition.Y + (float)checkBoxHeight) / 16f) + 1;
        if (num < 0)
            num = 0;

        if (num2 > Main.maxTilesX)
            num2 = Main.maxTilesX;

        if (num3 < 0)
            num3 = 0;

        if (num4 > Main.maxTilesY)
            num4 = Main.maxTilesY;

        bool[] tileCutIgnorance = Main.player[projectileThatCuts.owner].GetTileCutIgnorance(allowRegrowth: false, projectileThatCuts.trap);
        for (int i = num; i < num2; i++) {
            for (int j = num3; j < num4; j++) {
                if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
                    WorldGen.KillTile(i, j);
                    if (Main.netMode != 0)
                        NetMessage.SendData(17, -1, -1, null, 0, i, j);
                    // Extra patch context.
                }
            }
        }

        ProjectileLoader.CutTiles(projectileThatCuts);
    }

    public static void DrawSpearProjectile(Projectile projectile, Texture2D? texture = null, Texture2D? glowMaskTexture = null) {
        Projectile proj = projectile;
        texture ??= TextureAssets.Projectile[projectile.type].Value;
        SpriteEffects dir = SpriteEffects.None;
        float num = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X) + 2.355f;
        Player player = Main.player[proj.owner];
        Microsoft.Xna.Framework.Rectangle value = texture.Frame();
        Microsoft.Xna.Framework.Rectangle rect = proj.getRect();
        Vector2 vector = Vector2.Zero;
        if (player.direction > 0) {
            dir = SpriteEffects.FlipHorizontally;
            vector.X = texture.Width;
            num -= (float)Math.PI / 2f;
        }

        if (player.gravDir == -1f) {
            if (proj.direction == 1) {
                dir = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                vector = new Vector2(texture.Width, texture.Height);
                num -= (float)Math.PI / 2f;
            }
            else if (proj.direction == -1) {
                dir = SpriteEffects.FlipVertically;
                vector = new Vector2(0f, texture.Height);
                num += (float)Math.PI / 2f;
            }
        }

        Vector2.Lerp(vector, value.Center.ToVector2(), 0.25f);
        float num2 = 0f;
        Vector2 vector2 = proj.Center;
        Color color = Lighting.GetColor((int)proj.Center.X / 16, (int)proj.Center.Y / 16);
        Main.EntitySpriteDraw(texture, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
        color = Color.White * (1f - proj.alpha / 255f);

        if (projectile.type == ProjectileID.MushroomSpear) {
            DelegateMethods.v3_1 = new Vector3(0.1f, 0.4f, 1f);
            Utils.PlotTileLine(vector2, vector2 + Vector2.UnitY.RotatedBy(num) * value.Width, 4, DelegateMethods.CastLightOpen);
        }

        if (glowMaskTexture != null) {
            Main.EntitySpriteDraw(glowMaskTexture, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
        }
    }
}
