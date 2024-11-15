using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Projectiles.Enemies;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class ElderwoodWallProjectile : NatureProjectile {
    private const int MAX_TIMELEFT = 270;

    public override string Texture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<VileSpike>()).Texture;
    public static string TipTexture => ProjectileLoader.GetProjectile(ModContent.ProjectileType<VileSpikeTip>()).Texture;

    private int Length => (int)Projectile.ai[0];

    protected override void SafeSetDefaults() {
        int width = 30; int height = 32;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = AIType = -1;

        Projectile.penetrate = -1;
        Projectile.timeLeft = MAX_TIMELEFT;

        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;

        Projectile.hide = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 start = Projectile.Center;
        int index = 0;
        while (index < Length) {
            void next() {
                start.Y -= texture.Height;
                index++;
            }
            if (index == Length - 1) {
                texture = ModContent.Request<Texture2D>(TipTexture).Value;
            }
            Color color = Color.White;
            Vector2 origin = new(texture.Width / 2f, texture.Height);
            Main.EntitySpriteDraw(texture, start - Main.screenPosition, null, color, Projectile.rotation, origin, Projectile.scale, default);
            next();
        }
        return false;
    }
}
