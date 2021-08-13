using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 雷霆合区工具
{
    class Tool
    {
        /// <summary>
        /// 检测字符串是否为空
        /// </summary>
        /// <param name="textbox"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool checkStr(TextBox textbox, string msg)
        {
            string text = textbox.Text.Trim();
            if (text == string.Empty)
            {
                MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textbox.Focus();
                return false;
            }
            return true;
        }
    }
}
