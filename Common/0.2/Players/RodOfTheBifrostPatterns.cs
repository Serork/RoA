//using System.Collections.Generic;

//using Terraria;
//using Terraria.ModLoader;

//using static RoA.Content.Projectiles.Friendly.Nature.MagicalBifrostBlock;

//namespace RoA.Common.Players;

//sealed partial class PlayerCommon : ModPlayer {
//    public enum PatternType : byte {
//        Pattern1,
//        Pattern2,
//        Pattern3,
//        Count
//    }

//    public static Dictionary<PatternType, BifrostFigureType[]> Patterns { get; private set; } = new Dictionary<PatternType, BifrostFigureType[]> {
//        { PatternType.Pattern1, new BifrostFigureType[] { 
//            BifrostFigureType.Green2, BifrostFigureType.Blue2, BifrostFigureType.Purple3, BifrostFigureType.Blue1, BifrostFigureType.Purple2, BifrostFigureType.Orange1 
//        } },
//        { PatternType.Pattern2, new BifrostFigureType[] { 

//        } },
//        { PatternType.Pattern3, new BifrostFigureType[] { 

//        } }
//    };

//    public PatternType ActivePattern { get; private set; }
//    public BifrostFigureType ActiveFigureType { get; private set; }
//    public byte CurrentFigureType { get; private set; }

//    public void UseRodOfTheBifrostPattern() {
//        ActivePattern = PatternType.Pattern1;
//        BifrostFigureType[] figureTypes = Patterns[ActivePattern];
//        ActiveFigureType = figureTypes[CurrentFigureType];
//        CurrentFigureType++;
//        if (CurrentFigureType >= figureTypes.Length) {
//            CurrentFigureType = 0;
//            ActivePattern++;
//            if (ActivePattern == PatternType.Count) {
//                ActivePattern = PatternType.Pattern1;
//            }
//        }
//    }

//    public override void Unload() {
//        Patterns.Clear();
//        Patterns = null!;
//    }
//}
