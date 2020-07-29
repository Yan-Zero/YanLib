using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityUIKit.Core;
using UnityUIKit.GameObjects;
using YanLib.Core;

namespace YanLib.UI
{
    /// <summary>
    /// 设置界面的 UI
    /// </summary>
    internal static partial class SettingUI
    {
        /// <summary>
        /// 初始化UI
        /// </summary>
        public static void PrepareGUI()
        {
            RuntimeConfig.UI_Config.overlay = new Container.CanvasContainer()
            {
                Name = "YanLib.Canvas",
                Group =
                {
                    Padding = { 0 },
                },
                Children =
                {
                    (RuntimeConfig.UI_Config.windows = new TaiwuWindows()
                    {
                        Name = "YanLib.Windows",
                        Title = $"YanLib {YanLib.version}",
                        Direction = Direction.Vertical,
                        Spacing = 10,
                        Group =
                        {
                            ChildrenAlignment = TextAnchor.UpperCenter,
                        },
                        Children =
                        {
                            RuntimeConfig.UI_Config.SettingUIScroll
                        },
                        Element =
                        {
                            PreferredSize = { 1400, 1000 }
                        },
                    }),
                }
            };
        }
    }
}