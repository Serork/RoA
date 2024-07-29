using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Wreath;

sealed class WreathStats : ModPlayer {
    private const float BASEADDVALUE = 0.1f;

    private ushort _currentResource;
    private float _addValue;

    public ushort MaxResource { get; private set; } = 100;
    public ushort ExtraResource { get; private set; } = 0;

    public ushort CurrentResource {
        get => _currentResource;
        private set {
            if (value < 1) {
                value = 0;
            }
            if (value > TotalResource) {
                value = TotalResource;
            }
            _currentResource = value;
        }
    }

    public ushort TotalResource => (ushort)(MaxResource + ExtraResource);
    public float Progress => (float)CurrentResource / TotalResource;
    public float AddValue => BASEADDVALUE + _addValue;

    public ushort AddResource() => (ushort)(AddValue * TotalResource);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (!Player.IsHoldingNatureWeapon()) {
            return;
        }

        CurrentResource += AddResource();
    }
}
