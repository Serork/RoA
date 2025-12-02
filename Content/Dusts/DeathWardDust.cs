using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class DeathWardDust : ModDust {
    public override bool Update(Dust dust) {
        dust.scale -= 0.01f;
        float num55 = dust.scale * 1f;
        if (num55 > 0.6f)
            num55 = 0.6f;

        return true;
    }
}