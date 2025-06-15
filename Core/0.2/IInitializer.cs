using Terraria.ModLoader;

namespace RoA.Core;

interface IInitializer : ILoadable {
    void ILoadable.Unload() { }
}
