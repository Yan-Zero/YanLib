using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using v2;
using YanLib.Core;
using YanLib.DataManipulator;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 事件
    /// </summary>
    public class Event
    {
        internal static GameObject _choice1Object = null;
        internal static GameObject _choice2Object = null;


        /// <summary>
        /// 包含命名空间的ID
        /// </summary>
        public string ID;

        /// <summary>
        /// 事件显示的文本，没有自动解析
        /// </summary>
        public string Desc = "";
        /// <summary>
        /// 动态修改显示
        /// </summary>
        public CallInfo DynamicDesc;

        /// <summary>
        /// 背景 ID
        /// </summary>
        public int BackgroundID;

        /// <summary>
        /// 事件的选项
        /// </summary>
        public List<EventChoice> Choices;
        /// <summary>
        /// 动态选项，返回值为 List&lt;EventChoice&gt;
        /// </summary>
        public CallInfo DynamicChoices;

        /// <summary>
        /// 输入框信息
        /// </summary>
        public InputFieldInfo InputFieldInfo;
        /// <summary>
        /// 动态的，如果有就不访问 InputFieldInfo
        /// </summary>
        public CallInfo DynamicInputFieldInfo;

        /// <summary>
        /// 显示事件
        /// </summary>
        internal void ShowUI(int TargetActorID)
        {
            if (ui_MessageWindow.Instance.showActorMenuButton != null)
                ui_MessageWindow.Instance.showActorMenuButton.SetActive(TargetActorID != DateFile.instance.mianActorId 
                    && int.Parse(DateFile.instance.GetActorDate(TargetActorID, 8, applyBonus: false)) == 1);
            if (ui_MessageWindow.Instance.actorFavor != null)
                ui_MessageWindow.Instance.actorFavor.SetActive(value: false);
            if (ui_MessageWindow.Instance.actorGoodIcon != null)
                ui_MessageWindow.Instance.actorGoodIcon.SetActive(value: false);
            if (ui_MessageWindow.Instance.actorWariness != null)
                ui_MessageWindow.Instance.actorWariness.SetActive(value: false);
            if (ui_MessageWindow.Instance.actorLifeFace != null)
                ui_MessageWindow.Instance.actorLifeFace.SetActive(value: false);

            if (BackgroundID > 0)
                ui_MessageWindow.Instance.eventBackImage.sprite = Resources.Load<Sprite>("Graphics/EventBack/EventBack_" + BackgroundID);
            else
                ui_MessageWindow.Instance.eventBackImage.sprite = Resources.Load<Sprite>("Graphics/EventBack/EventBack_101_" + DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, 82));

            int num12 = 0;
            int taiwuClothes = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 305));
            if (DateFile.instance.HaveLifeDate(TargetActorID, 1001) && DateFile.instance.GetLifeDate(TargetActorID, 1001, 1) >= 100)
                num12 = DateFile.instance.GetLifeDate(TargetActorID, 1001, 0);
            else if (taiwuClothes > 0)
                num12 = int.Parse(DateFile.instance.GetItemDate(taiwuClothes, 15));

            if (TargetActorID > 0 && TargetActorID != DateFile.instance.mianActorId)
            {
                if (int.Parse(DateFile.instance.GetActorDate(TargetActorID, 8, applyBonus: false)) == 1)
                {
                    if (int.Parse(DateFile.instance.GetActorDate(TargetActorID, 26, applyBonus: false)) == 0)
                    {
                        if (!DateFile.instance.HaveLifeDate(TargetActorID, 1001))
                            DateFile.instance.ResetActorLifeFace(TargetActorID, num12, 5);
                        if (int.Parse(DateFile.instance.GetActorDate(TargetActorID, 3, applyBonus: false)) == -1)
                        {
                            DateFile.instance.ChangeFavor(TargetActorID, 0);
                            DateFile.instance.AddActorScore(1);
                        }
                    }
                    if (ui_MessageWindow.Instance.actorFavor != null)
                    {
                        ui_MessageWindow.Instance.actorFavor.SetActive(true);
                        if (int.Parse(DateFile.instance.GetActorDate(TargetActorID, 47, applyBonus: false)) > 0 
                            && int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(TargetActorID, 19, applyBonus: false)), 2)) == 1)
                        {
                            ui_MessageWindow.Instance.actorGoodIcon.SetActive(value: true);
                            ui_MessageWindow.Instance.actorPartValue.text = "+" + (DateFile.instance.GetActorPartValue(TargetActorID) / 10f).ToString("f1") + "%";
                        }
                        ui_MessageWindow.Instance.UpdateActorFavorBar(TargetActorID);
                    }
                    if (ui_MessageWindow.Instance.actorWariness != null)
                    {
                        ui_MessageWindow.Instance.actorWariness.SetActive(value: true);
                        ui_MessageWindow.Instance.UpdateActorWarinessBar(TargetActorID);
                    }
                    if (ui_MessageWindow.Instance.actorLifeFace != null)
                    {
                        ui_MessageWindow.Instance.actorLifeFace.name = "LifeFaceIcon,525," + TargetActorID;
                        ui_MessageWindow.Instance.actorLifeFace.SetActive(value: true);
                        ui_MessageWindow.Instance.UpdateActorFaceBar(TargetActorID);
                    }
                }
                if (TargetActorID >= 1501 && TargetActorID <= 1509 && ui_MessageWindow.Instance.actorFavor != null)
                {
                    ui_MessageWindow.Instance.actorFavor.SetActive(value: true);
                    JuniorXiangshuSystem instance = SingletonObject.getInstance<JuniorXiangshuSystem>();
                    int juniorXiangshuIdByPresetActorId = instance.GetJuniorXiangshuIdByPresetActorId(TargetActorID);
                    JuniorXiangshuSystem.JuniorXiangshuDisplayInfo displayInfo = instance.GetDisplayInfo(juniorXiangshuIdByPresetActorId);
                    ui_MessageWindow.Instance.actorFavorBar1.fillAmount = displayInfo.actorFavor / 30000f;
                    ui_MessageWindow.Instance.actorFavorBar2.fillAmount = (displayInfo.actorFavor - 30000) / 30000f;
                }
            }

            if (TargetActorID != -99)
            {
                int num14 = Mathf.Abs(TargetActorID);
                if (ui_MessageWindow.Instance.mianActorName != null)
                    ui_MessageWindow.Instance.mianActorName.SetActive(value: true);
                string actorGeneration = DateFile.instance.GetActorGeneration(num14, 999, massageShow: true);
                if (actorGeneration != "")
                {
                    ui_MessageWindow.Instance.mianActorGenerationText.gameObject.SetActive(value: true);
                    ui_MessageWindow.Instance.mianActorGenerationText.text = actorGeneration;
                }
                else
                    ui_MessageWindow.Instance.mianActorGenerationText.gameObject.SetActive(value: false);
                ui_MessageWindow.Instance.mianActorNameText.text = ((TargetActorID < 0) ? (DateFile.instance.GetActorDate(num14, 28, applyBonus: false) + WindowManage.instance.Mut()) : (DateFile.instance.GetActorName(num14) + WindowManage.instance.Mut()));
                ui_MessageWindow.Instance.mianActorFace.SetActorFace(num14);
            }
            else
            {
                if (ui_MessageWindow.Instance.mianActorGenerationText != null)
                    ui_MessageWindow.Instance.mianActorGenerationText.gameObject.SetActive(value: false);
                if (ui_MessageWindow.Instance.mianActorName != null)
                    ui_MessageWindow.Instance.mianActorName.SetActive(value: false);
            }

            var chooseHolder = ui_MessageWindow.Instance.chooseHolder;
            for (int i = 0; i < chooseHolder.childCount; i++)
                GameObject.Destroy(chooseHolder.GetChild(i).gameObject);

            EventMisc.QuQuCall = false;
            EventMisc.battleQuquCall = false;
            var inputInfo = (InputFieldInfo)DynamicInputFieldInfo?.Call(ID, TargetActorID) ?? InputFieldInfo;
            if (ui_MessageWindow.Instance.inputTextField != null)
            {
                if(inputInfo == null || !inputInfo.UseInputField)
                {
                    ui_MessageWindow.Instance.inputMassageText.text = "";
                    ui_MessageWindow.Instance.inputTextField.gameObject.SetActive(false);
                    ui_MessageWindow.Instance.mianMassageText.text = GetDesc(TargetActorID);
                }
                else
                {
                    RuntimeConfig.ChoiceEnvironment.InputFieldInfo = inputInfo;
                    ui_MessageWindow.Instance.inputMassageText.text = GetDesc(TargetActorID);
                    ui_MessageWindow.Instance.mianMassageText.text = " ";
                    ui_MessageWindow.Instance.inputTextField.inputType = inputInfo.InputType;
                    ui_MessageWindow.Instance.inputTextField.characterValidation = inputInfo.CharacterValidation;
                    ui_MessageWindow.Instance.inputTextField.contentType = inputInfo.ContentType;
                    ui_MessageWindow.Instance.inputTextField.gameObject.SetActive(true);
                    ui_MessageWindow.Instance.inputTextField.text = "";
                }
            }

            ui_MessageWindow.Instance.mianMassageShowText.text = ui_MessageWindow.Instance.mianMassageText.text;
            ui_MessageWindow.Instance.mianMassageText.GetComponent<UnityEngine.UI.ContentSizeFitter>().SetLayoutVertical();

            var dChoices = DynamicChoices?.Call(ID, TargetActorID) as AdditionalChoices;
            var choice = new List<EventChoice>(Choices);
            if(dChoices != null)
            {
                var index = dChoices.Index;
                if (index < 0)
                    index += choice.Count + 1;
                if (index < 0)
                    index = 0;
                if (index > choice.Count)
                    index = choice.Count;
                choice.InsertRange(index, dChoices.Choices);
            }

            for (var index = 0; index < choice.Count;index++)
                choice[index].DrawChoice(TargetActorID, $"{ ID }:{ index }");
           
        }



        /// <summary>
        /// 唤起事件
        /// </summary>
        /// <param name="TargetActorID">目标人物 ID</param>
        public void CallEvent(int TargetActorID)
        {
            RuntimeConfig.CurEvent = this;
            UIManager.Instance.AddUI("ui_MessageWindow", new int[] { 0, TargetActorID, 1, int.MinValue });
        }

        /// <summary>
        /// 获取到解析后的事件描述
        /// </summary>
        /// <param name="TargetActorID">目标角色 ID</param>
        /// <returns>事件描述</returns>
        public string GetDesc(int TargetActorID)
        {
            var result = Desc;
            result = EventHelper.AnalyzeDesc(ID, TargetActorID, result);
            result = (DynamicDesc?.Call(ID, TargetActorID, result) as string) ?? result;
            return result;
        }
    }


    internal static class EventMisc
    {
        private static FieldInfo _QuQuCall = AccessTools.Field(typeof(ui_MessageWindow), "QuQuCall");
        private static FieldInfo _battleQuquCall = AccessTools.Field(typeof(ui_MessageWindow), "battleQuquCall");
        private static FieldInfo _inputMax = AccessTools.Field(typeof(ui_MessageWindow), "inputMax");
        private static FieldInfo _inputMin = AccessTools.Field(typeof(ui_MessageWindow), "inputMin");

        public static bool QuQuCall
        {
            get => (bool)_QuQuCall.GetValue(ui_MessageWindow.Instance);
            set => _QuQuCall.SetValue(ui_MessageWindow.Instance, value);
        }
        public static bool battleQuquCall
        {
            get => (bool)_battleQuquCall.GetValue(ui_MessageWindow.Instance);
            set => _battleQuquCall.SetValue(ui_MessageWindow.Instance, value);
        }
        public static int inputMax
        {
            get => (int)_inputMax.GetValue(ui_MessageWindow.Instance);
            set => _inputMax.SetValue(ui_MessageWindow.Instance, value);
        }
        public static int inputMin
        {
            get => (int)_inputMin.GetValue(ui_MessageWindow.Instance);
            set => _inputMin.SetValue(ui_MessageWindow.Instance, value);
        }
    }
}
