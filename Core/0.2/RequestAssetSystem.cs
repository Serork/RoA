using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Core;

sealed class RequestAssetSystem : ILoadable {
    private static Dictionary<Type, Asset<Texture2D>?> _assets = [];

    void ILoadable.Load(Mod mod) {
        if (Main.dedServ) {
            return;
        }


    }

    void ILoadable.Unload() {
        _assets.Clear();
    }
}
