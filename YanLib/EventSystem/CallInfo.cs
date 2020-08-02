using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization;
using YanLib.DataManipulator;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 事件系统用来调用函数的
    /// </summary>
    public class CallInfo
    {
        /// <summary>
        /// 参数信息
        /// </summary>
        public class ParamInfo
        {
            /// <summary>
            /// 参数的类型
            /// </summary>
            public enum ParamType
            {
                /// <summary>
                /// 整型
                /// </summary>
                Int,
                /// <summary>
                /// 字符串
                /// </summary>
                String,
                /// <summary>
                /// 对面的角色 ID
                /// </summary>
                ActorID,
                /// <summary>
                /// 当前位置
                /// </summary>
                Location,
                /// <summary>
                /// 事件/选项的描述文本
                /// </summary>
                DescOrTip,
                /// <summary>
                /// （选项用）打开物品选择界面
                /// 值为 0 时候，获取太吾全部物品，否则为对方的
                /// </summary>
                ChooseItem,
                /// <summary>
                /// （选项用）打开人物选择界面，获取太吾同道（不包括囚犯与孩子）
                /// </summary>
                ChooseActor,
                /// <summary>
                /// （选项用）输入的内容，string
                /// </summary>
                InputText,
            }

            /// <summary>
            /// 类型
            /// </summary>
            public ParamType Type;
            /// <summary>
            /// 值
            /// </summary>
            public string Value;
        }

        /// <summary>
        /// 目标函数，全名
        /// </summary>
        public string Target;

        /// <summary>
        /// 目标类，可以更准确地定位，需要填写 TargetMethod
        /// </summary>
        [YamlIgnore]
        public Type TargetType;
        /// <summary>
        /// 函数的名字，需要填写 TargetType
        /// </summary>
        [YamlIgnore]
        public string TargetMethod;

        /// <summary>
        /// 需要传入的参数
        /// </summary>
        public List<ParamInfo> Params = new List<ParamInfo>();

        /// <summary>
        /// 获取函数信息
        /// </summary>
        /// <returns></returns>
        public MethodInfo GetMethod()
        {
            if (string.IsNullOrEmpty(Target) && string.IsNullOrEmpty(TargetMethod))
                throw new MethodAccessException("Call 数据不完整");
            if (TargetType != null && !string.IsNullOrEmpty(TargetMethod))
                return AccessTools.Method(TargetType, TargetMethod);
            else
                return AccessTools.Method(Target);
        }

        /// <summary>
        /// 调用函数
        /// </summary>
        /// <param name="EventID">事件 ID</param>
        /// <param name="TargetActorID">目标角色</param>
        /// <param name="DescOrTip">描述文本</param>
        /// <returns></returns>
        public object Call(string EventID, int TargetActorID, string DescOrTip = "")
        {
            var id = EventID.Split(':');
            bool isChoice = id.Length > 0;
            List<object> callParams = new List<object>() { EventID };
            foreach (var i in Params)
                switch (i.Type)
                {
                    case ParamInfo.ParamType.Int:
                        callParams.Add(int.Parse(i.Value));
                        break;
                    case ParamInfo.ParamType.String:
                        callParams.Add(i.Value);
                        break;
                    case ParamInfo.ParamType.ActorID:
                        callParams.Add(TargetActorID);
                        break;
                    case ParamInfo.ParamType.Location:
                        switch (i.Value)
                        {
                            case "Location.TaiwuCurrent":
                                callParams.Add(new Location(DateFile.instance.GetActorAtPlace(DateFile.instance.mianActorId).ToArray()));
                                break;
                            case "Location.TargetActorCurrent":
                                callParams.Add(new Location(DateFile.instance.GetActorAtPlace(TargetActorID).ToArray()));
                                break;
                            default:
                                callParams.Add(JsonConvert.DeserializeObject(i.Value));
                                break;
                        }
                        break;
                    case ParamInfo.ParamType.DescOrTip:
                        if (string.IsNullOrEmpty(DescOrTip))
                            DescOrTip = EventHelper.GetEvent(id[0]).Desc;
                        callParams.Add(DescOrTip);
                        break;
                    case ParamInfo.ParamType.ChooseItem:
                        if(isChoice)
                            callParams.Add(ActorMenu.choseItemId);
                        break;
                    case ParamInfo.ParamType.InputText:
                        if (isChoice)
                            callParams.Add(ui_MessageWindow.Instance.inputTextField.text);
                        break;
                    default:
                        break;
                }

            return GetMethod().Invoke(null, callParams.ToArray());
        }
    }

    /// <summary>
    /// 位置
    /// </summary>
    public struct Location
    {
        /// <summary>
        /// 大地图的 ID
        /// </summary>
        public int MapID;
        
        /// <summary>
        /// 地图中土块的 ID
        /// </summary>
        public int TileID;

        /// <summary>
        /// 土块横坐标
        /// </summary>
        public int x
        {
            get => (TileID % int.Parse(DateFile.instance.partWorldMapDate[MapID][98])) + 1;
            set
            {
                if (value <= 0 || value > int.Parse(DateFile.instance.partWorldMapDate[MapID][98]))
                    return;
                var Δ = value - x;
                TileID += Δ;
            }
        }

        /// <summary>
        /// 土块纵坐标
        /// </summary>
        public int y
        {
            get => (TileID / int.Parse(DateFile.instance.partWorldMapDate[MapID][98])) + 1;
            set
            {
                if (value <= 0 || value > int.Parse(DateFile.instance.partWorldMapDate[MapID][98]))
                    return;
                var Δ = value - y;
                TileID += Δ * int.Parse(DateFile.instance.partWorldMapDate[MapID][98]);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="places">位置信息</param>
        public Location(int[] places)
        {
            MapID = places[0];
            TileID = places[1];
        }
    }
}
