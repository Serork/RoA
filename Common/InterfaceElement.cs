using RoA.Common.Wreath;

using System;
using System.Collections.Generic;

using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.Common;

[Autoload(Side = ModSide.Client)]
abstract class InterfaceElement(string name, InterfaceScaleType scaleType) : GameInterfaceLayer(name, scaleType), ILoadable {
    public abstract int GetInsertIndex(List<GameInterfaceLayer> layers);

    public virtual void Load(Mod mod) { }
    public virtual void Unload() { }
}

sealed class InterfaceElementsSystem : ModSystem {
    private static readonly Dictionary<Type, InterfaceElement> _interfaceElements = [];

    public static IEnumerable<InterfaceElement> GetElements() => _interfaceElements.Values;

    public static void Register(InterfaceElement element) => _interfaceElements.Add(element.GetType(), element);

    public override void Load() {
        Register(new WreathSystem());
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        foreach (InterfaceElement element in GetElements()) {
            int index = element.GetInsertIndex(layers);
            if (index != -1) {
                layers.Insert(index, element);
            }
        }
    }
}

