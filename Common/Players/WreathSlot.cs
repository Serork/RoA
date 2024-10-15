using Microsoft.Xna.Framework;

using RoA.Common.InterfaceElements;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class WreathSlot : ModAccessorySlot {
    private static bool _equipped, _equipped2;

    public static WreathSlot GetSlot(Player player) => LoaderManager.Get<AccessorySlotLoader>().Get(ModContent.GetInstance<WreathSlot>().Type, player) as WreathSlot;
    public static Item GetFunctionalItem(Player player) => GetSlot(player).FunctionalItem;
    public static Item GetVanityItem(Player player) => GetSlot(player).VanityItem;
    public static Item GetDyeItem(Player player) => GetSlot(player).DyeItem;
    public static bool GetHideVisuals(Player player) => GetSlot(player).HideVisuals;

    public static Vector2 GetCustomLocation() {
        int mapH = (Main.mapEnabled && !Main.mapFullscreen && Main.mapStyle == 1) ? 256 : 0;
        mapH = (Main.mapEnabled && (mapH + 600) > Main.screenHeight) ? Main.screenHeight - 600 : mapH;
        int offsetX = 47 * 2;
        int x = (Main.netMode == NetmodeID.MultiplayerClient) ? Main.screenWidth - 135 - offsetX - 47 : Main.screenWidth - 94 - offsetX + 2 - 64;
        int y = mapH + 174;
        return new Vector2(x, y);
    }

    public override Vector2? CustomLocation => GetCustomLocation();

    public override bool IsHidden() => (!IsItemValidForSlot(Main.mouseItem) && !BeltButton.IsUsed) || Main.EquipPage == 2;

    public override string FunctionalTexture => ResourceManager.GUITextures + "Wreath_SlotBackground";

    public override bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) {
        if (isHovered) {
            _equipped = !FunctionalItem.IsEmpty() || !VanityItem.IsEmpty() || !DyeItem.IsEmpty();
            if (_equipped && !_equipped2) {
                BeltButton.ToggleTo(true);
            }
        }

        return base.PreDraw(context, item, position, isHovered);
    }

    public override void PostDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) {
        if (isHovered) {
            _equipped2 = _equipped;
            _equipped = false;
        }
    }

    public override void OnMouseHover(AccessorySlotType context) {
        string slotName = "Wreath";
        string vanitySlotName = "Social Wreath";
        switch (context) {
            case AccessorySlotType.FunctionalSlot:
                Main.hoverItemName = slotName;
                break;
            case AccessorySlotType.VanitySlot:
                Main.hoverItemName = vanitySlotName;
                break;
        }
    }

    public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) => IsItemValidForSlot(checkItem);

    public static bool IsItemValidForSlot(Item item) => item.ModItem is BaseWreathItem;

    public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
        bool result = IsItemValidForSlot(item);
        if (result) {
            BeltButton.ToggleTo(true);
        }

        return result;
    }
}
