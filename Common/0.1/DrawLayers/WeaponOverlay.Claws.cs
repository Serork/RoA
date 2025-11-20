using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using ReLogic.Content;
using ReLogic.Utilities;

using RoA.Common.GlowMasks;
using RoA.Content.Items.Dyes;
using RoA.Content.Items.LiquidsSpecific;
using RoA.Content.Items.Weapons;
using RoA.Content.Items.Weapons.Nature;
using RoA.Content.Items.Weapons.Nature.Hardmode.Claws;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Common.GlowMasks.ItemGlowMaskHandler;

namespace RoA.Common.DrawLayers;

sealed partial class WeaponOverlay : PlayerDrawLayer {
    private static Vector2 GetCompositeOffset_BackArm(ref PlayerDrawSet drawinfo) => new Vector2(6 * ((!drawinfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 2 * ((!drawinfo.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1)));
    private static Vector2 GetCompositeOffset_FrontArm(ref PlayerDrawSet drawinfo) => new Vector2(-5 * ((!drawinfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 0f);


    //private const string CLAWSTEXTURESPATH = $"/{ResourceManager.TEXTURESPATH}/Items/Weapons/Druidic/Claws";

    private static readonly Dictionary<string, Asset<Texture2D>?> _clawsOutfitTextures = [];
    private static readonly Dictionary<string, Asset<Texture2D>?> _clawsOutfitGlowTextures = [];

    private static void LoadClawsOutfitTextures() {
        for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
            ModItem item = ItemLoader.GetItem(i);
            if (item is ClawsBaseItem) {
                _clawsOutfitTextures.Add(item.Name, ModContent.Request<Texture2D>(item.Texture + REQUIREMENT));
                AutoloadGlowMaskAttribute? glowMaskAttribute = item.GetType().GetAttribute<AutoloadGlowMaskAttribute>();
                if (glowMaskAttribute is not null) {
                    _clawsOutfitGlowTextures.Add(item.Name, ModContent.Request<Texture2D>(item.Texture + REQUIREMENT + "_Glow"));
                }
            }
        }

        On_PlayerDrawLayers.DrawPlayer_12_SkinComposite_BackArmShirt += On_PlayerDrawLayers_DrawPlayer_12_SkinComposite_BackArmShirt;
        On_PlayerDrawLayers.DrawPlayer_17_TorsoComposite += On_PlayerDrawLayers_DrawPlayer_17_TorsoComposite;
        On_PlayerDrawLayers.DrawPlayer_28_ArmOverItemComposite += On_PlayerDrawLayers_DrawPlayer_28_ArmOverItemComposite;

        //foreach (Asset<Texture2D> texture in ResourceManager.GetAllTexturesInPath(CLAWSTEXTURESPATH, REQUIREMENT)) {
        //    string getName() {
        //        return texture.Name.Split("\\").Last().Replace(REQUIREMENT, string.Empty);
        //    }
        //    _clawsOutfitTextures.Add(getName(), texture);
        //}
    }

    private static void On_PlayerDrawLayers_DrawPlayer_28_ArmOverItemComposite(On_PlayerDrawLayers.orig_DrawPlayer_28_ArmOverItemComposite orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        DrawClawsOverArm(ref drawinfo, CompositePlayerDrawContext.FrontArm);
    }

    private static void On_PlayerDrawLayers_DrawPlayer_17_TorsoComposite(On_PlayerDrawLayers.orig_DrawPlayer_17_TorsoComposite orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);
    }

    private static void On_PlayerDrawLayers_DrawPlayer_12_SkinComposite_BackArmShirt(On_PlayerDrawLayers.orig_DrawPlayer_12_SkinComposite_BackArmShirt orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        DrawClawsOverArm(ref drawinfo, CompositePlayerDrawContext.BackArm);
    }

    private static void DrawClawsOverArm(ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context) {
        Player player = drawinfo.drawPlayer;
        if (!HasClawsSelectedAndLoaded(player, out ClawsBaseItem clawsBaseItem, out WeaponOverlayAttribute weaponAttribute, out Asset<Texture2D> texture, out Asset<Texture2D> glowTexture)) {
            return;
        }

        if (!clawsBaseItem.IsHardmodeClaws) {
            return;
        }

        switch (context) {
            case CompositePlayerDrawContext.FrontArm: {
                    Vector2 vector = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2);
                    Vector2 vector2 = Main.OffsetsPlayerHeadgear[drawinfo.drawPlayer.bodyFrame.Y / drawinfo.drawPlayer.bodyFrame.Height];
                    vector2.Y -= 2f;
                    vector += vector2 * -drawinfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
                    float bodyRotation = drawinfo.drawPlayer.bodyRotation;
                    float rotation = drawinfo.drawPlayer.bodyRotation + drawinfo.compositeFrontArmRotation;
                    Vector2 bodyVect = drawinfo.bodyVect;
                    Vector2 compositeOffset_FrontArm = GetCompositeOffset_FrontArm(ref drawinfo);
                    bodyVect += compositeOffset_FrontArm;
                    vector += compositeOffset_FrontArm;
                    Vector2 position = vector + drawinfo.frontShoulderOffset;
                    if (drawinfo.compFrontArmFrame.X / drawinfo.compFrontArmFrame.Width >= 7)
                        vector += new Vector2((!drawinfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1), (!drawinfo.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1));

                    _ = drawinfo.drawPlayer.invis;
                    bool num = drawinfo.drawPlayer.body > 0 && drawinfo.drawPlayer.body < ArmorIDs.Body.Count;
                    int num2 = (drawinfo.compShoulderOverFrontArm ? 1 : 0);
                    int num3 = ((!drawinfo.compShoulderOverFrontArm) ? 1 : 0);
                    int num4 = ((!drawinfo.compShoulderOverFrontArm) ? 1 : 0);
                    bool flag = !drawinfo.hidesTopSkin;
                    var data = new DrawData(texture.Value, vector, drawinfo.compFrontArmFrame, drawinfo.colorArmorBody, rotation, bodyVect, 1f, drawinfo.playerEffect) {
                    };
                    DrawData item2 = data;
                    if (weaponAttribute.Hex != null) {
                        item2.color = player.GetImmuneAlphaPure(Helper.FromHexRgb(weaponAttribute.Hex.Value), (float)drawinfo.shadow);
                    }
                    drawinfo.DrawDataCache.Add(item2);

                    AutoloadGlowMaskAttribute? glowMaskAttribute = clawsBaseItem.Item.GetAttribute<AutoloadGlowMaskAttribute>();
                    if (glowMaskAttribute is not null) {
                        DrawData glowMask = item2;
                        glowMask.texture = glowTexture.Value;
                        float brightnessFactor = Lighting.Brightness((int)glowMask.position.X / 16, (int)glowMask.position.Y / 16);
                        Color color = Color.Lerp(glowMaskAttribute.GlowColor * 0.9f, glowMask.color, brightnessFactor);
                        glowMask.color = player.GetImmuneAlphaPure(glowMaskAttribute.ShouldApplyItemAlpha ? color * (1f - clawsBaseItem.Item.alpha / 255f) : (glowMaskAttribute.GlowColor * 0.9f), (float)drawinfo.shadow);
                        if (clawsBaseItem is TerraClaws) {
                            glowMask.shader = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<TerraDye>());
                        }
                        drawinfo.DrawDataCache.Add(glowMask);
                    }
                    break;
                }
            case CompositePlayerDrawContext.BackArm: {
                    Vector2 vector = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2);
                    Vector2 vector2 = Main.OffsetsPlayerHeadgear[drawinfo.drawPlayer.bodyFrame.Y / drawinfo.drawPlayer.bodyFrame.Height];
                    vector2.Y -= 2f;
                    vector += vector2 * -drawinfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
                    vector.Y += drawinfo.torsoOffset;
                    float bodyRotation = drawinfo.drawPlayer.bodyRotation;
                    Vector2 vector3 = vector;
                    Vector2 position = vector;
                    Vector2 bodyVect = drawinfo.bodyVect;
                    Vector2 compositeOffset_BackArm = GetCompositeOffset_BackArm(ref drawinfo);
                    vector3 += compositeOffset_BackArm;
                    position += drawinfo.backShoulderOffset;
                    bodyVect += compositeOffset_BackArm;
                    float rotation = bodyRotation + drawinfo.compositeBackArmRotation;
                    var data = new DrawData(texture.Value, vector3, drawinfo.compBackArmFrame, drawinfo.colorArmorBody, rotation, bodyVect, 1f, drawinfo.playerEffect) {
                    };
                    DrawData item2 = data;
                    if (weaponAttribute.Hex != null) {
                        item2.color = player.GetImmuneAlphaPure(Helper.FromHexRgb(weaponAttribute.Hex.Value), (float)drawinfo.shadow);
                    }
                    drawinfo.DrawDataCache.Add(item2);

                    AutoloadGlowMaskAttribute? glowMaskAttribute = clawsBaseItem.Item.GetAttribute<AutoloadGlowMaskAttribute>();
                    if (glowMaskAttribute is not null) {
                        DrawData glowMask = item2;
                        glowMask.texture = glowTexture.Value;
                        float brightnessFactor = Lighting.Brightness((int)glowMask.position.X / 16, (int)glowMask.position.Y / 16);
                        Color color = Color.Lerp(glowMaskAttribute.GlowColor * 0.9f, glowMask.color, brightnessFactor);
                        glowMask.color = player.GetImmuneAlphaPure(glowMaskAttribute.ShouldApplyItemAlpha ? color * (1f - clawsBaseItem.Item.alpha / 255f) : (glowMaskAttribute.GlowColor * 0.9f), (float)drawinfo.shadow);
                        if (clawsBaseItem is TerraClaws) {
                            glowMask.shader = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<TerraDye>());
                        }
                        drawinfo.DrawDataCache.Add(glowMask);
                    }
                    break;
                }
        }
    }

    private static void DrawClawsOnPlayer_PreHardmode(PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (!HasClawsSelectedAndLoaded(player, out ClawsBaseItem clawsBaseItem, out WeaponOverlayAttribute weaponAttribute, out Asset<Texture2D> texture, out Asset<Texture2D> glowTexture)) {
            return;
        }

        if (clawsBaseItem.IsHardmodeClaws) {
            return;
        }

        float offsetX = (int)(drawInfo.Position.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2),
              offsetY = (int)(drawInfo.Position.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f);
        Vector2 offset = new Vector2(offsetX, offsetY) + drawInfo.drawPlayer.bodyFrame.Size() / 2f;
        Vector2 drawPosition = drawInfo.drawPlayer.bodyPosition + offset;
        Color immuneAlphaPure = drawInfo.drawPlayer.GetImmuneAlphaPure(weaponAttribute.Hex != null ? Helper.FromHexRgb(weaponAttribute.Hex.Value) : drawInfo.colorArmorBody, drawInfo.shadow);
        immuneAlphaPure *= drawInfo.drawPlayer.stealth;
        DrawData drawData = new(texture.Value, drawPosition - Main.screenPosition, player.bodyFrame, immuneAlphaPure, player.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect);
        drawInfo.DrawDataCache.Add(drawData);
    }

    private static bool HasClawsSelectedAndLoaded(Player player, out ClawsBaseItem clawsBaseItem, out WeaponOverlayAttribute weaponAttribute, out Asset<Texture2D> texture, out Asset<Texture2D> glowTexture) {
        Item item = player.GetSelectedItem();
        clawsBaseItem = null!;
        weaponAttribute = null!;
        texture = null!;
        glowTexture = null!;
        if (item.IsEmpty() || !item.IsModded(out ModItem modItem)) {
            return false;
        }

        if (modItem is not ClawsBaseItem clawsBaseItem2) {
            return false;
        }

        weaponAttribute = item.GetAttribute<WeaponOverlayAttribute>();
        if (weaponAttribute == null || weaponAttribute.WeaponType != WeaponType.Claws) {
            return false;
        }

        texture = _clawsOutfitTextures[GetItemNameForTexture(item)]!;
        if (texture?.IsLoaded != true) {
            return false;
        }

        clawsBaseItem = clawsBaseItem2;

        AutoloadGlowMaskAttribute? glowMaskAttribute = clawsBaseItem?.GetType().GetAttribute<AutoloadGlowMaskAttribute>();
        if (glowMaskAttribute is not null) {
            glowTexture = _clawsOutfitGlowTextures[GetItemNameForTexture(item)]!;
            if (glowTexture?.IsLoaded != true) {
                return false;
            }
        }

        return true;
    }
}
