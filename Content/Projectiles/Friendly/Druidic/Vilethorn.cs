﻿using RoA.Common.Configs;
using RoA.Core;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class VilethornTip : Vilethorn {
    public override string Texture => ModContent.GetInstance<RoAClientConfig>().VanillaResprites ? ResourceManager.FriendlyProjectileTextures + "Druidic/VilethornTip" :
        $"Terraria/Images/Projectile_{ProjectileID.VilethornTip}";
}

class Vilethorn : NatureProjectile {
    public override string Texture => ModContent.GetInstance<RoAClientConfig>().VanillaResprites ? base.Texture : $"Terraria/Images/Projectile_{ProjectileID.VilethornBase}";

    protected override void SafeSetDefaults() {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.alpha = 255;
        Projectile.ignoreWater = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Main.netMode != NetmodeID.Server && Projectile.ai[1] == 0f && Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            var legacySoundStyle = SoundID.Item8;
            SoundEngine.PlaySound(legacySoundStyle, Projectile.Center);
        }

        Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
        if (Projectile.ai[0] == 0f && Projectile.ai[1] < 8f) {
            Projectile.alpha -= 50;

            if (Projectile.alpha > 0)
                return;

            Projectile.alpha = 0;
            Projectile.ai[0] = 1f;
            if (Projectile.ai[1] == 0f) {
                Projectile.ai[1] += 1f;
                Projectile.position += Projectile.velocity * 1f;
            }

            if (Main.myPlayer == Projectile.owner) {
                int num71 = Projectile.type;
                if (Projectile.ai[1] >= 6f) {
                    num71 = ModContent.ProjectileType<VilethornTip>();
                }

                int num72 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + Projectile.velocity.X + (float)(Projectile.width / 2), Projectile.position.Y + Projectile.velocity.Y + (float)(Projectile.height / 2), Projectile.velocity.X, Projectile.velocity.Y, num71, Projectile.damage, Projectile.knockBack, Projectile.owner);
                Main.projectile[num72].damage = Projectile.damage;
                Main.projectile[num72].ai[1] = Projectile.ai[1] + 1f;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, num72);
            }

            return;
        }

        if (Projectile.alpha < 170 && Projectile.alpha + 5 >= 170) {
            for (int num82 = 0; num82 < 3; num82++) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 18, Projectile.velocity.X * 0.025f, Projectile.velocity.Y * 0.025f, 170, default, 1.2f);
            }

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 14, 0f, 0f, 170, default, 1.1f);
        }

        Projectile.alpha += 5;

        if (Projectile.alpha >= 255)
            Projectile.Kill();
    }
}
