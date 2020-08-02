using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 毫无疑问，是我自用的 PE
    /// </summary>
    public class YanPointerEnter : PointerEnter
    {
		/// <summary>
		/// Tip 标题
		/// </summary>
		public string TipTitle;
		/// <summary>
		/// Tip 内容
		/// </summary>
		public string TipContent;
	}
}
