using GameData;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaiwuUIKit.GameObjects;
using UnityEngine;
using UnityUIKit.Core;
using UnityUIKit.Core.GameObjects;
using UnityUIKit.GameObjects;
using YanLib.Core;

namespace YanLib.DataManipulator
{
    /// <summary>
    /// 对于角色的数据操作
    /// </summary>
    public static class Actor
    {
        /// <summary>
        /// 删除角色，彻底的删除
        /// </summary>
        /// <param name="ActorID">被删的可怜人的 ID</param>
        public static void DelActor(int ActorID)
        {
            if (!Characters.HasChar(ActorID))
                return;

            DateFile.instance.MoveOutPlace(ActorID);

            HashSet<int> TempIDList = new HashSet<int>();
            foreach (var socialType in DateFile.instance.socialOnly)
                switch (socialType.Key)
                {
                    case 301:
                    case 302:
                    case 306:
                    case 308:
                    case 309:
                        foreach (var i in GetSpecifiedLifeData(ActorID, socialType.Key))
                            DelSocial(i.Key, ActorID, socialType.Key);
                        break;

                    case 310:
                        TempIDList.Clear();
                        foreach (var i in GetSpecifiedLifeData(ActorID, 310))
                        {
                            foreach (var ii in i.Value)
                                TempIDList.Add(ii);
                            DelSocial(i.Key, ActorID, socialType.Key);
                        }

                        //孩子的父母
                        foreach (var ChildID in TempIDList)
                        {
                            //义父义母
                            foreach (var i in GetSpecifiedLifeData(ChildID, 304))
                                DelSocial(i.Key, ChildID, 304, ActorID);
                            //亲生父母
                            foreach (var i in GetSpecifiedLifeData(ChildID, 303))
                                DelSocial(i.Key, ChildID, 303, ActorID);
                        }
                        break;

                    case 303:
                    case 304:
                        TempIDList.Clear();
                        foreach (var i in GetSpecifiedLifeData(ActorID, socialType.Key))
                        {
                            foreach (var ii in i.Value)
                                TempIDList.Add(ii);
                            DelSocial(i.Key, ActorID, socialType.Key);
                        }
                        foreach (var ParentID in TempIDList)
                            foreach (var i in GetSpecifiedLifeData(ParentID, 310))
                                DelSocial(i.Key, ParentID, 310, ActorID);
                        break;

                    default:
                        //优化最不 OK 的一段
                        foreach (var actorId in DateFile.instance.actorLife.Keys)
                            foreach (var social in GetSpecifiedLifeData(actorId, socialType.Key))
                                foreach (var Id in social.Value)
                                    if (Id == ActorID)
                                    {
                                        DelSocial(social.Key, actorId, socialType.Key, ActorID);
                                        break;
                                    }
                        foreach (var i in GetSpecifiedLifeData(ActorID, socialType.Key))
                            DelSocial(i.Key, ActorID, socialType.Key);
                        break;
                }

            foreach (var actorId in DateFile.instance.actorLife.Keys)
                if (DateFile.instance.actorLife[actorId].TryGetValue(801, out var value))
                    value.Remove(ActorID);

            TempIDList.Clear();
            foreach (var i in GetLifeRecords(ActorID, ParamTypes: new List<int>() { 0 }))
            {
                string text = DateFile.instance.actorMassageDate[i.messageId][2];
                List<int> paramType = (text == "-1") ? new List<int>() : Enumerable.ToList(Enumerable.Select(text.Split('|'), int.Parse));
                var index = paramType.FindIndex(pt => pt == 0);
                switch(index)
                {
                    case 0:
                        index = i.param0;
                        break;
                    case 1:
                        index = i.param1;
                        break;
                    case 2:
                        index = i.param2;
                        break;
                    default:
                        continue;
                }
                TempIDList.Add(index);
            }

            foreach(var i in TempIDList)
                DelLifeRecord(i, ParamType: 0, ParamValue: ActorID);


            DateFile.instance.deadActors.Remove(ActorID);
            DateFile.instance.RemoveActor(new List<int>() { ActorID }, true, false);
            DateFile.instance.DeleteActor(ActorID);
            RuntimeConfig.TraverserLifeRecordFix.Remove(ActorID);

            RuntimeConfig.EmptyActorId.Push(ActorID);
        }

