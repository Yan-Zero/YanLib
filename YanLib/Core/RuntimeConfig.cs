using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityUIKit.Core;
using UnityUIKit.GameObjects;
using YamlDotNet.Core.Tokens;
using YanLib.DataManipulator;

namespace YanLib.Core
{
    internal static class RuntimeConfig
    {
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

        public static Stack<int> EmptySocialId = new Stack<int>();
        public static Stack<int> EmptyActorId = new Stack<int>();

        public static Dictionary<string, Dictionary<int, int>> ModKeys = new Dictionary<string, Dictionary<int, int>>();
        public static List<int> KeyUsed = new List<int>();
        public static Dictionary<int, Dictionary<int, string>> TraverserLifeRecordFix = new Dictionary<int, Dictionary<int, string>>();

        public static List<ModHelper.ModHelper> Mods = new List<ModHelper.ModHelper>();
        public static bool GameLoaded = false;

        /// <summary>
        /// 分配 Key 返回分配到的 Key。
        /// </summary>
        /// <param name="GUID">GUID</param>
        /// <param name="Key">Mod 内部的 Key</param>
        /// <returns></returns>
        public static int AllocateKey(string GUID, int Key)
        {
            do
            {
                if (KeyUsed.Contains(MaxKeyIndex))
                {
                    MaxKeyIndex++;
                    continue;
                }
                ModKeys[GUID].Add(Key, MaxKeyIndex);
                KeyUsed.Add(MaxKeyIndex);
                return ModKeys[GUID][Key];
            }
            while (true);
        }

        /// <summary>
        /// 加载当前存档中的配置
        /// </summary>
        public static void LoadData()
        {
            if (!DateFile.instance.modDate.ContainsKey(YanLib.GUID))
            {
                NewData();
                return;
            }

            TraverserLifeRecordFix = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, string>>>(DateFile.instance.modDate[YanLib.GUID]["TraverserLifeRecordFix"]);
            EmptyActorId = JsonConvert.DeserializeObject<Stack<int>>(DateFile.instance.modDate[YanLib.GUID]["EmptyActorId"]);
            EmptySocialId = JsonConvert.DeserializeObject<Stack<int>>(DateFile.instance.modDate[YanLib.GUID]["EmptySocialId"]);
            MaxKeyIndex = JsonConvert.DeserializeObject<int>(DateFile.instance.modDate[YanLib.GUID]["MaxKeyIndex"]);
        }

        /// <summary>
        /// 新建存档时创建基础配置
        /// </summary>
        public static void NewData()
        {
            EmptySocialId = new Stack<int>();
            EmptyActorId = new Stack<int>();
            TraverserLifeRecordFix = new Dictionary<int, Dictionary<int, string>>();
            DateFile.instance.modDate.Add(YanLib.GUID, new Dictionary<string, string>());
        }

        /// <summary>
        /// 存档保存时的行为
        /// </summary>
        public static void SaveData()
        {
            DateFile.instance.modDate[YanLib.GUID]["TraverserLifeRecordFix"] = JsonConvert.SerializeObject(TraverserLifeRecordFix);
            DateFile.instance.modDate[YanLib.GUID]["EmptySocialId"] = JsonConvert.SerializeObject(EmptySocialId);
            DateFile.instance.modDate[YanLib.GUID]["EmptyActorId"] = JsonConvert.SerializeObject(EmptyActorId);
        }

        public static class UI_Config
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

        public static void Init()
        {
            ModKeys = YanLib.settings.Config.Bind("Key", "DataKey-Set", new Dictionary<string, Dictionary<int, int>>(), "已设置的 Key").Value;
            KeyUsed = YanLib.settings.Config.Bind("Key", "DataKey-Used", GameUsedDataKey, "被使用的 Key").Value;
            MaxKeyIndex = YanLib.settings.Config.Bind("Key", "DataKey-Max", 3001, "被使用的 Key").Value;
        }

        public static void SaveConfig()
        {
            YanLib.settings.Config.Bind("Key", "DataKey-Set", new Dictionary<string, Dictionary<int, int>>(), "已设置的 Key").Value = ModKeys;
            YanLib.settings.Config.Bind("Key", "DataKey-Used", GameUsedDataKey, "被使用的 Key").Value = KeyUsed;
            YanLib.settings.Config.Bind("Key", "DataKey-Max", 3001, "Key 最大值").Value = MaxKeyIndex;
        }
    }
}
