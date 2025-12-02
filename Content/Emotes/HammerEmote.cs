using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace RoA.Content.Emotes;

sealed class HammerEmote : ModEmoteBubble, IRequestAssets {
    private int _frameCounter;

    public enum HammerEmoteRequstedTextureType : byte {
        Bubble,
        Hammer,
        Border
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)HammerEmoteRequstedTextureType.Bubble, "Terraria/Images/Extra_" + (short)48),
         ((byte)HammerEmoteRequstedTextureType.Hammer, EmoteBubbleLoader.GetEmoteBubble(ModContent.EmoteBubbleType<HammerEmote>()).Texture),
         ((byte)HammerEmoteRequstedTextureType.Border, "Terraria/Images/UI/EmoteBubbleBorder")];

    public override void SetStaticDefaults() {
        AddToCategory(EmoteID.Category.Items);
    }

    public override bool UpdateFrameInEmoteMenu(ref int frameCounter) {
        if (++_frameCounter >= 60) {
            _frameCounter = 0;
        }

        return false;
    }

    public override bool UpdateFrame() {
        if (++_frameCounter >= 8 * 4) {
            _frameCounter = 0;
        }

        return false;
    }

    public override bool PreDrawInEmoteMenu(SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) {
        if (AssetInitializer.TryGetRequestedTextureAssets<HammerEmote>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            Texture2D bubbleTexture = indexedTextureAssets[(byte)HammerEmoteRequstedTextureType.Bubble].Value,
                      emoteTexture = indexedTextureAssets[(byte)HammerEmoteRequstedTextureType.Hammer].Value,
                      borderTexture = indexedTextureAssets[(byte)HammerEmoteRequstedTextureType.Border].Value;
            Rectangle value = bubbleTexture.Frame(8, 39, 1, 0);
            Color white = Color.White;
            Color color = Color.Black;
            spriteBatch.Draw(bubbleTexture, position, value, white, 0f, origin, 1f, SpriteEffects.None, 0f);
            int num = _frameCounter / 15;
            spriteBatch.Draw(emoteTexture, position, emoteTexture.Frame(4, 1, num, 0), white, 0f, origin, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(borderTexture, position - Vector2.One * 2f, null, color, 0f, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) {
        if (AssetInitializer.TryGetRequestedTextureAssets<HammerEmote>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            Texture2D bubbleTexture = indexedTextureAssets[(byte)HammerEmoteRequstedTextureType.Bubble].Value,
                      emoteTexture = indexedTextureAssets[(byte)HammerEmoteRequstedTextureType.Hammer].Value,
                      borderTexture = indexedTextureAssets[(byte)HammerEmoteRequstedTextureType.Border].Value;
            Rectangle value = bubbleTexture.Frame(8, 39, 1, 0);
            Color white = Color.White;
            Color color = Color.Black;
            bool flag = EmoteBubble.lifeTime < 6 || EmoteBubble.lifeTimeStart - EmoteBubble.lifeTime < 6;
            Rectangle value2 = bubbleTexture.Frame(8, EmoteBubble.EMOTE_SHEET_VERTICAL_FRAMES, (!flag) ? 1 : 0);
            spriteBatch.Draw(bubbleTexture, position, value2, white, 0f, origin, 1f, spriteEffects, 0f);
            if (!flag) {
                int num = _frameCounter / 8;
                spriteBatch.Draw(emoteTexture, position, emoteTexture.Frame(4, 1, num, 0), white, 0f, origin, 1f, spriteEffects, 0f);
                //spriteBatch.DrawSelf(borderTexture, position - Vector2.One * 2f, null, color, 0f, origin, 1f, spriteEffects, 0f);
            }
            return false;
        }

        return base.PreDraw(spriteBatch, texture, position, frame, origin, spriteEffects);
    }
}
