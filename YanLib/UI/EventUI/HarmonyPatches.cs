using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YanLib.Core;
using YanLib.DataManipulator;
using YanLib.EventSystem;
using YanLib.UI.EventUI;
using Event = YanLib.EventSystem.Event;

namespace YanLib.Core
{
    static partial class HarmonyPatches
    {

        private static bool EventUIShow(int[] baseEventDate, int chooseId)
        {
            if (!(baseEventDate.Length > 3 && baseEventDate[3] == int.MinValue) || RuntimeConfig.CurEvent == null)
                return true;
            ui_MessageWindow.Instance.eventMianActorId = baseEventDate[1];
            baseEventDate[3] = baseEventDate[1];
            MessageEventManager.Instance.massageActors.Clear();
            MessageEventManager.Instance.MainEventData = (int[])baseEventDate.Clone();
            MessageEventManager.Instance.massageActors.Add(MessageEventManager.Instance.MainEventData[1]);
            RuntimeConfig.CurEvent.ShowUI(baseEventDate[1]);
            return false;
        }

        private static void AdditionalChoiceDraw(int[] baseEventDate, int chooseId,ref RectTransform ___chooseHolder)
        {
            var eventID = MessageEventManager.Instance.MainEventData[2];

            if (chooseId > 0)
            {
                var toEvent = DateFile.instance.eventDate[chooseId][7].ParseInt();
                if (toEvent < -99)
                    eventID = Mathf.Abs(DateFile.instance.eventDate[chooseId][7].ParseInt());
                else if(toEvent < 0)
                    eventID = chooseId;
            }

            if(RuntimeConfig.AdditionalChoices.ContainsKey(eventID))
            {
                int indexChoice = 0;
                foreach(var i in RuntimeConfig.AdditionalChoices[eventID])
                {
                    var index = i.Index;
                    if (index < 0)
                        index += ___chooseHolder.childCount + 1;
                    if (index < 0)
                        index = 0;
                    if (index > ___chooseHolder.childCount)
                        index = ___chooseHolder.childCount;
                    foreach (var choice in i.Choices)
                    {
                        choice.DrawChoice(MessageEventManager.Instance.MainEventData[1], $"{eventID}:{indexChoice}");
                        ___chooseHolder.Find($"Choose,{indexChoice++}")?.SetSiblingIndex(index);
                    }
                }
            }

            if(YanLib.Settings.ChoiceHotkey.Value)
            {
                int i = 0;
                for (int index = 0; index < ___chooseHolder.childCount;++index)
                {
                    var choice = ___chooseHolder.GetChild(index);
                    if (!choice.gameObject.activeSelf)
                        continue;
                    if (i > 11)
                        break;
                    var choiceNo = choice.Find("MassageChooseNo.");
                    choiceNo.gameObject.SetActive(true);
                    choiceNo.GetComponent<Text>().text = $"<color=#FFFFFF>({MessageEventManager.Instance.massageKeyCodeName[i]}).</color>";
                    var hotkey = choice.GetComponent<HotkeyMonitor>();
                    hotkey.choiceIndex = i;
                    hotkey.choiceButton = choice.GetComponent<Button>();
                    ++i;
                }
            }
        }

        /// <summary>
        /// 选项的快捷键
        /// </summary>
        /// <returns></returns>
        private static bool OnChoose_Update()
        {
            return false;
        }

        private static void InitEvent_Postfix()
        {
            if (Event._choice1Object == null)
            {
                ui_MessageWindow.Instance.massageChoose1.transform.Find("MassageChooseNo.").gameObject.SetActive(false);
                ui_MessageWindow.Instance.massageChoose2.transform.Find("MassageChooseNo.").gameObject.SetActive(false);
                GameObject.Destroy(ui_MessageWindow.Instance.massageChoose1.GetComponent<PointerEnter>());
                GameObject.Destroy(ui_MessageWindow.Instance.massageChoose2.GetComponent<PointerEnter>());
                ui_MessageWindow.Instance.massageChoose1.AddComponent<YanPointerEnter>().TipTitle = "Tip";
                ui_MessageWindow.Instance.massageChoose2.AddComponent<YanPointerEnter>().TipTitle = "Tip";
                ui_MessageWindow.Instance.massageChoose1.AddComponent<HotkeyMonitor>();
                ui_MessageWindow.Instance.massageChoose2.AddComponent<HotkeyMonitor>();

                Event._choice1Object = GameObject.Instantiate(ui_MessageWindow.Instance.massageChoose1);
                Event._choice2Object = GameObject.Instantiate(ui_MessageWindow.Instance.massageChoose2);
                Event._choice1Object.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                Event._choice2Object.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                GameObject.Destroy(Event._choice1Object.GetComponent<OnChoose>());
                GameObject.Destroy(Event._choice2Object.GetComponent<OnChoose>());

                GameObject needIconGameObject = Event._choice1Object.transform.Find("IconHolder").Find("NeedIcon").gameObject;
                needIconGameObject.tag = "Untagged";
                GameObject.Destroy(needIconGameObject.GetComponent<PointerEnter>());
                var i = needIconGameObject.AddComponent<YanPointerEnter>();
                i.TipTitle = "需求";
                i.changeSize = 0.9f;
                i.restSize = 0.8f;

                var Message1 = Event._choice1Object.transform.Find("MassageChooseText");
                var Message2 = Event._choice2Object.transform.Find("MassageChooseText");
                Message1.name = "MessageChooseText";
                Message2.name = "MessageChooseText";
                Message1.GetComponent<Text>().fontSize = 18;
                Message2.GetComponent<Text>().fontSize = 18;
                Message1.GetComponent<Text>().font = DateFile.instance.font;
                Message2.GetComponent<Text>().font = DateFile.instance.font;
                GameObject.Destroy(Message1.GetComponent<SetFont>());
                GameObject.Destroy(Message2.GetComponent<SetFont>());
            }
        }

