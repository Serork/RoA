using RoA.Common.Items;
using RoA.Core.Defaults;

namespace RoA.Content.Items;

sealed class BulbIcon : ItemIcon {
    public override void SetDefaults() {
        Item.SetSizeValues(20, 32);
    }
}
