﻿using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class MercuriumDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new(200, 200, 200, 0);

    public override void OnSpawn(Dust dust) {
        UpdateType = DustID.Clentaminator_Green;
    }

    public override bool Update(Dust dust) {
        //DustHelper.BasicDust(dust, false);

        float num82 = dust.scale * 0.1f;
        if (num82 > 1f)
            num82 = 1f;

        Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num82 * 0.8f, num82, num82 * 0.2f);

        return false;
    }
}
