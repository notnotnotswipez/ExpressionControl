using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow;
using UnityEngine;

namespace ExpressionControl
{
    [HarmonyPatch(typeof(Mirror), nameof(Mirror.LateUpdate))]
    public class MirrorPatches
    {
        public static Dictionary<int, Il2CppArrayBase<SkinnedMeshRenderer>> skinnedMeshRendererPairs = new Dictionary<int, Il2CppArrayBase<SkinnedMeshRenderer>>();

        public static void Prefix(Mirror __instance) {
            if (__instance.Reflection)
            {
                if (!skinnedMeshRendererPairs.ContainsKey(__instance.Reflection.GetInstanceID()))
                {
                    skinnedMeshRendererPairs.Add(__instance.Reflection.GetInstanceID(), __instance.Reflection.GetComponentsInChildren<SkinnedMeshRenderer>());
                    Il2CppArrayBase<SkinnedMeshRenderer> skinnedMeshRenderers = skinnedMeshRendererPairs[__instance.Reflection.GetInstanceID()];
                    RigManager rigManager = __instance.rigManager;
                    if (rigManager)
                    {
                        Dictionary<string, bool> keyValuePairs = Core.TryGetBlendShapePairFromRigmanager(rigManager);
                        if (keyValuePairs != null)
                        {
                            Core.ApplyBlendshapesToSkinnedMeshRenderers(skinnedMeshRenderers, keyValuePairs);
                        }
                    }
                }
            }
        }
    }
}
