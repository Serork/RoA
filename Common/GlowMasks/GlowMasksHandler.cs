using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Utilities;

using RiseofAges;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.GlowMasks;

// aequus
sealed class GlowMasksHandler : GlobalItem {
    private const string REQUIREMENT = "_Glow";

    private static Dictionary<string, short> _texturePathToGlowMaskID;
    private static Dictionary<int, short> _itemIDToGlowMask;

    public static void AddGlowMask(string texturePath) => _texturePathToGlowMaskID[texturePath] = -1;

    public static bool TryGetID(string texture, out short id) {
        id = -1;
        if (_itemIDToGlowMask != null && _texturePathToGlowMaskID.TryGetValue(texture, out var index)) {
            id = index;
            return true;
        }
        return false;
    }

    public static short GetID(int itemID) {
        if (_itemIDToGlowMask != null && _itemIDToGlowMask.TryGetValue(itemID, out var index)) {
            return index;
        }

        return -1;
    }

    public static short GetID(string texture) {
        if (_texturePathToGlowMaskID != null && _texturePathToGlowMaskID.TryGetValue(texture, out var index)) {
            return index;
        }

        return -1;
    }

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _itemIDToGlowMask = [];
        _texturePathToGlowMaskID = [];
    }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        List<Asset<Texture2D>> masks = TextureAssets.GlowMask.ToList();
        foreach (string name in _texturePathToGlowMaskID.Keys) {
            Asset<Texture2D> customTexture = ModContent.Request<Texture2D>(name, AssetRequestMode.ImmediateLoad);
            customTexture.Value.Name = name;
            _texturePathToGlowMaskID[name] = (short)masks.Count;
            masks.Add(customTexture);
        }

        LoadGlowMasks(masks);

        TextureAssets.GlowMask = masks.ToArray();
    }

    private void LoadGlowMasks(List<Asset<Texture2D>> masks) {
        foreach (ModItem item in RoA.Instance.GetContent<ModItem>()) {
            AutoloadGlowMaskAttribute attribute = item?.GetType().GetAttribute<AutoloadGlowMaskAttribute>();
            if (attribute != null) {
                string modItemTexture = item.Texture;
                if (attribute.CustomGlowMasks != null) {
                    foreach (string customGlowmaskPath in attribute.CustomGlowMasks) {
                        string customTexturePath = modItemTexture + customGlowmaskPath;
                        Asset<Texture2D> customTexture = ModContent.Request<Texture2D>(customTexturePath, AssetRequestMode.ImmediateLoad);
                        customTexture.Value.Name = customTexturePath;
                        _texturePathToGlowMaskID.Add(customTexturePath, (short)masks.Count);
                        masks.Add(customTexture);
                    }

                    continue;
                }

                Asset<Texture2D> texture = ModContent.Request<Texture2D>(modItemTexture + REQUIREMENT, AssetRequestMode.ImmediateLoad);
                texture.Value.Name = modItemTexture;
                masks.Add(texture);

                _texturePathToGlowMaskID.Add(modItemTexture, (short)(masks.Count - 1));

                if (attribute.AutoAssignItemID) {
                    _itemIDToGlowMask.Add(item.Type, (short)(masks.Count - 1));
                }
            }
        }
    }

    public override void Unload() {
        if (TextureAssets.GlowMask != null) {
            TextureAssets.GlowMask = TextureAssets.GlowMask.Where(delegate (Asset<Texture2D> tex) {
                bool? obj;
                if (tex == null) {
                    obj = null;
                }
                else {
                    string name = tex.Name;
                    obj = name != null ? new bool?(!name.StartsWith(RoA.ModName)) : null;
                }
                return obj ?? true;
            }).ToArray();
        }

        _texturePathToGlowMaskID?.Clear();
        _texturePathToGlowMaskID = null;
        _itemIDToGlowMask?.Clear();
        _itemIDToGlowMask = null;
    }

    public override void SetDefaults(Item item) {
        if (item.type >= ItemID.Count) {
            short id = GetID(item.type);
            if (id > 0) {
                item.glowMask = id;

                UsageItemGlowMaskHandler.AddGlowMask(item.ModItem);
            }
        }
    }
}