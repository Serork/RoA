using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Nature.MagicalBifrostBlock;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public MagicalBifrostBlockInfo[]? ActiveMagicalBlockData { get; private set; }

    public BifrostFigureType ActiveFigureType { get; private set; }
    public BifrostFigureType NextFigureType { get; private set; }

    public partial void RodOfTheBifrostItemCheck(Player player) {
        if (player.IsHolding<RodOfTheBifrost>() && ActiveMagicalBlockData is null) {
            NextFigureType = Main.rand.GetRandomEnumValue<BifrostFigureType>(1);
            ChooseNextBifrostPattern();
        }
    }

    public void ChooseNextBifrostPattern() {
        ActiveFigureType = NextFigureType;
        NextFigureType = Main.rand.GetRandomEnumValue<BifrostFigureType>(1);
        ActiveMagicalBlockData = GetBlockInfoForBlockType(NextFigureType);
    }
}
