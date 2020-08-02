using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YanLib.DataManipulator;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 选择后效果
    /// </summary>
    public class ChoiceEnd
    {

        /// <summary>
        /// 效果
        /// </summary>
        public class Effect
        {
            /// <summary>
            /// 类型
            /// </summary>
            public enum EffectTarget
            {
                /// <summary>
                /// 时间,A值
                /// </summary>
                Time,
                /// <summary>
                /// 物品，A值为物品 ID，B值如果是0则角色为太吾，如果不是，则就是目标人物
                /// </summary>
                Item,
                /// <summary>
                /// 移动到周围，ValueA 为 0 时对象为太吾，如果不是则为对方
                /// 无视 Reduce
                /// </summary>
                MoveToAround,
                /// <summary>
                /// 对方的好感
                /// </summary>
                Favor,
                /// <summary>
                /// Taiwu的资源，A值为资源的类型，B值为数量
                /// </summary>
                TaiwuResource,
                /// <summary>
                /// 对方的资源
                /// </summary>
                TargetResource,
            }

            /// <summary>
            /// True 为减少,False 为增加
            /// </summary>
            public bool Reduce;
            /// <summary>
            /// A值
            /// </summary>
            public int ValueA;
            /// <summary>
            /// B值
            /// </summary>
            public int ValueB;
            /// <summary>
            /// 作用类型
            /// </summary>
            public EffectTarget Type;
        }

        /// <summary>
        /// 调用的函数
        /// </summary>
        public CallInfo CallEnd;
        /// <summary>
        /// 效果
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
        /// <summary>
        /// 唤起的事件，0为改变窗口，-1为自定义
        /// </summary>
        public string ToID = "0";
        /// <summary>
        /// 毫无疑问，是选择物品/人物用的，返回值需求为  IEnumerable&lt;int&gt;
        /// </summary>
        public CallInfo Filter;

        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="TargetActorID"></param>
        public void End(string EventID,int TargetActorID)
        {
            foreach(var i in Effects)
            {
                switch (i.Type)
                {
                    case Effect.EffectTarget.Item:
                        if(i.Reduce)
                        {
                            int itemKey = -1;
                            foreach (var item in DateFile.instance.actorItemsDate[i.ValueB == 0 ? DateFile.instance.MianActorID() : TargetActorID])
                                if (DateFile.instance.GetItemDate(item.Key, 999) == i.ValueA.ToString())
                                {
                                    itemKey = item.Key;
                                    break;
                                }
                            if (itemKey != -1)
                                DateFile.instance.LoseItem(i.ValueB == 0 ? DateFile.instance.MianActorID() : TargetActorID, itemKey, 1, true, loseType: 0);
                        }
                        else
                            DateFile.instance.GetItem(i.ValueB == 0 ? DateFile.instance.MianActorID() : TargetActorID, i.ValueA, 1, true, 0);
                        break;
                    case Effect.EffectTarget.Time:
                        UIDate.instance.ChangeTime(false, i.Reduce ? i.ValueA : -i.ValueA);
                        break;
                    case Effect.EffectTarget.MoveToAround:
                        ActorMoveToAround(i.ValueA == 0 ? DateFile.instance.mianActorId : TargetActorID);
                        break;
                    case Effect.EffectTarget.Favor:
                        DateFile.instance.ChangeFavor(TargetActorID, i.Reduce ? -i.ValueA : i.ValueA);
                        break;
                    case Effect.EffectTarget.TaiwuResource:
                    case Effect.EffectTarget.TargetResource:
                        var iActorID = i.Type == Effect.EffectTarget.TaiwuResource ? DateFile.instance.mianActorId : TargetActorID;
                        UIDate.instance.ChangeResource(iActorID, i.ValueA, i.Reduce ? -i.ValueB : i.ValueB);
                        break;
                    default:
                        break;
                }

            }

            int id = 0;
            var isNum = int.TryParse(ToID, out id);
            var result = CallEnd?.Call(EventID, TargetActorID);
            if (isNum && id >= 0)
            {
                if (id == 0)
                    ui_MessageWindow.Instance.Hide();
                else if(id > 0)
                    UIManager.Instance.AddUI("ui_MessageWindow", new int[] { 0, TargetActorID, id , TargetActorID });
                return;
            }
            else if (!isNum)
            {
                if (string.IsNullOrWhiteSpace(ToID))
                    ui_MessageWindow.Instance.Hide();
                else
                    EventHelper.CallEvent(ToID, TargetActorID);
                return;
            }

            var returnType = CallEnd?.GetMethod().ReturnType;
            if(returnType == null || returnType == typeof(void))
                ui_MessageWindow.Instance.Hide();
            else if(returnType == typeof(string) && !string.IsNullOrWhiteSpace((string)result))
            {
                if (int.TryParse((string)result, out id))
                {
                    if (id > 0)
                        UIManager.Instance.AddUI("ui_MessageWindow", new int[] { 0, TargetActorID, id , TargetActorID });
                    else
                        ui_MessageWindow.Instance.Hide();
                }
                else
                    EventHelper.CallEvent((string)result, TargetActorID);
            }
            else if(returnType == typeof(int))
            {
                id = (int)result;
                if (id > 0)
                    UIManager.Instance.AddUI("ui_MessageWindow", new int[] { 0, TargetActorID, id , TargetActorID});
                else
                    ui_MessageWindow.Instance.Hide();
            }
        }

        /// <summary>
        /// 移动到周围
        /// </summary>
        /// <param name="TargetActorID"></param>
        public void ActorMoveToAround(int TargetActorID)
        {
            var i = DateFile.instance.GetActorAtPlace(TargetActorID);
            List<int> list = new List<int>(DateFile.instance.GetWorldMapNeighbor(i[0], i[1]));
            int num = list[UnityEngine.Random.Range(0, list.Count)];
            if (TargetActorID == DateFile.instance.mianActorId)
            {
                if (HomeSystemWindow.Exists)
                    UIState.HomeSystem.Back();
                if (WorldMapSystem.instance.gameObject.activeSelf)
                {
                    EscapeOnWorldMap();
                    return;
                }
                UIState mainWorld = UIState.MainWorld;
                mainWorld.OnOpened = (Action)Delegate.Combine(mainWorld.OnOpened, new Action(EscapeOnWorldMap));
                void EscapeOnWorldMap()
                {
                    Transform transform = WorldMapSystem.instance.worldMapPlaces[num].placeImage.transform;
                    WorldMapSystem.instance.ShowChoosePlaceMenu(DateFile.instance.mianWorldId, DateFile.instance.mianPartId, num, transform);
                    WorldMapSystem.instance.MoveToChoosePlace(needTime: false);
                }
            }
            else
                DateFile.instance.MoveToPlace(i[0], num, TargetActorID, true);
        }
    }
}
