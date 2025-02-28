using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

[AutoloadEquip(EquipType.Body)]
sealed class FlametrackerJacket : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("12% reduced mana usage\n5% increased magic damage");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 8;

        Item.value = Item.sellPrice(0, 0, 60, 0);
    }

    public override void UpdateEquip(Player player) {
        player.manaCost -= 0.12f;
        player.GetDamage(DamageClass.Magic) += 0.05f;
        Lighting.AddLight(player.Center - new Vector2(6f * player.direction, 4f), new Vector3(194, 44, 44) * 0.0035f);
    }
}

sealed class FlametrackerJacketFlame : PlayerDrawLayer {
    private Asset<Texture2D> _jacketFlameTexture;

    public override void Load()
        => _jacketFlameTexture = ModContent.Request<Texture2D>(ResourceManager.ItemsTextures + "FlametrackerJacket_Flame");

    public override void Unload()
        => _jacketFlameTexture = null;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.CheckArmorSlot(ModContent.ItemType<FlametrackerJacket>(), 1, 11) ||
           drawInfo.drawPlayer.CheckVanitySlot(ModContent.ItemType<FlametrackerJacket>(), 11);

    public override Position GetDefaultPosition()
        => new AfterParent(PlayerDrawLayers.EyebrellaCloud);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || player.dead)
            return;

        Texture2D texture = _jacketFlameTexture.Value;
        Rectangle bodyFrame = player.bodyFrame;
        Color color = new Color(255, 255, 255, 0) * 0.8f * 0.75f;

        ulong speed = (ulong)(player.miscCounter / 5);
        int height = texture.Height / 20;

        for (int i = 0; i < 7; ++i) {
            float shakePointX = Utils.RandomInt(ref speed, -10, 11) * 0.15f;
            float shakePointY = Utils.RandomInt(ref speed, -5, 6) * 0.3f;
            int x = (int)(drawInfo.Position.X + player.width / 2f - Main.screenPosition.X + shakePointX + 1);
            int y = (int)(drawInfo.Position.Y + player.height / 2f - Main.screenPosition.Y + shakePointY - 5);
            DrawData drawData = new DrawData(texture, new Vector2(x, y), new Rectangle?(bodyFrame), color, player.bodyRotation, new Vector2(texture.Width / 2f, height / 2f), 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}

sealed class FlametrackerJacketMask : PlayerDrawLayer {
    private Asset<Texture2D> _jacketMaskTexture;

    public override void Load()
        => _jacketMaskTexture = ModContent.Request<Texture2D>(ResourceManager.ItemsTextures + "FlametrackerMask_Down");

    public override void Unload()
        => _jacketMaskTexture = null;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.CheckArmorSlot(ModContent.ItemType<FlametrackerJacket>(), 1, 11) ||
           drawInfo.drawPlayer.CheckVanitySlot(ModContent.ItemType<FlametrackerJacket>(), 11);

    public override Position GetDefaultPosition()
        => new AfterParent(PlayerDrawLayers.Torso);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || player.dead || player.ZoneUnderworldHeight)
            return;

        Texture2D texture = _jacketMaskTexture.Value;
        Rectangle bodyFrame = player.bodyFrame;
        Color color = drawInfo.colorArmorBody;

        int height = texture.Height / 20;
        int x = (int)(drawInfo.Position.X + player.width / 2.0 - Main.screenPosition.X);
        int y = (int)(drawInfo.Position.Y + player.height / 2.0 - Main.screenPosition.Y - 3);
        DrawData drawData = new DrawData(texture, new Vector2(x, y), new Rectangle?(bodyFrame), color, player.bodyRotation, new Vector2(texture.Width / 2f, height / 2f), 1f, drawInfo.playerEffect, 0);
        drawInfo.DrawDataCache.Add(drawData);
    }
}