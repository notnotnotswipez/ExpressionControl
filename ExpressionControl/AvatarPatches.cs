using System.Collections;
using BoneLib;
using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using MelonLoader;

namespace ExpressionControl
{
    [HarmonyPatch(typeof(ArtRig), nameof(ArtRig.SetArtOutputAvatar))]
    public class AvatarPatches {
        
        public static void Postfix(ArtRig __instance, PhysicsRig inRig, Avatar avatar)
        {
            MelonCoroutines.Start(WaitForAvatar(inRig.manager, avatar));
        }

        private static IEnumerator WaitForAvatar(RigManager instance, Avatar newAvatar) {
            for (int i = 0; i < 5; i++) {
                yield return null;
            }

            if (instance.GetInstanceID() == Player.RigManager.GetInstanceID()) {
                Core.ApplyBlendshapesToAvatar(newAvatar, Core.blendshapePairs);
                Core.Broadcast();
            }
        }
    }
}
