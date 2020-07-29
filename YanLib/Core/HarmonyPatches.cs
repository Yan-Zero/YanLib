using GameData;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityUIKit.Core;
using UnityUIKit.Core.GameObjects;
using UnityUIKit.GameObjects;

namespace YanLib.Core
{
    static class HarmonyPatches
    {
        public static readonly Harmony harmony = new Harmony(YanLib.GUID);
        public static readonly Type PatchesType = typeof(HarmonyPatches);
        public static readonly Dictionary<string, PatchHandler> PatchHandlers = new Dictionary<string, PatchHandler>
        {
            { "社交关系 ID 修正", new PatchHandler
            {
                TargetType = typeof(DateFile),
                TargetMethonName = "GetNewSocialId",
                Prefix = AccessTools.Method(PatchesType,"DateFile_GetNewSocialId_Prefix")
            }},
            { "先用掉空人物ID", new PatchHandler
            {
                TargetType = typeof(DateFile),
                TargetMethonName = "GetNewActorId",
                Prefix = AccessTools.Method(PatchesType,"DateFile_GetNewActorId_Prefix")
            }},
            { "穿越者的经历修正", new PatchHandler
            {
                TargetType = typeof(DateFile),
                TargetMethonName = "LifeRecordParamToText",
                Prefix = AccessTools.Method(PatchesType,"DateFile_LifeRecordParamToText_Prefix")
            }},
            { "LoadData", new PatchHandler
            {
                TargetType = typeof(DateFile),
                TargetMethonName = "LoadDate",
                Postfix = AccessTools.Method(PatchesType,"DateFile_LoadDate_Postfix")
            }},
            { "SaveData", new PatchHandler
            {
                TargetType = typeof(DateFile.SaveDate),
                TargetMethonName = "FillDate",
                Prefix = AccessTools.Method(PatchesType,"DateFile_SaveDate_FillDate_Prefix")
            }},
            { "NewDate", new PatchHandler
            {
                TargetType = typeof(DateFile),
                TargetMethonName = "NewDate",
                Postfix = AccessTools.Method(PatchesType,"DateFile_NewDate_Postfix")
            }},
            { "GameLoaded", new PatchHandler
            {
                TargetType = typeof(DateFile),
                TargetMethonName = "Awake",
                Postfix = AccessTools.Method(PatchesType,"DateFile_Awake_Postfix")
            }},
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
                    YanLib.Logger.LogError($"{ patch.Key } Patch Failed");
                    YanLib.Logger.LogError(ex);
                }
            }
        }

        /// <summary>
        /// 删除关系时候的逆向优化（
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        private static bool DateFile_GetNewSocialId_Prefix(ref int __result)
        {
            if (RuntimeConfig.EmptySocialId.Count == 0)
                return true;
            __result = RuntimeConfig.EmptySocialId.Pop();
            return false;
        }

        /// <summary>
        /// 删除人物时候的逆向优化（
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        private static bool DateFile_GetNewActorId_Prefix(ref int __result)
        {
            do
            {
                if (RuntimeConfig.EmptyActorId.Count == 0)
                    return true;
                __result = RuntimeConfig.EmptyActorId.Pop();
            }
            while (Characters.HasChar(__result));
            return false;
        }

        /// <summary>
        /// 修正穿越者们的经历
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="charId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <returns></returns>
        private static bool DateFile_LifeRecordParamToText_Prefix(ref string __result, int charId, int type, int value, int partId, int placeId)
        {
            if (type == 0 && value < 0)
            {
                __result = DateFile.instance.SetColoer(20010, RuntimeConfig.TraverserLifeRecordFix[charId][value]);
                return false;
            }
            else
                return true;
        }

        private static void DateFile_LoadDate_Postfix()
        {
            RuntimeConfig.LoadData();
            foreach (var mod in RuntimeConfig.Mods)
                mod.LoadData?.Invoke();
        }
        
        private static void DateFile_SaveDate_FillDate_Prefix()
        {
            RuntimeConfig.SaveData();
            foreach (var mod in RuntimeConfig.Mods)
                mod.SaveData?.Invoke();
        }

        private static void DateFile_NewDate_Postfix()
        {
            RuntimeConfig.NewData();
            foreach (var mod in RuntimeConfig.Mods)
                mod.NewData?.Invoke();
        }

        private static void DateFile_Awake_Postfix()
        {
            RuntimeConfig.GameLoaded = true;
            foreach (var mod in RuntimeConfig.Mods)
                if (mod.SettingUI != null && mod.SettingUI is ManagedGameObject)
                {
                    var ui = mod.SettingUI;
                    mod.SettingUI = ui;
                }

        }
    }
}