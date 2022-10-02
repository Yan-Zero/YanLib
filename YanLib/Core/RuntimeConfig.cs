using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityUIKit.Core;
using UnityUIKit.GameObjects;
using YanLib.ModHelper;

namespace YanLib.Core
{
    /// <summary>
    /// 运行时候的配置
    /// </summary>
    public static class RuntimeConfig
    {
        internal static List<ModHelper.ModHelper> Mods = new List<ModHelper.ModHelper>();
        internal static Dictionary<KeyboardShortcut, Action> KSListener = new Dictionary<KeyboardShortcut, Action>();
        //internal static bool GameLoaded = false;

        static RuntimeConfig()
        {

        }

        internal static class UI_Config
        {
            public static Container.CanvasContainer overlay = null;
            public static TaiwuWindows windows = null;
            public static BaseScroll SettingUIScroll = new BaseScroll()
            {
                Name = "Setting",
                Group =
                {
                    Direction = Direction.Vertical,
                    Spacing = 15,
                    Padding = { 10 },
                    ForceExpandChildWidth = true
                },
            };

            static UI_Config()
            {
                SettingUIScroll.Create(true);
            }
        }

        internal static void Init()
        {
            //List<string> NeedToLoadFile = new List<string>();
            //Stack<string> Dir = new Stack<string>();
            //Dir.Push(BepInEx.Paths.PluginPath);
            //if (Directory.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "Mods")))
            //    Dir.Push(Path.Combine(BepInEx.Paths.GameRootPath, "Mods"));
            //do
            //{
            //    var directory = Dir.Pop();
            //    foreach (var i in Directory.GetFiles(directory))
            //        if(Path.GetExtension(i).ToLower() == ".yaml")
            //            NeedToLoadFile.Add(i);
            //    foreach (var i in Directory.GetDirectories(directory))
            //        Dir.Push(i);
            //} while (Dir.Count > 0);
            //foreach (var i in NeedToLoadFile)
            //{
            //    var folderName = Path.GetFileName(Path.GetDirectoryName(i)).ToLower();
            //    if (folderName == "eventdata" || folderName == "events")
            //    {
            //        try
            //        {
            //            EventHelper.LoadEventDataFromFile(i);
            //        }
            //        catch (Exception ex)
            //        {
            //            YanLib.Logger.LogError("错误文件：" + i);
            //            YanLib.Logger.LogError(ex);
            //        }
            //    }
            //    else if(folderName == "additionalchoices")
            //    {
            //        Dir.Push(i);
            //    }
            //}
            //while (Dir.Count > 0)
            //{
            //    var File = Dir.Pop();
            //    try
            //    {
            //        EventHelper.LoadAdditionalChoicesDataFromFile(File);
            //    }
            //    catch (Exception ex)
            //    {
            //        YanLib.Logger.LogError("错误文件：" + File);
            //        YanLib.Logger.LogError(ex);
            //    }
            //};
        }

        internal static void SaveConfig()
        {
            //YanLib.Settings.Config.Bind("Key", "DataKey-Set", new Dictionary<string, Dictionary<int, int>>(), "已设置的 Key").Value = ActorDataModKeys;
            //YanLib.Settings.Config.Bind("Key", "DataKey-Used", GameUsedDataKey, "被使用的 Key").Value = ActorDataKeyUsed;
            //YanLib.Settings.Config.Bind("Key", "DataKey-Max", 3001, "Key 最大值").Value = MaxKeyIndex;
        }
    }
}
