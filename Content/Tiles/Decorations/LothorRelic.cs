using RoA.Core;

namespace RoA.Content.Tiles.Decorations;

// If you want to make more relics but do not use the Item.placeStyle approach, you can use inheritance to avoid using duplicate code:
// Your tile code would then inherit from the MinionBossRelic class (which you should make abstract) and should look like this:

sealed class LothorRelic : BossRelic {
    public override string RelicTextureName => ResourceManager.DecorationTileTextures + nameof(LothorRelic);

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
    }
}


// Your item code would then just use the MyBossRelic tile type, and keep placeStyle on 0
// The textures for MyBossRelic item/tile have to be supplied separately
