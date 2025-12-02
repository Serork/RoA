using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies;

sealed class GoblinsDagger : ModProjectile {
    public int NPC
        => (int)Projectile.ai[0];

    public float Rotation {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public bool Changed {
        get => Projectile.localAI[1] == 1f;
        set => Projectile.localAI[1] = value ? 1f : 0f;
    }

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Goblin's Dagger");

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 20;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        bool flag = Projectile.spriteDirection != -1;
        hitbox = new Rectangle((int)Projectile.position.X - (flag ? 0 : Projectile.width), (int)Projectile.position.Y - 20 - (flag ? 0 : -7) + 5, Projectile.width, Projectile.height);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.Bleeding, 75);
        if (info.Damage <= 0) {
            return;
        }
        if (Main.netMode != NetmodeID.Server) {
            SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, Projectile.Center);
        }
        NPC npc = Main.npc[NPC];
        if (npc.active && Main.rand.NextBool()) {
            npc.ai[0] = 1f;
            npc.netUpdate = true;
        }
    }

    public override void AI() {
        NPC npc = Main.npc[NPC];
        if (!npc.active || npc.ai[1] != 3f) {
            Projectile.Kill();
        }
        bool flag = Projectile.spriteDirection != -1;
        Projectile.position = npc.Center + npc.velocity + Projectile.velocity.RotatedBy(Projectile.rotation) * 25f + new Vector2(flag ? 4f : -4f, (flag ? 10f : 0f) + 4f);
        float count = 0.5f;
        Rotation += count / 2f * (Changed ? -1f : 1f);
        if (Rotation >= count) {
            Changed = true;
        }
        else if (Rotation < -count) {
            Changed = false;
        }
        float max = count * 10f;
        Projectile.spriteDirection = npc.spriteDirection;
        if (npc.HasValidTarget) {
            Projectile.rotation = (Utils.GetLerpValue(-max, max, Rotation * 3f, clamped: true) + 1.1f) * -Projectile.spriteDirection;
        }
        Projectile.timeLeft = 2;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        bool flag = Projectile.spriteDirection != -1;
        Color color = Lighting.GetColor(new Point((int)Projectile.position.X / 16, (int)Projectile.position.Y / 16));
        Main.spriteBatch.Draw(texture, Projectile.position - Main.screenPosition, new Rectangle?(new Rectangle(flag ? 0 : 10, 0, 10, 24)), color * Projectile.Opacity, Projectile.rotation, Vector2.Zero, Projectile.scale, flag ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
        return false;
    }
}
