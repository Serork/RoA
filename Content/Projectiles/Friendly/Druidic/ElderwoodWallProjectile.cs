using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Projectiles.Enemies;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.ModBrowser;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class ElderwoodWallProjectile : NatureProjectile {
    private const int MAX_TIMELEFT = 180;

    private bool _offseted;

    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<VileSpike>()).Texture;
    public static string TipTexture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<VileSpikeTip>()).Texture;
    public static string StartTexture => ResourceManager.EnemyProjectileTextures + "VileSpikeStart";

    private int Length => (int)Projectile.ai[0];
    private bool Temporary => Projectile.ai[1] >= 1f;

    protected override void SafeSetDefaults() {
        int width = 30; int height = 32;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = AIType = -1;

        Projectile.penetrate = -1;
        Projectile.timeLeft = MAX_TIMELEFT;

        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.hide = true;

        ShouldIncreaseWreathPoints = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        return Collision.CheckAABBvAABBCollision(new Vector2(Projectile.position.X, Projectile.ai[2] - Projectile.localAI[0]), new Vector2(texture.Width, Projectile.localAI[0]), targetHitbox.Location.ToVector2(), targetHitbox.Size());
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.owner == Projectile.owner && projectile.type == Type && projectile.whoAmI != Projectile.whoAmI &&
                Projectile.position.X < projectile.position.X + texture.Width && Projectile.position.X > projectile.position.X - texture.Width) {
                Projectile.position.X -= (texture.Width + 2) * (Projectile.position - Projectile.GetOwnerAsPlayer().position).X.GetDirection();
                _offseted = true;
            }
        }
        Projectile.localAI[0] = Projectile.localAI[1] = texture.Height * Length;
        Projectile.ai[2] = Projectile.position.Y;
        if (Temporary) {
            Projectile.localAI[2] = Length * 10;
            Projectile.localAI[2] = Projectile.localAI[2] * (1f + Projectile.ai[1] - 1f);
            Projectile.timeLeft = (int)Projectile.localAI[2];
        }
        Projectile.netUpdate = true;
    }

    public override void AI() {
        float value = 1f + Projectile.localAI[1] / 10f;
        float min = MathHelper.Min(10f, Projectile.localAI[2] / 3f);
        if (Projectile.timeLeft > Projectile.localAI[2] - 6f && _offseted) {
            Point point = new Vector2(Projectile.Center.X - Projectile.width * 2f, Projectile.Center.Y - 20f).ToTileCoordinates();
            Point point2 = new Vector2(Projectile.Center.X + Projectile.width * 2f, Projectile.Center.Y + 20f).ToTileCoordinates();
            ElderwoodClaws.SpawnGroundDusts(point, point2, Projectile.ai[0] * 5f);
        }
        if (Projectile.timeLeft > (Projectile.localAI[2] - min)) {
            if (Projectile.localAI[0] > 0f) {
                Projectile.localAI[0] -= value;
            }
        }
        else if (Projectile.timeLeft < min) {
            if (Projectile.localAI[0] < Projectile.localAI[1]) {
                Projectile.localAI[0] += value;
            }
        }
        Projectile.position.Y = Projectile.ai[2] + Projectile.localAI[0];
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 start = Projectile.Center + Vector2.UnitY * texture.Height * 2f;
        int index = 0;
        int length = Length + 2;
        Texture2D startTexture = ModContent.Request<Texture2D>(StartTexture).Value;
        while (index < length) {
            if (index == 0) {
                texture = startTexture;
            }
            else {
                texture = ModContent.Request<Texture2D>(Texture).Value;
            }
            void next() {
                start.Y -= texture.Height;
                index++;
            }
            if (index == length - 1) {
                texture = ModContent.Request<Texture2D>(TipTexture).Value;
            }
            float value = Projectile.ai[2];
            next();
            if (start.Y <= value + texture.Height * 2f) {
                bool flag = start.Y > value + texture.Height;
                Color color = Lighting.GetColor((int)start.X / 16, (int)start.Y / 16);
                Texture2D usedTexture = flag ? startTexture : texture;
                Vector2 origin = new(usedTexture.Width / 2f, usedTexture.Height);
                Main.EntitySpriteDraw(usedTexture, start - Main.screenPosition, null, color, Projectile.rotation, origin, Projectile.scale, default);
            }
        }
        return false;
    }
}
