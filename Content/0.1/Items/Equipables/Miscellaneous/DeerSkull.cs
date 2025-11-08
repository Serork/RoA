using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

using static System.Net.Mime.MediaTypeNames;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head, EquipType.Face)]
sealed class DeerSkull : ModItem {
    private static Asset<Texture2D> _extraTexture = null!, _extraTexture2 = null!;

    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;

        if (!Main.dedServ) {
            _extraTexture = ModContent.Request<Texture2D>(Texture + "_Extra");
            _extraTexture2 = ModContent.Request<Texture2D>(Texture + "_Extra2");
        }
    }

    public override void Load() {
        ExtraDrawLayerSupport.PostHeadDrawEvent += ExtraDrawLayerSupport_PostHeadDrawEvent;
        ExtraDrawLayerSupport.PostFaceAccDrawEvent += ExtraDrawLayerSupport_PostFaceAccDrawEvent;

        WreathHandler.OnSlowChargedEvent += WreathHandler_OnSlowChargedEvent;
    }

    private void WreathHandler_OnSlowChargedEvent(Player player) {
        if (player.IsLocal()) {
            Vector2 targetPosition = player.Top;
            if (Main.netMode != NetmodeID.Server) {
                string tag = "Lightning Effect";
                float strength = Main.rand.NextFloat(15f, 26f) / 3f * 0.175f;
                PunchCameraModifier punchCameraModifier = new PunchCameraModifier(targetPosition, MathHelper.TwoPi.ToRotationVector2(), strength, 10f, 20, 1000f, tag);
                Main.instance.CameraModifiers.Add(punchCameraModifier);
            }
            ProjectileUtils.SpawnPlayerOwnedProjectile<HornsLightning>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_Misc("hornsattack")) {
                Position = targetPosition - Vector2.UnitY * Main.screenHeight / 2,
                Damage = 0,
                KnockBack = 0,
                AI0 = targetPosition.X,
                AI1 = targetPosition.Y,
                AI2 = 2f
            });
        }
    }

    private void ExtraDrawLayerSupport_PostFaceAccDrawEvent(ref PlayerDrawSet drawinfo) {
        DrawHorns(ref drawinfo);
    }

    private void ExtraDrawLayerSupport_PostHeadDrawEvent(ref PlayerDrawSet drawinfo) {
        if (!drawinfo.drawPlayer.HasEquipped<DeerSkull>(EquipType.Face)) {
            DrawHorns(ref drawinfo);
        }
    }

    private bool CanDrawDeerSkullHorns(PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (player.GetCommon().DeerSkullAppearanceProgress <= 0f) {
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

        Texture2D texture = _extraTexture.Value;

        bool deerSkullEquippedAndActivated = !(!player.GetCommon().ApplyDeerSkullSetBonus || !player.GetWreathHandler().ChargedBySlowFill);
        for (int i = 0; i < 2; i++) {
            bool leftHorn = i == 0;
            SpriteFrame hornsFrame = new(1, 3, 0, 0);
            Rectangle clip = hornsFrame.GetSourceRectangle(texture);

            float scale = Ease.QuartOut(player.GetCommon().DeerSkullAppearanceProgress);
            Vector2 origin = player.FacedRight() ? new Vector2(66, 44) : new Vector2(50, 44);
            float rotation = MathHelper.Lerp(MathHelper.PiOver4 * leftHorn.ToDirectionInt() * player.direction, 0f, scale);
            Color baseColor0 = Color.White;
            float progress = Utils.GetLerpValue(0.75f, 1f, scale, true);
            if (!deerSkullEquippedAndActivated) {
                progress = 1f;
            }
            Color baseColor = Color.Lerp(baseColor0.MultiplyRGB(drawinfo.colorArmorHead) with { A = 0 }, drawinfo.colorArmorHead, progress);
            Color baseColor2 = Color.Lerp(baseColor0 with { A = 0 }, new(35, 193, 179), progress);

            Vector2 helmetOffset = drawinfo.helmetOffset;
            Vector2 position = helmetOffset + new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect;
            position -= clip.Centered();
            position.X += 16f;
            position.Y -= 0f;
            if (!player.FacedRight()) {
                position.X += 0f;
            }
            position.X += 13f;
            position += player.MovementOffset();
            position += origin / 2f;
            DrawData item = new(texture, position, clip, baseColor, drawinfo.drawPlayer.headRotation + rotation, origin, scale, drawinfo.playerEffect) {
                shader = drawinfo.cHead
            };
            drawinfo.DrawDataCache.Add(item);

            float hornsOpacity = player.GetCommon().DeerSkullHornsOpacity;
            float hornsBorderOpacity = MathUtils.Clamp01(player.GetCommon().DeerSkullHornsBorderOpacity);
            float hornsBorderOpacity2 = MathUtils.Clamp01(player.GetCommon().DeerSkullHornsBorderOpacity2);
            // gradient
            hornsFrame = new(1, 3, 0, 1);
            clip = hornsFrame.GetSourceRectangle(texture);
            item = new(texture, position, clip, baseColor * hornsBorderOpacity2, drawinfo.drawPlayer.headRotation + rotation, origin, scale, drawinfo.playerEffect) {
                shader = drawinfo.cHead
            };
            drawinfo.DrawDataCache.Add(item);

            // border
            hornsFrame = new(1, 3, 0, 2);
            clip = hornsFrame.GetSourceRectangle(texture);
            Color borderColor = hornsBorderOpacity2 >= 0.925f ? Color.White : baseColor2;
            item = new(texture, position, clip, borderColor * hornsBorderOpacity * 0.75f, drawinfo.drawPlayer.headRotation + rotation, origin, scale, drawinfo.playerEffect) {
                shader = drawinfo.cHead
            };
            drawinfo.DrawDataCache.Add(item);

            borderColor.A /= 2;
            item = new(texture, position + Main.rand.RandomPointInArea(2f) * hornsBorderOpacity2, clip, borderColor * 0.5f * hornsBorderOpacity, drawinfo.drawPlayer.headRotation + rotation, origin, scale, drawinfo.playerEffect) {
                shader = drawinfo.cHead
            };
            drawinfo.DrawDataCache.Add(item);

            texture = _extraTexture2.Value;
        }
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
