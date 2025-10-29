using Microsoft.Xna.Framework;

using RoA.Content.NPCs.Enemies.Tar;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class PerfectMimicDarkenEffect : ModSystem {
    private static float MINDISTANCEFOREFFECT => 160f;
    private static float MAXDISTANCEFOREFFECT => 960f;
    private static float INTENSITY => 0.5f;
    private static float INTENSITYLERPVALUE => TimeSystem.LogicDeltaTime;

    private float _intensity, _intensity2;

    public PerfectMimic PerfectMimic => TrackedEntitiesSystem.GetSingleTrackedNPC<PerfectMimic>().As<PerfectMimic>();
    public bool CanApplyEffect => NPCUtils.AnyNPCs<PerfectMimic>();
    public Color DarkenColor => PerfectMimic.LiquidColor;
    public float Intensity => (INTENSITY * 0.75f + (INTENSITY / 4f * PerfectMimic.TeleportCount)) * PerfectMimic.NPC.Opacity;

    public override void Load() {
        On_ScreenDarkness.Update += On_ScreenDarkness_Update;
        On_ScreenDarkness.DrawBack += On_ScreenDarkness_DrawBack;
        On_ScreenDarkness.DrawFront += On_ScreenDarkness_DrawFront;
    }

    private void On_ScreenDarkness_DrawFront(On_ScreenDarkness.orig_DrawFront orig, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) {
        orig(spriteBatch);

        if (!CanApplyEffect) {
            return;
        }
        float appliedEffectStrength = GetDistanceProgress() * _intensity * 3.75f;
        Color color = DarkenColor * appliedEffectStrength;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), color);
    }

    private void On_ScreenDarkness_DrawBack(On_ScreenDarkness.orig_DrawBack orig, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) {
        orig(spriteBatch);

        if (!CanApplyEffect) {
            return;
        }
        float appliedEffectStrength = GetDistanceProgress() * _intensity * 3.75f;
        Color color = DarkenColor * appliedEffectStrength;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), color);
    }

    private void On_ScreenDarkness_Update(On_ScreenDarkness.orig_Update orig) {
        orig();

        float intensity = NPCUtils.AnyNPCs<PerfectMimic>() ? (Intensity * 2f) : 1f;
        if (!CanApplyEffect) {
            return;
        }
        float appliedEffectStrength = 1f - GetDistanceProgress() * (intensity - 0.25f);
        ref float fade = ref Main.musicFade[Main.curMusic];
        float to = MathUtils.Clamp01(appliedEffectStrength);
        fade = Helper.Approach(fade, to, INTENSITYLERPVALUE);
    }

    public override void ModifyLightingBrightness(ref float scale) {
        if (!CanApplyEffect) {
            _intensity = Helper.Approach(_intensity, 0f, INTENSITYLERPVALUE);
            return;
        }
        float intensity = Intensity * 0.15f;
        _intensity = Helper.Approach(_intensity, intensity, INTENSITYLERPVALUE);
        float appliedEffectStrength = GetDistanceProgress() * _intensity;
        scale -= appliedEffectStrength;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
        if (!CanApplyEffect) {
            _intensity2 = Helper.Approach(_intensity2, 0f, INTENSITYLERPVALUE);
            return;
        }
        float intensity = Intensity * 0.5f;
        _intensity2 = Helper.Approach(_intensity2, intensity, INTENSITYLERPVALUE);
        float appliedEffectStrength = GetDistanceProgress() * _intensity2;
        tileColor = Color.Lerp(tileColor, tileColor.MultiplyRGB(DarkenColor), appliedEffectStrength);
        backgroundColor = Color.Lerp(backgroundColor, backgroundColor.MultiplyRGB(DarkenColor), appliedEffectStrength);
    }

    private float GetDistanceProgress() {
        Player player = Main.LocalPlayer;
        if (!CanApplyEffect) {
            return 1f;
        }
        NPC perfectMimic = TrackedEntitiesSystem.GetSingleTrackedNPC<PerfectMimic>();
        Vector2 center = perfectMimic.Center,
                playerCenter = player.Center;
        float distance = playerCenter.Distance(center);
        float distance2 = (playerCenter + player.DirectionTo(center) * MINDISTANCEFOREFFECT).Distance(center);
        float distanceProgress = MathUtils.Clamp01(distance2 / MAXDISTANCEFOREFFECT * 0.75f);
        if (distance < MINDISTANCEFOREFFECT) {
            distanceProgress = 0f;
        }
        return 1f - distanceProgress;
    }
}
