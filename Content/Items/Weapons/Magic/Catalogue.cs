using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Tiles.Miscellaneous;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Content.Items.Weapons.Magic;

sealed class Catalogue : GlobalItem {
    private static Asset<Texture2D> _spellTomeIndicator = null!,
                                    _catalogueIcon = null!,
                                    _inventoryBorder = null!;

    //private static string ULTIMATESPELLTOMEKEY => RoA.ModName + "ultimatespelltome";

    private record struct LastReforgedData(bool Active, int[] SpellTomeItemTypes, int[] PrefixesPerItemType, byte CurrentSpellTomeIndex, Point16 ArchiveCoords, ScholarsArchiveTE.ArchiveSpellTomeType ArchiveSpellTomes);

    private static LastReforgedData _lastReforgedData;

    public int[] SpellTomeItemTypes { get; private set; } = null!;
    public int[] PrefixesPerItemType { get; private set; } = null!;

    public byte CurrentSpellTomeIndex { get; private set; }

    public int CurrentSpellTomeItemType =>  SpellTomeItemTypes[CurrentSpellTomeIndex];
    public int SpellTomeCount => SpellTomeItemTypes.Length;

    public bool Initialized => SpellTomeItemTypes != null;

    public Point16 ArchiveCoords { get; private set; }
    public ScholarsArchiveTE.ArchiveSpellTomeType ArchiveSpellTomes { get; private set; }

    public LerpColor BorderColor { get; private set; } = new(0.03f);

    public bool Active { get; private set; }

