using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class DevilSkull : ModItem {
    //public override void Load() {
    //    On_PlayerDrawLayers.DrawPlayer_21_Head_TheFace += On_PlayerDrawLayers_DrawPlayer_21_Head_TheFace;
    //}

    //private void On_PlayerDrawLayers_DrawPlayer_21_Head_TheFace(On_PlayerDrawLayers.orig_DrawPlayer_21_Head_TheFace orig, ref PlayerDrawSet drawinfo) {
    //    Player player = drawinfo.drawPlayer;
    //    if (!player.HasEquipped<DevilSkull>(EquipType.Head)) {
    //        orig(ref drawinfo);
    //        return;
    //    }

    //    orig(ref drawinfo);
    //    DrawData item;
    //    item = new DrawData(TextureAssets.Players[drawinfo.skinVar, 1].Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X + 2f * player.direction - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame, drawinfo.colorEyeWhites, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
    //    drawinfo.DrawDataCache.Add(item);
    //    item = new DrawData(TextureAssets.Players[drawinfo.skinVar, 2].Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X + 2f * player.direction - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect, drawinfo.drawPlayer.bodyFrame, drawinfo.colorEyes, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
    //    drawinfo.DrawDataCache.Add(item);
    //    Asset<Texture2D> asset = TextureAssets.Players[drawinfo.skinVar, 15];
    //    if (asset.IsLoaded) {
    //        Vector2 vector = Main.OffsetsPlayerHeadgear[drawinfo.drawPlayer.bodyFrame.Y / drawinfo.drawPlayer.bodyFrame.Height];
    //        vector.Y -= 2f;
    //        vector *= (float)(-drawinfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt());
    //        Rectangle value = asset.Frame(1, 3, 0, (byte)drawinfo.drawPlayer.eyeHelper.CurrentEyeFrame);
    //        DrawData drawData = new DrawData(asset.Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X + 2f * player.direction - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect + vector, value, drawinfo.colorHead, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect);
    //        drawData.shader = drawinfo.skinDyePacked;
    //        item = drawData;
    //        drawinfo.DrawDataCache.Add(item);
    //    }
    //}

    public override void SetStaticDefaults() {
    }

    public override void SetDefaults() {
        int width = 24, height = 26;
        Item.SetSizeValues(width, height);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        player.GetCommon().ApplyDevilSkullSetBonus = true;
    }
}
