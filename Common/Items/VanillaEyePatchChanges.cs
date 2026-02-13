using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA.Common.Items;

sealed class VanillaEyePatchChanges : GlobalItem {

    private static Asset<Texture2D> _eyePatchTexture = null!;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _eyePatchTexture = ModContent.Request<Texture2D>(ResourceManager.VANILLAPATH + "Armor_Head_97");
    }

    public override void Load() {
        On_ItemSlot.Handle_ItemArray_int_int += On_ItemSlot_Handle_ItemArray_int_int;
        On_ItemSlot.OverrideHover_ItemArray_int_int += On_ItemSlot_OverrideHover_ItemArray_int_int;

        ExtraDrawLayerSupport.PostHeadDrawEvent += ExtraDrawLayerSupport_PostHeadDrawEvent;
    }

    private void ExtraDrawLayerSupport_PostHeadDrawEvent(ref Terraria.DataStructures.PlayerDrawSet drawinfo) {
        Player player = drawinfo.drawPlayer;
        if (!player.GetCommon().IsEyePatchEffectActive) {
            return;
        }

        if (player.GetCommon().IsEyePatchEffectActive_Hidden) {
            return;
        }

        Vector2 vector = Vector2.Zero;
        // Extra patch context.
        if (drawinfo.drawPlayer.mount.Active && drawinfo.drawPlayer.mount.Type == 52)
            vector = new Vector2(28f, -2f);

        vector *= drawinfo.drawPlayer.Directions;
        /*if (drawinfo.drawPlayer.face > 0 && !ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[drawinfo.drawPlayer.face])*/ {
            Vector2 vector2 = Vector2.Zero;
            if (drawinfo.drawPlayer.face == 19)
                vector2 = new Vector2(0f, -6f) * drawinfo.drawPlayer.Directions;

            DrawData item = new DrawData(_eyePatchTexture.Value, vector2 + vector + new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame, drawinfo.colorArmorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
            item.shader = drawinfo.cFace;
            drawinfo.DrawDataCache.Add(item);
        }
    }

    private void On_ItemSlot_OverrideHover_ItemArray_int_int(On_ItemSlot.orig_OverrideHover_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);

        if (context == ItemSlot.Context.EquipAccessory) {
            if (!Main.mouseRight)
                return;

            if (Main.mouseRightRelease) {
                Item item = inv[slot];
                var handler = Main.LocalPlayer.GetCommon();
                handler.CurrentEyePatchMode++;
                if (handler.CurrentEyePatchMode > PlayerCommon.EyePatchMode.BothEyes) {
                    handler.CurrentEyePatchMode = PlayerCommon.EyePatchMode.LeftEye;
                }
            }
        }
    }

    private void On_ItemSlot_Handle_ItemArray_int_int(On_ItemSlot.orig_Handle_ItemArray_int_int orig, Item[] inv, int context, int slot) {
        orig(inv, context, slot);
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.EyePatch;

    public override void SetDefaults(Item entity) {
        entity.headSlot = -1;

        entity.vanity = false;

        entity.accessory = true;
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual) {
        player.GetCommon().IsEyePatchEffectActive = true;
        player.GetCommon().IsEyePatchEffectActive_Hidden = hideVisual;
    }

    public override void RightClick(Item item, Player player) {

    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        var handler = player.GetCommon();
        PlayerCommon.EyePatchMode currentEyePatchMode = handler.CurrentEyePatchMode;
        bool onChosenSide = player.GetViableMousePosition().X < player.GetPlayerCorePoint().X;
        if (currentEyePatchMode == PlayerCommon.EyePatchMode.RightEye) {
            onChosenSide = player.GetViableMousePosition().X > player.GetPlayerCorePoint().X;
        }
        else if (handler.CurrentEyePatchMode == PlayerCommon.EyePatchMode.BothEyes) {
            onChosenSide = true;
        }
        if (item.DamageType == DamageClass.Ranged && onChosenSide) {
            switch (currentEyePatchMode) {
                case PlayerCommon.EyePatchMode.LeftEye:
                case PlayerCommon.EyePatchMode.RightEye:
                    damage = (int)(damage * 1.2f);
                    break;
                case PlayerCommon.EyePatchMode.BothEyes:
                    damage = (int)(damage * 1.1f);
                    break;
            }
        }
    }
}