        private static void Hide_Postfix()
        {
            RuntimeConfig.CurEvent = null;
        }

        private static void ShowTip(GameObject tips, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips, ref int ___tipsH)
        {
            if(tips != null)
            {
                var i = tips.GetComponent<YanPointerEnter>();
                if (i != null && !string.IsNullOrWhiteSpace(i.TipContent))
                {
                    ___informationMassage.text = i.TipContent;
                    ___informationName.text = i.TipTitle;

                    ___anTips = true;
                }
            }
        }

        private static bool GetItem_Prefix(ref int ___massageItemTyp,ref Transform ___itemHolder)
        {
            if (___massageItemTyp != int.MinValue)
                return true;

            for (int i = 0; i < ___itemHolder.childCount; i++)
                GameObject.Destroy(___itemHolder.GetChild(i).gameObject);

            List<int> baseData = new List<int>();
            int TargetActorID = MessageEventManager.Instance.MainEventData[1];
            int actorId = DateFile.instance.MianActorID();

            if(RuntimeConfig.ChoiceEnvironment.Filter != null)
            {
                baseData = RuntimeConfig.ChoiceEnvironment.Filter.ToList();
                RuntimeConfig.ChoiceEnvironment.Filter = null;
            }
            else
                baseData = new List<int>(DateFile.instance.GetItemSort(new List<int>(DateFile.instance.GetActorItems(RuntimeConfig.ChoiceEnvironment.GetWhoItem == 0 ? actorId : TargetActorID).Keys)));
            
            for (int k = 0; k < baseData.Count; k++)
            {
                int num6 = baseData[k];
                if (int.Parse(DateFile.instance.GetItemDate(num6, 3)) != 1)
                    continue;
                int num7 = int.Parse(DateFile.instance.GetItemDate(num6, 5));

                GameObject gameObject = GameObject.Instantiate(UsefulPrefabs.instance.itemIconNoDrag, Vector3.zero, Quaternion.identity);
                gameObject.name = "Item," + num6;
                gameObject.transform.SetParent(___itemHolder, worldPositionStays: false);
                gameObject.GetComponent<Toggle>().group = ___itemHolder.GetComponent<ToggleGroup>();
                Image component = gameObject.transform.Find("ItemBack").GetComponent<Image>();
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(component, "itemBackSprites", int.Parse(DateFile.instance.GetItemDate(num6, 4)));
                component.color = DateFile.instance.LevelColor(int.Parse(DateFile.instance.GetItemDate(num6, 8)));
                if (int.Parse(DateFile.instance.GetItemDate(num6, 6)) > 0)
                    gameObject.transform.Find("ItemNumberText").GetComponent<Text>().text = "×" + DateFile.instance.GetItemNumber(actorId, num6);
                else
                {
                    int num8 = int.Parse(DateFile.instance.GetItemDate(num6, 901));
                    int num9 = int.Parse(DateFile.instance.GetItemDate(num6, 902));
                    gameObject.transform.Find("ItemNumberText").GetComponent<Text>().text = $"{DateFile.instance.Color3(num8, num9)}{num8}</color>/{num9}";
                }
                GameObject gameObject2 = gameObject.transform.Find("ItemIcon").gameObject;
                gameObject2.name = "ItemIcon," + num6;
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(gameObject2.GetComponent<Image>(), "itemSprites", int.Parse(DateFile.instance.GetItemDate(num6, 98)));

            }

            return false;
        }

