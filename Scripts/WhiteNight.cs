using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace StellaMagicNS
{
    public class WhiteNight : Mob
    {
        public override bool CanMove => false;

        public override bool CanBeDragged
        {
            get
            {
                return false;
            }
        }
    }
}
