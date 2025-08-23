using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class CactiGore : ModGore {
    public override bool Update(Gore gore) {
        gore.alpha += 2;
        if (gore.alpha >= 255) {
            gore.alpha = 255;
            gore.active = false;
        }

        return base.Update(gore);
    }
}
