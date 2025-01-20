using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class FeatherinABottleGore32 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore22 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore12 : FeatherinABottleGore1 { }

sealed class FeatherinABottleGore31 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore21 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore11 : FeatherinABottleGore1 { }

sealed class FeatherinABottleGore3 : FeatherinABottleGore1 { }
sealed class FeatherinABottleGore2 : FeatherinABottleGore1 { }
class FeatherinABottleGore1 : ModGore {
    public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White;

    public override void OnSpawn(Gore gore, IEntitySource source) {
		UpdateType = 11;
	}
}