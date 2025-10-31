using Microsoft.Xna.Framework;

using RoA.Common.Players;
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

    private float _intensity, _vignetteIntensity, _colorFadeIntensity, _lerpValue;

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

        _colorFadeIntensity = Helper.Approach(_colorFadeIntensity, (CanApplyEffect ? GetDistanceProgress() : 0f) * _intensity * 3.75f, _lerpValue);
        Color color = DarkenColor * _colorFadeIntensity;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), color);
    }

    private void On_ScreenDarkness_DrawBack(On_ScreenDarkness.orig_DrawBack orig, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) {
        orig(spriteBatch);

        Color color = DarkenColor * _colorFadeIntensity;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-2, -2, Main.screenWidth + 4, Main.screenHeight + 4), new Rectangle(0, 0, 1, 1), color);
    }

    private void On_ScreenDarkness_Update(On_ScreenDarkness.orig_Update orig) {
        orig();

        bool hasNPC = NPCUtils.AnyNPCs<PerfectMimic>();
        _vignetteIntensity = Helper.Approach(_vignetteIntensity, (CanApplyEffect ? GetDistanceProgress() * (PerfectMimic.TransformedEnough || (PerfectMimic.TeleportCount > 1 && PerfectMimic.Talked) ? Helper.Wave(0.5f, 1f, 5f, 0f) : 1f) : 0f) * _intensity * 10f, _lerpValue);
        foreach (Player player in Main.ActivePlayers) {
            VignettePlayer2 localVignettePlayer = player.GetModPlayer<VignettePlayer2>();
            float opacity = 0.5f * _vignetteIntensity * 1f;
            if (Main.netMode != NetmodeID.Server) {
                localVignettePlayer.SetVignette(-100, MathHelper.Lerp(250, 100, opacity), opacity, Color.Lerp(PerfectMimic.LiquidColor, Color.Black, 0.5f) * opacity, player.Center);
            }
        }

        float intensity = hasNPC ? (Intensity * 2f) : 1f;
        if (!CanApplyEffect) {
            return;
        }
        float appliedEffectStrength = 1f - GetDistanceProgress() * (intensity - 0.25f);
        ref float fade = ref Main.musicFade[Main.curMusic];
        float to = MathUtils.Clamp01(appliedEffectStrength);
        fade = Helper.Approach(fade, to, _lerpValue);
    }

    public override void ModifyLightingBrightness(ref float scale) {
        float appliedEffectStrength = _colorFadeIntensity / 3.75f;
        scale -= appliedEffectStrength;
        if (!CanApplyEffect) {
            _intensity = Helper.Approach(_intensity, 0f, _lerpValue);
            _lerpValue = INTENSITYLERPVALUE;
            return;
        }
        _lerpValue = INTENSITYLERPVALUE * (PerfectMimic.Talked ? 0.75f : 1f);
        float intensity = Intensity * 0.15f;
        _intensity = Helper.Approach(_intensity, intensity * (PerfectMimic.Talked ? (PerfectMimic.TransformedEnough ? 1f : 1.5f) : 1f), _lerpValue);
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
        float appliedEffectStrength = _colorFadeIntensity / 3.75f * 3.5f;
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
