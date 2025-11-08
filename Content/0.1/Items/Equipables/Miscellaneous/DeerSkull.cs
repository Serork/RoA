using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head, EquipType.Face)]
sealed class DeerSkull : ModItem {
    private static Asset<Texture2D> _extraTexture = null!;
    private static bool _isDrawing;

    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;

        if (!Main.dedServ) {
            _extraTexture = ModContent.Request<Texture2D>(Texture + "_Extra");
        }
    }

    public override void Load() {
        ExtraDrawLayerSupport.PostHeadDrawEvent += ExtraDrawLayerSupport_PostHeadDrawEvent;
        ExtraDrawLayerSupport.PostFaceAccDrawEvent += ExtraDrawLayerSupport_PostFaceAccDrawEvent;
    }

    private void ExtraDrawLayerSupport_PostFaceAccDrawEvent(ref PlayerDrawSet drawinfo) {
        if (_isDrawing) {
            _isDrawing = false;
            return;
        }
        DrawHorns(ref drawinfo);
    }

    private void ExtraDrawLayerSupport_PostHeadDrawEvent(ref PlayerDrawSet drawinfo) {
        DrawHorns(ref drawinfo);
    }

    private bool CanDrawDeerSkullHorns(PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.GetCommon().ApplyDeerSkullSetBonus || !player.GetWreathHandler().ChargedBySlowFill) {
            return false;
        }

        if (_extraTexture?.IsLoaded != true) {
            return false;
        }

        if (player.GetFormHandler().IsInADruidicForm) {
            return false;
        }

        if (drawinfo.headOnlyRender) {
            return false;
        }

        return true;
    }

    private void DrawHorns(ref PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!CanDrawDeerSkullHorns(drawinfo)) {
            return;
        }

        _isDrawing = true;

        Texture2D texture = _extraTexture.Value;
        SpriteFrame hornsFrame = new(1, 3, 0, 0);
        Rectangle clip = hornsFrame.GetSourceRectangle(texture);

        Vector2 helmetOffset = drawinfo.helmetOffset;
        Vector2 position = helmetOffset + new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
        position -= clip.Centered();
        position.X += 16f;
        position.Y -= 0f;
        if (!player.FacedRight()) {
            position.X += 8f;
        }
        position += player.MovementOffset();
        DrawData item = new(texture, position, clip, drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect) {
            shader = drawinfo.cHead
        };
        drawinfo.DrawDataCache.Add(item);

        float hornsOpacity = player.GetCommon().DeerSkullHornsOpacity;
        float hornsBorderOpacity = MathUtils.Clamp01(player.GetCommon().DeerSkullHornsBorderOpacity);
        float hornsBorderOpacity2 = MathUtils.Clamp01(player.GetCommon().DeerSkullHornsBorderOpacity2);
        // gradient
        hornsFrame = new(1, 3, 0, 1);
        clip = hornsFrame.GetSourceRectangle(texture);
        item = new(texture, position, clip, drawinfo.colorArmorHead * hornsBorderOpacity2, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect) {
            shader = drawinfo.cHead
        };
        drawinfo.DrawDataCache.Add(item);

        // border
        hornsFrame = new(1, 3, 0, 2);
        clip = hornsFrame.GetSourceRectangle(texture);
        Color borderColor = new(35, 193, 179);
        item = new(texture, position, clip, borderColor * hornsBorderOpacity * 0.75f, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect) {
            shader = drawinfo.cHead
        };
        drawinfo.DrawDataCache.Add(item);

        borderColor.A /= 2;
        item = new(texture, position + Main.rand.RandomPointInArea(2f) * hornsBorderOpacity2, clip, borderColor * 0.5f * hornsBorderOpacity, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect) {
            shader = drawinfo.cHead
        };
        drawinfo.DrawDataCache.Add(item);
    }

    public override void SetDefaults() {
        int width = 32; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.defense = 3;

        Item.rare = ItemRarityID.Orange;

        Item.value = Item.sellPrice(0, 0, 30, 0);

        Item.accessory = true;
    }

    public override void UpdateEquip(Player player) {
        player.GetCritChance(DruidClass.Nature) += 4;

        if (player.GetCommon().PerfectClotActivated) {
            player.GetCommon().ApplyDeerSkullSetBonus = true;
            player.GetWreathHandler().ShouldKeepSlowFill2 = true;
        }
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        player.GetCommon().ApplyDeerSkullSetBonus = true;
        player.GetWreathHandler().ShouldKeepSlowFill2 = true;
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => player.GetCommon().PerfectClotActivated;

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().ApplyDeerSkullSetBonus = true;
        player.GetWreathHandler().ShouldKeepSlowFill2 = true;
    }
}