        /// <summary>
        /// 获取 Actor 的关系
        /// 返回值：Key 为 关系的 ID，Value 为关系中的 ActorID
        /// </summary>
        /// <param name="ActorId">角色的 ID</param>
        /// <param name="SocialTyp">关系的类型</param>
        /// <returns></returns>
        public static Dictionary<int, List<int>> GetSpecifiedLifeData(int ActorId, int SocialTyp)
        {
            Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
            if (DateFile.instance.HaveLifeDate(ActorId, SocialTyp))
            {
                List<int> lifeDateList = DateFile.instance.GetLifeDateList(ActorId, SocialTyp, alreadyCheckHaveLife: true);
                foreach (int item in lifeDateList)
                {
                    if (!DateFile.instance.actorSocialDate.ContainsKey(item))
                        dictionary[item] = new List<int>();
                    else
                        dictionary[item] = DateFile.instance.actorSocialDate[item];
                }
            }
            return dictionary;
        }

        /// <summary>
        /// 删除关系
        /// </summary>
        /// <param name="SocialId">关系 ID</param>
        /// <param name="ActorAId">发起者（即储存在谁那）</param>
        /// <param name="SocialTyp">关系类型（饼：后期可能会去改成枚举）</param>
        /// <param name="ActorBId">被移除掉的对象（默认为 ActorA ）</param>
        public static void DelSocial(int SocialId, int ActorAId, int SocialTyp, int ActorBId = -1)
        {
            if (!DateFile.instance.HaveLifeDate(ActorAId, SocialTyp)
                || !DateFile.instance.actorSocialDate.ContainsKey(SocialTyp)
                || SocialTyp == 601
                || SocialTyp == 602)
                return;

            if (ActorBId < 0)
                ActorBId = ActorAId;

            if (!DateFile.instance.actorSocialDate.ContainsKey(SocialId))
            {
                if (DateFile.instance.HaveLifeDate(ActorBId, SocialTyp))
                    DateFile.instance.actorLife[ActorBId][SocialTyp].Remove(SocialId);

                if (DateFile.instance.HaveLifeDate(ActorAId, SocialTyp))
                    DateFile.instance.actorLife[ActorAId][SocialTyp].Remove(SocialId);

                return;
            }

            if (DateFile.instance.socialOnly[SocialTyp][0] == false && DateFile.instance.socialOnly[SocialTyp][1])
            {
                foreach (var ID in DateFile.instance.actorSocialDate[SocialId])
                    if (DateFile.instance.HaveLifeDate(ID, SocialTyp))
                        DateFile.instance.actorLife[ID][SocialTyp].Remove(SocialId);

                DateFile.instance.actorSocialDate[SocialId].Clear();
                RuntimeConfig.EmptySocialId.Push(SocialId);
            }
            else
            {
                DateFile.instance.actorSocialDate[SocialId].Remove(ActorBId);

                if (DateFile.instance.socialOnly[SocialTyp][1] && ActorAId != ActorBId)
                    if (DateFile.instance.actorLife[ActorBId].TryGetValue(SocialTyp,out var value))
                        value.Remove(SocialId);

                if (DateFile.instance.actorSocialDate[SocialId].Count <= 0
                    || (DateFile.instance.socialOnly[SocialTyp][1] && ActorAId == ActorBId))
                {
                    if (DateFile.instance.actorLife[ActorAId].TryGetValue(SocialTyp, out var value))
                        value.Remove(SocialId);

                    DateFile.instance.actorSocialDate[SocialId].Clear();
                    RuntimeConfig.EmptySocialId.Push(SocialId);
                }
            }
        }

