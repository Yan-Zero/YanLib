using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YanLib.DataManipulator;


namespace YanLib.EventSystem.Discussion
{
#if DEBUG
    /// <summary>
    /// 衣服的选项
    /// </summary>
    public static class ClothesChoices
    {
        /// <summary>
        /// 动态文本
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static string ClothesDynamicDesc(string ID)
        {
            var i = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 305));
            if (i > 0)
                return $"（{DateFile.instance.GetItemDate(i, 0, false)}互动……）";
            return "？";
        }

        /// <summary>
        /// 可以点击的需求
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TargetActorID"></param>
        /// <returns></returns>
        public static List<KeyValuePair<bool,string>> ClothesRequirement(string ID,int TargetActorID)
        {
            var result = new List<KeyValuePair<bool, string>>();
            var clothesID = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 305));
            if (clothesID <= 0)
                return result;
            int ClothesType = int.Parse(DateFile.instance.GetItemDate(clothesID, 15));
            int EventID = int.Parse(DateFile.instance.identityDate[ClothesType][1]);
            if (EventID <= 0)
                return result;
            int ClothesNeed = int.Parse(EventHelper.GetEvent(EventID)[6].Split('&').Last());

            result.Add(new KeyValuePair<bool, string>(!(DateFile.instance.GetLifeDate(TargetActorID, 1001, 0) != ClothesType ||
                DateFile.instance.GetLifeDate(TargetActorID, 1001, 1) < ClothesNeed),
                string.Format("{0}{1} {2}", 
                    DateFile.instance.massageDate[8011][2].Split('|')[15],
                    DateFile.instance.SetColoer(10002, DateFile.instance.identityDate[ClothesType][0]),
                    DateFile.instance.SetColoer(20005, $"（{ClothesNeed}%）"))));
            bool NOF = true;
            switch(ClothesType)
            {
                case 5:
                case 7:
                case 13:
                case 14:
                case 15:
                case 18:
                    NOF = false;
                    break;
                default:
                    break;
            }
            if(NOF)
                result.Add(new KeyValuePair<bool, string>(!DateFile.instance.GetFamily(true).Contains(TargetActorID),
                    DateFile.instance.GetActorName(TargetActorID) + "不是同道"));
            return result;
        }

        /// <summary>
        /// 衣服按钮显示的需求
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static List<KeyValuePair<bool, string>> ClothesShowRequirement(string ID)
        {
            var i = int.Parse(DateFile.instance.GetActorDate(DateFile.instance.mianActorId, 305));
            var result = new List<KeyValuePair<bool, string>>()
            {
                new KeyValuePair<bool, string>(
                    i > 0,
                    "在？衣服穿好没？"),
            };
            if(i > 0)
            {
                result.Add(new KeyValuePair<bool, string>(
                    DateFile.instance.identityDate[int.Parse(DateFile.instance.GetItemDate(i, 15))][1] != "0",
                    "有衣服没事件？"));
            }
            return result;
        }
    }
#endif
}
