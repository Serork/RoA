//using Terraria;
//using Terraria.ModLoader;
//using Terraria.ModLoader.IO;

//namespace RoA.Common.Players;

//sealed partial class PlayerCommon : ModPlayer {
//    public static byte NEWGUIDETEXTCOUNT => 5;

//    public bool[] _shownGuideTexts { get; private set; } = new bool[NEWGUIDETEXTCOUNT];

//    public bool HasNewGuideTextToShow;

//    public string GetKeyText(int index) => RoA.ModName + "newguidetexts" + nameof(_shownGuideTexts) + Utils.Clamp(index, 0, NEWGUIDETEXTCOUNT);

//    public override void SaveData(TagCompound tag) {
//        for (int i = 0; i < NEWGUIDETEXTCOUNT; i++) {
//            tag[GetKeyText(i)] = _shownGuideTexts[i];
//        }
//    }

//    public override void LoadData(TagCompound tag) {
//        for (int i = 0; i < NEWGUIDETEXTCOUNT; i++) {
//            _shownGuideTexts[i] = tag.GetBool(GetKeyText(i));
//        }
//    }
//}
