using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 追加按钮
    /// </summary>
    public class AdditionalChoices
    {
        /// <summary>
        /// 对应 EventID
        /// </summary>
        public string ID;
        /// <summary>
        /// 选项
        /// </summary>
        public List<EventChoice> Choices = new List<EventChoice>();
        /// <summary>
        /// 开始添加的地方
        /// </summary>
        public int Index = -1;
    }
}
