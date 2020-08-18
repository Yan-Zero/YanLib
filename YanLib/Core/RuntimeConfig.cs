using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityUIKit.Core;
using UnityUIKit.GameObjects;
using YamlDotNet.Core.Tokens;
using YanLib.DataManipulator;
using YanLib.EventSystem;

namespace YanLib.Core
{
    /// <summary>
    /// 运行时候的配置
    /// </summary>
    public static class RuntimeConfig
    {
        internal static Dictionary<int, string> ResourceName = new Dictionary<int, string>()
        {
            {
                401,
                "食材"
            },
            {
                402,
                "木材"
            },
            {
                403,
                "金石"
            },
            {
                404,
                "织物"
            },
            {
                405,
                "药材"
            },
            {
                406,
                "银钱"
            },
            {
                407,
                "威望"
            },
            {
                408,
                "人力"
            }
        };

        private static List<int> GameUsedDataKey = new List<int>();
        static RuntimeConfig()
        {
            foreach (var i in pairs)
                for (int key = i.Key; key <= i.Value; key++)
                    GameUsedDataKey.Add(key);
        }

        private static int MaxKeyIndex = 3001;
        private static Dictionary<int, int> pairs = new Dictionary<int, int>()
        {
            { 0, 9 },
            { 11, 37 },
            { 39, 49 },
            { 51, 56 },
            { 61, 66 },
            { 71, 73 },
            { 81, 86 },
            { 92, 98 },
            { 110, 195 },
            { 201, 210 },
            { 301, 312 },
            { 401, 408 },
            { 501, 516 },
            { 551, 551 },
            { 601, 614 },
            { 651, 651 },
            { 701, 706 },
            { 801, 802 },
            { 901, 905 },
            { 993, 997 },
            { 1101, 1111 },
            { 1361, 1372 },
            { 2001, 2007 },
        };

        //Fix
        /// <summary>
        /// 没有占用的社交 ID
        /// </summary>
        public static Stack<int> EmptySocialId = new Stack<int>();
        /// <summary>
        /// 没有占用的人物 ID
        /// </summary>
        public static Stack<int> EmptyActorId = new Stack<int>();
        internal static Dictionary<int, Dictionary<int, string>> TraverserLifeRecordFix = new Dictionary<int, Dictionary<int, string>>();

        //ActorData
        internal static Dictionary<string, Dictionary<int, int>> ActorDataModKeys = new Dictionary<string, Dictionary<int, int>>();
        internal static List<int> ActorDataKeyUsed = new List<int>();

        //EventData
        internal static Dictionary<string, Dictionary<int, Event>> EventData = new Dictionary<string, Dictionary<int, Event>>();
        internal static Event CurEvent = null;
        /// <summary>
        /// 选择次数，用来限制每个时节的次数的
        /// </summary>
        internal static Dictionary<string, Dictionary<int, int>> ChoiceCount = new Dictionary<string, Dictionary<int, int>>();
        internal static class ChoiceEnvironment
        {
            public static int GetWhoItem = 0;
            public static bool HasChoose = false;
            public static InputFieldInfo InputFieldInfo;
            public static Action<string, int> ChoiceChoose;
            public static string ID;
            public static IEnumerable<int> Filter;

            public static void Choose()
            {
                ChoiceChoose?.Invoke(ID, MessageEventManager.Instance.MainEventData[1]);
            }
        }
        internal static Dictionary<int, List<AdditionalChoices>> AdditionalChoices = new Dictionary<int, List<AdditionalChoices>>();


        internal static List<ModHelper.ModHelper> Mods = new List<ModHelper.ModHelper>();
        internal static bool GameLoaded = false;

        /// <summary>
        /// 分配 Key 返回分配到的 Key。
        /// </summary>
        /// <param name="GUID">GUID</param>
        /// <param name="Key">Mod 内部的 Key</param>
        /// <returns></returns>
        internal static int AllocateActorDataKey(string GUID, int Key)
        {
            do
            {
                if (ActorDataKeyUsed.Contains(MaxKeyIndex))
                {
                    MaxKeyIndex++;
                    continue;
                }
                ActorDataModKeys[GUID].Add(Key, MaxKeyIndex);
                ActorDataKeyUsed.Add(MaxKeyIndex);
                return ActorDataModKeys[GUID][Key];
            }
            while (true);
        }