        /// <summary>
        /// 删除角色生平经历
        /// </summary>
        /// <param name="ActorID">被删的角色 ID</param>
        /// <param name="Record">对应的记录</param>
        public static void DelLifeRecord(int ActorID, LifeRecords.LifeRecord Record)
        {
            DelLifeRecord(ActorID, new LifeRecords.LifeRecord[] { Record });
        }

        /// <summary>
        /// 删除角色生平经历
        /// </summary>
        /// <param name="ActorID">被删的角色 ID</param>
        /// <param name="Records">被删掉的记录数组</param>
        public static void DelLifeRecord(int ActorID, LifeRecords.LifeRecord[] Records)
        {
            var records = LifeRecords.GetAllRecords(ActorID).ToList();
            records.RemoveAll(i => Records.Contains(i));
            LifeRecords.RemoveRecords(ActorID);
            LifeRecords.AddRecords(ActorID, records.ToArray());
        }

        /// <summary>
        /// 删除角色生平经历
        /// </summary>
        /// <param name="ActorID">被删的角色 ID</param>
        /// <param name="RecordType">被删掉的记录类型</param>
        public static void DelLifeRecord(int ActorID, int RecordType)
        {
            var records = LifeRecords.GetAllRecords(ActorID).ToList();
            records.RemoveAll(i => i.messageId == RecordType);
            LifeRecords.RemoveRecords(ActorID);
            LifeRecords.AddRecords(ActorID, records.ToArray());
        }

        /// <summary>
        /// 删除角色生平经历，只要对应内容匹配就会删掉
        /// </summary>
        /// <param name="ActorID">被删的角色 ID</param>
        /// <param name="RecordType">-1 = 任意，被删掉的记录类型</param>
        /// <param name="ParamType">-1 = 任意，参数类型</param>
        /// <param name="ParamValue">必填，参数内容</param>
        public static void DelLifeRecord(int ActorID, int RecordType = -1, int ParamType = -1, int ParamValue = -1)
        {
            var temp = DelLifeRecord(GetLifeRecords(ActorID), RecordType, ParamType, ParamValue).ToArray();
            LifeRecords.RemoveRecords(ActorID);
            LifeRecords.AddRecords(ActorID, temp);
        }

        /// <summary>
        /// 删除提供的记录中符合条件的记录
        /// </summary>
        /// <param name="LifeRecords"></param>
        /// <param name="RecordType">-1 = 任意，被删掉的记录类型</param>
        /// <param name="ParamType">-1 = 任意，参数类型</param>
        /// <param name="ParamValue">必填，参数内容</param>
        /// <returns></returns>
        public static List<LifeRecords.LifeRecord> DelLifeRecord(List<LifeRecords.LifeRecord> LifeRecords, int RecordType = -1, int ParamType = -1, int ParamValue = -1)
        {
            if (ParamType < 0 && ParamValue < 0 && RecordType < 0)
                return new List<LifeRecords.LifeRecord>();

            var records = LifeRecords;
            records.RemoveAll(i =>
            {
                if (RecordType > 0 && RecordType != i.messageId)
                    return false;
                if (ParamType >= 0)
                {
                    string text = DateFile.instance.actorMassageDate[i.messageId][2];
                    int[] array = (text == "-1") ? new int[] { } : Enumerable.ToArray(Enumerable.Select(text.Split('|'), int.Parse));
                    if (!array.Contains(ParamType))
                        return false;
                }
                if (ParamValue >= 0 && i.param0 != ParamValue && i.param1 != ParamValue && i.param2 != ParamValue)
                    return false;
                return true;
            });

            return records;
        }

