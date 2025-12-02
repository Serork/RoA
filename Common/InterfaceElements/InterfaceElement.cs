using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

[Autoload(Side = ModSide.Client)]
abstract class InterfaceElement(string name, InterfaceScaleType scaleType) : GameInterfaceLayer(name, scaleType), ILoadable {
    public abstract int GetInsertIndex(List<GameInterfaceLayer> layers);

    public virtual void Load(Mod mod) { }
    public virtual void Unload() { }

    public virtual bool ShouldDrawSelfInMenu() => false;

    public virtual void Update() { }
}

[Autoload(Side = ModSide.Client)]
sealed class InterfaceElementsSystem : ModSystem {
    public static Dictionary<Type, InterfaceElement> InterfaceElements { get; private set; } = [];

    public static IEnumerable<InterfaceElement> GetElements() => InterfaceElements.Values;

    public static void Register(InterfaceElement element) => InterfaceElements.Add(element.GetType(), element);

    public override void Load() {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(InterfaceElement)))) {
            var instance = Activator.CreateInstance(type);
            Register((InterfaceElement)instance);
        }
    }

    public override void Unload() {
        InterfaceElements.Clear();
        InterfaceElements = null!;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        foreach (InterfaceElement element in GetElements()) {
            if (!element.ShouldDrawSelfInMenu() && Main.gameMenu) {
                continue;
            }

            int index = element.GetInsertIndex(layers);
            if (index != -1) {
                layers.Insert(index, element);
            }
        }
    }

    public override void PostUpdatePlayers() {
        foreach (InterfaceElement element in GetElements()) {
            if (element.Active) {
                element.Update();
            }
        }
    }
}

