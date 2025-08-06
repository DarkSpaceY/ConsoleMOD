using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SFS.Career;

namespace ConsoleMOD.Patches
{
    [HarmonyPatch(typeof(AllowPerMode), nameof(AllowPerMode.Allowed), MethodType.Getter)]
    public static class AllowPerModePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result)
        {
            // Override the original result to always allow regardless of game mode
            __result = true;
        }
    }
}
