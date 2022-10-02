using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityUIKit.Core;


namespace YanLib.Core
{
#if DEBUG
    /// <summary>
    /// 如果你看到这东西，记得提醒我编译的是 Debug 版本
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// 添加事件
        /// </summary>
        public static void AddEvent()
        {
            RuntimeConfig.EventData.Add("Debug", new Dictionary<int, Event>());

            var i = File.OpenText(@"E:\SteamLibrary\steamapps\common\The Scroll Of Taiwu\BepInEx\plugins\0.YanCore\x.yaml");
            var events = ManagedGameObjectIO.Load<List<Event>>(i.ReadToEnd());
            foreach(var @event in events)
            {
                YanLib.Logger.LogInfo(@event.ID);
                var id = @event.ID.Split(new char[] { '.' }, 2);
                foreach(var t in id)
                {
                    YanLib.Logger.LogInfo(t);
                }
                RuntimeConfig.EventData[id[0]].Add(int.Parse(id[1]), @event);
            }
        }

        /// <summary>
        /// 动态描述
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Desc"></param>
        /// <param name="TargetActorID"></param>
        /// <param name="MyString"></param>
        /// <param name="TaiwuLocation"></param>
        /// <returns></returns>
        public static string Desc(string ID,string Desc,int TargetActorID)
        {
            return Desc.Replace("[这会被替代]", DateFile.instance.GetActorName(TargetActorID));
        }

        /// <summary>
        /// 测试事件
        /// </summary>
        /// <param name="key"></param>
        public static void CallEvent(int key = 1)
        {
            EventHelper.CallEvent("Debug", key, 10003);
        }

        /// <summary>
        /// 测试需求
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TargetActorID"></param>
        /// <returns></returns>
        public static List<KeyValuePair<bool,string>> ChoiceRequirement(string ID,int TargetActorID)
        {
            var result = new List<KeyValuePair<bool, string>>();
            if(int.Parse(DateFile.instance.GetActorDate(TargetActorID, 15)) < 850)
                result.Add(new KeyValuePair<bool, string>(false, "魅力不小于 850"));
            else
                result.Add(new KeyValuePair<bool, string>(true, "魅力不小于 850"));
            return result;
        }

        /// <summary>
        /// 动态 Tip
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="Tip"></param>
        /// <param name="ActorID"></param>
        /// <returns></returns>
        public static string Tip(string EventID,int ActorID,string Tip)
        {
            if (int.Parse(DateFile.instance.GetActorDate(ActorID, 15)) >= 900)
                return "草？魅力 900 你都砍？做个人";
            return Tip;
        }

        /// <summary>
        /// 测试 End 的
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="str"></param>
        /// <param name="ActorID"></param>
        /// <param name="mySet"></param>
        public static void GetThing(string ID,string str,int ActorID,int mySet)
        {
            YanLib.Logger.LogInfo(str);
            DateFile.instance.GetItem(str == "GiveTaiwu" ? DateFile.instance.mianActorId : ActorID, mySet, 1, true);
        }
    }
#endif
}