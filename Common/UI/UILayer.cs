using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.UI;

// aequus
/// <summary>
/// Used to simplify UI implementation. Only loads in singleplayer or on multiplayer clients.
/// </summary>
/// <param name="Name">Name of this layer.</param>
/// <param name="InsertLayer">Name of the layer to search the index of.</param>
/// <param name="ScaleType">Scale Type of this layer.</param>
/// <param name="InsertOffset">Index offset for inserting the layer. Defaults to 1 (Inserts after layer)</param>
[Autoload(Side = ModSide.Client)]
abstract class UILayer(string Name, string InsertLayer, InterfaceScaleType ScaleType, int InsertOffset = 1)
    : GameInterfaceLayer($"{RoA.ModName}: " + Name, ScaleType), ILoadable, IPostSetupContent {
    public readonly string InsertLayer = InsertLayer;
    public readonly int InsertOffset = InsertOffset;

    public virtual bool IsLoadingEnabled(Mod mod) {
        return true;
    }
    public virtual void OnLoad() { }
    public virtual void PostSetupContent() { }
    public virtual void OnUnload() { }

    public virtual void OnClearWorld() { }
    public virtual void OnPreUpdatePlayers() { }
    public virtual bool OnUIUpdate(GameTime gameTime) {
        return true;
    }

    protected virtual void OnActivate() { }
    protected virtual void OnDeactivate() { }

    /// <summary>
    /// Activates the UI Layer.
    /// </summary>
    public void Activate() {
        if (Active) {
            return;
        }

        OnActivate();
        ModContent.GetInstance<UILayersSystem>()._activeInterfaces.AddLast(this);
        Active = true;
    }

    /// <summary>
    /// Deactivates the UI Layer. This does not instantly remove the layer, instead it will be removed on the next ui update.
    /// </summary>
    public void Deactivate() {
        if (!Active) {
            return;
        }

        OnDeactivate();
        Active = false;
    }

    public void Load(Mod mod) {
        OnLoad();
        Active = false;
        UILayersSystem.Register(this);
    }

    void IPostSetupContent.PostSetupContent() {
        PostSetupContent();
    }

    public void Unload() {
        OnUnload();
    }
}

/// <summary>
/// Only loads in singleplayer or on multiplayer clients.
/// </summary>
[Autoload(Side = ModSide.Client)]
sealed class UILayersSystem : ModSystem {
    public override void ClearWorld() {
        foreach (var i in _userInterfaces) {
            i.OnClearWorld();
        }
    }

    public override void PreUpdatePlayers() {
        foreach (var i in _onPreUpdatePlayers) {
            i.OnPreUpdatePlayers();
        }
    }

    public override void UpdateUI(GameTime gameTime) {
        if (_activeInterfaces.Count <= 0) {
            return;
        }

        LinkedListNode<UILayer> node = _activeInterfaces.First;

        do {
            if (!node.Value.Active || !node.Value.OnUIUpdate(gameTime)) {
                node.Value.Active = false;
                _activeInterfaces.Remove(node);
            }
        }
        while ((node = node.Next) != null);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        foreach (var i in _activeInterfaces) {
            int index = layers.FindIndex((l) => l.Name.Equals(i.InsertLayer));
            if (index != -1) {
                layers.Insert(index + i.InsertOffset, i);
            }
        }
    }

    internal static readonly List<UILayer> _userInterfaces = new();
    internal readonly LinkedList<UILayer> _activeInterfaces = new();

    private readonly List<UILayer> _onPreUpdatePlayers = new();

    public static void Register(UILayer layer) {
        _userInterfaces.Add(layer);
    }

    public override void SetStaticDefaults() {
        foreach (UILayer i in _userInterfaces) {
            Type t = i.GetType();
            if (t.HasMethodOverride(nameof(UILayer.OnPreUpdatePlayers))) {
                _onPreUpdatePlayers.Add(i);
            }
        }
    }
}