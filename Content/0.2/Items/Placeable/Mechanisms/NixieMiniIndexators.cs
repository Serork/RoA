using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Mechanisms;

sealed class NixieIndexator1Plus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator1Plus>();
    }
}
sealed class NixieIndexator1Minus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator1Minus>();
    }
}
sealed class NixieIndexator3Plus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator3Plus>();
    }
}
sealed class NixieIndexator3Minus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator3Minus>();
    }
}
sealed class NixieIndexator5Plus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator5Plus>();
    }
}
sealed class NixieIndexator5Minus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator5Minus>();
    }
}
sealed class NixieIndexator10Plus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator10Plus>();
    }
}
sealed class NixieIndexator10Minus : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieIndexator10Minus>();
    }
}
sealed class NixieResetter : NixieIndexator {
    public override void SetDefaults() {
        base.SetDefaults();
        Item.createTile = ModContent.TileType<Tiles.Mechanisms.NixieResetter>();
    }
}
