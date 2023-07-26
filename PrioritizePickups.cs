using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.UI;
using Il2CppVampireSurvivors;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static UnityEngine.Random;

namespace PreferPickups
{
    public static class ModInfo
    {
        public const string Name = "Prioritize Pickup";
        public const string Description = "Prioritizes item pickups over leveling.";
        public const string Author = "Nick";
        public const string Company = "Nick's Box Emporium";
        public const string Version = "0.1.0";
        public const string Download = "";
    }
    public class ConfigData
    {
        public bool Enabled { get; set; }
    }

    public class PrioritizePickup : MelonMod
    {
        static readonly string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "com", "Nick");
        static readonly string filePath = Path.Combine(configFolder, "PrioritizePickup.json");

        static readonly string enabledKey = "Enabled";
        static bool enabled;

        static void UpdateDebug(bool value) => UpdateEnabled(value);
        static bool scaleSettingAdded = false;
        static Action<bool> scaleSettingChanged = UpdateDebug;

        public override void OnInitializeMelon()
        {
            ValidateConfig();
        }

        // Listens and logs whenever character levels up pre/post
        [HarmonyPatch(typeof(MainGamePage), "LevelUp")]
        public class MainGamePage_LevelUp
        {

            //[HarmonyPrefix]
            //public static bool Prefix(MainGamePage __instance)
            //{
            //    Int64 level = Int64.Parse(__instance._LevelText.text.Remove(0, 3));
            //    if (enabled)
            //        Melon<PrioritizePickup>.Logger.Msg($"MainGamePage LevelUp! - Prefix: {level}");
            //    return true;
            //}

            //[HarmonyPostfix]
            //public static void Postfix(MainGamePage __instance)
            //{
            //    Int64 level = Int64.Parse(__instance._LevelText.text.Remove(0, 3));
            //    if (enabled)
            //        Melon<PrioritizePickup>.Logger.Msg($"MainGamePage LevelUp! - Postfix: {level}");
            //}
        }

