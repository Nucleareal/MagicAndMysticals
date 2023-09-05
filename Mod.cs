using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StellaMagicNS
{
    public class StellaMagic : Mod
    {
        public static ModLogger _Logger;
        private Harmony _harmony;

        private static bool _isFarmingLoaded;
        private static bool _isMachineLoaded;
        private static bool _isFoodsLoaded;
        private static bool _isMagicLoaded;

        private const string FARMING_MOD_CLASSNAME = "StellaFarming";
        private const string MACHINE_MOD_CLASSNAME = "StellaMachine";
        private const string FOODS_MOD_CLASSNAME = "StellaFoods";
        private const string MAGIC_MOD_CLASSNAME = "StellaMagic";

        private static SetCardBagType StellaMagicBooster;
        private SetCardBagData _StellaMagicBoosterData;

        private static ConfigEntry<float> _BoosterpackDistanceEntry;
        public static float BoosterpackDistance => _BoosterpackDistanceEntry.Value;


        private void Awake()
        {
            Logger.Log("Awaking StellaMagic...");

            _Logger = Logger;

            StellaMagicBooster = EnumHelper.ExtendEnum<SetCardBagType>("StellaMagic");

            _StellaMagicBoosterData = ScriptableObject.CreateInstance<SetCardBagData>();
            _StellaMagicBoosterData.Chances = new List<SimpleCardChance>();
            _StellaMagicBoosterData.SetCardBagType = StellaMagicBooster;

            _harmony = new Harmony("StellaMagicNS.StellaMagic");
            _harmony.PatchAll();
        }

        public override void Ready()
        {
            DetectBridges();

            RegisterRecipes();

            _BoosterpackDistanceEntry = Config.GetEntry<float>("stella_magic_boosterpack_distance", .75f);

            Logger.Log("StellaMagic Ready!");
        }

        private void DetectBridges()
        {
            foreach (var m in ModManager.LoadedMods)
            {
                var mod_classname = m.GetType().Name;

                _isFarmingLoaded |= mod_classname == FARMING_MOD_CLASSNAME;
                _isMachineLoaded |= mod_classname == MACHINE_MOD_CLASSNAME;
                _isFoodsLoaded |= mod_classname == FOODS_MOD_CLASSNAME;
                _isMagicLoaded |= mod_classname == MAGIC_MOD_CLASSNAME;
            }
        }

        private void RegisterRecipes()
        {
            WorldManager.instance.GameDataLoader.SetCardBags.Add(_StellaMagicBoosterData);

            var mainboard = WorldManager.instance.Boards.Where(e => e.Location == Location.Mainland).Single();
            mainboard.BoosterIds.Add("stella_magic_booster");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}