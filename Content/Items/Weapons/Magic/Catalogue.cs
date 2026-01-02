using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Tiles.Miscellaneous;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Content.Items.Weapons.Magic;

sealed class Catalogue : GlobalItem {
    private static Asset<Texture2D> _spellTomeIndicator = null!,
                                    _catalogueIcon = null!,
                                    _catalogueIcon_Empty = null!,
                                    _inventoryBorder = null!;

    private static string ULTIMATESPELLTOMEKEY => RoA.ModName + "ultimatespelltome";

    private record struct LastReforgedData(bool Active, int[] SpellTomeItemTypes, int[] PrefixesPerItemType, byte CurrentSpellTomeIndex, Point16 ArchiveCoords, ScholarsArchiveTE.ArchiveSpellTomeType ArchiveSpellTomes);

    private static LastReforgedData _lastReforgedData;
    private static bool _useSoundPlayed;

    public static SoundStyle Empty { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "PageTurn") with { Volume = 0f };
    public static SoundStyle PageTurn { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "PageTurn");
    public static SoundStyle PageClose { get; private set; } = new SoundStyle(ResourceManager.ItemSounds + "PageClose");

    public int[] SpellTomeItemTypes { get; private set; } = null!;
    public int[] PrefixesPerItemType { get; private set; } = null!;

    public byte CurrentSpellTomeIndex { get; private set; }

    public int CurrentSpellTomeItemType =>  SpellTomeItemTypes[CurrentSpellTomeIndex];
    public int SpellTomeCount => SpellTomeItemTypes.Length;

    public bool Initialized => SpellTomeItemTypes != null;

    public Point16 ArchiveCoords { get; private set; }
    public ScholarsArchiveTE.ArchiveSpellTomeType ArchiveSpellTomes { get; private set; }

    //public LerpColor BorderColor { get; private set; } = new(0.03f);

    public bool Active { get; private set; }
    public bool HasSpells { get; private set; }

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
        HasSpells = result;
        if (!HasSpells) {
            CurrentSpellTomeIndex = 0;
        }
        Active = true;
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => ScholarsArchiveTE.IsSpellTome(entity.type);

