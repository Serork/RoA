using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.GlowMasks;
using RoA.Core.Utility;
using RoA.Utilities;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

[AutoloadEquip(EquipType.Head)]
sealed class LothorMask : ModItem, ItemGlowMaskHandler.IAdvancedGlowMaskDraw {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Lothor Mask");
        Item.ResearchUnlockCount = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(Item.headSlot, this);
    }

    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 75);

        Item.vanity = true;
    }

    void ItemGlowMaskHandler.IAdvancedGlowMaskDraw.Draw(ref PlayerDrawSet drawInfo, ref Texture2D texture, ref Color color) {
        float lifeProgress = 1f - drawInfo.drawPlayer.statLife / drawInfo.drawPlayer.statLifeMax2;
        for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            bodyFrame.Width += 2;
            Vector2 helmetOffset = drawInfo.helmetOffset;
            DrawData item = new(texture,
                helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                (float)(drawInfo.drawPlayer.width / 2)),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect +
                Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress,
                bodyFrame, color.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress, drawInfo.drawPlayer.headRotation + Main.rand.NextFloatRange(0.05f) * lifeProgress, drawInfo.headVect, 1f, drawInfo.playerEffect) {
                shader = drawInfo.cHead
            };
            drawInfo.DrawDataCache.Add(item);
        }
    }
}