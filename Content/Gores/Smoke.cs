using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class Smoke3 : Smoke1 { }
class Smoke2 : Smoke1 { }
class Smoke1 : ModGore, IPostLiquidDraw {
    public override void OnSpawn(Gore gore, IEntitySource source) {
        gore.sticky = false;
    }

    public override bool Update(Gore gore) {
        gore.velocity.Y *= 0.98f;
        gore.velocity.X *= 0.98f;
        gore.scale -= 0.007f;
        if ((double)gore.scale < 0.1) {
            gore.scale = 0.1f;
            gore.alpha = 255;
            gore.active = false;
        }

        gore.position += gore.velocity;

        return false;
    }
}
