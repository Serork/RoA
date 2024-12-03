using RoA.Content.Items;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Players;

interface IDoubleTap {
    void OnDoubleTap(Player player);
}

sealed class DoubleTapHandler : ModPlayer, IPostSetupContent {
    private static readonly List<IDoubleTap> _doubleTapTypes = [];

    void IPostSetupContent.PostSetupContent() {
        IEnumerable<ILoadable> content = Mod.GetContent<ModPlayer>().Concat((IEnumerable<ILoadable>)Mod.GetContent<NatureItem>());
        foreach (ILoadable element in content) {
            if (element is not IDoubleTap) {
                continue;
            }

            _doubleTapTypes.Add(element as IDoubleTap);
        }
    }

    private void KeyDoubleTap(int keyDir) {
        int inputKey = 0;
        if (Main.ReversedUpDownArmorSetBonuses) {
            inputKey = 1;
        }
        if (keyDir == inputKey) {
            foreach (IDoubleTap type in _doubleTapTypes) {
                type.OnDoubleTap(Player);
            }
        }
    }

    public override void SetControls() {
        for (int i = 0; i < 4; i++) {
            bool justPressed = false;
            switch (i) {
                case 0:
                    justPressed = Player.controlDown && Player.releaseDown;
                    break;
                case 1:
                    justPressed = Player.controlUp && Player.releaseUp;
                    break;
                case 2:
                    justPressed = Player.controlRight && Player.releaseRight;
                    break;
                case 3:
                    justPressed = Player.controlLeft && Player.releaseLeft;
                    break;
            }
            if (justPressed && Player.doubleTapCardinalTimer[i] > 0 && Player.doubleTapCardinalTimer[i] < 15) {
                KeyDoubleTap(i);
            }
        }
    }
}
