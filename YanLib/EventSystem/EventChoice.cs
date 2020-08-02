using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityUIKit.Core;
using YanLib.Core;
using YanLib.DataManipulator;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 事件的选项
    /// </summary>
    public class EventChoice
    {
        /// <summary>
        /// Desc 原文本
        /// </summary>
        public string Desc = "";
        /// <summary>
        /// 动态修改显示
        /// </summary>
        public CallInfo DynamicDesc;
            
        /// <summary>
        /// 选项显示的 Tip
        /// </summary>
        public string Tip = "[Effect]";
        /// <summary>
        /// 动态修改显示 Tip
        /// </summary>
        public CallInfo DynamicTip;

        /// <summary>
        /// 选择效果
        /// </summary>
        public ChoiceEnd Effect = new ChoiceEnd();

        /// <summary>
        /// 显示的需求
        /// </summary>
        public ChoiceRequirement ShowRequirement = new ChoiceRequirement();
        /// <summary>
        /// 可以点击的需求
        /// </summary>
        public ChoiceRequirement CanChooseRequirement = new ChoiceRequirement();

        /// <summary>
        /// 选项头像风格 -99 为无，-1为太吾，0为对方
        /// </summary>
        public int ProfileStyle = -1;

        /// <summary>
        /// 选中该选项
        /// </summary>
        public void Choose(string EventID, int TargetActorID)
        {
            if(Effect.CallEnd != null)
            {
                foreach (var i in Effect.CallEnd.Params)
                    if (i.Type == CallInfo.ParamInfo.ParamType.ChooseItem || i.Type == CallInfo.ParamInfo.ParamType.ChooseActor)
                        if (!RuntimeConfig.ChoiceEnvironment.HasChoose)
                        {
                            RuntimeConfig.ChoiceEnvironment.GetWhoItem = int.Parse(i.Value ?? "0");
                            RuntimeConfig.ChoiceEnvironment.ChoiceChoose = Choose;
                            RuntimeConfig.ChoiceEnvironment.ID = EventID;
                            RuntimeConfig.ChoiceEnvironment.Filter = Effect.Filter?.Call(EventID, TargetActorID) as IEnumerable<int>;
                            if(i.Type == CallInfo.ParamInfo.ParamType.ChooseItem)
                                ui_MessageWindow.Instance.ShowItemsWindow(int.MinValue);
                            else
                                ui_MessageWindow.Instance.ShowActorsWindow(int.MinValue);
                            return;
                        }
            }
            if(ShowRequirement.HasCountPerMonth || CanChooseRequirement.HasCountPerMonth)
            {
                if (!RuntimeConfig.ChoiceCount.ContainsKey(EventID))
                    RuntimeConfig.ChoiceCount.Add(EventID, new Dictionary<int, int>());
                if (!RuntimeConfig.ChoiceCount[EventID].ContainsKey(TargetActorID))
                    RuntimeConfig.ChoiceCount[EventID].Add(TargetActorID, 0);
                RuntimeConfig.ChoiceCount[EventID][TargetActorID]++;
            }
            if(RuntimeConfig.ChoiceEnvironment.InputFieldInfo != null)
            {
                RuntimeConfig.ChoiceEnvironment.InputFieldInfo = null;
                ui_MessageWindow.Instance.inputTextField.inputType = UnityEngine.UI.InputField.InputType.Standard;
                ui_MessageWindow.Instance.inputTextField.characterValidation = UnityEngine.UI.InputField.CharacterValidation.None;
                ui_MessageWindow.Instance.inputTextField.contentType = UnityEngine.UI.InputField.ContentType.Standard;
            }

            Effect.End(EventID, TargetActorID);
        }

        /// <summary>
        /// 获取解析后的文本
        /// </summary>
        /// <returns>描述</returns>
        public string GetDesc(string EventID,int TargetActorID)
        {
            var result = Desc;
            result = EventHelper.AnalyzeDesc(EventID, TargetActorID, result);
            result = (DynamicDesc?.Call(EventID, TargetActorID, result) as string) ?? result;
            return result;
        }

        /// <summary>
        /// 获取到解析后的 Tip
        /// </summary>
        /// <param name="EventID"></param>
        /// <param name="TargetActorID"></param>
        /// <returns></returns>
        public string GetTip(string EventID,int TargetActorID)
        {
            var result = Tip;
            result = EventHelper.AnalyzeTip(EventID, this, TargetActorID, result);
            result = (DynamicTip?.Call(EventID, TargetActorID, result) as string) ?? result;
            return result;
        }

        /// <summary>
        /// 在选项区绘制按钮，请于 SetMessageWindow 中使用（SetMassageWindow）
        /// </summary>
        /// <param name="TargetActorID"></param>
        /// <param name="ChoiceID"></param>
        public void DrawChoice(int TargetActorID, string ChoiceID)
        {
            if (!ShowRequirement.Allow(ChoiceID, TargetActorID))
                return;

            GameObject choiceGameobject = GameObject.Instantiate((ProfileStyle != -99) ? Event._choice1Object : Event._choice2Object, Vector3.zero, Quaternion.identity);
            choiceGameobject.transform.SetParent(ui_MessageWindow.Instance.chooseHolder, worldPositionStays: false);
            int index = ChoiceID.Split(':').Last().ParseInt();
            choiceGameobject.name = $"Choose,{index}";

            var profilePictrueID = ProfileStyle == 0 ? TargetActorID : DateFile.instance.mianActorId;
            if (ProfileStyle != -99)
            {
                choiceGameobject.transform.Find("NameText").GetComponent<UnityEngine.UI.Text>().text = DateFile.instance.GetActorName(profilePictrueID) + WindowManage.instance.Mut();
                choiceGameobject.transform.Find("FaceHolder").Find("FaceMask").Find("MianActorFace")
                    .GetComponent<ActorFace>()
                    .SetActorFace(profilePictrueID);

                GameObject needIconGameObject = choiceGameobject.transform.Find("IconHolder").Find("NeedIcon").gameObject;

                var canChoose = CanChooseRequirement.GetRequirement(ChoiceID, TargetActorID);
                needIconGameObject.SetActive(!string.IsNullOrEmpty(canChoose));
                if (needIconGameObject.activeSelf)
                {
                    needIconGameObject.GetComponent<YanPointerEnter>().TipContent = canChoose;
                    choiceGameobject.GetComponent<UnityEngine.UI.Button>().interactable = CanChooseRequirement.Allow(ChoiceID, TargetActorID);
                }
            }

            var choiceText = choiceGameobject.transform.Find("MessageChooseText").GetComponent<UnityEngine.UI.Text>();
            choiceText.text = GetDesc(ChoiceID, TargetActorID);
            choiceGameobject.GetComponent<YanPointerEnter>().TipContent = GetTip(ChoiceID, TargetActorID);

            choiceGameobject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate ()
            {
                Choose(ChoiceID, TargetActorID);
            });
            if (RuntimeConfig.ChoiceEnvironment.InputFieldInfo != null && RuntimeConfig.ChoiceEnvironment.InputFieldInfo.ChoiceIndex == index)
            {
                choiceGameobject.GetComponent<UnityEngine.UI.Button>().interactable = false;
                choiceText.color = Color.red;
            }
        }
    }
}
