using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;
sealed class BaseFormHandler : ModPlayer {
    private PlayerDrawLayer[] _layers;

    public bool IsInDruidicForm { get; internal set; }

    public override void ResetEffects() {
        IsInDruidicForm = false;
    }

    public override void PostUpdateEquips() {
        if (!IsInDruidicForm) {
            return;
        }

        Player.controlUseItem = false;
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
        if (!IsInDruidicForm) {
            return;
        }

        _layers = PlayerDrawLayerLoader.GetDrawLayers(drawInfo);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo) {
        if (!IsInDruidicForm || _layers == null) {
            return;
        }

        foreach (PlayerDrawLayer layer in _layers) {
            if (layer.FullName.Contains("Terraria") && !layer.FullName.Contains("Mount")) {
                layer.Hide();
            }
        }
    }
}
