using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Mechanisms;

class NixieIndexator1Plus : ModItem {
    public override void SetDefaults() {
        Item.mech = true;
        Item.noWet = true;
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 10;
        Item.height = 12;
        Item.SetShopValues(ItemRarityColor.White0, Item.buyPrice(0, 2));
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator1Plus>();
    }
}
sealed class NixieIndexator1Minus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator1Minus>();
    }
}
sealed class NixieIndexator3Plus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator3Plus>();
    }
}
sealed class NixieIndexator3Minus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator3Minus>();
    }
}
sealed class NixieIndexator5Plus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator5Plus>();
    }
}
sealed class NixieIndexator5Minus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator5Minus>();
    }
}
sealed class NixieIndexator10Plus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator10Plus>();
    }
}
sealed class NixieIndexator10Minus : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator10Minus>();
    }
}
sealed class NixieResetter : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieResetter>();
    }
}
sealed class NixieCategoryChanger : NixieIndexator1Plus {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieCategoryChanger>();
    }
}
