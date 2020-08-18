using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace YanLib.UI.EventUI
{
    /// <summary>
    /// 选项快捷键用的
    /// </summary>
    public class HotkeyMonitor : MonoBehaviour
    {
        /// <summary>
        /// 选项的 Button 组件
        /// </summary>
        public Button choiceButton = null;
        /// <summary>
        /// 选项的按钮值
        /// </summary>
        public int choiceIndex = -1;

        private void Update()
        {
            if (choiceButton == null || choiceIndex < 0)
                return;
            if (!ui_MessageWindow.Exists || !ui_MessageWindow.Instance.inputTextField.gameObject.activeInHierarchy)
                if (Input.GetKeyDown(DateFile.instance.massageKeyCode[choiceIndex]))
                    choiceButton.onClick.Invoke();
            
        }
    }
}
