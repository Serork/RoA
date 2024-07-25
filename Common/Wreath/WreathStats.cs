using System;

using Terraria.ModLoader;

namespace RoA.Common.Wreath;

sealed class WreathStats : ModPlayer {
    private ushort _currentResource;

    public ushort MaxResource { get; private set; } = 100;
    public ushort ExtraResource { get; private set; } = 0;

    public ushort CurrentResource {
        get => _currentResource;
        private set {
            _currentResource = Math.Min(_currentResource, TotalResource);
        } 
    }

    public ushort TotalResource => (ushort)(MaxResource + ExtraResource);
    public float Progress => (float)CurrentResource / TotalResource;
}
