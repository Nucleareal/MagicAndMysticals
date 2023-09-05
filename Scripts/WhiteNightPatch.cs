using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace StellaMagicNS
{
    [HarmonyPatch]
    public static class WhiteNightPatch
    {
        [HarmonyPatch(typeof(GameScreen), "Awake")]
        [HarmonyPostfix]
        public static void PatchAwakeScreen()
        {
            Type t = typeof(GameScreen);
            _gameSpeedButtonClicked_field = t.GetField("gameSpeedButtonClicked", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyPatch(typeof(GameScreen), "TimePause")]
        [HarmonyPrefix]
        public static bool PatchTimePauseScreen(GameScreen __instance)
        {
            return InputController.instance.PlayerInput.actions["time_3"].enabled;
        }

        private static FieldInfo _gameSpeedButtonClicked_field;

        [HarmonyPatch(typeof(GameScreen), "Update")]
        [HarmonyPrefix]
        public static bool PatchUpdateScreen(GameScreen __instance)
        {
            if (WorldManager.instance.InAnimation || GameCanvas.instance.ModalIsOpen || InputController.instance.PlayerInput.actions["time_3"].enabled)
            {
                return true;
            }

            var value = (bool)(_gameSpeedButtonClicked_field.GetValue(__instance));

            if(value)
            {
                _gameSpeedButtonClicked_field.SetValue(__instance, false);
                if (WorldManager.instance.SpeedUp == 1f)
                {
                    WorldManager.instance.SpeedUp = 5f;
                }
                else if (WorldManager.instance.SpeedUp == 5f)
                {
                    WorldManager.instance.SpeedUp = 1f;
                }
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(GameCard), "Update")]
        [HarmonyPostfix]
        public static void PatchUpdateCard(GameCard __instance)
        {
            if (!IsWhiteNight(__instance))
            {
                return;
            }

            InputController.instance.PlayerInput.actions["time_pause"].Disable();
            InputController.instance.PlayerInput.actions["pause"].Disable();
            InputController.instance.PlayerInput.actions["time_3"].Disable();

            if (_pulse != null)
            {
                //update
                var scale = 10f * _pulseCount / 6000f;
                _pulseCount++;
                _pulseTransform.localScale = new Vector3(scale, 1, scale);

                var maxRange = scale;
                foreach(var d in WorldManager.instance.AllDraggables)
                {
                    if(d is GameCard c && c.CardData is Combatable cb && !_cards.Contains(d) && IsInRange(__instance, c, maxRange) && c != __instance)
                    {
                        _cards.Add(d);

                        if(cb is Enemy)
                        {
                            cb.Damage(-1);
                            cb.CreateHitText("+1", PrefabManager.instance.HealHitText);
                            AudioManager.me.PlaySound(AudioManager.me.HitMagic, c.Position);
                        }
                        if(cb is BaseVillager)
                        {
                            cb.Damage(1);
                            cb.CreateHitText("-1", PrefabManager.instance.BleedHitText);
                            AudioManager.me.PlaySound(AudioManager.me.HitMagic, c.Position);
                        }
                    }
                }
            }
            if (_counter++ == PULSE_COUNT)
            {
                StellaMagic._Logger.Log($"{__instance.transform.eulerAngles}");

                StellaMagic._Logger.Log("WhiteNight Pulse!");
                _cards.Clear();
                _counter = 0;
                GeneratePulse(__instance);
            }
        }

        [HarmonyPatch(typeof(GameCard), "OnDestroy")]
        [HarmonyPostfix]
        public static void PatchOnDestroyCard(GameCard __instance)
        {
            if(IsWhiteNight(__instance))
            {
                InputController.instance.PlayerInput.actions["time_pause"].Enable();
                InputController.instance.PlayerInput.actions["pause"].Enable();
                InputController.instance.PlayerInput.actions["time_3"].Enable();
                UnityEngine.Object.Destroy(_pulse);
                UnityEngine.Object.Destroy(_pulse.gameObject);
            }
        }

        private static bool IsInRange(GameCard instance, GameCard c, float maxRange)
        {
            var pos_a = instance.Position;
            var pos_b = c.Position;
            var d = Vector3.Distance(pos_a, pos_b);
            return maxRange - .1f <= d && d <= maxRange + .1f;
        }

        private static GameObject _pulse;
        private static Transform _pulseTransform;
        private static int SEGMENTS = 360;
        private static int _pulseCount;

        private static List<Draggable> _cards = new List<Draggable>();

        private static void GeneratePulse(GameCard instance)
        {
            if(_pulse != null)
            {
                Destroy(_pulse);
                Destroy(_pulse.gameObject);
                _pulseCount = 0;
            }

            var transform = instance.transform;
            var parent = transform.parent;

            var radius = 1f;

            _pulse = new GameObject();
            _pulseTransform = _pulse.transform;
            _pulseTransform.SetParent(parent, false);
            _pulseTransform.position = transform.position;
            _pulseTransform.localScale = Vector3.zero;

            var renderer = _pulse.AddComponent<LineRenderer>();
            renderer.startWidth = renderer.endWidth = 0.05f;
            renderer.positionCount = SEGMENTS;
            renderer.loop = true;
            renderer.useWorldSpace = false;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.startColor = renderer.endColor = Color.red;

            var points = new Vector3[SEGMENTS];
            for(var i = 0; i < SEGMENTS; ++i)
            {
                var rad = Mathf.Deg2Rad * (i * 360f / SEGMENTS);
                var x = Mathf.Sin(rad) * radius;
                var z = Mathf.Cos(rad) * radius;
                points[i] = new Vector3(x, transform.position.y, z);
            }
            renderer.SetPositions(points);
        }

        private static void Destroy(GameObject pulse)
        {
            UnityEngine.Object.Destroy(pulse);
        }

        private static int _counter;
        private static int PULSE_COUNT = 6000;

        private static bool IsWhiteNight(GameCard instance)
        {
            return instance.CardData.Id == "stella_magic_white_night";
        }
    }
}
