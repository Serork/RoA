using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RoA.Common.UI;

// stormtuna thank you!

// Adapted from https://github.com/absoluteAquarian/SerousCommonLib/blob/f0bf78ce357e740c2dd61e37a669fd4f8ff93283/src/UI/EnhancedItemSlotV2.cs

public delegate bool CanAcceptItemDelegate(ItemSlotUIElement itemSlot, Item item);

public delegate void ItemChangedDelegate(ItemSlotUIElement itemSlot, Item oldItem, Item newItem);

/// <summary>
/// Represents a UI element for an item slot.
/// </summary>
/// <remarks>
/// This element allows for interaction with items, including mouse interactions and sharing items to chat.
/// </remarks>
public class ItemSlotUIElement : UIElement {
    /// <summary>
    /// The item slot context, see <see cref="ItemSlot.Context"/> for more information.
    /// </summary>
    public int Context;

    /// <summary>
    /// The scale of the item slot.
    /// </summary>
    public float Scale;

    /// <summary>
    /// Whether the next mouse interaction should be ignored.
    /// </summary>
    public bool IgnoreNextInteraction;

    /// <summary>
    /// Whether the item can be shared to chat while holding alt.
    /// </summary>
    public bool CanShareItemToChat;

    /// <summary>
    /// The item stored in this slot.
    /// </summary>
    public Item? StoredItem = new();

    /// <summary>
    /// A function to determine if an item is valid for this slot.
    /// </summary>
    public CanAcceptItemDelegate IsItemValid;

    /// <summary>
    /// A function called when the item in the slot changes.
    /// </summary>
    public ItemChangedDelegate OnItemChanged;

    private readonly Item[] _dummyItemArray = new Item[11];

    /// <summary>
    /// Creates a new <see cref="ItemSlotUIElement"/> with the specified context and scale. This will also set the width and height of the element.
    /// </summary>
    /// <param name="context">The context for the item slot, see <see cref="ItemSlot.Context"/> for more information.</param>
    /// <param name="scale">The scale of the item slot.</param>
    public ItemSlotUIElement(int context = ItemSlot.Context.InventoryItem, float scale = 1f) {
        Context = context;
        Scale = scale;

    }

    protected virtual Item? GetStoredItem {
        get => StoredItem;
        set => StoredItem = value;
    }

    protected virtual string HoverText => string.Empty;

    public override void Update(GameTime gameTime) {
        Width.Set(TextureAssets.InventoryBack9.Width() * Scale, 0f);
        Height.Set(TextureAssets.InventoryBack9.Height() * Scale, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        float oldScale = Main.inventoryScale;
        Main.inventoryScale = Scale;

        if (!IgnoreNextInteraction && !PlayerInput.IgnoreMouseInterface && IsMouseHovering && IsItemValid?.Invoke(this, Main.mouseItem) is not false) {
            HandleMouseInteraction();
        }

        IgnoreNextInteraction = false;

        if (GetStoredItem != null) {
            _dummyItemArray[10] = GetStoredItem;
            ItemSlot.Draw(spriteBatch, _dummyItemArray, Context, 10, GetDimensions().Position().Floor());
        }

        Main.inventoryScale = oldScale;
    }

    private void HandleMouseInteraction() {
        Main.LocalPlayer.mouseInterface = true;

        if (GetStoredItem == null) {
            return;
        }
        Item item = GetStoredItem!;
        Item oldItem = item.Clone();
        _dummyItemArray[10] = item;

        ItemSlot.Handle(_dummyItemArray, Context, 10);
        GetStoredItem = _dummyItemArray[10];

        if (StoredItem!.IsNotSameTypePrefixAndStack(oldItem)) {
            OnItemChanged?.Invoke(this, oldItem, GetStoredItem);
        }

        HandleShareInteraction();

        if (HoverText != string.Empty) {
            Main.hoverItemName = HoverText;
        }
    }

    private void HandleShareInteraction() {
        if (GetStoredItem == null) {
            return;
        }

        Item item = GetStoredItem;

        if (CanShareItemToChat && Main.keyState.IsKeyDown(Main.FavoriteKey) && !item.IsAir && Main.drawingPlayerChat) {
            Main.cursorOverride = CursorOverrideID.Magnifiers;

            if (Main.mouseLeft && Main.mouseLeftRelease) {
                if (ChatManager.AddChatText(FontAssets.MouseText.Value, ItemTagHandler.GenerateTag(item), Vector2.One))
                    SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
        //else if (Main.cursorOverride == CursorOverrideID.Magnifiers) {
        //    Main.cursorOverride = CursorOverrideID.FavoriteStar;
        //}
    }
}
