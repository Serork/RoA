using RoA.Content.Items.Placeable.Mechanisms;

using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Mechanisms;

sealed class NixieIndexator1Plus : NixieMiniIndexatorBase { }
sealed class NixieIndexator1Minus : NixieMiniIndexatorBase { }
sealed class NixieIndexator3Plus : NixieMiniIndexatorBase { }
sealed class NixieIndexator3Minus : NixieMiniIndexatorBase { }
sealed class NixieIndexator5Plus : NixieMiniIndexatorBase { }
sealed class NixieIndexator5Minus : NixieMiniIndexatorBase { }
sealed class NixieIndexator10Plus : NixieMiniIndexatorBase { }
sealed class NixieIndexator10Minus : NixieMiniIndexatorBase { }
sealed class NixieResetter : NixieMiniIndexatorBase { }
sealed class NixieCategoryChanger : NixieMiniIndexatorBase { }

abstract class NixieMiniIndexatorBase : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
    }

    public override bool CreateDust(int i, int j, ref int type) {
        type = 1;

        return base.CreateDust(i, j, ref type);
    }
}
