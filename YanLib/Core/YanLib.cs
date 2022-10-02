
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityEngine.UI;
using UnityUIKit.Core;
using UnityUIKit.GameObjects;
using YanLib.Core;
using YanLib.UI;

namespace YanLib
{
    /// <summary>
    /// Mod 入口
    /// </summary>
    [TaiwuModdingLib.Core.Plugin.PluginConfig("Yan.Core","Yan", Version)]
    public class YanLib : TaiwuModdingLib.Core.Plugin.TaiwuRemakePlugin
    {
        /// <summary>版本</summary>
        public const string Version = "1.0";
        /// <summary>GUID</summary>
        public const string GUID = "0.0Yan.Lib";
        /// <summary>日志</summary>
        public static ILogger Logger = Debug.unityLogger;
        /// <summary>Yan Lib 的设置</summary>
        internal static Settings Settings = new Settings();
        /// <summary>
        /// Debug 模式
        /// </summary>
        public static bool DebugMode = false;

        /// <summary>
        /// 开/关 UI
        /// </summary>
        public static bool ToggleUI = false;

        /// <summary>
        /// Mod 初始化
        /// </summary>
        public override void Initialize()
        {
            if (RuntimeConfig.Mods.Count > 0)
                return;
            //DontDestroyOnLoad(this);
            //Logger = base.Logger;
            //Settings.Init(Config);
            RuntimeConfig.Init();
            HarmonyPatches.Init();

            ToggleUI = true;

            var Lib = new ModHelper.ModHelper(GUID, "YanLib")
            {
                SettingUI = new Container()
                {
                    Group =
                    {
                        Direction = Direction.Horizontal,
                        Spacing = 3
                    },
                    Element = { PreferredSize = { 0, 50 } },
                },
                OnUpdate = () =>
                {
                    // ESC 键
                    if (Input.GetKeyDown(KeyCode.Escape))
                        if (RuntimeConfig.UI_Config.overlay != null && RuntimeConfig.UI_Config.overlay.Created && RuntimeConfig.UI_Config.overlay.IsActive)
                            ToggleUI = true;

                    if (ToggleUI || Settings.Hotkey.OpenUI.IsDown())
                    {
                        ToggleUI = false;

                        if (RuntimeConfig.UI_Config.overlay != null && RuntimeConfig.UI_Config.overlay.Created)
                        {
                            RuntimeConfig.UI_Config.overlay.RectTransform.SetAsLastSibling();
                            if (RuntimeConfig.UI_Config.overlay.IsActive)
                            {
                                RuntimeConfig.UI_Config.windows.CloseButton.OnClick(RuntimeConfig.UI_Config.windows.CloseButton);
                                Settings.Save();
                            }
                            else
                                RuntimeConfig.UI_Config.overlay.SetActive(true);
                        }
                        else
                        {
                            SettingUI.PrepareGUI();
                            var parent = GameObject.Find("Camera_UIRoot/Canvas/LayerPopUp").transform;
                            RuntimeConfig.UI_Config.overlay.SetParent(parent);
                            RuntimeConfig.UI_Config.overlay.GameObject.layer = 5;
                            RuntimeConfig.UI_Config.overlay.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                            RuntimeConfig.UI_Config.overlay.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                            RuntimeConfig.UI_Config.overlay.RectTransform.anchoredPosition = Vector2.zero;
                        }
                    }
                }
            };

            GameObject.Find("Game").AddComponent<Listener>();

        }

        /// <summary>
        /// 禁用 Mod 的时候调用
        /// </summary>
        public override void Dispose()
        {
        }
    }

    internal class Listener : MonoBehaviour
    {
        private void Update()
        {
            foreach(var k in RuntimeConfig.KSListener)
                if (k.Key.IsDown())
                    k.Value?.Invoke();
            foreach (var i in RuntimeConfig.Mods)
                i.OnUpdate?.Invoke();
        }
    }

    /// <summary>
    /// 用来标记界面的位置的
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MGOInfoAttribute : Attribute
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 位置
        /// </summary>
        public int Order;

        /// <summary>
        /// 界面初始化的类
        /// </summary>
        public Type InitType = null;
        /// <summary>
        /// 初始化的函数名字
        /// </summary>
        public string InitTypeName = null;
    }

    /// <summary>
    /// Patch 用的
    /// </summary>
    public class PatchHandler
    {
        /// <summary>
        /// Parch 的目标 Type
        /// </summary>
        public Type TargetType;
        /// <summary>
        /// Parch 的目标函数
        /// </summary>
        public string TargetMethonName;

        /// <summary>
        /// 前置
        /// </summary>
        public MethodInfo Prefix;
        /// <summary>
        /// 后置
        /// </summary>
        public MethodInfo Postfix;
        /// <summary>
        /// IL修改的
        /// </summary>
        public MethodInfo Transpiler;

        /// <summary>
        /// Patch
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        public void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(TargetType, TargetMethonName), GetHarmonyMethod(Prefix), GetHarmonyMethod(Postfix), GetHarmonyMethod(Transpiler));
        }

        private HarmonyMethod GetHarmonyMethod(MethodInfo method)
        {
            if (method == null)
                return null;
            return new HarmonyMethod(method);
        }

        /// <summary>
        /// 取消 Patch
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <param name="patchType">卸载的 Patch 类型</param>
        public void Unpatch(Harmony harmony, HarmonyPatchType patchType = HarmonyPatchType.All)
        {
            List<MethodInfo> patches = new List<MethodInfo>();

            if ((patchType & HarmonyPatchType.Prefix) == HarmonyPatchType.Prefix)
            {
                if (Prefix != null)
                    patches.Add(Prefix);
            }
            if ((patchType & HarmonyPatchType.Postfix) == HarmonyPatchType.Postfix)
            {
                if (Postfix != null)
                    patches.Add(Postfix);
            }
            if ((patchType & HarmonyPatchType.Transpiler) == HarmonyPatchType.Transpiler)
            {
                if (Transpiler != null)
                    patches.Add(Transpiler);
            }

            if (patches.Count != 0)
                foreach (var i in patches)
                {
                    harmony.Unpatch(AccessTools.Method(TargetType, TargetMethonName), i);
                }
        }

        /// <summary>
        /// Patch 类型
        /// </summary>
        public enum HarmonyPatchType
        {
            /// <summary>
            /// 全部
            /// </summary>
            All = 0b111,
            /// <summary>
            /// 前置
            /// </summary>
            Prefix = 1,
            /// <summary>
            /// 后置
            /// </summary>
            Postfix = 2,
            /// <summary>
            /// Transpiler
            /// </summary>
            Transpiler = 4,
        }
    }
}
