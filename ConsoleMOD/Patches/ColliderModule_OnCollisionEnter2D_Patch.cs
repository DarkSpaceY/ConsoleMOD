using System;
using HarmonyLib;
using UnityEngine;
using SFS.Parts.Modules;
using SFS.World;
using SFS.Parts;
using ModLoader.IO;
using Console = ModLoader.IO.Console;

namespace ConsoleMOD.Patches
{
    [HarmonyPatch(typeof(ColliderModule), "OnCollisionEnter2D")]
    public static class ColliderModule_OnCollisionEnter2D_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ColliderModule __instance, Collision2D collision)
        {
            try
            {
                // 使用 AccessTools 反射调用 private 方法 GetPart(out Part part)
                var getPartMethod = AccessTools.Method(typeof(ColliderModule), "GetPart");
                object[] parameters = new object[] { null };
                getPartMethod.Invoke(__instance, parameters);
                Part thisPart = parameters[0] as Part;

                float thisMass = thisPart != null ? thisPart.mass.Value : -1f;

                // 获取另一侧的组件
                ColliderModule otherModule = collision.otherCollider.GetComponent<ColliderModule>();
                float otherMass = -1f;
                if (otherModule != null)
                {
                    Part otherPart = otherModule.GetComponentInParent<Part>();
                    if (otherPart != null)
                        otherMass = otherPart.mass.Value;
                }
                if (ConsoleMOD.Main.entercollsioninfo) {
                    string msg = $"[Collision] This mass: {thisMass:F3}, Other mass: {otherMass:F3}";
                    Console.main.WriteText(msg);
                }
            }
            catch (Exception ex)
            {
                Console.main.WriteText($"[Patch Error] {ex.Message}");
            }
        }
    }
}
