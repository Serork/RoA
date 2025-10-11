﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Players;

sealed class ExtraDrawLayerSupport : ILoadable {
    public readonly record struct ExtraDrawLayerInfo(ModItem ModItem, Asset<Texture2D> Texture);

    private static Dictionary<EquipType, ExtraDrawLayerInfo> _extraDrawLayerByItemType = [];

    public static void RegisterExtraDrawLayer(EquipType equipType, ModItem modItem) {
        string textureTypePath = modItem.Texture + $"_{EquipType.Back}";
        _extraDrawLayerByItemType.TryAdd(equipType, new ExtraDrawLayerInfo(modItem, ModContent.Request<Texture2D>(textureTypePath)));
    }

    void ILoadable.Load(Mod mod) {
        On_PlayerDrawLayers.DrawPlayer_08_Backpacks += On_PlayerDrawLayers_DrawPlayer_08_Backpacks;
    }

    void ILoadable.Unload() {
        _extraDrawLayerByItemType.Clear();
        _extraDrawLayerByItemType = null!;
    }

    private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        foreach (var drawInfo in _extraDrawLayerByItemType) {
            if (drawInfo.Key != EquipType.Back) {
                continue;
            }
            int itemType = drawInfo.Value.ModItem.Type;
            if (!drawinfo.hideEntirePlayer && !player.dead) {
                if (player.CheckArmorSlot(itemType, 1, 11) || player.CheckVanitySlot(itemType, 11)) {
                    int type = drawinfo.heldItem.type;
                    int num2 = 1;
                    float num3 = -4f;
                    float num4 = -8f;
                    int shader = 0;
                    shader = drawinfo.cBody;

                    Vector2 vector3 = new Vector2(-4f * player.direction, 12f);
                    Vector2 vec5 = drawinfo.Position - Main.screenPosition + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0f, -4f) + vector3;
                    vec5 = vec5.Floor();
                    Vector2 vec6 = drawinfo.Position - Main.screenPosition + new Vector2(drawinfo.drawPlayer.width / 2, drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height / 2) + new Vector2((-9f + num3) * (float)drawinfo.drawPlayer.direction, (2f + num4) * drawinfo.drawPlayer.gravDir) + vector3;
                    vec6 = vec6.Floor();

                    var asset = drawInfo.Value.Texture;
                    DrawData item = new DrawData(asset.Value, vec5,
                        new Rectangle(0, 0, asset.Width(), asset.Height()), drawinfo.colorArmorBody, drawinfo.drawPlayer.bodyRotation, new Vector2((float)asset.Width() * 0.5f, drawinfo.bodyVect.Y), 1f, drawinfo.playerEffect);
                    item.shader = shader;
                    drawinfo.DrawDataCache.Add(item);

                    return;
                }
            }
        }

        orig(ref drawinfo);
    }
}
