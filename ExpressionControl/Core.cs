using BoneLib;
using BoneLib.BoneMenu;
using ExpressionControl.Messages;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ExpressionControl.Core), "ExpressionControl", "1.0.2", "notnotnotswipez", null)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace ExpressionControl
{
    public class Core : MelonMod
    {
        public static string lastBlendshapeName;
        private static Page mainPage;

        private static bool deletionMode = false;

        public static Dictionary<string, bool> blendshapePairs = new Dictionary<string, bool>();
        public static Dictionary<int, Dictionary<string, bool>> rigmanagerBlendshapePairs = new Dictionary<int, Dictionary<string, bool>>();

        public static MelonPreferences_Category category;
        public static MelonPreferences_Entry<string> savedString;

        public override void OnInitializeMelon()
        {
            // Register the ExpressionControl module
            ModuleManager.RegisterModule<ExpressionControlModule>();

            category = MelonPreferences.CreateCategory("ExpressionControl");
            savedString = category.CreateEntry<string>("SavedString", "");

            blendshapePairs = GetDictionaryFromString(savedString.Value);

            mainPage = Page.Root.CreatePage("Expression Control", Color.yellow);

            RemakeMenu();
        }

        void RemakeMenu()
        {
            mainPage.RemoveAll();
            mainPage.CreateString("Blendshape Name", Color.white, "", (str) =>
            {
                lastBlendshapeName = str;
            });
            mainPage.CreateFunction("+", Color.green, () =>
            {
                blendshapePairs.Add(lastBlendshapeName, false);
                RemakeMenu();
            });

            mainPage.CreateBool("Deletion Mode", Color.red, deletionMode, (b) =>
            {
                deletionMode = b;
            });

            foreach (var keyPair in blendshapePairs)
            {
                mainPage.CreateBool(keyPair.Key, Color.white, keyPair.Value, (b) =>
                {
                    if (deletionMode)
                    {
                        blendshapePairs[keyPair.Key] = false;
                        ApplyBlendshapesToLocalPlayer();
                        blendshapePairs.Remove(keyPair.Key);
                        RemakeMenu();
                        SaveDictToFile();
                        Broadcast();
                        return;
                    }
                    blendshapePairs[keyPair.Key] = b;
                    ApplyBlendshapesToLocalPlayer();
                    SaveDictToFile();
                    Broadcast();
                });
            }

            // Refresh
            mainPage.Remove(new Element[] { });

        }

        public static void Broadcast()
        {
            if (!NetworkInfo.HasServer)
            {
                return;
            }
            ExpressionMessageData expressionMessageData = ExpressionMessageData.Create(blendshapePairs);
            using (var writer = FusionWriter.Create())
            {
                var data = expressionMessageData;
                writer.Write(data);
                using (var message = FusionMessage.ModuleCreate<ExpressionMessage>(writer))
                {
                    MessageSender.SendToServer(NetworkChannel.Reliable, message);
                }
            }
        }

        void SaveDictToFile()
        {
            savedString.Value = GetStringFromDictionary(blendshapePairs);
            category.SaveToFile(false);
        }

        public static string GetStringFromDictionary(Dictionary<string, bool> keyValuePairs)
        {
            int count = keyValuePairs.Count;
            string totalString = count + ";";
            foreach (var kv in keyValuePairs)
            {
                totalString += kv.Key + ";";
                totalString += kv.Value + ";";
            }

            return totalString;
        }

        public static Dictionary<string, bool> GetDictionaryFromString(string targetString)
        {
            if (targetString == "")
            {
                return new Dictionary<string, bool>();
            }

            string[] split = targetString.Split(";");
            int count = int.Parse(split[0]);
            int starting = 1;

            Dictionary<string, bool> dict = new Dictionary<string, bool>();

            for (int i = starting; i < count * 2; i += 2)
            {
                dict.Add(split[i], bool.Parse(split[i + 1]));
            }

            return dict;
        }

        void ApplyBlendshapesToLocalPlayer()
        {
            ApplyBlendshapesToRigmanager(Player.RigManager, blendshapePairs);
        }

        public static void ApplyBlendshapesToRigmanager(RigManager rigManager, Dictionary<string, bool> blendshapePairs)
        {
            if (rigmanagerBlendshapePairs.ContainsKey(rigManager.GetInstanceID()))
            {
                rigmanagerBlendshapePairs[rigManager.GetInstanceID()] = blendshapePairs;
            }
            else
            {
                rigmanagerBlendshapePairs.Add(rigManager.GetInstanceID(), blendshapePairs);
            }

            ApplyBlendshapesToAvatar(rigManager.avatar, blendshapePairs);
            MirrorPatches.skinnedMeshRendererPairs.Clear();
        }

        public static Dictionary<string, bool> TryGetBlendShapePairFromRigmanager(RigManager rigManager)
        {
            if (rigmanagerBlendshapePairs.ContainsKey(rigManager.GetInstanceID()))
            {
                return rigmanagerBlendshapePairs[rigManager.GetInstanceID()];
            }

            return null;
        }

        public static void ApplyBlendshapesToAvatar(Il2CppSLZ.VRMK.Avatar avatar, Dictionary<string, bool> blendshapePairs)
        {
            ApplyBlendshapesToSkinnedMeshRenderers(avatar.GetComponentsInChildren<SkinnedMeshRenderer>(), blendshapePairs);
        }

        public static void ApplyBlendshapesToSkinnedMeshRenderers(Il2CppArrayBase<SkinnedMeshRenderer> skinnedMeshRenderers, Dictionary<string, bool> blendshapePairs)
        {
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                foreach (var blendShapeKeypair in blendshapePairs)
                {
                    int index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeKeypair.Key);
                    if (index != -1)
                    {
                        float targetValue = blendShapeKeypair.Value ? 100f : 0f;
                        skinnedMeshRenderer.SetBlendShapeWeight(index, targetValue);
                    }
                }
            }
        }
    }
}
