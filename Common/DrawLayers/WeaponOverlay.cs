using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.DrawLayers;

[Autoload(Side = ModSide.Client)]
sealed partial class WeaponOverlay : PlayerDrawLayer {
    private const string REQUIREMENT = "_Outfit";

    public override void Load() => LoadOutfitTextures();

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.shadow == 0f && drawInfo.drawPlayer.active;

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HandOnAcc);

    public static string GetItemNameForTexture(Item item) => item.ModItem.GetType().Name.Replace(" ", string.Empty);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        DrawClawsOnPlayer(drawInfo);
    }

    private void LoadOutfitTextures() {
        if (Main.dedServ) {
            return;
        }

        LoadClawsOutfitTextures();
    }
}
