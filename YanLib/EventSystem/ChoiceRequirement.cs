using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YanLib.Core;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 选项需求
    /// </summary>
    public class ChoiceRequirement
    {
        private bool? hasCountPerMonth = null;
        /// <summary>
        /// 是否有每月次数限制
        /// </summary>
        public bool HasCountPerMonth
        {
            get
            {
                if (hasCountPerMonth != null)
                    return (bool)hasCountPerMonth;
                foreach(var i in Requirements)
                    if(i.Type == Requirement.RequirementTarget.CountPerMonth)
                    {
                        hasCountPerMonth = true;
                        return true;
                    }
                hasCountPerMonth = false;
                return false;
            }
        }

        /// <summary>
        /// 返回是否符合需求的函数，返回值 IEnumerable&lt;KeyValuePair&lt;bool, string&gt;&gt;
        /// </summary>
        public CallInfo RequirementCall;

        /// <summary>
        /// 需求
        /// </summary>
        public class Requirement
        {
            /// <summary>
            /// 类型
            /// </summary>
            public enum RequirementTarget
            {
                /// <summary>
                /// 时间
                /// </summary>
                Time,
                /// <summary>
                /// 物品，A值为物品 ID，B值如果是0则角色为太吾，如果不是，则就是目标人物
                /// </summary>
                Item,
                /// <summary>
                /// 同道，0 为 不是同道，1 为 是同道
                /// </summary>
                Teammate,
                /// <summary>
                /// 好感度，ValueA 为需求的好感等级 ValueB 为 0 时，大于等于，为 1 时，小于等于
                /// </summary>
                Favor,
                /// <summary>
                /// 每个时节的次数
                /// </summary>
                CountPerMonth,
                /// <summary>
                /// Taiwu的资源，A值为资源的类型，B值为数量
                /// </summary>
                TaiwuResource,
                /// <summary>
                /// 对方的资源
                /// </summary>
                TargetResource,
                /// <summary>
                /// 地区恩义，ValueA为数值，B值为0时，为大于等于，否则为小于等于
                /// </summary>
                BasePartValue,
                /// <summary>
                /// 没有超过同道最大成员数
                /// </summary>
                NotOverTeammateMaxValue,
            }

            /// <summary>
            /// A值
            /// </summary>
            public int ValueA;
            /// <summary>
            /// B值
            /// </summary>
            public int ValueB;
            /// <summary>
            /// 需求类型
            /// </summary>
            public RequirementTarget Type;
        }
        /// <summary>
        /// 需求
        /// </summary>
        public List<Requirement> Requirements = new List<Requirement>();

        /// <summary>
        /// 获取是否符合条件
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="TargetActorID"></param>
        /// <returns></returns>
        public bool Allow(string EventID, int TargetActorID)
        {
            foreach(var i in Requirements)
            {
                switch(i.Type)
                {
                    case Requirement.RequirementTarget.Time:
                        if (DateFile.instance.dayTime < i.ValueA)
                            return false;
                        break;
                    case Requirement.RequirementTarget.Item:
                        int itemKey = -1;
                        foreach (var item in DateFile.instance.actorItemsDate[i.ValueB == 0 ? DateFile.instance.MianActorID() : TargetActorID])
                            if (DateFile.instance.GetItemDate(item.Key, 999) == i.ValueA.ToString())
                            {
                                itemKey = item.Key;
                                break;
                            }
                        if (itemKey == -1)
                            return false;
                        break;
                    case Requirement.RequirementTarget.Teammate:
                        var flag = DateFile.instance.GetFamily(getPrisoner: true).Contains(TargetActorID);
                        if (flag && i.ValueA == 0)
                            return false;
                        if (!flag && i.ValueA != 0)
                            return false;
                        break;
                    case Requirement.RequirementTarget.Favor:
                        if(i.ValueB == 0)
                        {
                            if (DateFile.instance.GetActorFavor(false, DateFile.instance.mianActorId, TargetActorID, true) < i.ValueA)
                                return false;
                        }
                        else
                        {
                            if (DateFile.instance.GetActorFavor(false, DateFile.instance.mianActorId, TargetActorID, true) > i.ValueA)
                                return false;
                        }
                        break;
                    case Requirement.RequirementTarget.CountPerMonth:
                        if(RuntimeConfig.ChoiceCount.ContainsKey(EventID))
                        {
                            int count = 0;
                            RuntimeConfig.ChoiceCount[EventID].TryGetValue(TargetActorID, out count);
                            if (count >= i.ValueA)
                                return false;
                        }
                        break;
                    case Requirement.RequirementTarget.TaiwuResource:
                    case Requirement.RequirementTarget.TargetResource:
                        if(DateFile.instance.GetActorDate(i.Type == Requirement.RequirementTarget.TaiwuResource ? DateFile.instance.mianActorId : TargetActorID, i.ValueA).ParseInt() < i.ValueB)
                            return false;
                        break;
                    case Requirement.RequirementTarget.BasePartValue:
                        int id = DateFile.instance.GetActorDate(TargetActorID, 19, false).ParseInt();
                        var value = DateFile.instance.GetBasePartValue(DateFile.instance.GetGangDate(id, 11).ParseInt(), DateFile.instance.GetGangDate(id, 3).ParseInt());
                        if (i.Type == 0)
                        {
                            if (value < i.ValueA)
                                return false;
                        }
                        else if (value > i.ValueA)
                            return false;
                        break;
                    case Requirement.RequirementTarget.NotOverTeammateMaxValue:
                        if (DateFile.instance.GetFamily(getPrisoner: true).Count >= DateFile.instance.GetMaxFamilySize())
                            return false;
                        break;
                    default:
                        break;
                }
            }


            var call = RequirementCall?.Call(EventID, TargetActorID) as IEnumerable<KeyValuePair<bool, string>>;
            if (call != null)
                foreach (var i in call)
                    if (!i.Key)
                        return false;

            return true;
        }

        private const string Yes = "<color=#00FF00FF>Y</color> ";
        private const string No = "<color=#FF0000FF>N</color> ";

        /// <summary>
        /// 获取未满足的需求
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="TargetActorID"></param>
        /// <returns></returns>
        public string GetRequirement(string EventID, int TargetActorID)
        {
            string result = "";
            foreach (var i in Requirements)
            {
                switch (i.Type)
                {
                    case Requirement.RequirementTarget.Time:
                        result += $"\n{(DateFile.instance.dayTime < i.ValueA ? No : Yes)}可用时间不低于 {i.ValueA}";
                        break;
                    case Requirement.RequirementTarget.Item:
                        int itemKey = -1;
                        foreach (var item in DateFile.instance.actorItemsDate[i.ValueB == 0 ? DateFile.instance.MianActorID() : TargetActorID])
                            if (DateFile.instance.GetItemDate(item.Key, 999) == i.ValueA.ToString())
                            {
                                itemKey = item.Key;
                                break;
                            }
                        result += $"\n{(itemKey != -1 ? Yes : No)}" + (i.ValueB == 0 ? "太吾" : DateFile.instance.GetActorName(TargetActorID)) + $"拥有物品：\n {DateFile.instance.GetItemDate(i.ValueA, 0)}";
                        break;
                    case Requirement.RequirementTarget.Teammate:
                        var flag = DateFile.instance.GetFamily(getPrisoner: true).Contains(TargetActorID);
                        if (i.ValueA == 0)
                            result += $"\n{(flag ? No : Yes)}{DateFile.instance.GetActorName(TargetActorID)}不是同道";
                        else
                            result += $"\n{(flag ? Yes : No)}{DateFile.instance.GetActorName(TargetActorID)}是同道";
                        break;
                    case Requirement.RequirementTarget.Favor:
                        flag = true;
                        if (i.ValueB == 0)
                        {
                            if (DateFile.instance.GetActorFavor(false, DateFile.instance.mianActorId, TargetActorID, true) < i.ValueA)
                                flag = false;
                        }
                        else
                        {
                            if (DateFile.instance.GetActorFavor(false, DateFile.instance.mianActorId, TargetActorID, true) > i.ValueA)
                                flag = false;
                        }
                        result += $"\n{(flag ? Yes : No)}好感" + (i.ValueB == 0 ? "达到或超过" : "只有或没有") + DateFile.instance.Color5(0, otherMassage: true, i.ValueA);
                        break;
                    case Requirement.RequirementTarget.CountPerMonth:
                        int count = 0;
                        if (RuntimeConfig.ChoiceCount.ContainsKey(EventID))
                            RuntimeConfig.ChoiceCount[EventID].TryGetValue(TargetActorID, out count);
                        result += $"\n{ (count < i.ValueA ? Yes : No) }每月次数限制：{ count }/{ i.ValueA }";
                        break;
                    case Requirement.RequirementTarget.TaiwuResource:
                    case Requirement.RequirementTarget.TargetResource:
                        flag = i.Type == Requirement.RequirementTarget.TaiwuResource;
                        result += $"\n{ ((DateFile.instance.GetActorDate(flag ? DateFile.instance.mianActorId : TargetActorID, i.ValueA).ParseInt() >= i.ValueB) ? Yes : No) }" +
                            (flag ? "太吾" : DateFile.instance.GetActorName(TargetActorID))
                            + $"的{RuntimeConfig.ResourceName[i.ValueA]}资源数量达到或超过{i.ValueB}";
                        break;
                    case Requirement.RequirementTarget.BasePartValue:
                        flag = true;
                        int id = DateFile.instance.GetActorDate(TargetActorID, 19, false).ParseInt();
                        var value = DateFile.instance.GetBasePartValue(DateFile.instance.GetGangDate(id, 11).ParseInt(), DateFile.instance.GetGangDate(id, 3).ParseInt());
                        if (i.Type == 0)
                        {
                            if (value < i.ValueA)
                                flag = false;
                        }
                        else if (value > i.ValueA)
                            flag = false;
                        result += $"\n{ (flag ? Yes : No) }地区恩义达到或超过{ i.ValueA / 10f:n1}";
                        break;
                    case Requirement.RequirementTarget.NotOverTeammateMaxValue:
                        flag = DateFile.instance.GetFamily(getPrisoner: true).Count < DateFile.instance.GetMaxFamilySize();
                        result += $"\n{ (flag ? Yes : No) }同道成员数量小于{ DateFile.instance.GetMaxFamilySize() }";
                        break;
                    default:
                        break;
                }
            }

            var call = RequirementCall?.Call(EventID, TargetActorID) as List<KeyValuePair<bool, string>>;
            if (call != null)
                foreach (var i in call)
                    result += $"\n{(i.Key ? Yes : No)}{i.Value}";

            return result.Length > 0 ? result.Substring(1) : "";
        }
    }
}
