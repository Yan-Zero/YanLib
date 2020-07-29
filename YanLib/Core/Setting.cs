using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityUIKit.GameObjects;
using YanLib.Core;
using YanLib.DataManipulator;

namespace YanLib
{
    /// <summary>
    /// Mod设置类
    /// </summary>
    internal class Settings
    {

        /// <summary>
        /// 快捷键设置
        /// </summary>
        public HotkeyConfig Hotkey;

        /// <summary>
        /// 快捷键设置类
        /// </summary>
        public class HotkeyConfig
        {
            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="Config"></param>
            public void Init(ConfigFile Config)
            {
                OpenUI = Config.Bind("Hotkey", "OpenUI", new KeyboardShortcut(KeyCode.F5, new KeyCode[] { KeyCode.LeftControl, KeyCode.LeftShift }), "打开窗口");
            }

            /// <summary>
            /// 打开/关闭 UI 的快捷键
            /// </summary>
            public ConfigEntry<KeyboardShortcut> OpenUI;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        public void Init(ConfigFile config)
        {
            this.Config = config;

            Hotkey = new HotkeyConfig();
            Hotkey.Init(this.Config);
        }

        public void Save()
        {
            RuntimeConfig.SaveConfig();
            Config.Save();
        }

        public ConfigFile Config { get; private set; }
    }
}