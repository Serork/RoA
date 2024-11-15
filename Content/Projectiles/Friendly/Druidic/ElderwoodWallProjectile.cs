using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Projectiles.Enemies;
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
        return Collision.CheckAABBvAABBCollision(Projectile.position - Vector2.UnitY * texture.Height, new Vector2(texture.Width, Projectile.localAI[0] + texture.Height), targetHitbox.Location.ToVector2(), targetHitbox.Size());
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
        while (index < length) {
            void next() {
                start.Y -= texture.Height;
                index++;
            }
            if (index == length - 1) {
                texture = ModContent.Request<Texture2D>(TipTexture).Value;
            }
            Color color = Lighting.GetColor((int)start.X / 16, (int)start.Y / 16);
            Vector2 origin = new(texture.Width / 2f, texture.Height);
            if (start.Y <= Projectile.ai[2] + texture.Height * 2f) {
                Main.EntitySpriteDraw(texture, start - Main.screenPosition, null, color, Projectile.rotation, origin, Projectile.scale, default);
            }
            next();
        }
        return false;
    }
}
