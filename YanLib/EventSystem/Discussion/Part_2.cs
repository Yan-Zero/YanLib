using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace YanLib.EventSystem.Discussion
{
#if DEBUG
	/// <summary>
	/// 对话的第二部分
	/// </summary>
	public static class Part_2
	{
		private static int GiveItemEnd;
		private static int GiveItemID;

		/// <summary>
		/// GiveItem
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <param name="ItemID"></param>
		public static string GiveItem(string ID, int TargetID, int ItemID)
		{
			int TaiwuID = DateFile.instance.MianActorID();
			GiveItemID = ItemID;
			int num3 = int.Parse(DateFile.instance.GetItemDate(ItemID, 8));
			int num4 = int.Parse(DateFile.instance.GetItemDate(ItemID, 103));
			int Favor = int.Parse(DateFile.instance.GetItemDate(ItemID, 102));
			if (ui_MessageWindow.getDouble)
			{
				Favor *= 2;
				ui_MessageWindow.getDouble = false;
			}
			DateFile.instance.ChangeTwoActorItem(TaiwuID, TargetID, ItemID);
			bool flag = false;
			if (int.Parse(DateFile.instance.GetItemDate(ItemID, 4)) == 3)
			{
				for (int i = 0; i < 6; i++)
					if (int.Parse(DateFile.instance.GetItemDate(ItemID, 71 + i)) > 0)
						flag = true;

				if (!flag)
					DateFile.instance.AIUseItem(powerUp: false, TargetID, TargetID, ItemID, 0, hpCut: true, mood: true, Random.Range(1, 4));
			}
			if (flag &&
				!(int.Parse(DateFile.instance.GetItemDate(ItemID, 8)) * 50 + DateFile.instance.GetActorValue(TaiwuID, 510) > DateFile.instance.GetActorValue(TargetID, 510) + DateFile.instance.GetActorWariness(TargetID)))
			{
				if (int.Parse(DateFile.instance.GetItemDate(ItemID, 6)) == 1)
					DateFile.instance.LoseItem(TargetID, ItemID, 1, removeItem: true, removeGive: true, 1);
				else
					DateFile.instance.ChangeItemHp(TargetID, ItemID, -1);
				DateFile.instance.actorLife[TargetID].Remove(708);
				DateFile.instance.ChangeFavor(TargetID, -int.Parse(DateFile.instance.GetActorDate(TargetID, 3, applyBonus: false)));
				DateFile.instance.AddSocial(TargetID, TaiwuID, 402);
				PeopleLifeAI.instance.AISetMassage(117, TargetID, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], TaiwuID);
				DateFile.instance.SetActorMood(TaiwuID, -num4);
				DateFile.instance.SetActorFameList(TaiwuID, 105, 1);
				DateFile.instance.SetTalkFavor(TargetID, 6, -3000);
				//去下毒
				return "Discussion.12";

			}
			else if (int.Parse(DateFile.instance.GetItemDate(ItemID, 5)) == int.Parse(DateFile.instance.GetActorDate(TargetID, 202, applyBonus: false)))
			{
				DateFile.instance.SetActorMood(TargetID, int.Parse(DateFile.instance.GetItemDate(ItemID, 103)));
				DateFile.instance.ChangeFavor(TargetID, Favor * 2);
				Characters.SetCharProperty(TargetID, 207, "1");
				GiveItemEnd = 9112;
			}
			else if (int.Parse(DateFile.instance.GetItemDate(ItemID, 5)) == int.Parse(DateFile.instance.GetActorDate(TargetID, 203, applyBonus: false)))
			{
				DateFile.instance.ChangeFavor(TargetID, Favor / 2);
				Characters.SetCharProperty(TargetID, 208, "1");
				GiveItemEnd = 9113;
			}
			else
			{
				DateFile.instance.SetActorMood(TargetID, int.Parse(DateFile.instance.GetItemDate(ItemID, 103)) / 2);
				DateFile.instance.ChangeFavor(TargetID, Favor);
				GiveItemEnd = 9114;
			}

			if (flag)
				MessageEventManager.Instance.ChangeGoodnees(3, TaiwuID);

			return "Discussion.12";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <returns></returns>
		public static string GiveItemEndDesc(string ID, int TargetID)
		{
			switch (GiveItemEnd)
			{
				case 9112:
					return $"“竟然要将{DateFile.instance.GetItemDate(GiveItemID, 0, false)}送给我吗？！”\n\n<color=#8E8E8EFF>（{DateFile.instance.GetActorName(TargetID)}十分喜爱{DateFile.instance.GetActorName()}赠送的礼物！）</color>";
				case 9113:
					return $"“怎么会是{DateFile.instance.GetItemDate(GiveItemID, 0, false)}……”\n\n<color=#8E8E8EFF>（{DateFile.instance.GetActorName(TargetID)}十分厌恶{DateFile.instance.GetActorName()}赠送的礼物……）</color>";
				case 9114:
					return $"“原来要将{DateFile.instance.GetItemDate(GiveItemID, 0, false)}送给我么……”";
				default:
					return $"Hmmmm，你理应无法触发这个";
			}
		}

		/// <summary>
		/// 🦗动态描述
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <param name="Desc"></param>
		/// <returns></returns>
		public static string QuquBattleDesc(string ID, int TargetID, string Desc)
		{
			int num4 = 0;
			int num5 = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 312, applyBonus: false));
			if (num5 > 0 && int.Parse(DateFile.instance.GetItemDate(num5, 901)) > 0)
				num4++;
			List<int> list = new List<int>(DateFile.instance.actorItemsDate[DateFile.instance.mianActorId].Keys);
			for (int i = 0; i < list.Count; i++)
			{
				int id2 = list[i];
				if (int.Parse(DateFile.instance.GetItemDate(id2, 5)) == 19 && int.Parse(DateFile.instance.GetItemDate(id2, 901)) > 0)
					num4++;
				if (num4 >= 3)
					break;
			}
			if (num4 < 3)
				return "“哦？想斗促织吗？但你似乎拿不出三只参战的促织？”\n\n<color=#8E8E8EFF>（没有足够的促织进行促织决斗！）\n（至少需要准备三只促织才能进行促织决斗……）</color>";
			switch (DateFile.instance.GetActorGoodness(TargetID))
			{
				case 0:
					return "“随意斗斗便好，也不必定要分出胜负……”" + Desc;
				case 1:
					return "“我很是爱惜自己的促织，极少与人相斗……”" + Desc;
				case 2:
					return "“促织是刚勇之物，自然逢战必应！”" + Desc;
				case 3:
					return "“有趣！让我瞧瞧你有什么稀罕的促织！”" + Desc;
				case 4:
					return "“胜者王侯败者寇！放‘马’过来吧！”" + Desc;
				default:
					return "？你咋触发的？";
			}
		}

		/// <summary>
		/// 三🦗
		/// </summary>
		/// <param name="ID"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<bool, string>> HasThreeQuqu(string ID)
		{
			int num4 = 0;
			int num5 = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 312, applyBonus: false));
			if (num5 > 0 && int.Parse(DateFile.instance.GetItemDate(num5, 901)) > 0)
				num4++;
			List<int> list = new List<int>(DateFile.instance.actorItemsDate[DateFile.instance.mianActorId].Keys);
			for (int i = 0; i < list.Count; i++)
			{
				int id2 = list[i];
				if (int.Parse(DateFile.instance.GetItemDate(id2, 5)) == 19 && int.Parse(DateFile.instance.GetItemDate(id2, 901)) > 0)
					num4++;
				if (num4 >= 3)
					break;
			}
			yield return new KeyValuePair<bool, string>(num4 >= 3, "三只蛐蛐，谢谢老板");
		}

		/// <summary>
		/// 🦗对战！
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		public static void QuquBattle(string ID, int TargetID)
		{
			QuquBattleSystem.instance.ququBattleEnemyId = TargetID;
			QuquBattleSystem.instance.ququBattleId = Mathf.Clamp(Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(TargetID, 20, applyBonus: false))) + Random.Range(-1, 2), 1, 9);
			if (Random.Range(0, 100) < 20)
				QuquBattleSystem.instance.ququBattleId += 10;
			QuquBattleSystem.instance.ShowQuquBattleWindow();
		}

		/// <summary>
		/// 比试技艺
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		public static string CompareSkillDesc(string ID, int TargetID)
		{
			switch (DateFile.instance.GetActorGoodness(TargetID))
			{
				case 0:
					return "“各人见解多有不同，遑论输赢？自然是点到为止……”";
				case 1:
					return "“你我各抒己见，旨在相互学习，可千万别影响了心情……”";
				case 2:
					return "“既然要较艺，可得拿出真本事！”";
				case 3:
					return "“只是较艺，未免枯燥，不如再赌点什么……”";
				case 4:
					return "“你想试探我的才学吗？出题吧！”";
				default:
					return "？你咋触发的？";
			}
		}

		/// <summary>
		/// 比试技艺
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <param name="Type"></param>
		public static void CompareSkill(string ID, int TargetID, int Type)
		{
			UIManager.Instance.StackState();
			SkillBattleSystem.instance.ShowSkillBattleWindow(Type, new List<int>
			{
				TargetID
			});
		}

		/// <summary>
		/// 切磋武功
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <returns></returns>
		public static string CompareKungfuDesc(string ID, int TargetID)
		{
			switch (DateFile.instance.GetActorGoodness(TargetID))
			{
				case 0:
					return "“切磋武功，自然是莫论输赢，点到为止……”";
				case 1:
					return "“拳脚无眼，可千万别有什么损伤……”";
				case 2:
					return "“既然要切磋，可得拿出真本事！”";
				case 3:
					return "“只是切磋，未免枯燥，不如再赌点什么……”";
				case 4:
					return "“你想试探我的武功吗？出招吧！”";
				default:
					return "？你咋触发的？";
			}
		}

		/// <summary>
		/// 开始干架
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="battleId"></param>
		/// <param name="Typ2"></param>
		public static void CompareKungfu(string ID, int battleId, int Typ2)
		{
			MessageEventManager.Instance.MakeEventBattel(battleId, Typ2);
		}

		private static Dictionary<string, EventChoice> ChoiceTable = new Dictionary<string, EventChoice>()
		{
			{
				"2045200001",
				new EventChoice()
				{
					Desc = "（协助筹备……）",
					Effect = new ChoiceEnd()
					{
						ToID = "WulinGeneral.1",
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 5
							}
						}
					}
				}
			},
			{
				"2045200002",
				new EventChoice()
				{
					Desc = "（捐赠资源……）",
					Effect = new ChoiceEnd()
					{
						ToID = "WulinGeneral.3",
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 2
							}
						}
					}
				}
			},
			{
				"901000001",
				new EventChoice()
				{
					Desc = "（打坐念经……）",
					Effect = new ChoiceEnd()
					{
						ToID = "Discussion.16",
						CallEnd = new CallInfo()
						{
							TargetType = typeof(Part_2),
							TargetMethod = "SetInteractionID",
							Params = new List<CallInfo.ParamInfo>()
							{
								new CallInfo.ParamInfo()
								{
									Type = CallInfo.ParamInfo.ParamType.Int,
									Value = "1"
								}
							}
						}
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 10
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Favor,
								ValueA = 2
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.BasePartValue,
								ValueA = 200
							}
						}
					},
					Tip = "减少自己或一名同道的各项名誉的持续时间（恶名的减少量更大）"
				}
			},
			{
				"901000002",
				new EventChoice()
				{
					Desc = "（天府规略……）",
					Effect = new ChoiceEnd()
					{
						ToID = "Discussion.16",
						CallEnd = new CallInfo()
						{
							TargetType = typeof(Part_2),
							TargetMethod = "SetInteractionID",
							Params = new List<CallInfo.ParamInfo>()
							{
								new CallInfo.ParamInfo()
								{
									Type = CallInfo.ParamInfo.ParamType.Int,
									Value = "2"
								}
							}
						}
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 10
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Favor,
								ValueA = 2
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.BasePartValue,
								ValueA = 500
							}
						}
					},
					Tip = "使太吾村中各资源点的规模提升1~3级",
				}
			},
			{
				"901000003",
				new EventChoice()
				{
					Desc = "（起死回生……）",
					Effect = new ChoiceEnd()
					{
						ToID = "Discussion.16",
						CallEnd = new CallInfo()
						{
							TargetType = typeof(Part_2),
							TargetMethod = "SetInteractionID",
							Params = new List<CallInfo.ParamInfo>()
							{
								new CallInfo.ParamInfo()
								{
									Type = CallInfo.ParamInfo.ParamType.Int,
									Value = "3"
								}
							}
						}
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 10
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Favor,
								ValueA = 2
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.BasePartValue,
								ValueA = 300
							}
						}
					},
					Tip = "治愈自己或一名同道的所有内外伤",
				}
			},
			{
				"901000004",
				new EventChoice()
				{
					Desc = "（七星调元……）",
					Effect = new ChoiceEnd()
					{
						ToID = "Discussion.16",
						CallEnd = new CallInfo()
						{
							TargetType = typeof(Part_2),
							TargetMethod = "SetInteractionID",
							Params = new List<CallInfo.ParamInfo>()
							{
								new CallInfo.ParamInfo()
								{
									Type = CallInfo.ParamInfo.ParamType.Int,
									Value = "4"
								}
							}
						}
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 10
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Favor,
								ValueA = 2
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.BasePartValue,
								ValueA = 300
							}
						}
					},
					Tip = "将自己或一名同道的内息紊乱完全消除",
				}
			},
			{
				"901000005",
				new EventChoice()
				{
					Desc = "（石牢静坐……）",
					Effect = new ChoiceEnd()
					{
						ToID = "Discussion.16",
						CallEnd = new CallInfo()
						{
							TargetType = typeof(Part_2),
							TargetMethod = "SetInteractionID",
							Params = new List<CallInfo.ParamInfo>()
							{
								new CallInfo.ParamInfo()
								{
									Type = CallInfo.ParamInfo.ParamType.Int,
									Value = "5"
								}
							}
						}
					},
					CanChooseRequirement = new ChoiceRequirement()
					{
						Requirements = new List<ChoiceRequirement.Requirement>()
						{
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Time,
								ValueA = 10
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.Favor,
								ValueA = 2
							},
							new ChoiceRequirement.Requirement()
							{
								Type = ChoiceRequirement.Requirement.RequirementTarget.BasePartValue,
								ValueA = 300
							}
						}
					},
					Tip = "降低自己或一名同道的50点相枢化程度",
				}
			}
		};

		/// <summary>
		/// 毫无疑问，动态选项
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <returns></returns>
		public static AdditionalChoices InteractionDynamicChoices(string ID, int TargetID)
		{
			var result = new AdditionalChoices()
			{
				Index = -2,
			};

			int num6 = int.Parse(DateFile.instance.GetActorDate(TargetID, 19, applyBonus: false));
			int num7 = int.Parse(DateFile.instance.GetActorDate(TargetID, 20, applyBonus: false));
			if (int.Parse(DateFile.instance.GetGangDate(num6, 2)) != 1 || DateFile.instance.mianPartId == int.Parse(DateFile.instance.GetGangDate(num6, 3)))
			{
				string[] array = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(num6, num7)][812].Split('|');
				for (int n = 0; n < array.Length; n++)
					if (int.Parse(array[n]) > 0)
						result.Choices.Add(ChoiceTable[array[n]]);
			}

			if (WuLinGeneralAssembly.Instance.GAState != 0 || num6 >= 16)
				return result;

			if (int.Parse(DateFile.instance.GetActorDate(TargetID, 20, applyBonus: false)) <= 4)
			{
				if (DateFile.instance.GetFamily(getPrisoner: false, getBaby: false).Count > 0)
					result.Choices.Add(ChoiceTable["2045200001"]);
				result.Choices.Add(ChoiceTable["2045200002"]);
			}
			return result;
		}
		private static int _interactionID = 0;
		/// <summary>
		/// 设置交互 ID
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="InteractionID"></param>
		public static void SetInteractionID(string ID,int InteractionID)
		{
			_interactionID = InteractionID;
		}
	}
#endif
}