        // OnEnter of ItemFound gui and Receive of the item?
        [HarmonyPatch(typeof(GameStateItemFound))]
        public class GameStateItemFound_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Receive")]
            public static bool Receive_Prefix(GameStateItemFound __instance)
            {
                if (enabled)
                {
                    Melon<PrioritizePickup>.Logger.Msg("Receive_Prefix");
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnEnter")]
            public static bool OnEnter_Prefix(GameStateItemFound __instance)
            {
                if (enabled)
                {
                    Melon<PrioritizePickup>.Logger.Msg("OnEnter_Prefix");
                }
                return true;
            }
        }

        // Listens for state change
        [HarmonyPatch(typeof(StateMachine))]
        public class StateMachine_Patch
        {

            [HarmonyPrefix]
            [HarmonyPatch("FireEvent")]
            public static bool FireEvent_Prefix(StateMachine __instance, string eventStr)
            {
                if (enabled)
                {
                    // ITEM_FOUND is sent when the gui pops up, not when the item is destroyed.
                    // SELECT_ARCANA is sent when purple chest gui pops up.
                    Melon<PrioritizePickup>.Logger.Msg($"StateMachine Fired! - Prefix: {eventStr}");
                
                    //// All the allowed Transitions from current state?
                    //Melon<PrioritizePickup>.Logger.Msg($"Transition map size: {__instance.currentTransitionMap.Count}");
                    //foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, Il2CppSystem.Type> entry in __instance.currentTransitionMap)
                    //{
                    //    Melon<PrioritizePickup>.Logger.Msg($"<{entry.Key}>: <{entry.Value.FullName}>");
                    //}

                    ////Dictionary<Il2CppSystem.Type, StateMachineState> instanceCache
                    //// Some sort of history?
                    //Melon<PrioritizePickup>.Logger.Msg($"instanceCache size: {__instance.instanceCache.Count}");
                    //foreach (Il2CppSystem.Collections.Generic.KeyValuePair<Il2CppSystem.Type, StateMachineState> entry in __instance.instanceCache)
                    //{
                    //    Melon<PrioritizePickup>.Logger.Msg($"<{entry.Key.FullName}>: <{entry.Value.ToString()}>");
                    //}

                    ////overallTransitionMap
                    //Melon<PrioritizePickup>.Logger.Msg($"overallTransitionMap size: {__instance.overallTransitionMap.Count}");
                    //foreach (Il2CppSystem.Collections.Generic.KeyValuePair<Il2CppSystem.Type, Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Type>> entry in __instance.overallTransitionMap)
                    //{
                    //    Melon<PrioritizePickup>.Logger.Msg($"{entry.Key.FullName}:");
                    //    foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, Il2CppSystem.Type> inner in entry.Value)
                    //    {
                    //        Melon<PrioritizePickup>.Logger.Msg($"\t{inner.Key}: {inner.Value.FullName}");
                    //    }
                    //    Melon<PrioritizePickup>.Logger.Msg("");
                    //}

                }
                return true;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("FireEvent")]
            //public static void FireEvent_Postfix(StateMachine __instance)
            //{
            //    if (enabled)
            //        Melon<PrioritizePickup>.Logger.Msg($"StateMachine Fired! - Postfix");
            //}

            //[HarmonyPrefix]
            //[HarmonyPatch("GoToState")]
            //public static void GoToState_Prefix(StateMachine __instance, Il2CppSystem.Type state)
            //{
            //    if (enabled)
            //        Melon<PrioritizePickup>.Logger.Msg($"GoToState called - Prefix: {state.ToString()}");

            //    if (enabled)
            //    {
            //        Melon<PrioritizePickup>.Logger.Msg($"Transition map size: {__instance.currentTransitionMap.Count}");
            //        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, Il2CppSystem.Type> entry in __instance.currentTransitionMap)
            //        {
            //            Melon<PrioritizePickup>.Logger.Msg($"<{entry.Key}>: <{entry.Value.ToString()}>"); 
            //        }
            //    }
            //}

            //[HarmonyPrefix]
            //[HarmonyPatch("AddStateTransition")]
            //public static void AddStateTransition_Prefix<TFromState, TToState>(StateMachine __instance, string eventStr)
            //{
            //    if (enabled)
            //        Melon<PrioritizePickup>.Logger.Msg($"AddStateTransition called - Prefix: {eventStr}");
            //}
        }

        // D:\SteamLibrary\steamapps\common\Vampire Survivors\MelonLoader\Il2CppAssemblies
        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.BuildGameplayPage))]
        static class PatchBuildGameplayPage
        {
            static void Postfix(OptionsController __instance)
            {
                if (!scaleSettingAdded) __instance.AddTickBox("PrioritizePickup", enabled, scaleSettingChanged, false);
                scaleSettingAdded = true;
            }
        }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.AddVisibleJoysticks))]
        static class PatchAddVisibleJoysticks { static void Postfix() => scaleSettingAdded = false; }
        private static void UpdateEnabled(bool value)
        {
            ModifyConfigValue(enabledKey, value);
            enabled = value;
        }

        private static void ValidateConfig()
        {
            try
            {
                if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
                if (!File.Exists(filePath)) File.WriteAllText(filePath, JsonConvert.SerializeObject(new ConfigData { }, Formatting.Indented));

                LoadConfig();
            }
            catch (Exception ex) { Melon<PrioritizePickup>.Logger.Msg($"Error: {ex}"); }
        }

        private static void ModifyConfigValue<T>(string key, T value)
        {
            string file = File.ReadAllText(filePath);
            JObject json = JObject.Parse(file);

            if (!json.ContainsKey(key)) json.Add(key, JToken.FromObject(value));
            else
            {
                Type type = typeof(T);
                JToken newValue = JToken.FromObject(value);

                if (type == typeof(string)) json[key] = newValue.ToString();
                else if (type == typeof(int)) json[key] = newValue.ToObject<int>();
                else if (type == typeof(bool)) json[key] = newValue.ToObject<bool>();
                else { Melon<PrioritizePickup>.Logger.Msg($"Unsupported type '{type.FullName}'"); return; }
            }

            string finalJson = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(filePath, finalJson);
        }

        private static void LoadConfig() => enabled = JObject.Parse(File.ReadAllText(filePath) ?? "{}").Value<bool>(enabledKey);
    }
}