    public void InitializeWith(ScholarsArchiveTE scholarsArchiveTE) {
        SpellTomeItemTypes = ScholarsArchiveTE.GetSpellTomeItemTypes(scholarsArchiveTE);
        PrefixesPerItemType = new int[SpellTomeItemTypes.Length];
        CurrentSpellTomeIndex = 0;
        ArchiveCoords = scholarsArchiveTE.Position;
        ArchiveSpellTomes = scholarsArchiveTE.SpellTomes;
        //UpdateActive();
    }

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _spellTomeIndicator = ModContent.Request<Texture2D>(ResourceManager.MagicWeaponTextures + "Catalogue_Icon");
            _inventoryBorder = ModContent.Request<Texture2D>(ResourceManager.UITextures + "Inventory_Back_Border");
            _catalogueIcon = ModContent.Request<Texture2D>(ResourceManager.MagicWeaponTextures + "Catalogue");
            _catalogueIcon_Empty = ModContent.Request<Texture2D>(ResourceManager.MagicWeaponTextures + "Catalogue_Empty");
        }

        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
        On_ItemSlot.SellOrTrash += On_ItemSlot_SellOrTrash;
        On_ItemSlot.LeftClick_ItemArray_int_int += On_ItemSlot_LeftClick_ItemArray_int_int;
        On_Item.GetStoreValue += On_Item_GetStoreValue;
        On_Item.AffixName += On_Item_AffixName;
        On_Player.ItemCheck_StartActualUse += On_Player_ItemCheck_StartActualUse;
        On_SoundEngine.PlaySound_refNullable1_Nullable1_SoundUpdateCallback += On_SoundEngine_PlaySound_refNullable1_Nullable1_SoundUpdateCallback;
        On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback += On_SoundEngine_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback;
    }

    private ReLogic.Utilities.SlotId On_SoundEngine_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback(On_SoundEngine.orig_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback orig, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback) {
        if (_useSoundPlayed) {
            _useSoundPlayed = false;
            return ReLogic.Utilities.SlotId.Invalid;
        }
        return orig(ref style, position, updateCallback);
    }

    private ReLogic.Utilities.SlotId On_SoundEngine_PlaySound_refNullable1_Nullable1_SoundUpdateCallback(On_SoundEngine.orig_PlaySound_refNullable1_Nullable1_SoundUpdateCallback orig, ref SoundStyle? style, Vector2? position, SoundUpdateCallback updateCallback) {
        return orig(ref style, position, updateCallback);
    }

    private void On_Player_ItemCheck_StartActualUse(On_Player.orig_ItemCheck_StartActualUse orig, Player self, Item sItem) {
        if (ScholarsArchiveTE.IsSpellTome(sItem.type)) {
            if (self.altFunctionUse == 2) {
                _useSoundPlayed = true;
            }
        }
        orig(self, sItem);
    }

    private string On_Item_AffixName(On_Item.orig_AffixName orig, Item self) {
        if (ScholarsArchiveTE.IsSpellTome(self.type) && self.GetGlobalItem<Catalogue>().Initialized) {
            if (!self.GetGlobalItem<Catalogue>().Active) {
                return Language.GetTextValue($"Mods.RoA.Catalogue0");
            }
            if (!self.GetGlobalItem<Catalogue>().HasSpells) {
                return Language.GetTextValue($"Mods.RoA.Catalogue1");
            }
        }
        return orig(self);
    }

    public override void SaveData(Item item, TagCompound tag) {
        tag[ULTIMATESPELLTOMEKEY + "spellitemtypes"] = SpellTomeItemTypes;
        tag[ULTIMATESPELLTOMEKEY + "prefixesperitemtype"] = PrefixesPerItemType;
        tag[ULTIMATESPELLTOMEKEY + "currentspelltomeindex"] = CurrentSpellTomeIndex;
        tag[ULTIMATESPELLTOMEKEY + "archivecoordsX"] = ArchiveCoords.X;
        tag[ULTIMATESPELLTOMEKEY + "archivecoordsY"] = ArchiveCoords.Y;
        tag[ULTIMATESPELLTOMEKEY + "archivespelltomes"] = (short)ArchiveSpellTomes;
        tag[ULTIMATESPELLTOMEKEY + "active"] = Active;
        tag[ULTIMATESPELLTOMEKEY + "hasnospells"] = HasSpells;
    }

    public override void LoadData(Item item, TagCompound tag) {
        SpellTomeItemTypes = tag.GetIntArray(ULTIMATESPELLTOMEKEY + "spellitemtypes");
        PrefixesPerItemType = tag.GetIntArray(ULTIMATESPELLTOMEKEY + "prefixesperitemtype");
        CurrentSpellTomeIndex = tag.GetByte(ULTIMATESPELLTOMEKEY + "currentspelltomeindex");
        ArchiveCoords = new Point16(tag.GetAsShort(ULTIMATESPELLTOMEKEY + "archivecoordsX"), tag.GetAsShort(ULTIMATESPELLTOMEKEY + "archivecoordsY"));
        ArchiveSpellTomes = (ScholarsArchiveTE.ArchiveSpellTomeType)tag.GetAsShort(ULTIMATESPELLTOMEKEY + "archivespelltomes");
        Active = tag.GetBool(ULTIMATESPELLTOMEKEY + "active");
        HasSpells = tag.GetBool(ULTIMATESPELLTOMEKEY + "hasnospells");
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

            TooltipLine line2 = new(Mod, "nospellsintome0", Language.GetTextValue("Mods.RoA.Catalogue0"));
            tooltips.Add(line2);
            line2 = new(Mod, "nospellsintome1", Language.GetTextValue("Mods.RoA.SwitchSpells"));
            tooltips.Add(line2);

            return;
        }

        if (!handler.HasSpells) {
            foreach (TooltipLine tooltipLine in tooltips) {
                tooltipLine.Hide();
            }

            TooltipLine line2 = new(Mod, "nospellsintome2", Language.GetTextValue("Mods.RoA.Catalogue1"));
            tooltips.Add(line2);
            line2 = new(Mod, "nospellsintome3", Language.GetTextValue("Mods.RoA.CatalogueNoSpells"));
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
        if (index <= 0) {
            index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("UseMana"));
        }
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
                    handler.HasSpells = false;
                    SoundEngine.PlaySound(Empty);
                    SoundEngine.PlaySound(PageClose);
                    return;
                }

                if (handler.CurrentSpellTomeIndex <= handler.SpellTomeCount - 2) {
                    SoundEngine.PlaySound(Empty);
                    SoundEngine.PlaySound(PageTurn);
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
                item.ChangeItemType(handler.SpellTomeItemTypes[index]);
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

                handler.UpdateActive();

                if (handler.CurrentSpellTomeIndex == 1) {
                    SoundEngine.PlaySound(Empty);
                    SoundEngine.PlaySound(PageClose);
                    handler.Active = false;
                }
                if (handler.CurrentSpellTomeIndex > handler.SpellTomeCount - 1) {
                    SoundEngine.PlaySound(Empty);
                    SoundEngine.PlaySound(PageTurn);
                    handler.CurrentSpellTomeIndex = 0;
                }
                //else {
                //    SoundEngine.PlaySound(PageTurn);
                //}

            }
            return;
        }
        item.GetGlobalItem<Catalogue>().UpdateActive();
    }

    public override bool CanUseItem(Item item, Player player) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active || !handler.HasSpells) {
                return player.altFunctionUse == 2;
            }
        }

        return base.CanUseItem(item, player);
    }

    public override bool? UseItem(Item item, Player player) {
        var handler = item.GetGlobalItem<Catalogue>();
        if (handler.Initialized) {
            if (!handler.Active || !handler.HasSpells) {
                if (player.altFunctionUse == 2) {
                    player.itemAnimation = 0;
                    player.itemTime = 0;
                    player.reuseDelay = 0;

                    if (!handler.Active && !handler.HasSpells) {
                        SoundEngine.PlaySound(Empty);
                        SoundEngine.PlaySound(PageTurn);
                    }
                    if (!handler.HasSpells) {
                        handler.SwitchTome(item);
                        item.GetGlobalItem<Catalogue>().UpdateActive();
                        return false;
                    }
                    SoundEngine.PlaySound(Empty);
                    SoundEngine.PlaySound(PageTurn);

                    item.GetGlobalItem<Catalogue>().UpdateActive();
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
            if (!handler.HasSpells) {
                Color color = Color.White;
                ItemUtils.DrawItem(item, color, 0f, _catalogueIcon_Empty.Value, scale, position);

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
            if (!handler.HasSpells) {
                ItemUtils.DrawItem(item, lightColor, rotation, _catalogueIcon_Empty.Value);

                return false;
            }
        }

        return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Initialized || !Active || !HasSpells) {
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
