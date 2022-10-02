using GameData;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityUIKit.Core;
using UnityUIKit.Core.GameObjects;
using UnityUIKit.GameObjects;

namespace YanLib.Core
{
    static partial class HarmonyPatches
    {
        public static readonly Harmony harmony = new Harmony(YanLib.GUID);
        public static readonly Type PatchesType = typeof(HarmonyPatches);
        public static readonly Dictionary<string, PatchHandler> PatchHandlers = new Dictionary<string, PatchHandler>
        {
        };

        public static void Init()
        {
            foreach (var patch in PatchHandlers)
            {
                try
                {
                    patch.Value.Patch(harmony);
                }
                catch (Exception ex)
                {
                    //YanLib.Logger.LogError($"{ patch.Key } Patch Failed");
                    //YanLib.Logger.LogError(ex);
                }
            }
        }
    }
}