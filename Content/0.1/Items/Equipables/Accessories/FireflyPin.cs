using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Content.NPCs.Friendly;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class FireflyPinHandler : PlayerDrawLayer {
    private static Asset<Texture2D> glowTexture;

    public override void Load() => glowTexture = ModContent.Request<Texture2D>(GetType().Namespace.Replace(".", "/") + "/FireflyPin_Face_Glow");
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FaceAcc);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        return true;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.drawPlayer.face != EquipLoader.GetEquipSlot(RoA.Instance, nameof(FireflyPin), EquipType.Face)) {
            return;
        }

        if (drawInfo.shadow != 0f)
            return;

        var texture = glowTexture.Value;
        Color color = Color.White;

        float progress = (1f - MathHelper.Clamp(Helper.Wave(0f, 15f, speed: 5f), 0f, 1f));
        Vector2 movementOffset = drawInfo.drawPlayer.bodyFrame.Y > 168 ? drawInfo.drawPlayer.PlayerMovementOffset() : Vector2.Zero;
        drawInfo.DrawDataCache.Add(new DrawData(
            texture,
            (drawInfo.drawPlayer.mount.Active ? drawInfo.drawPlayer.position : drawInfo.Position).Floor() + new Vector2(0f, drawInfo.drawPlayer.gfxOffY) +
            movementOffset - Main.screenPosition
            + Vector2.UnitY * (drawInfo.drawPlayer.gravDir == -1f ? drawInfo.drawPlayer.height / 2 + 6 : 0),
            new Rectangle(0, progress < 0.5f ? drawInfo.drawPlayer.bodyFrame.Height : 0, drawInfo.drawPlayer.bodyFrame.Width, drawInfo.drawPlayer.bodyFrame.Height),
            color * progress,
            0f,
            new Vector2(10),
            1f,
            drawInfo.drawPlayer.direction == -1 ? SpriteEffects.None | SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0
        ));
    }
}

[AutoloadEquip(EquipType.Face)]
sealed class FireflyPin : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Firefly Pin");
        //Tooltip.SetDefault("Keeps Wreath full charge bonuses for 2 seconds after discharging");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 24; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;

        Item.useStyle = 1;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.consumable = true;
        Item.makeNPC = ModContent.NPCType<FireflyMimic>();
        Item.noUseGraphic = true;

        Item.value = Item.sellPrice(0, 3, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<DruidStats>().KeepBonusesForTime += 120f;
}