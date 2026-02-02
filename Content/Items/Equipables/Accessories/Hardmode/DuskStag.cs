using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class DuskStag : ModItem {
    private static Asset<Texture2D> _bettleTexture = null!;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _bettleTexture = ModContent.Request<Texture2D>(Texture + "_Player");
    }

    public override void Load() {
        On_PlayerDrawLayers.DrawPlayer_37_BeetleBuff += On_PlayerDrawLayers_DrawPlayer_37_BeetleBuff;
    }

    private void On_PlayerDrawLayers_DrawPlayer_37_BeetleBuff(On_PlayerDrawLayers.orig_DrawPlayer_37_BeetleBuff orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);

        if (drawinfo.shadow != 0f)
            return;

        if (!drawinfo.drawPlayer.GetCommon().IsDuskStagEffectActive_Vanity) {
            return;
        }

        int frame = (byte)(TimeSystem.TimeForVisualEffects * 60f % 3);

        DrawData item;
        SpriteEffects effects = drawinfo.drawPlayer.GetCommon().DustStagDirection.ToSpriteEffects();
        float rotation = drawinfo.drawPlayer.GetCommon().DuskStagVelocity.X * 0.25f;
        for (int j = 0; j < 5; j++) {
            Color colorArmorBody = drawinfo.colorArmorBody;
            float num = (float)j * 0.1f;
            num = 0.5f - num;
            colorArmorBody.R = (byte)((float)(int)colorArmorBody.R * num);
            colorArmorBody.G = (byte)((float)(int)colorArmorBody.G * num);
            colorArmorBody.B = (byte)((float)(int)colorArmorBody.B * num);
            colorArmorBody.A = (byte)((float)(int)colorArmorBody.A * num);
            Vector2 vector = -drawinfo.drawPlayer.GetCommon().DuskStagVelocity * j;
            item = new DrawData(_bettleTexture.Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)(drawinfo.drawPlayer.height / 2)))
                + drawinfo.drawPlayer.GetCommon().DuskStagPosition + vector, 
                new Rectangle(0, _bettleTexture.Height() / 3 * frame + 1, _bettleTexture.Width(), _bettleTexture.Height() / 3 - 2),
                colorArmorBody, rotation, new Vector2(_bettleTexture.Width() / 2, _bettleTexture.Height() / 6), 1f, effects);
            drawinfo.DrawDataCache.Add(item);
        }

        item = new DrawData(_bettleTexture.Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)(drawinfo.drawPlayer.height / 2)))
            + drawinfo.drawPlayer.GetCommon().DuskStagPosition, 
            new Rectangle(0, _bettleTexture.Height() / 3 * frame + 1, _bettleTexture.Width(), _bettleTexture.Height() / 3 - 2),
            drawinfo.colorArmorBody, rotation, new Vector2(_bettleTexture.Width() / 2, _bettleTexture.Height() / 6), 1f, effects);
        drawinfo.DrawDataCache.Add(item);
    }

    public override void SetDefaults() {
        Item.DefaultToAccessory(30, 36);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsDuskStagEffectActive = true;
        player.GetCommon().IsDuskStagEffectActive_Vanity = !hideVisual;

        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.2f;
        player.GetModPlayer<DruidStats>().KeepBonusesForTime += 120f;
    }

    public override void UpdateVanity(Player player) {
        player.GetCommon().IsDuskStagEffectActive_Vanity = true;
    }
}
