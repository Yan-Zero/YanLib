using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YanLib.Core;

namespace YanLib.EventSystem.WulinGeneral
{
#if DEBUG
    /// <summary>
    /// 武林大会相关
    /// </summary>
    public static class Part_1
    {
        private static int _resKey = 0;

        /// <summary>
        /// 送出队友
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TargetID"></param>
        /// <param name="TeammateID"></param>
        /// <returns></returns>
        public static void GiveTeammate(string ID, int TargetID, int TeammateID)
        {
            WuLinGeneralAssembly.Instance.SetFamilyActorToGang(TeammateID, DateFile.instance.GetActorGangId(TargetID));
        }
        /// <summary>
        /// 动态生成的按钮，很耗资源的，别学
        /// </summary>
        /// <param name="EventID"></param>
        /// <returns></returns>
        public static AdditionalChoices GiveResourceChoice(string EventID)
        {
            var result = new AdditionalChoices()
            {
                Index = 1
            };
            var needTime = new ChoiceRequirement.Requirement()
            {
                Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
                ValueA = 2
            };
            for (var i = 401; i <= 405; i++)
            {
                result.Choices.Add(new EventChoice()
                {
                    Desc = $"（捐赠{RuntimeConfig.ResourceName[i]}……）",
                    CanChooseRequirement = new ChoiceRequirement()
                    {
                        Requirements = new List<ChoiceRequirement.Requirement>()
                        {
                            needTime,
                            new ChoiceRequirement.Requirement()
                            {
                                Type = ChoiceRequirement.Requirement.RequirementTarget.TaiwuResource,
                                ValueA = i,
                                ValueB = 500
                            }
                        }
                    },
                    Effect = new ChoiceEnd()
                    {
                        ToID = "WulinGeneral.5",
                        CallEnd = new CallInfo()
                        {
                            TargetType = typeof(Part_1),
                            TargetMethod = "SetResourceChoice",
                            Params = new List<CallInfo.ParamInfo>()
                            {
                                new CallInfo.ParamInfo()
                                {
                                    Type = CallInfo.ParamInfo.ParamType.Int,
                                    Value = i.ToString()
                                }
                            }
                        }
                    }
                });
            }
            return result;
        }
        /// <summary>
        /// 充分利用手动控制的优势
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="ResKey"></param>
        public static void SetResourceChoice(string EventID,int ResKey)
        {
            _resKey = ResKey;
        }
        /// <summary>
        /// 给予资源
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="TargetID"></param>
        /// <param name="InputText"></param>
        public static void GiveResource(string EventID,int TargetID,string InputText)
        {
            int count = InputText.ParseInt();
            UIDate.instance.ChangeResource(DateFile.instance.mianActorId, _resKey - 401, -count);
            WuLinGeneralAssembly.Instance.ResourceDonation(DateFile.instance.GetActorGangId(TargetID), count * 4, 1);
        }
    }
#endif
}