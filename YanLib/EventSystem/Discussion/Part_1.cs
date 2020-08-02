using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanLib.EventSystem.Discussion
{
#if DEBUG
	/// <summary>
	/// 第一个部分
	/// </summary>
	public static class Part_1
    {
		private static int _InviteType = 1;

        /// <summary>
        /// 羞辱
        /// </summary>
        public static void Diss(string ID,int TargetID)
        {
			DateFile.instance.actorLife[TargetID].Remove(708);
			int actorFavor = DateFile.instance.GetActorFavor(isEnemy: false, DateFile.instance.mianActorId, TargetID);
			if (actorFavor <= 0)
			{
				if (!DateFile.instance.GetActorSocial(TargetID, 401).Contains(DateFile.instance.mianActorId))
				{
					DateFile.instance.AddSocial(TargetID, DateFile.instance.mianActorId, 401);
					PeopleLifeAI.instance.AISetMassage(38, TargetID, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], DateFile.instance.mianActorId);
				}
				else if (!DateFile.instance.GetActorSocial(TargetID, 402).Contains(TargetID))
				{
					DateFile.instance.AddSocial(TargetID, DateFile.instance.mianActorId, 402);
					PeopleLifeAI.instance.AISetMassage(39, TargetID, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], DateFile.instance.mianActorId);
				}
			}
			DateFile.instance.SetActorMood(TargetID, -(actorFavor / 2000));
			DateFile.instance.ChangeFavor(TargetID, -6000);
		}

		/// <summary>
		/// 交谈
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		public static void Discuss(string ID, int TargetID)
		{
			var TaiwuID = DateFile.instance.mianActorId;
			int TargetGoodness = DateFile.instance.GetActorGoodness(TargetID);
			int TaiwuGoodness = DateFile.instance.GetActorGoodness(TaiwuID);
			int TalkGetFavor = DateFile.instance.TalkGetFavor();
			if (TargetGoodness == TaiwuGoodness)
				DateFile.instance.SetTalkFavor(TargetID, 0, TalkGetFavor * 4);
			else if (((TargetGoodness == 1 || TargetGoodness == 2) && (TaiwuGoodness == 3 || TaiwuGoodness == 4)) || ((TaiwuGoodness == 1 || TaiwuGoodness == 2) && (TargetGoodness == 3 || TargetGoodness == 4)))
				DateFile.instance.SetTalkFavor(TargetID, 0, TalkGetFavor);
			else
				DateFile.instance.SetTalkFavor(TargetID, 0, TalkGetFavor * 2);
			DateFile.instance.ChangeActorLifeFace(TargetID, 0, 1);
		}

		/// <summary>
		/// 动态描述
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <returns></returns>
		public static string DiscussEndDesc(string ID, int TargetID)
		{
			var TaiwuID = DateFile.instance.mianActorId;
			int TargetGoodness = DateFile.instance.GetActorGoodness(TargetID);
			int TaiwuGoodness = DateFile.instance.GetActorGoodness(TaiwuID);
			if (TargetGoodness == TaiwuGoodness)
				return "“如此……这般……不错……不错……正是如此！”";
			else if (((TargetGoodness == 1 || TargetGoodness == 2) && (TaiwuGoodness == 3 || TaiwuGoodness == 4)) || ((TaiwuGoodness == 1 || TaiwuGoodness == 2) && (TargetGoodness == 3 || TargetGoodness == 4)))
				return "“如此……这般……不然……不然……岂有此理！？”";
			else
				return "“如此……这般……或许……也不尽然……”";
		}

		/// <summary>
		/// 邀请
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="TargetID"></param>
		/// <param name="Type"></param>
		public static void Invite(string ID, int TargetID, int Type)
		{
			_InviteType = Type;
			int id = 16;
			DateFile.instance.actorLife[TargetID].Remove(708);
			switch (Type)
			{
				case 2:
					id = DateFile.instance.mianWorldId + 101;
					break;
				case 3:
					id = DateFile.instance.mianWorldId + 1;
					break;
			}
			DateFile.instance.actorLife[TargetID].Add(708, new List<int>
			{
				int.Parse(DateFile.instance.GetGangDate(id, 3)),
				int.Parse(DateFile.instance.GetGangDate(id, 4)),
				DateFile.instance.MianActorID(),
				3
			});
		}

		/// <summary>
		/// 动态选项 Desc
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="Desc"></param>
		/// <returns></returns>
		public static string InviteDynamicDesc(string ID,string Desc)
		{
			int key = int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][401]);
			int key2 = int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][402]);
			return Desc.Replace("MCITY", DateFile.instance.placeWorldDate[key][0])
				.Replace("MGNG", DateFile.instance.placeWorldDate[key2][0]);
		}

		/// <summary>
		/// 邀请结束动态 Desc
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="Desc"></param>
		/// <returns></returns>
		public static string InviteEndDynamicDesc(string ID,string Desc)
		{
			string RpStr = "";
			switch (_InviteType)
			{
				case 1:
					RpStr = "太吾村";
					break;
				case 2:
					RpStr = DateFile.instance.placeWorldDate[int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][401])][0];
					break;
				case 3:
					RpStr = DateFile.instance.placeWorldDate[int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][402])][0];
					break;
				default:
					return "那啥，别乱调用";
			}
			return Desc.Replace("[Destination]", RpStr);
		}

		/// <summary>
		/// 邀请显示
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="Type"></param>
		/// <returns></returns>
		public static List<KeyValuePair<bool, string>> InviteShowRequirement(string ID, int Type)
		{
			bool flag = true;
			switch(Type)
			{
				case 1:
					flag = DateFile.instance.mianPartId != int.Parse(DateFile.instance.GetGangDate(16, 3));
					break;
				case 2:
					flag = (DateFile.instance.mianPartId != int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][401]) - 1);
					break;
				case 3:
					flag = DateFile.instance.mianPartId != int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][402]);
					break;
				default:
					//?
					break;
			}
			return new List<KeyValuePair<bool, string>>()
			{
				new KeyValuePair<bool, string>(flag,"Nothing"),
			};
		}
	}
#endif
}
