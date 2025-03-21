using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class LothorGore7_Enraged : LothorGore1_Enraged { }
class LothorGore6_Enraged : LothorGore1_Enraged { }
class LothorGore4_Enraged : LothorGore1_Enraged { }
class LothorGore3_Enraged : LothorGore1_Enraged { }
class LothorGore2_Enraged : LothorGore1_Enraged { }
class LothorGore1_Enraged : ModGore {
    public override Color? GetAlpha(Gore gore, Color lightColor) {
        void enrage(ref Color color) {
            color = Color.Lerp(Helper.BuffColor(color, 0.3f, 0.3f, 0.3f, 1f), color, 0.6f);
        }
        enrage(ref lightColor);

        return lightColor;
    }
}