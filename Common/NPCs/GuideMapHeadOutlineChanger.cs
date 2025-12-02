using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.World;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility.Vanilla;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace RoA.Common.NPCs;

sealed class GuideMapHeadOutlineChanger : IInitializer {
    private static int _guideHead = -1;

    public static LerpColor LerpColor { get; private set; } = new(0.03f);

    public static Color GetLerpColor() => LerpColor.GetLerpColor([Color.Lerp(Color.Orange, Color.Yellow, 0.75f), Color.Lerp(Color.Orange, Color.Yellow, 0.25f)]);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_contents")]
    public extern static ref NPCHeadDrawRenderTargetContent[] NPCHeadRenderer__contents(NPCHeadRenderer self);

    void ILoadable.Load(Mod mod) {
        On_NPCHeadRenderer.DrawWithOutlines += On_NPCHeadRenderer_DrawWithOutlines;
        On_NPCHeadRenderer.PrepareRenderTarget += On_NPCHeadRenderer_PrepareRenderTarget;

        On_ChatManager.DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_Color_float_Vector2_Vector2_float_float += On_ChatManager_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_Color_float_Vector2_Vector2_float_float;
    }

    private Vector2 On_ChatManager_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_Color_float_Vector2_Vector2_float_float(On_ChatManager.orig_DrawColorCodedStringWithShadow_SpriteBatch_DynamicSpriteFont_string_Vector2_Color_Color_float_Vector2_Vector2_float_float orig, 
        SpriteBatch spriteBatch, ReLogic.Graphics.DynamicSpriteFont font, string text, Vector2 position, Color baseColor, Color shadowColor, float rotation, Vector2 origin, Vector2 baseScale, float maxWidth, float spread) {
        
        if (WorldCommon.HasNewGuideTextToShow && text == Lang.inter[51].Value) {
            shadowColor = Color.Lerp(shadowColor, GetLerpColor(), 0.5f).MultiplyRGB(GetLerpColor());
        }
        
        return orig(spriteBatch, font, text, position, baseColor, shadowColor, rotation, origin, baseScale, maxWidth, spread);
    }

    private void On_NPCHeadRenderer_PrepareRenderTarget(On_NPCHeadRenderer.orig_PrepareRenderTarget orig, NPCHeadRenderer self, GraphicsDevice device, SpriteBatch spriteBatch) {
        orig(self, device, spriteBatch);

        ref NPCHeadDrawRenderTargetContent[] contents = ref NPCHeadRenderer__contents(self);
        if (_guideHead != -1) {
            contents[_guideHead].PrepareRenderTarget(device, spriteBatch);
            _guideHead = -1;
        }
    }

    private void On_NPCHeadRenderer_DrawWithOutlines(On_NPCHeadRenderer.orig_DrawWithOutlines orig, NPCHeadRenderer self, Terraria.Entity entity, int headId, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Color color, float rotation, float scale, Microsoft.Xna.Framework.Graphics.SpriteEffects effects) {
        int headIndexSafe = TownNPCProfiles.GetHeadIndexSafe(entity as NPC);
        if (headIndexSafe == 1) {
            NPCHeadDrawRenderTargetContent? head = NPCHeadRenderer__contents(Main.TownNPCHeadRenderer)[headIndexSafe];
            if (head != null) {
                if (WorldCommon.HasNewGuideTextToShow) {
                    head.UseColor(GetLerpColor());
                    LerpColor.Update();
                    head.Request();
                }
                else {
                    head.UseColor(Color.White);
                    head.Request();
                }
                _guideHead = headIndexSafe;
            }
        }

        orig(self, entity, headId, position, color, rotation, scale, effects);
    }
}