    public void UpdateActive() {
        bool result = false;
        Tile archiveTile = Main.tile[ArchiveCoords.X, ArchiveCoords.Y];
        if (archiveTile.TileType == ModContent.TileType<ScholarsArchive>()) {
            ScholarsArchiveTE? scholarsArchiveTE = TileHelper.GetTE<ScholarsArchiveTE>(ArchiveCoords.X, ArchiveCoords.Y);
            if (scholarsArchiveTE is not null) {
                result = true;
                if (!scholarsArchiveTE.HasAnySpellTome()) {
                    result = false;
                }
            }
        }
        Active = result;
        if (!Active) {
            CurrentSpellTomeIndex = 0;
        }
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => ScholarsArchiveTE.IsSpellTome(entity.type);

    public void InitializeWith(ScholarsArchiveTE scholarsArchiveTE) {
        SpellTomeItemTypes = ScholarsArchiveTE.GetSpellTomeItemTypes(scholarsArchiveTE);
        PrefixesPerItemType = new int[SpellTomeItemTypes.Length];
        CurrentSpellTomeIndex = 0;
        ArchiveCoords = scholarsArchiveTE.Position;
        ArchiveSpellTomes = scholarsArchiveTE.SpellTomes;
        UpdateActive();
    }

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _spellTomeIndicator = ModContent.Request<Texture2D>(ResourceManager.MagicWeaponTextures + "Catalogue_Icon");
            _inventoryBorder = ModContent.Request<Texture2D>(ResourceManager.UITextures + "Inventory_Back_Border");
            _catalogueIcon = ModContent.Request<Texture2D>(ResourceManager.MagicWeaponTextures + "Catalogue");
        }

        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
        On_ItemSlot.SellOrTrash += On_ItemSlot_SellOrTrash;
        On_ItemSlot.LeftClick_ItemArray_int_int += On_ItemSlot_LeftClick_ItemArray_int_int;
        On_Item.GetStoreValue += On_Item_GetStoreValue;
    }

    public override void SaveData(Item item, TagCompound tag) {

    }

    public override void LoadData(Item item, TagCompound tag) {

    }

    public override void NetSend(Item item, BinaryWriter writer) {

    }

    public override void NetReceive(Item item, BinaryReader reader) {
  
    }

    private void On_ItemSlot_LeftClick_ItemArray_int_int(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);
    }

    private void On_ItemSlot_SellOrTrash(On_ItemSlot.orig_SellOrTrash orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);
    }

    private int On_Item_GetStoreValue(On_Item.orig_GetStoreValue orig, Item self) {
        if (ScholarsArchiveTE.IsSpellTome(self.type) && self.GetGlobalItem<Catalogue>().Initialized) {
            return 0;
        }
        return orig(self);
    }

    public override void PreReforge(Item item) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (!handler.Initialized) {
            return;
        }

        _lastReforgedData = new(handler.Active, handler.SpellTomeItemTypes, handler.PrefixesPerItemType, handler.CurrentSpellTomeIndex, handler.ArchiveCoords, handler.ArchiveSpellTomes);
    }

    public override void PostReforge(Item item) {
        var handler = item.GetGlobalItem<Catalogue>();
        handler.Active = _lastReforgedData.Active;
        handler.SpellTomeItemTypes = _lastReforgedData.SpellTomeItemTypes;
        handler.CurrentSpellTomeIndex = _lastReforgedData.CurrentSpellTomeIndex;
        handler.PrefixesPerItemType = _lastReforgedData.PrefixesPerItemType;
        handler.PrefixesPerItemType[handler.CurrentSpellTomeIndex] = item.prefix;
        handler.ArchiveCoords = _lastReforgedData.ArchiveCoords;
        handler.ArchiveSpellTomes = _lastReforgedData.ArchiveSpellTomes;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (!handler.Initialized) {
            return;
        }

        if (!handler.Active) {
            foreach (TooltipLine tooltipLine in tooltips) {
                tooltipLine.Hide();
            }

            TooltipLine line2 = new(Mod, "nospellsintome", Language.GetTextValue("Mods.RoA.NoSpells"));
            tooltips.Add(line2);

            return;
        }

        foreach (TooltipLine tooltipLine in tooltips) {
            if (tooltipLine.Name == "Price") {
                tooltipLine.Hide();
            }
        }

        TooltipLine line = new(Mod, "switchspells", Language.GetTextValue("Mods.RoA.SwitchSpells"));
        int index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Tooltip0"));
        tooltips.Insert(index + 1, line);
    }

    private void On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig,
        SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor) {
        orig(spriteBatch, inv, context, slot, position, lightColor);

        //Player player = Main.player[Main.myPlayer];
        //Item hoverItem = inv[slot];
        //if (!ScholarsArchiveTE.IsSpellTome(hoverItem.type)) {
        //    return;
        //}
        //var handler = hoverItem.GetGlobalItem<UltimateSpellTome>();
        //if (!handler.Initialized || !handler.Active) {
        //    return;
        //}
        //float inventoryScale = Main.inventoryScale;
        //Color color = Color.White;
        //if (lightColor != Color.Transparent)
        //    color = lightColor;
        //List<Color> colors = [];
        //for (int i = 0; i < handler.SpellTomeCount; i++) {
        //    colors.Add(ScholarsArchiveTE.GetColorPerSpellTome(handler.SpellTomeItemTypes[i]));
        //}
        //Texture2D inventory = _inventoryBorder.Value;
        //Color baseColor = handler.BorderColor.GetLerpColor(colors);
        //handler.BorderColor.Update();
        //spriteBatch.Draw(inventory, position, null, baseColor.MultiplyRGB(color), 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
    }

    public void SwitchTome(Item item) {
        Tile archiveTile = Main.tile[ArchiveCoords.X, ArchiveCoords.Y];
        if (archiveTile.HasTile && archiveTile.TileType == ModContent.TileType<ScholarsArchive>()) {
            ScholarsArchiveTE? scholarsArchiveTE = TileHelper.GetTE<ScholarsArchiveTE>(ArchiveCoords.X, ArchiveCoords.Y);
            if (scholarsArchiveTE is not null) {
                var handler = item.GetGlobalItem<Catalogue>();
                if (!scholarsArchiveTE.HasAnySpellTome()) {
                    handler.Active = false;
                    return;
                }

                handler.SpellTomeItemTypes = ScholarsArchiveTE.GetSpellTomeItemTypes(scholarsArchiveTE);

                int[] spellTomeItemTypes = handler.SpellTomeItemTypes;
                int[] prefixesPerItemType = handler.PrefixesPerItemType;
                byte index = handler.CurrentSpellTomeIndex;
                if (handler.CurrentSpellTomeIndex > handler.SpellTomeCount - 1) {
                    handler.CurrentSpellTomeIndex = 0;
                }
                Point16 archiveCoords = handler.ArchiveCoords;
                ScholarsArchiveTE.ArchiveSpellTomeType spellTomes = handler.ArchiveSpellTomes;
                item.ChangeItemType(SpellTomeItemTypes[index]);
                handler = item.GetGlobalItem<Catalogue>();
                handler.SpellTomeItemTypes = spellTomeItemTypes;
                handler.PrefixesPerItemType = prefixesPerItemType;
                int prefixIndex = index + 1;
                if (prefixIndex > handler.SpellTomeCount - 1) {
                    prefixIndex = 0;
                }
                item.prefix = handler.PrefixesPerItemType[prefixIndex];
                index++;
                handler.CurrentSpellTomeIndex = index;
                handler.ArchiveCoords = archiveCoords;
                handler.ArchiveSpellTomes = spellTomes;
                float damageIncreaseModifier = 1f + 0.1f * handler.SpellTomeCount;
                item.damage = (int)(item.damage * damageIncreaseModifier);
                if (handler.CurrentSpellTomeIndex > handler.SpellTomeCount - 1) {
                    handler.CurrentSpellTomeIndex = 0;
                }

                handler.UpdateActive();
            }
            return;
        }
        item.GetGlobalItem<Catalogue>().UpdateActive();
    }

    public override bool CanUseItem(Item item, Player player) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active) {
                return player.altFunctionUse == 2;
            }
        }

        return base.CanUseItem(item, player);
    }

    public override bool? UseItem(Item item, Player player) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active) {
                if (player.altFunctionUse == 2) {
                    item.GetGlobalItem<Catalogue>().UpdateActive();

                    player.itemAnimation = 0;
                    player.itemTime = 0;
                    player.reuseDelay = 0;
                }
                return false;
            }
            if (player.altFunctionUse == 2) {
                if (player.ItemAnimationJustStarted) {
                    handler.SwitchTome(item);

                    player.itemAnimation = 0;
                    player.itemTime = 0;
                    player.reuseDelay = 0;

                    return false;
                }
            }
        }

        return base.UseItem(item, player);
    }

    public override bool CanShoot(Item item, Player player) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active) {
                return false;
            }
            if (player.altFunctionUse == 2) {
                return false;
            }
        }

        return base.CanShoot(item, player);
    }

    public override bool AltFunctionUse(Item item, Player player) => item.GetGlobalItem<Catalogue>().Initialized;

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active) {
                Color color = Color.White;
                ItemUtils.DrawItem(item, color, 0f, _catalogueIcon.Value, scale, position);

                return false;
            }
        }

        return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active) {
                ItemUtils.DrawItem(item, lightColor, rotation, _catalogueIcon.Value);

                return false;
            }
        }

        return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Initialized || !Active) {
            return;
        }

        Texture2D inventory = _spellTomeIndicator.Value;
        position.Y += inventory.Height / 2f * scale;
        position.X += inventory.Width / 3f * scale;
        float xOffset = 0f;
        var handler = item.GetGlobalItem<Catalogue>();
        //for (int i = 0; i < handler.SpellTomeCount; i++) {
        //    xOffset += inventory.Width / 2f;
        //}
        spriteBatch.Draw(inventory, position - Vector2.UnitX * xOffset / 2f * scale, null, drawColor, 0f, default, scale, SpriteEffects.None, 0f);
    }
}
