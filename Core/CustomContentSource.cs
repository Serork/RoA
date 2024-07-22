using Microsoft.CodeAnalysis;

using ReLogic.Content;
using ReLogic.Content.Sources;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoA.Core;

sealed class CustomContentSource(IContentSource source) : IContentSource {
    private readonly IContentSource _source = source;

    IContentValidator IContentSource.ContentValidator {
        get => _source.ContentValidator;
        set => _source.ContentValidator = value;
    }

    RejectedAssetCollection IContentSource.Rejections => _source.Rejections;

    IEnumerable<string> IContentSource.EnumerateAssets() {
        return _source.EnumerateAssets().SelectMany(GetRewrittenPaths);
    }

    string IContentSource.GetExtension(string assetName) {
        foreach (string path in GetRewrittenPaths(assetName)) {
            return _source.GetExtension(path) ?? null;
        }

        return null;
    }

    Stream IContentSource.OpenStream(string fullAssetName) {
        foreach (string path in GetRewrittenPaths(fullAssetName)) {
            return _source.OpenStream(path) ?? null;
        }

        return null;
    }

    private List<string> GetRewrittenPaths(string path) {
        List<string> tempPaths = [];
        string from = "Content", to = ResourceManager.TEXTURESPATH;
        tempPaths.Add(path.Replace(from, to));
        return tempPaths;
    }
}
