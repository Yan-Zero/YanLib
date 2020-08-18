using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YanLib.EventSystem;
using YanLib.Core;
using YanLib.YanException;
using UnityUIKit.Core;
using System.IO;

namespace YanLib.DataManipulator
{
    /// <summary>
    /// 事件操作者
    /// </summary>
    public static class EventHelper
    {
        /// <summary>
        /// 从文件中加载数据
        /// </summary>
        /// <param name="Path">文件目录</param>
        public static void LoadEventDataFromFile(string Path)
        {
            LoadEventData(File.ReadAllText(Path));
        }

        /// <summary>
        /// 加载事件数据
        /// </summary>
        /// <param name="data"></param>
        public static void LoadEventData(string data)
        {
            List<Event> Temp = new List<Event>();
            if (data[0] == '-')
            {
                Temp = ManagedGameObjectIO.Load<List<Event>>(data);
                foreach (var e in Temp)
                    AddEvent(e);
            }
            else
                AddEvent(ManagedGameObjectIO.Load<Event>(data));
        }


        /// <summary>
        /// 添加事件到系统中
        /// </summary>
        /// <param name="event">事件</param>
        public static void AddEvent(Event @event)
        {
            var id = @event.ID.Split(new char[] { '.' }, 2);
            if (!RuntimeConfig.EventData.ContainsKey(id[0]))
                RuntimeConfig.EventData.Add(id[0], new Dictionary<int, Event>());
            RuntimeConfig.EventData[id[0]][int.Parse(id[1])] = @event;
        }

        /// <summary>
        /// 从文件中加载数据
        /// </summary>
        /// <param name="Path">文件目录</param>
        public static void LoadAdditionalChoicesDataFromFile(string Path)
        {
            LoadAdditionalChoicesData(File.ReadAllText(Path));
        }

        /// <summary>
        /// 加载事件数据
        /// </summary>
        /// <param name="data"></param>
        public static void LoadAdditionalChoicesData(string data)
        {
            List<AdditionalChoices> Temp = new List<AdditionalChoices>();
            if (data[0] == '-')
            {
                Temp = ManagedGameObjectIO.Load<List<AdditionalChoices>>(data);
                foreach (var e in Temp)
                    AddChoices(e);
            }
            else
                AddChoices(ManagedGameObjectIO.Load<AdditionalChoices>(data));
        }

        /// <summary>
        /// 给其追加按钮
        /// </summary>
        /// <param name="choices"></param>
        public static void AddChoices(AdditionalChoices choices)
        {
            int id = 0;
            if(int.TryParse(choices.ID,out id))
            {
                if (!RuntimeConfig.AdditionalChoices.ContainsKey(id))
                    RuntimeConfig.AdditionalChoices.Add(id, new List<AdditionalChoices>());
                RuntimeConfig.AdditionalChoices[id].Add(choices);
            }
            else
            {
                var Event = GetEvent(choices.ID);
                var index = choices.Index;
                if (index < 0)
                    index += Event.Choices.Count + 1;
                if (index < 0)
                    index = 0;
                if (index > Event.Choices.Count)
                    index = Event.Choices.Count;
                Event.Choices.InsertRange(index, choices.Choices);
            }
        }

        /// <summary>
        /// 获取事件数据事件
        /// </summary>
        /// <param name="Namespace">事件所处的命名空间</param>
        /// <param name="Key">事件的 Key</param>
        /// <returns>事件数据</returns>
        public static Event GetEvent(string Namespace, int Key)
        {
            Event result;
            if (!RuntimeConfig.EventData.ContainsKey(Namespace))
                throw new InvalidEventIDException("命名空间不存在");
            if (!RuntimeConfig.EventData[Namespace].TryGetValue(Key, out result))
                throw new InvalidEventIDException("ID 不存在");
            return result;
        }

        /// <summary>
        /// 获取事件数据事件
        /// </summary>
        /// <param name="EventID">事件 ID</param>
        /// <returns>事件数据</returns>
        public static Event GetEvent(string EventID)
        {
            var param = EventID.Split(new char[] { '.' }, 2);
            if (param.Length > 1)
            {
                int key;
                if (!int.TryParse(param[1], out key))
                    throw new InvalidEventIDException();
                return GetEvent(param[0], key);
            }
            else
                throw new InvalidEventIDException();
        }

        /// <summary>
        /// 唤起事件
        /// </summary>
        /// <param name="Namespace">事件所处的命名空间</param>
        /// <param name="Key">事件的 Key</param>
        /// <param name="TargetActorID">对面的角色 ID</param>
        public static void CallEvent(string Namespace, int Key, int TargetActorID)
        {
            GetEvent(Namespace, Key).CallEvent(TargetActorID);
        }

        /// <summary>
        /// 唤起事件
        /// </summary>
        /// <param name="EventID">事件 ID</param>
        /// <param name="TargetActorID">对面的角色 ID</param>
        public static void CallEvent(string EventID, int TargetActorID)
        {
            GetEvent(EventID).CallEvent(TargetActorID);
        }

