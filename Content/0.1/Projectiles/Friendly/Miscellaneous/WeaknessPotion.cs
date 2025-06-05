using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class WeaknessPotion : ModProjectile {
    //public override void SetStaticDefaults()		
    //	=> DisplayName.SetDefault("Weakness Potion");

    public override void SetDefaults() {
        int width = 22; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;
        Projectile.aiStyle = 2;
        Projectile.penetrate = 1;
    }

    public override void AI() {
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] >= 45f) {
            Projectile.velocity.Y = Projectile.velocity.Y + 0.3f;
            Projectile.velocity.X = Projectile.velocity.X * 0.98f;
        }
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            float _distance = 80f;
            for (int _findPlayer = 0; _findPlayer < byte.MaxValue; _findPlayer++) {
                Player player = Main.player[_findPlayer];
                if (player.active && !player.dead && Vector2.Distance(Projectile.Center, player.Center) < _distance)
                    player.AddBuff(BuffID.Weak, 1800, false);
            }
            for (int _findNPC = 0; _findNPC < Main.maxNPCs; _findNPC++) {
                NPC npc = Main.npc[_findNPC];
                if (npc.active && npc.life > 0 && Vector2.Distance(Projectile.Center, npc.Center) < _distance)
                    npc.AddBuff(ModContent.BuffType<Weak>(), 1800);
            }
        }

        SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
        for (int i = 0; i < 5; i++)
            Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 13, 0f, 0f, 0, new Color(100, 148, 32), 1f);

        for (int n = 0; n < 30; n++) {
            Vector2 value30 = new Vector2(Main.rand.Next(-10, 11), Main.rand.Next(-10, 11));
            value30.Normalize();
            value30 *= 0.4f;

            if (!Main.dedServ) {
                int _gore = Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center + value30 * 10f, value30 * Main.rand.Next(4, 9) * 0.66f + Vector2.UnitY * 1.5f, Main.rand.Next(435, 438), Main.rand.Next(20, 100) * 0.01f);
                Main.gore[_gore].sticky = false;
                Main.gore[_gore].GetAlpha(new Color(100, 148, 36, 100));
            }
        }
    }
}
