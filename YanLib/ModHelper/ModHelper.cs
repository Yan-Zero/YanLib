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
using YanLib.Core;

namespace YanLib.ModHelper
{

    /// <summary>
    /// 用来方便 Mod 工作的
    /// </summary>
    public class ModHelper
    {
        private ManagedGameObject ui = null;

        /// <summary>
        /// Mod 的 GUID
        /// </summary>
        public string GUID { private set; get; }
        /// <summary>
        /// Mod 的名字
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// Mod 的设置
        /// </summary>
        public ManagedGameObject SettingUI
        {
            get
            {
                return ui;
            }
            set
            {
                if (RuntimeConfig.UI_Config.SettingUIScroll.ContentChildren.ContainsKey(GUID))
                    RuntimeConfig.UI_Config.SettingUIScroll.ContentChildren.Remove(GUID);
                ui = value;
                //if (!RuntimeConfig.GameLoaded)
                //    return;
                RuntimeConfig.UI_Config.SettingUIScroll.Add(GUID, new BoxAutoSizeModelGameObject()
                {
                    Name = GUID,
                    Group =
                    {
                        Spacing = 3,
                        Direction = Direction.Vertical,
                        ForceExpandChildWidth = true,
                    },
                    SizeFitter =
                    {
                        VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                    },
                    Children =
                    {
                        new TaiwuButton
                        {
                            Name = "Button-Show",
                            Text = Name,
                            OnClick = (Button button) =>
                            {
                                for(int i = 1;i<button.Parent.Children.Count;i++)
                                    button.Parent.Children[i].SetActive(!button.Parent.Children[i].IsActive);
                            },
                            Element =
                            {
                                PreferredSize = { 0 , 50 }
                            },
                            //FontColor = Color.white
                        },
                        new BoxAutoSizeModelGameObject()
                        {
                            Name = GUID,
                            Group =
                            {
                                Padding = { 0 , 50 },
                            },
                            SizeFitter =
                            {
                                VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                            },
                            Children =
                            {
                                ui
                            },
                            DefaultActive = false
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 在 Unity Update 中调用
        /// </summary>
        public Action OnUpdate = null;

        /// <summary>
        /// 新建 ModHelper 实例
        /// </summary>
        /// <param name="_GUID">Mod 的 GUID</param>
        /// <param name="ModName">Mod 显示的名字</param>
        public ModHelper(string _GUID, string ModName)
        {
            GUID = _GUID;
            Name = ModName;
            //if (!RuntimeConfig.ActorDataModKeys.ContainsKey(GUID))
            //    RuntimeConfig.ActorDataModKeys.Add(GUID, new Dictionary<int, int>() { });
            RuntimeConfig.Mods.Add(this);
        }

        /// <summary>
        /// 添加监听的快捷键，全局有效
        /// </summary>
        /// <param name="shortcut">快捷键</param>
        /// <param name="action">回调函数</param>
        /// <returns>false 是添加失败，true 则为添加成功</returns>
        public static bool AddKeyboardShortcutListener(KeyboardShortcut shortcut, Action action)
        {
            // TODO：区分场景
            if (RuntimeConfig.KSListener.ContainsKey(shortcut))
                return false;
            RuntimeConfig.KSListener.Add(shortcut, action);
            return true;
        }
    }
}
