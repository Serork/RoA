using RoA.Common.Configs;
using RoA.Content.Items.Equipables.Vanity.Developer;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class DropRoADevsItems : ILoadable {
    private static bool _roaDevItemsDropped;

    void ILoadable.Load(Mod mod) {
        On_Player.OpenBossBag += On_Player_OpenBossBag;
        On_Player.TryGettingDevArmor += On_Player_TryGettingDevArmor;
    }

    private void On_Player_TryGettingDevArmor(On_Player.orig_TryGettingDevArmor orig, Player self, IEntitySource source) {
        if (_roaDevItemsDropped) {
            _roaDevItemsDropped = false;
            return;
        }

        orig(self, source);
    }

    private void On_Player_OpenBossBag(On_Player.orig_OpenBossBag orig, Player self, int type) {
        TryGettingRoADevArmor(self, self.GetSource_OpenItem(type));

        orig(self, type);
    }

    private void TryGettingRoADevArmor(Player self, IEntitySource source) {
        bool flag = ModContent.GetInstance<RoAServerConfig>().DropDevSets;
        if (flag || (Main.hardMode && !flag)) {
            if (Main.rand.Next(Main.tenthAnniversaryWorld ? 8 : 16) == 0) {
                _roaDevItemsDropped = true;
                switch (Main.rand.Next(7)) {
                    case 0:
                        self.QuickSpawnItem(source, ModContent.ItemType<PeegeonHood>());
                        self.QuickSpawnItem(source, ModContent.ItemType<PeegeonChestguard>());
                        self.QuickSpawnItem(source, ModContent.ItemType<PeegeonGreaves>());
                        //self.QuickSpawnItem(source, ModContent.ItemType<PeegeonCape>());
                        break;
                    case 1:
                        self.QuickSpawnItem(source, Main.rand.NextBool() ? ModContent.ItemType<SerorkHelmet>() : ModContent.ItemType<SerorkMask>());
                        self.QuickSpawnItem(source, ModContent.ItemType<SerorkBreastplate>());
                        self.QuickSpawnItem(source, ModContent.ItemType<SerorkGreaves>());
                        break;
                    case 2:
                        self.QuickSpawnItem(source, Main.rand.NextBool() ? ModContent.ItemType<Has2rMask>() : ModContent.ItemType<Has2rShades>());
                        self.QuickSpawnItem(source, ModContent.ItemType<Has2rJacket>());
                        self.QuickSpawnItem(source, ModContent.ItemType<Has2rPants>());
                        //self.QuickSpawnItem(source, ModContent.ItemType<EldritchRing>());
                        break;
                    case 3:
                        self.QuickSpawnItem(source, ModContent.ItemType<BRIPEsHelmet>());
                        self.QuickSpawnItem(source, ModContent.ItemType<BRIPEsHeart>());
                        self.QuickSpawnItem(source, ModContent.ItemType<BRIPEsRocketBoots>());
                        break;
                    case 4:
                        self.QuickSpawnItem(source, ModContent.ItemType<NFAHorns>());
                        self.QuickSpawnItem(source, ModContent.ItemType<NFAJacket>());
                        self.QuickSpawnItem(source, ModContent.ItemType<NFAPants>());
                        self.QuickSpawnItem(source, ModContent.ItemType<NFAWings>());
                        break;
                    case 5:
                        self.QuickSpawnItem(source, ModContent.ItemType<cleoMask>());
                        self.QuickSpawnItem(source, ModContent.ItemType<cleoChestguard>());
                        self.QuickSpawnItem(source, ModContent.ItemType<cleoPants>());
                        break;
                    case 6:
                        self.QuickSpawnItem(source, ModContent.ItemType<HereticHood>());
                        self.QuickSpawnItem(source, ModContent.ItemType<HereticChestguard>());
                        self.QuickSpawnItem(source, ModContent.ItemType<HereticPants>());
                        break;
                }
            }
        }
    }

    void ILoadable.Unload() { }
}
