using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Tiles.Miscellaneous;
using RoA.Core;
using RoA.Core.Data;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Content.Items.Weapons.Magic;

sealed class UltimateSpellTome : GlobalItem {
    private static Asset<Texture2D> _spellTomeIndicator = null!,
                                    _inventoryBorder = null!;

    //private static string ULTIMATESPELLTOMEKEY => RoA.ModName + "ultimatespelltome";

    public int[] SpellTomeItemTypes { get; private set; } = null!;

    public byte CurrentSpellTomeIndex { get; private set; }

    public int CurrentSpellTomeItemType =>  SpellTomeItemTypes[CurrentSpellTomeIndex];
    public int SpellTomeCount => SpellTomeItemTypes.Length;

    public bool Initialized => SpellTomeItemTypes != null;

    public static LerpColor BorderColor { get; private set; } = new(0.03f);

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => ScholarsArchiveTE.IsSpellTome(entity.type);

    public void InitializeWith(ScholarsArchiveTE scholarsArchiveTE) {
        SpellTomeItemTypes = ScholarsArchiveTE.GetSpellTomeItemTypes(scholarsArchiveTE);
        CurrentSpellTomeIndex = 0;
    }

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _spellTomeIndicator = ModContent.Request<Texture2D>(ResourceManager.UITextures + "SpellTomeIndicator");
            _inventoryBorder = ModContent.Request<Texture2D>(ResourceManager.UITextures + "Inventory_Back_Border");
        }

        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
    }

    private void On_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig,
        SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor) {
        orig(spriteBatch, inv, context, slot, position, lightColor);

        Player player = Main.player[Main.myPlayer];
        Item hoverItem = inv[slot];
        if (!ScholarsArchiveTE.IsSpellTome(hoverItem.type)) {
            return;
        }
        if (!hoverItem.GetGlobalItem<UltimateSpellTome>().Initialized) {
            return;
        }
        float inventoryScale = Main.inventoryScale;
        Color color = Color.White;
        if (lightColor != Color.Transparent)
            color = lightColor;
        List<Color> colors = [];
        var handler = hoverItem.GetGlobalItem<UltimateSpellTome>();
        for (int i = 0; i < handler.SpellTomeCount; i++) {
            colors.Add(ScholarsArchiveTE.GetColorPerSpellTome(handler.SpellTomeItemTypes[i]));
        }
        Texture2D inventory = _inventoryBorder.Value;
        Color baseColor = BorderColor.GetLerpColor(colors);
        BorderColor.Update();
        spriteBatch.Draw(inventory, position, null, baseColor.MultiplyRGB(color), 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
    }

    public void SwitchTome(Item item) {
        var handler = item.GetGlobalItem<UltimateSpellTome>();
        int[] spellTomeItemTypes = handler.SpellTomeItemTypes;
        byte index = CurrentSpellTomeIndex;
        item.ChangeItemType(SpellTomeItemTypes[index++]);
        handler = item.GetGlobalItem<UltimateSpellTome>();
        handler.SpellTomeItemTypes = spellTomeItemTypes;
        handler.CurrentSpellTomeIndex = index;
        float damageIncreaseModifier = 1f + 0.1f * handler.SpellTomeCount;
        item.damage = (int)(item.damage * damageIncreaseModifier);
        Main.NewText(damageIncreaseModifier);
        if (handler.CurrentSpellTomeIndex > handler.SpellTomeCount - 1) {
            handler.CurrentSpellTomeIndex = 0;
        }
    }

    public override bool CanUseItem(Item item, Player player) {
        return base.CanUseItem(item, player);
    }

    public override bool? UseItem(Item item, Player player) {
        var handler = item.GetGlobalItem<UltimateSpellTome>();
        if (handler.Initialized) {
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
        var handler = item.GetGlobalItem<UltimateSpellTome>();
        if (handler.Initialized) {
            if (player.altFunctionUse == 2) {
                return false;
            }
        }

        return base.CanShoot(item, player);
    }

    public override bool AltFunctionUse(Item item, Player player) => item.GetGlobalItem<UltimateSpellTome>().Initialized;

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (!Initialized) {
            return;
        }

        //Texture2D inventory = _spellTomeIndicator.Value;
        //position.Y += inventory.Height / 2f;
        //position.X += inventory.Width / 3f;
        //float xOffset = 0f;
        //var handler = item.GetGlobalItem<UltimateSpellTome>();
        //for (int i = 0; i < handler.SpellTomeCount; i++) {
        //    xOffset += inventory.Width / 2f;
        //}
        //for (int i = 0; i < handler.SpellTomeCount; i++) {
        //    spriteBatch.Draw(inventory, position - Vector2.UnitX * xOffset / 2f, null, ScholarsArchiveTE.GetColorPerSpellTome(handler.SpellTomeItemTypes[i]).MultiplyRGB(drawColor), 0f, default, 1f, SpriteEffects.None, 0f);
        //    position.X += inventory.Width / 2f;
        //}
    }
}
