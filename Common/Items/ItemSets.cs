using Microsoft.Xna.Framework;

using RoA.Content.Items.Placeable.Seeds;

using System;
using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class ItemSets : ILoadable {
    public static bool[] ShouldCreateTile = ItemID.Sets.Factory.CreateBoolSet(true);

    public void Load(Mod mod) {
        On_SmartCursorHelper.Step_GrassSeeds += On_SmartCursorHelper_Step_GrassSeeds;
        On_Player.PlaceThing_Tiles_PlaceIt += On_Player_PlaceThing_Tiles_PlaceIt;
    }

    private TileObject On_Player_PlaceThing_Tiles_PlaceIt(On_Player.orig_PlaceThing_Tiles_PlaceIt orig, Player self, bool newObjectType, TileObject data, int tileToCreate) {
        Item item = self.inventory[self.selectedItem];
        if (!ShouldCreateTile[item.type]) {
            return data;
        }

        return orig(self, newObjectType, data, tileToCreate);
    }

    private void On_SmartCursorHelper_Step_GrassSeeds(On_SmartCursorHelper.orig_Step_GrassSeeds orig, object providedInfo, ref int focusedX, ref int focusedY) {
        var SmartCursorUsageInfo = typeof(SmartCursorHelper).GetNestedType("SmartCursorUsageInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        Item item = (Item)SmartCursorUsageInfo.GetField("item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableStartX = (int)SmartCursorUsageInfo.GetField("reachableStartX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableEndX = (int)SmartCursorUsageInfo.GetField("reachableEndX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableStartY = (int)SmartCursorUsageInfo.GetField("reachableStartY", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        int reachableEndY = (int)SmartCursorUsageInfo.GetField("reachableEndY", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        Vector2 mouse = (Vector2)SmartCursorUsageInfo.GetField("mouse", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(providedInfo);
        List<Tuple<int, int>> _targets = (List<Tuple<int, int>>)typeof(SmartCursorHelper).GetField("_targets", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(null);

        if (focusedX > -1 || focusedY > -1)
            return;

        int type = item.type;
        if (type < 0 || !ItemID.Sets.GrassSeeds[type])
            return;

        _targets.Clear();
        for (int i = reachableStartX; i <= reachableEndX; i++) {
            for (int j = reachableStartY; j <= reachableEndY; j++) {
                Tile tile = Main.tile[i, j];
                bool flag = !Main.tile[i - 1, j].HasTile || !Main.tile[i, j + 1].HasTile || !Main.tile[i + 1, j].HasTile || !Main.tile[i, j - 1].HasTile;
                bool flag2 = !Main.tile[i - 1, j - 1].HasTile || !Main.tile[i - 1, j + 1].HasTile || !Main.tile[i + 1, j + 1].HasTile || !Main.tile[i + 1, j - 1].HasTile;
                if (tile.HasTile && !tile.IsActuated && (flag || flag2)) {
                    bool flag3 = false;
                    switch (type) {
                        default:
                            flag3 = tile.TileType == 0;
                            break;
                        case 59:
                        case 2171:
                            flag3 = tile.TileType == 0 || tile.TileType == 59;
                            break;
                        case 194:
                        case 195:
                            flag3 = tile.TileType == 59;
                            break;
                        case 5214:
                            flag3 = tile.TileType == 57;
                            break;
                    }
                    if (type == ModContent.ItemType<BackwoodsGrassSeeds>()) {
                        flag3 = tile.TileType == 0;
                    }

                    if (flag3)
                        _targets.Add(new Tuple<int, int>(i, j));
                }
            }
        }

        if (_targets.Count > 0) {
            float num = -1f;
            Tuple<int, int> tuple = _targets[0];
            for (int k = 0; k < _targets.Count; k++) {
                float num2 = Vector2.Distance(new Vector2(_targets[k].Item1, _targets[k].Item2) * 16f + Vector2.One * 8f, mouse);
                if (num == -1f || num2 < num) {
                    num = num2;
                    tuple = _targets[k];
                }
            }

            if (Collision.InTileBounds(tuple.Item1, tuple.Item2, reachableStartX, reachableStartY, reachableEndX, reachableEndY)) {
                focusedX = tuple.Item1;
                focusedY = tuple.Item2;
            }
        }

        _targets.Clear();
    }

    public void Unload() { }
}