        /// <summary>
        /// 获取原版事件的数据
        /// </summary>
        /// <param name="GameEventID">原版事件ID</param>
        /// <returns>事件数据</returns>
        public static Dictionary<int, string> GetEvent(int GameEventID)
        {
            Dictionary<int, string> result;
            if (!DateFile.instance.eventDate.TryGetValue(GameEventID, out result))
                throw new InvalidEventIDException();
            return result;
        }

        /// <summary>
        /// 原版事件，原版事件转原版事件只会改 MainEventData 中的 ID
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TargetActorID"></param>
        /// <param name="chooseId">只有转场的时候才用得到</param>
        public static void CallEvent(int ID, int TargetActorID,int chooseId = 0)
        {
            if (ui_MessageWindow.Exists && RuntimeConfig.CurEvent == null)
            {
                MessageEventManager.Instance.MainEventData[2] = ID;
                EventMisc.SetMassageWindow(MessageEventManager.Instance.MainEventData, chooseId);
                EventMisc.SetSizeDelta(2);
            }
            else
                UIManager.Instance.AddUI("ui_MessageWindow", new int[] { 0, TargetActorID, ID, TargetActorID });
        }

        /// <summary>
        /// 解析文本中的描述
        /// </summary>
        /// <param name="ID">事件 ID</param>
        /// <param name="TargetActorID">对方的 ID</param>
        /// <param name="Desc">事件文本</param>
        /// <returns></returns>
        public static string AnalyzeDesc(string ID, int TargetActorID, string Desc)
        {
            var result = Desc.Replace("[TargetActorName]", DateFile.instance.GetActorName(TargetActorID))
                .Replace("[TaiwuName]", DateFile.instance.GetActorName())
                .Replace("[TargetActorGang]", DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate((TargetActorID > 0) ? TargetActorID : DateFile.instance.mianActorId, 19, applyBonus: false)), 0));
            return result;
        }

        /// <summary>
        /// 解析 Tip
        /// </summary>
        /// <param name="ID">事件 ID</param>
        /// <param name="Choice">选项</param>
        /// <param name="TargetActorID">对方的 ID</param>
        /// <param name="Tip"></param>
        /// <returns></returns>
        public static string AnalyzeTip(string ID,EventChoice Choice, int TargetActorID,string Tip)
        {

            if(Tip.Contains("[Effect]"))
            {
                string effect = "Effect:";
                foreach (var i in Choice.Effect.Effects)
                {
                    switch (i.Type)
                    {
                        case ChoiceEnd.Effect.EffectTarget.Time:
                            effect += "\n·时间" + (i.Reduce ? "<color=#FF0000FF>减少" : "<color=#00FF00FF>增加") + $"</color> {i.ValueA}";
                            break;
                        case ChoiceEnd.Effect.EffectTarget.Item:
                            effect += "\n·" + (i.ValueB == 0 ? "太吾" : DateFile.instance.GetActorName(TargetActorID)) + (i.Reduce ? "<color=#FF0000FF>失去" : "<color=#00FF00FF>获得") + $"</color>物品：\n {DateFile.instance.GetItemDate(i.ValueA, 0)}";
                            break;
                        case ChoiceEnd.Effect.EffectTarget.MoveToAround:
                            effect += "\n·" + (i.ValueA == 0 ? "太吾" : DateFile.instance.GetActorName(TargetActorID)) + $"移动到周围";
                            break;
                        case ChoiceEnd.Effect.EffectTarget.Favor:
                            effect += "\n·对方好感" + (i.Reduce ? "<color=#FF0000FF>减少" : "<color=#00FF00FF>增加") + $"</color> {i.ValueA}";
                            break;
                        case ChoiceEnd.Effect.EffectTarget.TaiwuResource:
                        case ChoiceEnd.Effect.EffectTarget.TargetResource:
                            var flag = i.Type == ChoiceEnd.Effect.EffectTarget.TaiwuResource;
                            effect += "\n·" + (flag ? "太吾" : DateFile.instance.GetActorName(TargetActorID)) + RuntimeConfig.ResourceName[i.ValueA] + (i.Reduce ? "<color=#FF0000FF>减少" : "<color=#00FF00FF>增加") + $"</color> {i.ValueA}";
                            break;
                        default:
                            break;
                    }
                }
                Tip = Tip.Replace("[Effect]", effect.Length > 7 ? effect + "\n" : "");
            }
            var result = Tip.Replace("[TargetActorName]", DateFile.instance.GetActorName(DateFile.instance.mianActorId))
                .Replace("[TaiwuName]", DateFile.instance.GetActorName())
                .Replace("[TargetActorGang]", DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate((TargetActorID > 0) ? TargetActorID : DateFile.instance.mianActorId, 19, applyBonus: false)), 0));

            return result;
        }
    }
}
