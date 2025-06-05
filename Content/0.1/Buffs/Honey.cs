using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Honey : ModBuff {
    public override string Texture => $"Terraria/Images/Buff_{BuffID.Honey}";

    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}
