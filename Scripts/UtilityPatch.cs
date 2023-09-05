using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StellaMagicNS
{
    [HarmonyPatch]
    internal class UtilityPatch
    {
        [HarmonyPatch(typeof(GameCard), "Clicked")]
        [HarmonyPrefix]
        public static bool Clicked(GameCard __instance)
        {
            StellaMagic._Logger.Log($"Clicked Class: {__instance.CardData.GetType().Name}");
            return true;
        }

        [HarmonyPatch(typeof(CreatePackLine), "SetPositions")]
        [HarmonyPrefix]
        private static void SetPositions(CreatePackLine __instance)
        {
            __instance.Distance = StellaMagic.BoosterpackDistance;
        }
    }
}
