using Terraria;

namespace RoA.Core.Utility;

static class GoreHelper {
    public static void FadeOutOverTime(Gore gore) {
        if (gore.timeLeft < Gore.goreTime - 30) {
            gore.alpha += 2;
            if (gore.alpha >= 255) {
                gore.alpha = 255;
                gore.active = false;
            }
        }
    }
}
