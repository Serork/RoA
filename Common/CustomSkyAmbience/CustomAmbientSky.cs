using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Utilities;

using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Ambience;
using Terraria.Graphics.Effects;
using Terraria.Utilities;

namespace RoA.Common.CustomSkyAmbience;

sealed class CustomAmbientSky : CustomSky {
    private delegate SkyEntity EntityFactoryMethod(Player player, int seed);

    private bool _isActive;
    private readonly SlotVector<SkyEntity> _entities = new SlotVector<SkyEntity>(500);
    private int _frameCounter;

    public override void Activate(Vector2 position, params object[] args) {
        _isActive = true;
    }

    public override void Deactivate(params object[] args) {
        _isActive = false;
    }

    private bool AnActiveSkyConflictsWithAmbience() {
        if (!SkyManager.Instance["MonolithMoonLord"].IsActive())
            return SkyManager.Instance["MoonLord"].IsActive();

        return true;
    }

    public override void Update(GameTime gameTime) {
        if (Main.gamePaused)
            return;

        _frameCounter++;
        if (Main.netMode != 2 && AnActiveSkyConflictsWithAmbience() && SkyManager.Instance["CustomAmbience"].IsActive())
            SkyManager.Instance.Deactivate("CustomAmbience");

        foreach (SlotVector<SkyEntity>.ItemPair item in _entities) {
            SkyEntity value = item.Value;
            value.Update(_frameCounter);
            if (!value.IsActive) {
                _entities.Remove(item.Id);
                if (Main.netMode != 2 && _entities.Count == 0 && SkyManager.Instance["CustomAmbience"].IsActive())
                    SkyManager.Instance.Deactivate("CustomAmbience");
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
        if (Main.gameMenu && Main.netMode == 0 && SkyManager.Instance["CustomAmbience"].IsActive()) {
            _entities.Clear();
            SkyManager.Instance.Deactivate("CustomAmbience");
        }

        foreach (SlotVector<SkyEntity>.ItemPair item in _entities) {
            item.Value.Draw(spriteBatch, 3f, minDepth, maxDepth);
        }
    }

    public override bool IsActive() => _isActive;

    public override void Reset() {
    }

    public void Spawn(Player player, CustomSkyEntityType type, int seed) {
        FastRandom random = new FastRandom(seed);
        switch (type) {
            case CustomSkyEntityType.BackwoodsBirdsV:
                _entities.Add(new BackwoodsBirdsPackSkyEntity(player, random));
                break;
            case CustomSkyEntityType.LittleFleder:
                _entities.Add(new LittleFlederSkyEntity(player, random));
                break;
        }

        if (Main.netMode != 2 && !AnActiveSkyConflictsWithAmbience() && !SkyManager.Instance["CustomAmbience"].IsActive())
            SkyManager.Instance.Activate("CustomAmbience", default);
    }
}
