﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Electric : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White;

    public override void OnSpawn(Dust dust) {
        UpdateType = DustID.Electric;
        dust.noLight = true;
    }

    public override bool PreDraw(Dust dust) {
        if (!Main.dedServ) {
            Lighting.AddLight(dust.position, new Color(86, 173, 177).ToVector3());
        }

        return base.PreDraw(dust);
    }
}