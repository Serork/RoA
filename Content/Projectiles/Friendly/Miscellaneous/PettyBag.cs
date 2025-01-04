using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Content.Items;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class PettyBag : InteractableProjectile {
    private sealed class PettyBagHandler : ModPlayer {
        public HashSet<Item> BagItems { get; private set; } = [];

        public override void Unload() {
            BagItems.Clear();
            BagItems = null;
        }

        public void AddItem(Item item) => BagItems.Add(item);
    }

    protected override Vector2 DrawOffset => Vector2.UnitY * 4f;

    protected override SpriteEffects SetSpriteEffects() => base.SetSpriteEffects();

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 6;
    }

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;

        Projectile.aiStyle = -1;

        Projectile.tileCollide = true;
        Projectile.timeLeft = 10800;
        Projectile.hide = false;
    }

    protected override void OnHover(Player player) {
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Miscellaneous.PettyBag>();
    }

    protected override void OnInteraction(Player player) {

    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override void SafeAI() {
        KillSame();

        Projectile.velocity.X *= 0.925f;
        if ((double)Projectile.velocity.X < 0.1 && (double)Projectile.velocity.X > -0.1)
            Projectile.velocity.X = 0f;
        
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            if (Projectile.velocity.X < 0f)
                Projectile.direction = -1;
            else
                Projectile.direction = 1;

            Projectile.spriteDirection = Projectile.direction;
        }

        float gravity = 0.2f;
        float maxFallSpeed = 7f;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            int num = (int)(Projectile.position.X + (float)(Projectile.width / 2)) / 16;
            int num2 = (int)(Projectile.position.Y + (float)(Projectile.height / 2)) / 16;
            if (num >= 0 && num2 >= 0 && num < Main.maxTilesX && num2 < Main.maxTilesY && Main.tile[num, num2] == null) {
                gravity = 0f;
                Projectile.velocity.X = 0f;
                Projectile.velocity.Y = 0f;
            }
        }

        Vector2 wetVelocity = Projectile.velocity * 0.5f;
        if (Projectile.shimmerWet) {
            gravity = 0.065f;
            maxFallSpeed = 4f;
            wetVelocity = Projectile.velocity * 0.375f;
        }
        else if (Projectile.honeyWet) {
            gravity = 0.05f;
            maxFallSpeed = 3f;
            wetVelocity = Projectile.velocity * 0.25f;
        }
        else if (Projectile.wet) {
            gravity = 0.08f;
            maxFallSpeed = 5f;
        }

        Projectile.velocity.Y += gravity;
        if (Projectile.velocity.Y > maxFallSpeed)
            Projectile.velocity.Y = maxFallSpeed;

        if (Projectile.wet) {
            Vector2 vector = Projectile.velocity;
            Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            if (Projectile.velocity.X != vector.X)
                wetVelocity.X = Projectile.velocity.X;

            if (Projectile.velocity.Y != vector.Y)
                wetVelocity.Y = Projectile.velocity.Y;
        }
    }

    private void KillSame() {
        if (Projectile.owner == Main.myPlayer) {
            for (int num825 = 0; num825 < 1000; num825++) {
                if (num825 != Projectile.whoAmI && Main.projectile[num825].active && Main.projectile[num825].owner == Projectile.owner && Main.projectile[num825].type == Projectile.type) {
                    if (Projectile.timeLeft >= Main.projectile[num825].timeLeft)
                        Main.projectile[num825].Kill();
                    else
                        Projectile.Kill();
                }
            }
        }
    }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 10) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 6) {
            Projectile.frame = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        int frameHeight = projectileTexture.Height / Main.projFrames[Type];
        Rectangle frameRect = new(0, Projectile.frame * frameHeight, projectileTexture.Width, frameHeight);
        Vector2 drawOrigin = new(projectileTexture.Width / 2f, projectileTexture.Height / Main.projFrames[Projectile.type] * 0.5f);
        Vector2 drawPos = Projectile.position + DrawOffset + drawOrigin - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor);
        spriteBatch.Draw(projectileTexture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, 1f, SetSpriteEffects(), 0f);

        return false;
    }
}
