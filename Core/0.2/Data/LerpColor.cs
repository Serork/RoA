using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Core.Data;

record LerpColor(float LerpValue = 0.005f, bool ShouldBeRandom = false) {
    private bool _lock;

    private Color _lerpColor;
    private Color? _currentColor, _nextColor;

    public float LerpColorProgress { get; private set; }

    public void Randomize() {
        if (_currentColor == null) {
            _currentColor = Color.Yellow;
        }
        if (_nextColor == null) {
            _nextColor = new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256));
        }
    }

    public void Update() {
        LerpColorProgress += LerpValue;
    }

    public Color GetLerpColor(List<Color> from) {
        int colorCount = from.Count;
        for (int i = 0; i < colorCount; i++) {
            float part = 1f / colorCount;
            float min = part * i;
            float max = part * (i + 1);
            if (LerpColorProgress >= min && LerpColorProgress <= max) {
                _lerpColor = Color.Lerp(from[i], from[i == colorCount - 1 ? 0 : (i + 1)], Utils.Remap(LerpColorProgress, min, max, 0f, 1f, true));
            }
        }
        if (ShouldBeRandom && Math.Round(LerpColorProgress, 2) == 0.5f) {
            _currentColor = new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256));
        }
        if (LerpColorProgress > 1f) {
            LerpColorProgress = 0f;
            if (ShouldBeRandom) {
                _nextColor = new Color(Main.rand.Next(256), Main.rand.Next(256), Main.rand.Next(256));
            }
        }
        return _lerpColor;
    }
}
