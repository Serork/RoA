using Terraria.ModLoader;

namespace RoA;

internal interface IPostSetupContent : ILoadable {
    void PostSetupContent();

    void ILoadable.Load(Mod mod) { }
    void ILoadable.Unload() { }
}
