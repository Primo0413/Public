using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace WPFSerialAssistant
{
    public static class Utilities
    {
        public static string BytesToText(List<byte> bytesBuffer, ReceiveMode mode, Encoding encoding, bool addSeparator = true)
        {
            StringBuilder result = new StringBuilder();

            if (mode == ReceiveMode.Character)
            {
                return encoding.GetString(bytesBuffer.ToArray<byte>());
            }

            foreach (var item in bytesBuffer)
            {
                switch (mode)
                {
                    case ReceiveMode.Hex:
                        // 转换为16进制，确保高位0（使用 "X2" 格式）
                        result.Append(item.ToString("X2").ToUpper());
                        break;
                    default:
                        break;
                }

                // 如果需要添加分隔符
                if (addSeparator)
                {
                    result.Append(" ");
                }
            }

            return result.ToString();
        }

        public static string ToSpecifiedText(string text, SendMode mode, Encoding encoding)
        {
            string result = "";
            switch (mode)
            {
                case SendMode.Character:
                    text = text.Trim();

                    // 转换成字节
                    List<byte> src = new List<byte>();

                    string[] grp = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var item in grp)
                    {
                        src.Add(Convert.ToByte(item, 16));
                    }

                    // 转换成字符串
                    result = encoding.GetString(src.ToArray<byte>());
                    break;
                    
                case SendMode.Hex:
                    
                    byte[] byteStr = encoding.GetBytes(text.ToCharArray());

                    foreach (var item in byteStr)
                    {
                        result += Convert.ToString(item, 16).ToUpper() + " ";
                    }
                    break;
                default:
                    break;
            }

            return result.Trim();
        }

    }
}
