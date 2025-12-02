using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class Topaz2 : Amethyst2 { }

class Sapphire2 : Amethyst2 { }

class Emerald2 : Amethyst2 { }

class Ruby2 : Amethyst2 { }

class Diamond2 : Amethyst2 { }

class Amber2 : Amethyst2 { }

class Amethyst2 : ModGore {
    public override string Texture => base.Texture[..^1];

    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
