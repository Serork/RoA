using Terraria.ModLoader;

namespace RoA;

internal interface IPostSetupContent : ILoadable {
    void PostSetupContent();
}
