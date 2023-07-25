using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.UI;
using Il2CppVampireSurvivors;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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

        [HarmonyPatch(typeof(GoldFeverUIManager), "OnEnable")]
        public class GoldFeverUIManagerOnEnable_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(GoldFeverUIManager __instance)
            {
                if (!enabled) return true;
                __instance.IntroTween();
                return false;
            }
        }

        // GameStateLevelUp Seems to only trigger when the character isn't maxed out on items.
        [HarmonyPatch(typeof(GameStateLevelUp), "OnEnter")]
        public class GameStateLevelUpOnEnter_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(GameStateLevelUp __instance)
            {
                if (enabled)
                    Melon<PrioritizePickup>.Logger.Msg("Leveling up! - Pre-OnEnter");
                return true;
            }
        }

        /*        // GameStateLevelUp Seems to only trigger when the character isn't maxed out on items.
                [HarmonyPatch(typeof(GameStateLevelUp), "OnEnter")]
                public class GameStateLevelUpOnEnter_Patch
                {

                    [HarmonyPrefix]
                    public static bool Prefix(GameStateLevelUp __instance)
                    {
                        if (enabled)
                            Melon<PrioritizePickup>.Logger.Msg("Leveling up! - Pre-OnEnter");
                        return true;
                    }
                }*/

        // This is to force level them up I think
        [HarmonyPatch(typeof(MainGamePage), "LevelUp")]
        public class MainGamePage_LevelUp
        {

            [HarmonyPrefix]
            public static bool Prefix(MainGamePage __instance)
            {
                Int64 level = Int64.Parse(__instance._LevelText.text.Remove(0, 3));
                // This seems to work? Yep, returns during any level up! Int64.Parse(__instance._LevelText.text.Remove(0,3))
                if (enabled)
                    Melon<PrioritizePickup>.Logger.Msg($"CharacterController LevelUp! - Prefix: {__instance._LevelText.text.Remove(0, 3)}");
                return true;
            }
        }

        // This is to force level them up I think
        [HarmonyPatch(typeof(Il2CppVampireSurvivors.Objects.Characters.CharacterController), "LevelUp")]
        public class CharacterController_LevelUp
        {

            [HarmonyPrefix]
            public static bool Prefix(Il2CppVampireSurvivors.Objects.Characters.CharacterController __instance)
            {
                if (enabled)
                    Melon<PrioritizePickup>.Logger.Msg("CharacterController LevelUp! - Prefix");
                return true;
            }
        }

        // FakePlayerUILevelUp Doesn't seem to do anything
        [HarmonyPatch(typeof(Il2CppVampireSurvivors.App.Objects.FakePlayerUILevelUp), "Update")]
        public class FakePlayerUILevelUpUpdate_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(Il2CppVampireSurvivors.App.Objects.FakePlayerUILevelUp __instance)
            {
                if (enabled)
                    Melon<PrioritizePickup>.Logger.Msg("FakePlayerUILevelUp Update! - Prefix");
                return true;
            }
        }

        [HarmonyPatch(typeof(Il2CppVampireSurvivors.App.Objects.FakePlayerUILevelUp), "UpdateLevelDisplay")]
        public class FakePlayerUILevelUp_UpdateLevelDisplay_Patch
        {

            [HarmonyPrefix]
            public static bool Prefix(Il2CppVampireSurvivors.App.Objects.FakePlayerUILevelUp __instance)
            {
                if (enabled)
                    Melon<PrioritizePickup>.Logger.Msg("FakePlayerUILevelUp_UpdateLevelDisplay_Patch - Prefix");
                return true;
            }
        }

        [HarmonyPatch(typeof(GameStateLevelUp), "OnExit")]
        public class GameStateLevelUpOnExit_Patch
        {

            [HarmonyPrefix]
            public static bool Postfix(GameStateLevelUp __instance)
            {
                if (enabled)
                    Melon<PrioritizePickup>.Logger.Msg("Leveling up! - Post-OnExit");
                return true;
            }
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
