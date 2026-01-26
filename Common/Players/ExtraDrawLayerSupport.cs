using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Items;
using RoA.Content.Items.Equipables.Accessories.Hardmode;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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
        On_PlayerDrawLayers.DrawPlayer_01_3_BackHead += On_PlayerDrawLayers_DrawPlayer_01_3_BackHead;
        On_PlayerDrawLayers.DrawPlayer_21_Head += On_PlayerDrawLayers_DrawPlayer_21_Head;
        On_PlayerDrawLayers.DrawPlayer_22_FaceAcc += On_PlayerDrawLayers_DrawPlayer_22_FaceAcc;
        On_PlayerDrawLayers.DrawPlayer_12_Skin += On_PlayerDrawLayers_DrawPlayer_12_Skin;
        On_PlayerDrawLayers.DrawPlayer_27_HeldItem += On_PlayerDrawLayers_DrawPlayer_27_HeldItem;
        On_PlayerDrawLayers.DrawPlayer_31_ProjectileOverArm += On_PlayerDrawLayers_DrawPlayer_31_ProjectileOverArm;
        On_PlayerDrawLayers.DrawPlayer_20_NeckAcc += On_PlayerDrawLayers_DrawPlayer_20_NeckAcc;
    }

    private void On_PlayerDrawLayers_DrawPlayer_20_NeckAcc(On_PlayerDrawLayers.orig_DrawPlayer_20_NeckAcc orig, ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        Item heldItem = player.GetSelectedItem();
        if (drawinfo.drawPlayer.neck == EquipLoader.GetEquipSlot(RoA.Instance, nameof(ChromaticScarf), EquipType.Neck)) {
            Color glowColor = player.GetImmuneAlphaPure(Color.White * 0.9f * 0.5f, drawinfo.shadow);
            if (player.GetCommon().IsChromaticScarfEffectActive && !heldItem.IsEmpty() && heldItem.IsAWeapon() && heldItem.TryGetGlobalItem(out ChromaticScarfDebuffPicker modItem)) {
                Texture2D getScarfTexture(PlayerDrawSet drawinfo, int debuff) {
                    Texture2D result = TextureAssets.AccNeck[drawinfo.drawPlayer.neck].Value;
                    if (debuff == ChromaticScarfDebuffPicker.DebuffList[0]) {
                        result = ChromaticScarf.Neck1.Value;
                    }
                    else if (debuff == ChromaticScarfDebuffPicker.DebuffList[1]) {
                        result = ChromaticScarf.Neck2.Value;
                    }
                    else {
                        result = ChromaticScarf.Neck3.Value;
                    }
                    return result;
                }
                Texture2D currentNeckTexture = getScarfTexture(drawinfo, modItem.CurrentDebuff),
                          nextNeckTexture = getScarfTexture(drawinfo, modItem.NextDebuff);
                float opacity1 = 1f,
                      opacity2 = Utils.GetLerpValue(ChromaticScarfDebuffPicker.CHANGETIMEINTICKS * 0.9f, ChromaticScarfDebuffPicker.CHANGETIMEINTICKS, modItem.CurrentDebuffCounter, true);
                DrawData item = new DrawData(currentNeckTexture, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame,
                    drawinfo.colorArmorBody * opacity1,
                    drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
                item.shader = drawinfo.cNeck;
                drawinfo.DrawDataCache.Add(item);
                item = new DrawData(nextNeckTexture, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame,
                      drawinfo.colorArmorBody * opacity2,
                    drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
                item.shader = drawinfo.cNeck;
                drawinfo.DrawDataCache.Add(item);

                item = new DrawData(currentNeckTexture, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame,
                    glowColor * opacity1,
                    drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
                item.shader = drawinfo.cNeck;
                drawinfo.DrawDataCache.Add(item);
                item = new DrawData(nextNeckTexture, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame,
                    glowColor * opacity2,
                    drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
                item.shader = drawinfo.cNeck;
                drawinfo.DrawDataCache.Add(item);

                return;
            }

            DrawData item2 = new DrawData(TextureAssets.AccNeck[drawinfo.drawPlayer.neck].Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame,
                    drawinfo.colorArmorBody,
                    drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
            item2.shader = drawinfo.cNeck;
            drawinfo.DrawDataCache.Add(item2);

            item2 = new DrawData(TextureAssets.AccNeck[drawinfo.drawPlayer.neck].Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame,
                    glowColor,
                    drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
            item2.shader = drawinfo.cNeck;
            drawinfo.DrawDataCache.Add(item2);

            return;
        }

        orig(ref drawinfo);
    }

    public delegate void PreProjectileOverArmDelegate(ref PlayerDrawSet drawinfo);
    public static event PreProjectileOverArmDelegate PreProjectileOverArmDrawEvent;

    private void On_PlayerDrawLayers_DrawPlayer_31_ProjectileOverArm(On_PlayerDrawLayers.orig_DrawPlayer_31_ProjectileOverArm orig, ref PlayerDrawSet drawinfo) {
        PreProjectileOverArmDrawEvent?.Invoke(ref drawinfo);

        orig(ref drawinfo);
    }

    public delegate void PreHeldItemDelegate(ref PlayerDrawSet drawinfo);
    public static event PreHeldItemDelegate PreHeldItemDrawEvent;
    public delegate void PostHeldItemDelegate(ref PlayerDrawSet drawinfo);
    public static event PostHeldItemDelegate PostHeldItemDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_27_HeldItem(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo) {
        PreHeldItemDrawEvent?.Invoke(ref drawinfo);

        orig(ref drawinfo);

        PostHeldItemDrawEvent?.Invoke(ref drawinfo);
    }

    public delegate void PostSkinDrawDelegate(ref PlayerDrawSet drawinfo);
    public static event PostSkinDrawDelegate PostSkinDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_12_Skin(On_PlayerDrawLayers.orig_DrawPlayer_12_Skin orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        PostSkinDrawEvent?.Invoke(ref drawinfo);
    }

    public delegate void PostFaceAccDrawDelegate(ref PlayerDrawSet drawinfo);
    public static event PostFaceAccDrawDelegate PostFaceAccDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_22_FaceAcc(On_PlayerDrawLayers.orig_DrawPlayer_22_FaceAcc orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        PostFaceAccDrawEvent?.Invoke(ref drawinfo);
    }

    public delegate void PostHeadDrawDelegate(ref PlayerDrawSet drawinfo);
    public static event PostHeadDrawDelegate PostHeadDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_21_Head(On_PlayerDrawLayers.orig_DrawPlayer_21_Head orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        PostHeadDrawEvent?.Invoke(ref drawinfo);
    }

    public delegate void PostBackHeadDrawDelegate(ref PlayerDrawSet drawinfo);
    public static event PostBackHeadDrawDelegate PostBackHeadDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_01_3_BackHead(On_PlayerDrawLayers.orig_DrawPlayer_01_3_BackHead orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        PostBackHeadDrawEvent?.Invoke(ref drawinfo);
    }

    void ILoadable.Unload() {
        _extraDrawLayerByItemType.Clear();
        _extraDrawLayerByItemType = null!;
    }

    public delegate void PreBackpackDrawDelegate(ref PlayerDrawSet drawinfo);
    public static event PreBackpackDrawDelegate PreBackpackDrawEvent;
    private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawinfo) {
        PreBackpackDrawEvent?.Invoke(ref drawinfo);

        orig(ref drawinfo);

        DrawBackpacks(ref drawinfo);
    }

    public static void DrawBackpacks(ref PlayerDrawSet drawinfo) {
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

                    if (drawinfo.drawPlayer.gravDir < 0) {
                        vec5.Y -= 8f;
                    }

                    var asset = drawInfo.Value.Texture;
                    DrawData item = new DrawData(asset.Value, vec5 + player.MovementOffset(),
                        new Rectangle(0, 0, asset.Width(), asset.Height()), drawinfo.colorArmorBody, drawinfo.drawPlayer.bodyRotation, new Vector2((float)asset.Width() * 0.5f, drawinfo.bodyVect.Y), 1f, drawinfo.playerEffect);
                    item.shader = shader;
                    drawinfo.DrawDataCache.Add(item);

                    //return;
                }
            }
        }
    }
}
