using Humanizer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common;

sealed class MannequinWreathSlotSupport : ILoadable {
    private sealed class MannequinsInWorldSystem : ModSystem {
        public class ExtraMannequinData {
            public Point16 Position;
            public Item Wreath;
            public Item Dye;
        }

        public static List<ExtraMannequinData> MannequinsInWorld { get; private set; } = [];

        public override void Load() {
            On_TEDisplayDoll.Place += On_TEDisplayDoll_Place;
            On_TEDisplayDoll.Kill += On_TEDisplayDoll_Kill;
        }

        public override void PostUpdatePlayers() {
            foreach (ExtraMannequinData data in MannequinsInWorld) {
                Dust.NewDustPerfect(data.Position.ToWorldCoordinates(), DustID.Adamantite);
            }
        }

        private void On_TEDisplayDoll_Kill(On_TEDisplayDoll.orig_Kill orig, int x, int y) {
            orig(x, y);

            Point16 position = new(x, y);
            MannequinsInWorld.Remove(MannequinsInWorld.FirstOrDefault(x => x.Position == position));
        }

        private int On_TEDisplayDoll_Place(On_TEDisplayDoll.orig_Place orig, int x, int y) {
            int result = orig(x, y);

            Point16 position = new(x, y);
            ExtraMannequinData data = new() {
                Position = position,
                Wreath = new Item(),
                Dye = new Item()
            };
            if (!MannequinsInWorld.Contains(data)) {
                MannequinsInWorld.Add(data);
            }

            return result;
        }

        public override void Unload() {
            MannequinsInWorld.Clear();
            MannequinsInWorld = null;
        }
    }


    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
    public extern static ref Item[] TEDisplayDoll__items(TEDisplayDoll self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_dyes")]
    public extern static ref Item[] TEDisplayDoll__dyes(TEDisplayDoll self);

    public void Load(Mod mod) {
        On_TEDisplayDoll.DrawInner += On_TEDisplayDoll_DrawInner;
    }

    private void On_TEDisplayDoll_DrawInner(On_TEDisplayDoll.orig_DrawInner orig, TEDisplayDoll self, Terraria.Player player, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) {
        Main.inventoryScale = 0.72f;
        DrawSlotPairSet(self, player, spriteBatch, 3, 0, 0f, 0.5f, 23);
        DrawSlotPairSet(self, player, spriteBatch, 5, 3, 3f, 0.5f, 24);

        var data = MannequinsInWorldSystem.MannequinsInWorld.FirstOrDefault(x => x.Position == self.Position);
        if (data == null) {
            return;
        }
        int x = (int)(73f + 8 * 56f * Main.inventoryScale);
        int y = (int)((float)Main.instance.invBottom + ((float)0.5f) * 56f * Main.inventoryScale);
        if (data.Wreath != null) {
            Item[] items = [data.Wreath];
            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x, y, (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface) {
                player.mouseInterface = true;
                int context = -10;
                int slot = 0;
                //Main.armorHide = true;
                ItemSlot.OverrideHover(items, Math.Abs(context), slot);

                bool flag = Main.mouseLeftRelease && Main.mouseLeft;
                if (flag) {
                    if (!Main.mouseItem.IsAir && Main.mouseItem.ModItem is BaseWreathItem) {
                        data.Wreath = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                    else if (!data.Wreath.IsEmpty()) {
                        Main.mouseItem = ItemLoader.TransferWithLimit(data.Wreath, 1);
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }

                var inv = items;
                if (inv[slot].type > 0 && inv[slot].stack > 0) {
                    //_customCurrencyForSavings = inv[slot].shopSpecialCurrency;
                    Main.hoverItemName = inv[slot].Name;
                    if (inv[slot].stack > 1)
                        Main.hoverItemName = Main.hoverItemName + " (" + inv[slot].stack + ")";

                    Main.HoverItem = inv[slot].Clone();
                    Main.HoverItem.tooltipContext = context;
                    if (context == 8 && slot <= 2) {
                        Main.HoverItem.wornArmor = true;
                        return;
                    }

                    switch (context) {
                        case 9:
                        case 11:
                            Main.HoverItem.social = true;
                            break;
                        case 15:
                            Main.HoverItem.buy = true;
                            break;
                    }
                }
                else {
                    Main.hoverItemName = Language.GetTextValue("Mods.RoA.WreathSlot");
                }
            }
            ItemSlot.Draw(spriteBatch, items, -10, WreathSlot.GetSlot(player).Type, new Vector2(x, y), default);
        }
        y = (int)((float)Main.instance.invBottom + ((float)1 + 0.5f) * 56f * Main.inventoryScale);
        if (data.Dye != null) {
            Item[] items = [data.Dye];
            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x, y, (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface) {
                player.mouseInterface = true;
                int slot = 0;
                int context = 25;
                bool flag = Main.mouseLeftRelease && Main.mouseLeft;
                if (flag) {
                    if (!Main.mouseItem.IsAir && Main.mouseItem.dye > 0) {
                        data.Dye = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                    else if (!data.Dye.IsEmpty()) {
                        Main.mouseItem = ItemLoader.TransferWithLimit(data.Dye, 1);
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }

                var inv = items;
                if (inv[slot].type > 0 && inv[slot].stack > 0) {
                    //_customCurrencyForSavings = inv[slot].shopSpecialCurrency;
                    Main.hoverItemName = inv[slot].Name;
                    if (inv[slot].stack > 1)
                        Main.hoverItemName = Main.hoverItemName + " (" + inv[slot].stack + ")";

                    Main.HoverItem = inv[slot].Clone();
                    Main.HoverItem.tooltipContext = context;
                    if (context == 8 && slot <= 2) {
                        Main.HoverItem.wornArmor = true;
                        return;
                    }

                    switch (context) {
                        case 9:
                        case 11:
                            Main.HoverItem.social = true;
                            break;
                        case 15:
                            Main.HoverItem.buy = true;
                            break;
                    }
                }
                else {
                    Main.hoverItemName = Lang.inter[57].Value;
                }
            }
            ItemSlot.Draw(spriteBatch, items, 25, 0, new Vector2(x, y), default);
        }
    }

    private static void DrawSlotPairSet(TEDisplayDoll self, Player player, SpriteBatch spriteBatch, int slotsToShowLine, int slotsArrayOffset, float offsetX, float offsetY, int inventoryContextTarget) {
        Item[] _items = TEDisplayDoll__items(self);
        Item[] _dyes = TEDisplayDoll__dyes(self);
        Item[] items = _items;
        int num = inventoryContextTarget;
        for (int i = 0; i < slotsToShowLine; i++) {
            for (int j = 0; j < 2; j++) {
                int num2 = (int)(73f + ((float)i + offsetX) * 56f * Main.inventoryScale);
                int num3 = (int)((float)Main.instance.invBottom + ((float)j + offsetY) * 56f * Main.inventoryScale);
                if (j == 0) {
                    items = _items;
                    num = inventoryContextTarget;
                }
                else {
                    items = _dyes;
                    num = 25;
                }

                if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, num2, num3, (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface) {
                    player.mouseInterface = true;
                    ItemSlot.Handle(items, num, i + slotsArrayOffset);
                }

                ItemSlot.Draw(spriteBatch, items, num, i + slotsArrayOffset, new Vector2(num2, num3));
            }
        }
    }

    public void Unload() { }
}