        /// <summary>
        /// 获取角色生平经历
        /// </summary>
        /// <param name="ActorID">角色 ID</param>
        /// <param name="RecordTypes">null 为任意，获取的记录类型</param>
        /// <param name="ParamTypes">null 为任意，参数类型</param>
        /// <param name="ParamValues">null 为任意，参数内容</param>
        public static List<LifeRecords.LifeRecord> GetLifeRecords(int ActorID, List<int> RecordTypes = null, List<int> ParamTypes = null, List<int> ParamValues = null)
        {
            var result = new List<LifeRecords.LifeRecord>();
            if ((RecordTypes?.Count ?? 0) == 0)
                RecordTypes = null;
            if ((ParamTypes?.Count ?? 0) == 0)
                ParamTypes = null;
            if ((ParamValues?.Count ?? 0) == 0)
                ParamValues = null;
            if (ParamValues == null && ParamTypes == null && RecordTypes == null)
                return LifeRecords.GetAllRecords(ActorID)?.ToList() ?? result;
            var temp = LifeRecords.GetAllRecords(ActorID)?.ToList() ?? result;
            foreach (var i in temp)
            {
                if (RecordTypes != null && !RecordTypes.Contains(i.messageId))
                    continue;

                if(ParamTypes != null)
                {
                    string text = DateFile.instance.actorMassageDate[i.messageId][2];
                    int[] pramType = (text == "-1") ? new int[] { } : Enumerable.ToArray(Enumerable.Select(text.Split('|'), int.Parse));
                    bool ParmTypeContains = false;
                    foreach (var ii in ParamTypes)
                        if (pramType.Contains(ii))
                        {
                            ParmTypeContains = true;
                            break;
                        }
                    if (!ParmTypeContains)
                        continue;
                }

                if (ParamTypes != null && !ParamTypes.Contains(i.param0) && !ParamTypes.Contains(i.param1) && !ParamTypes.Contains(i.param2))
                    continue;

                result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// 导出人物数据
        /// </summary>
        /// <param name="ActorID">导出的角色</param>
        /// <param name="DumpLifeRecord">是否导出记录</param>
        /// <param name="FixedActorName">是否导出生平中角色 ID 对应的名字</param>
        /// <param name="FileName">导出的名字</param>
        /// <returns></returns>
        public static string DumpActorData(int ActorID, bool DumpLifeRecord = true, bool FixedActorName = false, string FileName = "")
        {
            var Address = Path.Combine(ArchiveSystem.SaveGame.GetSavingRootDir(), $"Date_{SaveDateFile.instance.dateId}");
            if (string.IsNullOrEmpty(FileName))
                FileName = DateFile.instance.GetActorName(ActorID);
            Address = Path.Combine(Address, FileName + $"{ActorID}.json");

            ActorAllData AllData = new ActorAllData();

            AllData.LifeRecords = DumpLifeRecord ? (FixedActorName ? GetLifeRecords(ActorID) : DelLifeRecord(GetLifeRecords(ActorID),RecordType: 0)) : new List<LifeRecords.LifeRecord>();

            AllData.LifeRecordFix = new Dictionary<int, string>();
            AllData.InjuryData = DateFile.instance.actorInjuryDate[ActorID];
            AllData.StadyData = DateFile.instance.actorStudyDate[ActorID];
            AllData.ItemsData = DateFile.instance.actorItemsDate[ActorID];
            AllData.EquipGongFas = DateFile.instance.actorEquipGongFas[ActorID];
            AllData.GongFas = DateFile.instance.actorGongFas[ActorID];
            AllData.ActorData = new Dictionary<int, string>();

            foreach (var key in RuntimeConfig.ActorDataKeyUsed)
                if (Characters.HasCharProperty(ActorID, key))
                    AllData.ActorData[key] = Characters.GetCharProperty(ActorID, key);

            if(FixedActorName)
            {
                for(var i = 0; i < AllData.LifeRecords.Count; i++)
                {
                    string text = DateFile.instance.actorMassageDate[AllData.LifeRecords[i].messageId][2];
                    var paramType = (text == "-1") ? new List<int>() : Enumerable.ToList(Enumerable.Select(text.Split('|'), int.Parse));
                    if (!paramType.Contains(0))
                        continue;

                    object temp = AllData.LifeRecords[i];
                    for (int index = 0; index < paramType.Count; index++)
                        if(paramType[index] == 0)
                        {
                            var paramInfo = typeof(LifeRecords.LifeRecord).GetField("param" + index.ToString());
                            var value = (int)paramInfo.GetValue(temp);
                            value = Math.Abs(value);
                            paramInfo.SetValue(temp, -value);

                            AllData.LifeRecordFix[-value] =  DateFile.instance.GetActorName(value);
                        }

                    AllData.LifeRecords[i] = (LifeRecords.LifeRecord)temp;
                }
            }

            var result = JsonConvert.SerializeObject(AllData);

            var file = File.CreateText(Address);
            file.Write(result);
            file.Flush();
            file.Close();

            return result;
        }

        /// <summary>
        /// 用以储存 Actor 的所有数据
        /// </summary>
        public struct ActorAllData
        {
            /// <summary>
            /// 生平经历
            /// </summary>
            public List<LifeRecords.LifeRecord> LifeRecords;
            /// <summary>
            /// 生平修正，储存着 ID 与对应的名字
            /// </summary>
            public Dictionary<int,string> LifeRecordFix;

            /// <summary>
            /// 受伤数据
            /// </summary>
            public Dictionary<int, int> InjuryData;

            /// <summary>
            /// 
            /// </summary>
            public List<int[]> StadyData;

            /// <summary>
            /// 持有物品
            /// </summary>
            public Dictionary<int, int> ItemsData;

            /// <summary>
            /// 
            /// </summary>
            public Dictionary<int, int[]> EquipGongFas;

            /// <summary>
            /// 
            /// </summary>
            public SortedDictionary<int, int[]> GongFas;

            /// <summary>
            /// 独属于一个人的数据
            /// </summary>
            public Dictionary<int, string> ActorData;
        }

        /// <summary>
        /// 加载角色数据到存档中
        /// </summary>
        /// <param name="DataJson"></param>
        public static void LoadActorData(string DataJson)
        {
            ActorAllData allData = JsonConvert.DeserializeObject<ActorAllData>(DataJson);
            int ActorID = 0;
            if (RuntimeConfig.EmptyActorId.Count == 0)
                ActorID = (int)AccessTools.Method(typeof(DateFile), "GetNewActorId").Invoke(DateFile.instance, new object[] { });
            else
                ActorID = RuntimeConfig.EmptyActorId.Pop();
            Characters.AddChar(ActorID, allData.ActorData);
            LifeRecords.AddRecords(ActorID, allData.LifeRecords.ToArray());
            RuntimeConfig.TraverserLifeRecordFix.Add(ActorID, allData.LifeRecordFix);

            DateFile.instance.actorInjuryDate[ActorID] = allData.InjuryData;
            DateFile.instance.actorStudyDate[ActorID] = allData.StadyData;
            DateFile.instance.actorItemsDate[ActorID] = allData.ItemsData;
            DateFile.instance.actorEquipGongFas[ActorID] = allData.EquipGongFas;
            DateFile.instance.actorGongFas[ActorID] = allData.GongFas;
            DateFile.instance.actorLife[ActorID] = new Dictionary<int, List<int>>();
            DateFile.instance.MoveOutPlace(ActorID);

            var place = DateFile.instance.GetActorAtPlace(DateFile.instance.mianActorId);
            DateFile.instance.MoveToPlace(place[0], place[1], ActorID, true);
        }

        /// <summary>
        /// 删除经历适配用的
        /// </summary>
        /// <param name="ActorID"></param>
        public static void DelTraverserLifeRecordFix(int ActorID)
        {
            if (RuntimeConfig.TraverserLifeRecordFix.ContainsKey(ActorID))
                RuntimeConfig.TraverserLifeRecordFix.Remove(ActorID);
        }
    }
}
