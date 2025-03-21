using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class SlipperyGlowstick : ModProjectile {
    private Vector2 memorizeVelocity;
    private int effectCounter;
    private int effectCounterMax = 1;

    public override void SetDefaults() {
        int width = 6; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;
        Projectile.netImportant = true;

        Projectile.aiStyle = 14;
        Projectile.penetrate = -1;

        Projectile.alpha = 75;
        Projectile.timeLeft *= 5;

        DrawOffsetX = -3;
        DrawOriginOffsetY = -3;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.95f, 0.75f, 0.4f);
        if (!Projectile.tileCollide) {
            memorizeVelocity *= 0.955f;
            Projectile.velocity = memorizeVelocity;
            if (Projectile.ai[2] <= 0f) {
                if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
                    memorizeVelocity = Vector2.Zero;
                    Projectile.tileCollide = true;
                }
            }
            else {
                Projectile.ai[2]--;
            }
            effectCounter++;
            if (effectCounter == effectCounterMax && effectCounterMax < 20) {
                effectCounterMax += 3;
                effectCounter = 0;
                SoundEngine.PlaySound(SoundID.WormDig, Projectile.position);
            }
            if (effectCounter % 4 == 0 && effectCounterMax < 20) {
                int dustDig = Dust.NewDust(Projectile.Center - Vector2.One * 10, 20, 20, ModContent.DustType<Galipot2>(), 0f, 0f, 0, default(Color), 1f);
                Main.dust[dustDig].velocity *= 0.1f;
                Main.dust[dustDig].noGravity = true;
            }
        }
        return;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (memorizeVelocity == Vector2.Zero)
            memorizeVelocity = oldVelocity * 0.75f;
        Projectile.tileCollide = false;
        Projectile.ai[2] = 1f;
        return false;
    }

    public override bool? CanCutTiles()
        => false;

    public override Color? GetAlpha(Color lightColor)
        => new Color?(new Color(255, 225, 135, 120));

    public override void OnKill(int timeLeft) {
        if (Main.rand.Next(3) == 0)
            Item.NewItem(Projectile.GetSource_Death(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, ModContent.ItemType<Items.Consumables.SlipperyGlowstick>(), 1, false, 0, false, false);
    }
}
