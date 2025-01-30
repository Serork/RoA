using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Players;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core.Utility;
using RoA.Utilities;

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
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;

namespace RoA.Common;

sealed class MannequinWreathSlotSupport : ILoadable {
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowTime")]
    public extern static ref int[] ItemSlot_inventoryGlowTime(ItemSlot self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowHue")]
    public extern static ref float[] ItemSlot_inventoryGlowHue(ItemSlot self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowTimeChest")]
    public extern static ref int[] ItemSlot_inventoryGlowTimeChest(ItemSlot self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowHueChest")]
    public extern static ref float[] ItemSlot_inventoryGlowHueChest(ItemSlot self);

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
            On_TEDisplayDoll.TryFitting += On_TEDisplayDoll_TryFitting;
        }

        private bool On_TEDisplayDoll_TryFitting(On_TEDisplayDoll.orig_TryFitting orig, TEDisplayDoll self, Item[] inv, int context, int slot, bool justCheck) {
            Item item = inv[slot];
            if (item.ModItem != null && item.ModItem is BaseWreathItem) {
                var data = MannequinsInWorld.FirstOrDefault(x => x.Position == self.Position);
                if (data == null) {
                    return false;
                }
                SoundEngine.PlaySound(SoundID.Grab);
                Utils.Swap(ref data.Wreath, ref inv[slot]);
                //if (Main.netMode == 1)
                //    NetMessage.SendData(121, -1, -1, null, Main.myPlayer, ID, num);

                return true;
            }

            return orig(self, inv, context, slot, justCheck);   
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
        int x = (int)(73f + 0 * 56f * Main.inventoryScale);
        int y = (int)((float)Main.instance.invBottom + ((float)2 + 0.5f) * 56f * Main.inventoryScale);
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
                    if (!Main.mouseItem.IsAir && Main.mouseItem.ModItem is BaseWreathItem && Main.mouseItem.stack == 1) {
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
            Draw(spriteBatch, items, -10, WreathSlot.GetSlot(player).Type, new Vector2(x, y), default, TextureAssets.InventoryBack8.Value);
            //ItemSlot.Draw(spriteBatch, items, -10, WreathSlot.GetSlot(player).Type, new Vector2(x, y), default);
        }
        y = (int)((float)Main.instance.invBottom + ((float)3 + 0.5f) * 56f * Main.inventoryScale);
        if (data.Dye != null) {
            Item[] items = [data.Dye];
            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x, y, (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface) {
                player.mouseInterface = true;
                int slot = 0;
                int context = 25;
                bool flag = Main.mouseLeftRelease && Main.mouseLeft;
                if (flag) {
                    if (!data.Dye.IsEmpty() && !Main.mouseItem.IsAir && data.Dye.stack == Main.mouseItem.stack) {
                        if (data.Dye.type != Main.mouseItem.type) {
                            (data.Dye, Main.mouseItem) = (Main.mouseItem, data.Dye);
                            SoundEngine.PlaySound(SoundID.Grab);
                        }
                    }
                    else {
                        if (!Main.mouseItem.IsAir && Main.mouseItem.dye > 0) {
                            if (Main.mouseItem.stack == 1) {
                                data.Dye = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                                SoundEngine.PlaySound(SoundID.Grab);
                            }
                            else if (data.Dye.IsEmpty()) {
                                data.Dye.type = Main.mouseItem.type;
                                data.Dye.stack = 1;
                                Main.mouseItem.stack -= 1;
                                SoundEngine.PlaySound(SoundID.Grab);
                            }
                        }
                        else if (!data.Dye.IsEmpty()) {
                            Main.mouseItem = ItemLoader.TransferWithLimit(data.Dye, 1);
                            SoundEngine.PlaySound(SoundID.Grab);
                        }
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

    internal static void DrawSlotTexture(Texture2D value6, Vector2 position, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, int slot, int context) {
        var thisSlot = LoaderManager.Get<AccessorySlotLoader>().Get(slot);
        Texture2D texture = null;
        switch (context) {
            case -10:
                if (ModContent.RequestIfExists<Texture2D>(thisSlot.FunctionalTexture, out var funcTexture))
                    texture = funcTexture.Value;
                break;
            case -11:
                if (ModContent.RequestIfExists<Texture2D>(thisSlot.VanityTexture, out var vanityTexture))
                    texture = vanityTexture.Value;
                break;
            case -12:
                if (ModContent.RequestIfExists<Texture2D>(thisSlot.DyeTexture, out var dyeTexture))
                    texture = dyeTexture.Value;
                break;
        }

        if (texture == null) {
            texture = value6;
        }
        else {
            rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            origin = rectangle.Size() / 2;
        }

        Main.spriteBatch.Draw(texture, position, rectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    internal static Texture2D GetBackgroundTexture(int slot, int context) {
        var thisSlot = LoaderManager.Get<AccessorySlotLoader>().Get(slot);
        switch (context) {
            case -10:
                if (ModContent.RequestIfExists<Texture2D>(thisSlot.FunctionalBackgroundTexture, out var funcTexture))
                    return funcTexture.Value;
                return TextureAssets.InventoryBack3.Value;
            case -11:
                if (ModContent.RequestIfExists<Texture2D>(thisSlot.VanityBackgroundTexture, out var vanityTexture))
                    return vanityTexture.Value;
                return TextureAssets.InventoryBack8.Value;
            case -12:
                if (ModContent.RequestIfExists<Texture2D>(thisSlot.DyeBackgroundTexture, out var dyeTexture))
                    return dyeTexture.Value;
                return TextureAssets.InventoryBack12.Value;
        }

        // Default to a functional slot
        return TextureAssets.InventoryBack3.Value;
    }

    public static void Draw(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor = default(Color), Texture2D inventoryBack = null) {
        Player player = Main.player[Main.myPlayer];
        Item item = inv[slot];
        float inventoryScale = Main.inventoryScale;
        Color color = Color.White;
        if (lightColor != Color.Transparent)
            color = lightColor;

        bool flag = false;
        int num = 0;
        int gamepadPointForSlot = Helper.GetGamepadPointForSlot(inv, context, slot);
        if (PlayerInput.UsingGamepadUI) {
            flag = UILinkPointNavigator.CurrentPoint == gamepadPointForSlot;
            if (PlayerInput.SettingsForUI.PreventHighlightsForGamepad)
                flag = false;

            if (context == 0) {
                num = player.DpadRadial.GetDrawMode(slot);
                if (num > 0 && !PlayerInput.CurrentProfile.UsingDpadHotbar())
                    num = 0;
            }
        }

        Texture2D value = TextureAssets.InventoryBack.Value;
        Color color2 = Main.inventoryBack;
        bool flag2 = false;
        bool highlightThingsForMouse = PlayerInput.SettingsForUI.HighlightThingsForMouse;
        if (item.type > 0 && item.stack > 0 && item.favorited && context != 13 && context != 21 && context != 22 && context != 14) {
            value = TextureAssets.InventoryBack10.Value;
            if (context == 32)
                value = TextureAssets.InventoryBack19.Value;
        }
        else if (item.type > 0 && item.stack > 0 && ItemSlot.Options.HighlightNewItems && item.newAndShiny && context != 13 && context != 21 && context != 14 && context != 22) {
            value = TextureAssets.InventoryBack15.Value;
            float num2 = (float)(int)Main.mouseTextColor / 255f;
            num2 = num2 * 0.2f + 0.8f;
            color2 = color2.MultiplyRGBA(new Color(num2, num2, num2));
        }
        else if (!highlightThingsForMouse && item.type > 0 && item.stack > 0 && num != 0 && context != 13 && context != 21 && context != 22) {
            value = TextureAssets.InventoryBack15.Value;
            float num3 = (float)(int)Main.mouseTextColor / 255f;
            num3 = num3 * 0.2f + 0.8f;
            color2 = ((num != 1) ? color2.MultiplyRGBA(new Color(num3 / 2f, num3, num3 / 2f)) : color2.MultiplyRGBA(new Color(num3, num3 / 2f, num3 / 2f)));
        }
        else if (context == 0 && slot < 10) {
            value = TextureAssets.InventoryBack9.Value;
        }
        else {
            switch (context) {
                case 28:
                    value = TextureAssets.InventoryBack7.Value;
                    color2 = Color.White;
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    value = TextureAssets.InventoryBack3.Value;
                    break;
                case 8:
                case 10:
                    value = TextureAssets.InventoryBack13.Value;
                    color2 = ItemSlot.GetColorByLoadout(slot, context);
                    break;
                case 23:
                case 24:
                case 26:
                    value = TextureAssets.InventoryBack8.Value;
                    break;
                case 9:
                case 11:
                    value = TextureAssets.InventoryBack13.Value;
                    color2 = ItemSlot.GetColorByLoadout(slot, context);
                    break;
                case 25:
                case 27:
                case 33:
                    value = TextureAssets.InventoryBack12.Value;
                    break;
                case 12:
                    value = TextureAssets.InventoryBack13.Value;
                    color2 = ItemSlot.GetColorByLoadout(slot, context);
                    break;
                case ItemSlot.Context.ModdedAccessorySlot:
                case ItemSlot.Context.ModdedVanityAccessorySlot:
                case ItemSlot.Context.ModdedDyeSlot:
                    value = GetBackgroundTexture(slot, context);
                    break;
                case 3:
                    value = TextureAssets.InventoryBack5.Value;
                    break;
                case 4:
                case 32:
                    value = TextureAssets.InventoryBack2.Value;
                    break;
                case 5:
                case 7:
                    value = TextureAssets.InventoryBack4.Value;
                    break;
                case 6:
                    value = TextureAssets.InventoryBack7.Value;
                    break;
                case 13: {
                    byte b = 200;
                    if (slot == Main.player[Main.myPlayer].selectedItem) {
                        value = TextureAssets.InventoryBack14.Value;
                        b = byte.MaxValue;
                    }

                    color2 = new Color(b, b, b, b);
                    break;
                }
                case 14:
                case 21:
                    flag2 = true;
                    break;
                case 15:
                    value = TextureAssets.InventoryBack6.Value;
                    break;
                case 29:
                    color2 = new Color(53, 69, 127, 255);
                    value = TextureAssets.InventoryBack18.Value;
                    break;
                case 30:
                    flag2 = !flag;
                    break;
                case 22:
                    value = TextureAssets.InventoryBack4.Value;
                    if (ItemSlot.DrawGoldBGForCraftingMaterial) {
                        ItemSlot.DrawGoldBGForCraftingMaterial = false;
                        value = TextureAssets.InventoryBack14.Value;
                        float num4 = (float)(int)color2.A / 255f;
                        num4 = ((!(num4 < 0.7f)) ? 1f : Utils.GetLerpValue(0f, 0.7f, num4, clamped: true));
                        color2 = Color.White * num4;
                    }
                    break;
            }
        }

        if (inventoryBack != null) {
            value = inventoryBack;
        }

        if ((context == 0 || context == 2) && ItemSlot_inventoryGlowTime(null)[slot] > 0 && !inv[slot].favorited && !inv[slot].IsAir) {
            float num5 = Main.invAlpha / 255f;
            Color value2 = new Color(63, 65, 151, 255) * num5;
            Color value3 = Main.hslToRgb(ItemSlot_inventoryGlowHue(null)[slot], 1f, 0.5f) * num5;
            float num6 = (float)ItemSlot_inventoryGlowTime(null)[slot] / 300f;
            num6 *= num6;
            color2 = Color.Lerp(value2, value3, num6 / 2f);
            value = TextureAssets.InventoryBack13.Value;
        }

        if ((context == 4 || context == 32 || context == 3) && ItemSlot_inventoryGlowTimeChest(null)[slot] > 0 && !inv[slot].favorited && !inv[slot].IsAir) {
            float num7 = Main.invAlpha / 255f;
            Color value4 = new Color(130, 62, 102, 255) * num7;
            if (context == 3)
                value4 = new Color(104, 52, 52, 255) * num7;

            Color value5 = Main.hslToRgb(ItemSlot_inventoryGlowHueChest(null)[slot], 1f, 0.5f) * num7;
            float num8 = (float)ItemSlot_inventoryGlowTimeChest(null)[slot] / 300f;
            num8 *= num8;
            color2 = Color.Lerp(value4, value5, num8 / 2f);
            value = TextureAssets.InventoryBack13.Value;
        }

        if (flag) {
            value = TextureAssets.InventoryBack14.Value;
            color2 = Color.White;
            if (item.favorited)
                value = TextureAssets.InventoryBack17.Value;
        }

        if (context == 28 && Main.MouseScreen.Between(position, position + value.Size() * inventoryScale) && !player.mouseInterface) {
            value = TextureAssets.InventoryBack14.Value;
            color2 = Color.White;
        }

        if (!flag2)
            spriteBatch.Draw(value, position, null, color2, 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);

        int num9 = -1;
        switch (context) {
            case 8:
            case 23:
                if (slot == 0)
                    num9 = 0;
                if (slot == 1)
                    num9 = 6;
                if (slot == 2)
                    num9 = 12;
                break;
            case 26:
                num9 = 0;
                break;
            case 9:
                if (slot == 10)
                    num9 = 3;
                if (slot == 11)
                    num9 = 9;
                if (slot == 12)
                    num9 = 15;
                break;
            case 10:
            case 24:
                num9 = 11;
                break;
            case 11:
                num9 = 2;
                break;
            case 12:
            case 25:
            case 27:
            case 33:
                num9 = 1;
                break;
            case ItemSlot.Context.ModdedAccessorySlot:
                // 'num9' is the vertical frame of some texture?
                num9 = 11;
                break;
            case ItemSlot.Context.ModdedVanityAccessorySlot:
                num9 = 2;
                break;
            case ItemSlot.Context.ModdedDyeSlot:
                num9 = 1;
                break;
            case 16:
                num9 = 4;
                break;
            case 17:
                num9 = 13;
                break;
            case 19:
                num9 = 10;
                break;
            case 18:
                num9 = 7;
                break;
            case 20:
                num9 = 17;
                break;
        }

        if ((item.type <= 0 || item.stack <= 0) && num9 != -1) {
            Texture2D value6 = TextureAssets.Extra[54].Value;
            Rectangle rectangle = value6.Frame(3, 6, num9 % 3, num9 / 3);
            rectangle.Width -= 2;
            rectangle.Height -= 2;

            if (context is ItemSlot.Context.ModdedAccessorySlot or ItemSlot.Context.ModdedVanityAccessorySlot or ItemSlot.Context.ModdedDyeSlot) {
                DrawSlotTexture(value6, position + value.Size() / 2f * inventoryScale, rectangle, Color.White * 0.35f, 0f, rectangle.Size() / 2f, inventoryScale, SpriteEffects.None, 0f, slot, context);
                goto SkipVanillaDraw;
            }

            spriteBatch.Draw(value6, position + value.Size() / 2f * inventoryScale, rectangle, Color.White * 0.35f, 0f, rectangle.Size() / 2f, inventoryScale, SpriteEffects.None, 0f);
        SkipVanillaDraw:;
        }

        Vector2 vector = value.Size() * inventoryScale;
        if (item.type > 0 && item.stack > 0) {
            float scale = ItemSlot.DrawItemIcon(item, context, spriteBatch, position + vector / 2f, inventoryScale, 32f, color);
            if (ItemID.Sets.TrapSigned[item.type])
                spriteBatch.Draw(TextureAssets.Wire.Value, position + new Vector2(40f, 40f) * inventoryScale, new Rectangle(4, 58, 8, 8), color, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);

            if (ItemID.Sets.DrawUnsafeIndicator[item.type]) {
                Vector2 vector2 = new Vector2(-4f, -4f) * inventoryScale;
                Texture2D value7 = TextureAssets.Extra[258].Value;
                Rectangle rectangle2 = value7.Frame();
                spriteBatch.Draw(value7, position + vector2 + new Vector2(40f, 40f) * inventoryScale, rectangle2, color, 0f, rectangle2.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }

            if (item.type == 5324 || item.type == 5329 || item.type == 5330) {
                Vector2 vector3 = new Vector2(2f, -6f) * inventoryScale;
                switch (item.type) {
                    case 5324: {
                        Texture2D value10 = TextureAssets.Extra[257].Value;
                        Rectangle rectangle5 = value10.Frame(3, 1, 2);
                        spriteBatch.Draw(value10, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle5, color, 0f, rectangle5.Size() / 2f, 1f, SpriteEffects.None, 0f);
                        break;
                    }
                    case 5329: {
                        Texture2D value9 = TextureAssets.Extra[257].Value;
                        Rectangle rectangle4 = value9.Frame(3, 1, 1);
                        spriteBatch.Draw(value9, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle4, color, 0f, rectangle4.Size() / 2f, 1f, SpriteEffects.None, 0f);
                        break;
                    }
                    case 5330: {
                        Texture2D value8 = TextureAssets.Extra[257].Value;
                        Rectangle rectangle3 = value8.Frame(3);
                        spriteBatch.Draw(value8, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle3, color, 0f, rectangle3.Size() / 2f, 1f, SpriteEffects.None, 0f);
                        break;
                    }
                }
            }

            if (item.stack > 1)
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), position + new Vector2(10f, 26f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);

            int num10 = -1;
            if (context == 13) {
                if (item.DD2Summon) {
                    for (int i = 0; i < 58; i++) {
                        if (inv[i].type == 3822)
                            num10 += inv[i].stack;
                    }

                    if (num10 >= 0)
                        num10++;
                }

                if (item.useAmmo > 0) {
                    int useAmmo = item.useAmmo;
                    num10 = 0;
                    for (int j = 0; j < 58; j++) {
                        /*
						if (inv[j].ammo == useAmmo)
						*/
                        if (inv[j].stack > 0 && ItemLoader.CanChooseAmmo(item, inv[j], player))
                            num10 += inv[j].stack;
                    }
                }

                if (item.fishingPole > 0) {
                    num10 = 0;
                    for (int k = 0; k < 58; k++) {
                        if (inv[k].bait > 0)
                            num10 += inv[k].stack;
                    }
                }

                if (item.tileWand > 0) {
                    int tileWand = item.tileWand;
                    num10 = 0;
                    for (int l = 0; l < 58; l++) {
                        if (inv[l].type == tileWand)
                            num10 += inv[l].stack;
                    }
                }

                if (item.type == 509 || item.type == 851 || item.type == 850 || item.type == 3612 || item.type == 3625 || item.type == 3611) {
                    num10 = 0;
                    for (int m = 0; m < 58; m++) {
                        if (inv[m].type == 530)
                            num10 += inv[m].stack;
                    }
                }
            }

            if (num10 != -1)
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, num10.ToString(), position + new Vector2(8f, 30f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale * 0.8f), -1f, inventoryScale);

            if (context == 13) {
                string text = string.Concat(slot + 1);
                if (text == "10")
                    text = "0";

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, position + new Vector2(8f, 4f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);
            }

            if (context == 13 && item.potion) {
                Vector2 position2 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                Color color3 = item.GetAlpha(color) * ((float)player.potionDelay / (float)player.potionDelayTime);
                spriteBatch.Draw(TextureAssets.Cd.Value, position2, null, color3, 0f, default(Vector2), scale, SpriteEffects.None, 0f);

                // Extra context.
            }

            // TML: Added handling of the new 'masterOnly' item field.
            if ((Math.Abs(context) == 10 || context == 18) && ((item.expertOnly && !Main.expertMode) || (item.masterOnly && !Main.masterMode))) {
                Vector2 position3 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                Color white = Color.White;
                spriteBatch.Draw(TextureAssets.Cd.Value, position3, null, white, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
            }

            // Extra context.
        }
        else if (context == 6) {
            Texture2D value11 = TextureAssets.Trash.Value;
            Vector2 position4 = position + value.Size() * inventoryScale / 2f - value11.Size() * inventoryScale / 2f;
            spriteBatch.Draw(value11, position4, null, new Color(100, 100, 100, 100), 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
        }

        if (context == 0 && slot < 10) {
            float num11 = inventoryScale;
            string text2 = string.Concat(slot + 1);
            if (text2 == "10")
                text2 = "0";

            Color baseColor = Main.inventoryBack;
            int num12 = 0;
            if (Main.player[Main.myPlayer].selectedItem == slot) {
                baseColor = Color.White;
                baseColor.A = 200;
                num12 -= 2;
                num11 *= 1.4f;
            }

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text2, position + new Vector2(6f, 4 + num12) * inventoryScale, baseColor, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);
        }

        if (gamepadPointForSlot != -1)
            UILinkPointNavigator.SetPosition(gamepadPointForSlot, position + vector * 0.75f);
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
