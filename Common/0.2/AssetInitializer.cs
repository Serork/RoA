using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Terraria.ModLoader;

namespace RoA.Common;

interface IRequestAsset {
    (byte, string)[] IndexedPathsToTexture { get; }
}

sealed class AssetInitializer : IPostSetupContent {
    public static Dictionary<Type, Dictionary<byte, Asset<Texture2D>?>?> TexturesPerType { get; private set; } = [];

    public static bool TryGetTextureAssets<T>(out Dictionary<byte, Asset<Texture2D>?>? indexedTextureAssets) where T : IRequestAsset {
        Type requestedAssetsType = typeof(T);
        if (TexturesPerType!.TryGetValue(requestedAssetsType, out Dictionary<byte, Asset<Texture2D>?>? textures)) {
            if (textures == null) {
                throw new Exception($"{requestedAssetsType} assets are not loaded!");
            }
            foreach (Asset<Texture2D>? requestedTextureAsset in textures.Values) {
                if (requestedTextureAsset?.IsLoaded != true) {
                    throw new Exception($"Asset {nameof(requestedTextureAsset)} is not loaded!");
                }
            }

            indexedTextureAssets = textures;
            return true;
        }

        indexedTextureAssets = null;
        return false;
    }

    public static bool TryGetTextureAsset<T>(byte textureIndex, out Asset<Texture2D>? textureAsset) where T : IRequestAsset {
        Type requestedAssetsType = typeof(T);
        if (TexturesPerType!.TryGetValue(requestedAssetsType, out Dictionary<byte, Asset<Texture2D>?>? textures)) {
            if (textures == null) {
                throw new Exception($"{requestedAssetsType} assets are not loaded!");
            }
            Asset<Texture2D>? requestedTextureAsset = textures[textureIndex];
            if (requestedTextureAsset?.IsLoaded != true) {
                throw new Exception($"Asset {nameof(requestedTextureAsset)} is not loaded!");
            }

            textureAsset = requestedTextureAsset;
            return true;
        }

        textureAsset = null;
        return false;
    }

    void IPostSetupContent.PostSetupContent() {
        LoadRequestedAssets();
    }

    private void LoadRequestedAssets() {
        foreach (ILoadable loadableContent in RoA.Instance.GetContent()) {
            if (loadableContent is not IRequestAsset requestAsset) {
                continue;
            }
            foreach ((byte, string) indexedPath in requestAsset.IndexedPathsToTexture) {
                Type type = loadableContent.GetType();
                if (!TexturesPerType.TryGetValue(type, out Dictionary<byte, Asset<Texture2D>?>? indexedTextureAssets)) {
                    indexedTextureAssets = [];
                    TexturesPerType.Add(type, indexedTextureAssets);
                }
                indexedTextureAssets!.Add(indexedPath.Item1, ModContent.Request<Texture2D>(indexedPath.Item2));
            }
        }
    }
}