        /// <summary>
        /// 加载当前存档中的配置
        /// </summary>
        internal static void LoadData()
        {
            if (!DateFile.instance.modDate.ContainsKey(YanLib.GUID))
            {
                NewData();
                return;
            }

            TraverserLifeRecordFix = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, string>>>(DateFile.instance.modDate[YanLib.GUID]["TraverserLifeRecordFix"]);
            EmptyActorId = JsonConvert.DeserializeObject<Stack<int>>(DateFile.instance.modDate[YanLib.GUID]["EmptyActorId"]);
            EmptySocialId = JsonConvert.DeserializeObject<Stack<int>>(DateFile.instance.modDate[YanLib.GUID]["EmptySocialId"]);
            if(DateFile.instance.modDate[YanLib.GUID].ContainsKey("ChoiceCount"))
                ChoiceCount = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, int>>>(DateFile.instance.modDate[YanLib.GUID]["ChoiceCount"]);
            
        }

        /// <summary>
        /// 新建存档时创建基础配置
        /// </summary>
        internal static void NewData()
        {
            EmptySocialId = new Stack<int>();
            EmptyActorId = new Stack<int>();
            TraverserLifeRecordFix = new Dictionary<int, Dictionary<int, string>>();
            DateFile.instance.modDate.Add(YanLib.GUID, new Dictionary<string, string>());
            ChoiceCount = new Dictionary<string, Dictionary<int, int>>();
        }

        /// <summary>
        /// 存档保存时的行为
        /// </summary>
        internal static void SaveData()
        {
            DateFile.instance.modDate[YanLib.GUID]["TraverserLifeRecordFix"] = JsonConvert.SerializeObject(TraverserLifeRecordFix);
            DateFile.instance.modDate[YanLib.GUID]["EmptySocialId"] = JsonConvert.SerializeObject(EmptySocialId);
            DateFile.instance.modDate[YanLib.GUID]["EmptyActorId"] = JsonConvert.SerializeObject(EmptyActorId);
            DateFile.instance.modDate[YanLib.GUID]["ChoiceCount"] = JsonConvert.SerializeObject(ChoiceCount);
        }

        internal static void ChangeTrun()
        {
            ChoiceCount.Clear();
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
            ActorDataModKeys = YanLib.Settings.Config.Bind("Key", "DataKey-Set", new Dictionary<string, Dictionary<int, int>>(), "已设置的 Key").Value;
            ActorDataKeyUsed = YanLib.Settings.Config.Bind("Key", "DataKey-Used", GameUsedDataKey, "被使用的 Key").Value;
            MaxKeyIndex = YanLib.Settings.Config.Bind("Key", "DataKey-Max", 3001, "被使用的 Key").Value;

            List<string> NeedToLoadFile = new List<string>();
            Stack<string> Dir = new Stack<string>();
            Dir.Push(BepInEx.Paths.PluginPath);
            if (Directory.Exists(Path.Combine(BepInEx.Paths.GameRootPath, "Mods")))
                Dir.Push(Path.Combine(BepInEx.Paths.GameRootPath, "Mods"));
            do
            {
                var directory = Dir.Pop();
                foreach (var i in Directory.GetFiles(directory))
                    if(Path.GetExtension(i).ToLower() == ".yaml")
                        NeedToLoadFile.Add(i);
                foreach (var i in Directory.GetDirectories(directory))
                    Dir.Push(i);
            } while (Dir.Count > 0);
            foreach (var i in NeedToLoadFile)
            {
                var folderName = Path.GetFileName(Path.GetDirectoryName(i)).ToLower();
                if (folderName == "eventdata" || folderName == "events")
                {
                    try
                    {
                        EventHelper.LoadEventDataFromFile(i);
                    }
                    catch (Exception ex)
                    {
                        YanLib.Logger.LogError("错误文件：" + i);
                        YanLib.Logger.LogError(ex);
                    }
                }
                else if(folderName == "additionalchoices")
                {
                    Dir.Push(i);
                }
            }
            while (Dir.Count > 0)
            {
                var File = Dir.Pop();
                try
                {
                    EventHelper.LoadAdditionalChoicesDataFromFile(File);
                }
                catch (Exception ex)
                {
                    YanLib.Logger.LogError("错误文件：" + File);
                    YanLib.Logger.LogError(ex);
                }
            };
        }

        internal static void SaveConfig()
        {
            YanLib.Settings.Config.Bind("Key", "DataKey-Set", new Dictionary<string, Dictionary<int, int>>(), "已设置的 Key").Value = ActorDataModKeys;
            YanLib.Settings.Config.Bind("Key", "DataKey-Used", GameUsedDataKey, "被使用的 Key").Value = ActorDataKeyUsed;
            YanLib.Settings.Config.Bind("Key", "DataKey-Max", 3001, "Key 最大值").Value = MaxKeyIndex;
        }

        /// <summary>
        /// 这个是清除掉 RuntimeConfig 中有设定的数据，并且将 ID Push 进空人物 ID 列表
        /// </summary>
        public static void DelActorData(int ActorID)
        {
            EmptyActorId.Push(ActorID);
            if (TraverserLifeRecordFix.ContainsKey(ActorID))
                TraverserLifeRecordFix.Remove(ActorID);
        }
    }
}
