using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.NPCs.Enemies.Bosses.Filament;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace RoA.Common.BossBars;

sealed class FilamentPillarBigProgressBar : ModBossBar {
    private BigProgressBarCache _cache;
    private int _headIndex;
    private bool _shouldDraw;

    private int bossHeadIndex = -1;

    public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
        // Display the previously assigned head index
        if (bossHeadIndex != -1) {
            return TextureAssets.NpcHeadBoss[bossHeadIndex];
        }
        return null;
    }

    public bool ValidateAndCollectNecessaryInfo(ref BigProgressBarInfo info) {
        if (info.npcIndexToAimAt < 0 || info.npcIndexToAimAt > 200)
            return false;

        NPC nPC = Main.npc[info.npcIndexToAimAt];
        if (!nPC.active)
            return false;

        int bossHeadTextureIndex = nPC.GetBossHeadTextureIndex();
        if (bossHeadTextureIndex == -1)
            return false;

        if (!IsPlayerInCombatArea())
            return false;

        if (nPC.ai[2] == 1f)
            return false;

        Utils.Clamp((float)nPC.life / (float)nPC.lifeMax, 0f, 1f);
        _ = (float)(int)MathHelper.Clamp(GetCurrentShieldValue(), 0f, GetMaxShieldValue()) / GetMaxShieldValue();
        _ = 600f * Main.GameModeInfo.EnemyMaxLifeMultiplier * GetMaxShieldValue() / (float)nPC.lifeMax;
        _cache.SetLife(nPC.life, nPC.lifeMax);
        _cache.SetShield(GetCurrentShieldValue(), GetMaxShieldValue());
        _headIndex = bossHeadTextureIndex;
        return true;
    }

    public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) {
        // Here the game wants to know if to draw the boss bar or not. Return false whenever the conditions don't apply.
        // If there is no possibility of returning false (or null) the bar will get drawn at times when it shouldn't, so write defensive code!

        if (ValidateAndCollectNecessaryInfo(ref info)) {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            _shouldDraw = true;
            bossHeadIndex = npc.GetBossHeadTextureIndex();

            life = npc.life;
            lifeMax = npc.lifeMax;
            return true;
        }
        _shouldDraw = false;

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
        if (!_shouldDraw) {
            return false;
        }
        Texture2D value = TextureAssets.NpcHeadBoss[_headIndex].Value;
        Rectangle barIconFrame = value.Frame();
        BigProgressBarHelper.DrawFancyBar(spriteBatch, _cache.LifeCurrent, _cache.LifeMax, value, barIconFrame, _cache.ShieldCurrent, _cache.ShieldMax);

        return false;
    }

    private float GetCurrentShieldValue() => FilamentPillar.ShieldStrengthTowerFilamentTower;
    private float GetMaxShieldValue() => NPC.ShieldStrengthTowerMax;
    private bool IsPlayerInCombatArea() => Main.LocalPlayer.GetCommon().ZoneFilament;
}
