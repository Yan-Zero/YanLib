using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YanLib.EventSystem
{
    /// <summary>
    /// 输入框
    /// </summary>
    public class InputFieldInfo
    {
        /// <summary>
        /// 使用输入框
        /// </summary>
        public bool UseInputField = false;
        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue = 50000;
        /// <summary>
        /// 最小值
        /// </summary>
        public int MinValue = 500;
        /// <summary>
        /// Input Type
        /// </summary>
        public UnityEngine.UI.InputField.InputType InputType = UnityEngine.UI.InputField.InputType.Standard;
        /// <summary>
        /// 合法字符
        /// </summary>
        public UnityEngine.UI.InputField.CharacterValidation CharacterValidation = UnityEngine.UI.InputField.CharacterValidation.None;
        /// <summary>
        /// 内容类型
        /// </summary>
        public UnityEngine.UI.InputField.ContentType ContentType = UnityEngine.UI.InputField.ContentType.Standard;
        /// <summary>
        /// 第几个按钮是输入的
        /// </summary>
        public int ChoiceIndex = 0;
    }
}