        private static bool GetActor_Prefix(ref int ___massageItemTyp, ref Transform ___actorHolder,ref GameObject ___actorIcon)
        {
            if (___massageItemTyp != int.MinValue)
                return true;

            for (int i = 0; i < ___actorHolder.childCount; i++)
                GameObject.Destroy(___actorHolder.GetChild(i).gameObject);
            int TaiwuID = DateFile.instance.MianActorID();
            List<int> ActorIDList;
            if (RuntimeConfig.ChoiceEnvironment.Filter != null)
            {
                ActorIDList = RuntimeConfig.ChoiceEnvironment.Filter.ToList();
                RuntimeConfig.ChoiceEnvironment.Filter = null;
            }
            else
                ActorIDList = DateFile.instance.GetFamily(false, false);
            for (int index = 0; index < ActorIDList.Count; index++)
            {
                GameObject actorIcon = GameObject.Instantiate(___actorIcon, Vector3.zero, Quaternion.identity);
                actorIcon.name = "Actor," + ActorIDList[index];
                actorIcon.transform.SetParent(___actorHolder, worldPositionStays: false);
                actorIcon.GetComponent<Toggle>().group = ___actorHolder.GetComponent<ToggleGroup>();
                if (DateFile.instance.acotrTeamDate.Contains(ActorIDList[index]))
                    actorIcon.transform.Find("IsInTeamIcon").gameObject.SetActive(value: true);
                actorIcon.transform.Find("IsInBuildingIcon").gameObject.SetActive(DateFile.instance.ActorIsWorking(ActorIDList[index]) != null);
                int num31 = DateFile.instance.GetActorFavor(isEnemy: false, TaiwuID, ActorIDList[index]);
                actorIcon.transform.Find("ListActorFavorText").GetComponent<Text>().text = ((ActorIDList[index] == TaiwuID || num31 == -1) ? DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[303][2]) : DateFile.instance.Color5(num31));
                actorIcon.transform.Find("ListActorNameText").GetComponent<Text>().text = DateFile.instance.GetActorName(ActorIDList[index]);
                Transform transform = actorIcon.transform.Find("ListActorFaceHolder").Find("FaceMask").Find("MianActorFace");
                transform.GetComponent<ActorFace>().SetActorFace(ActorIDList[index]);
            }

            return false;
        }

        private static bool SetItem_Prefix(ref int ___massageItemTyp)
        {
            if (___massageItemTyp != int.MinValue)
                return true;
            MessageEventManager.Instance.robItemId = 0;
            RuntimeConfig.ChoiceEnvironment.HasChoose = true;
            ui_MessageWindow.Instance.CloseItemsWindow();
            RuntimeConfig.ChoiceEnvironment.Choose();
            RuntimeConfig.ChoiceEnvironment.HasChoose = false;
            return false;
        }

        private static bool SetActor_Prefix(ref int ___massageItemTyp)
        {
            if (___massageItemTyp != int.MinValue)
                return true;

            RuntimeConfig.ChoiceEnvironment.HasChoose = true;
            ui_MessageWindow.Instance.CloseActorsWindow(); 
            RuntimeConfig.ChoiceEnvironment.Choose();
            RuntimeConfig.ChoiceEnvironment.HasChoose = false;
            return false;
        }

        private static void RestChooseWindow_Fix(ref RectTransform ___chooseHolder)
        {
            if (YanLib.Settings.ChoiceHotkey.Value)
            {
                for (int i = 0; i < ___chooseHolder.childCount; i++)
                {
                    ___chooseHolder.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        private static bool UpdateInputText_Prefix(string changedValue,ref string ___inputText, ref RectTransform ___chooseHolder,ref InputField ___inputTextField)
        {
            if (RuntimeConfig.ChoiceEnvironment.InputFieldInfo == null)
                return true;

            var inputInfo = RuntimeConfig.ChoiceEnvironment.InputFieldInfo;
            bool flag = changedValue != "";
            if (flag && (inputInfo.ContentType == InputField.ContentType.IntegerNumber || inputInfo.ContentType == InputField.ContentType.DecimalNumber))
            {
                int num = changedValue.ParseInt();
                if (inputInfo.MinValue > num || num > inputInfo.MaxValue)
                    flag = false;
            }
            ___inputText = changedValue;
            var i = ___chooseHolder.GetChild(inputInfo.ChoiceIndex).gameObject;
            if (i != null)
            {
                i.GetComponent<Button>().interactable = flag;
                i.transform.Find("MessageChooseText").GetComponent<Text>().color = flag ? Color.white : Color.red;
            }
            return false;
        }
    }
}
