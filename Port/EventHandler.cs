using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFSerialAssistant
{
    public enum ReceiveMode
    {
        Character,  //字符显示
        Hex,        //十六进制显示
    }

    public enum SendMode
    {
        Character,  //字符发送
        Hex         //十六进制发送
    }

    public enum CheckMode
    {
        None,       // 无校验
        Crc16,      // CRC-16 校验
    }

    public enum CheckPos
    {
        None,       // 无校验
        Last,       // 在最后添加
        LastTow,    // 在倒数第二个字节前添加
    }
    public partial class SerialAssistantPage : UserControl
    {
        #region Global
        // 接收并显示的方式
        private ReceiveMode receiveMode = ReceiveMode.Character;

        // 发送的方式
        private SendMode sendMode = SendMode.Character;

        private CheckMode checkmode = CheckMode.None;

        private CheckPos checkpos = CheckPos.None;

        private List<string> sendHistory = new List<string>();
        private const int MaxHistoryCount = 20;

        #endregion

        #region Event handler for menu items
        /// <summary>
        /// 保存串口数据菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void saveSerialDataMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 保存配置信息菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void saveConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            // 状态栏显示保存成功
            Information("配置信息保存成功。");
        }

        /// <summary>
        /// 加载配置信息菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void loadConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            // 状态栏显示加载成功
            Information("配置信息加载成功。");
        }

        /// <summary>
        /// 退出菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window parentWindow)
            {
                parentWindow.Close();
            }
        }

        /// <summary>
        /// 串口设置视图菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void serialSettingViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool state = serialSettingViewMenuItem.IsChecked;

            if (state == false)
            {
                serialPortConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                serialPortConfigPanel.Visibility = Visibility.Collapsed;
                if (IsCompactViewMode())
                {
                    serialPortConfigPanel.Visibility = Visibility.Visible;
                    EnterCompactViewMode();
                }
            }

            serialSettingViewMenuItem.IsChecked = !state;
        }

        /// <summary>
        /// 自动发送数据设置视图菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void autoSendDataSettingViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool state = autoSendDataSettingViewMenuItem.IsChecked;

            if (state == false)
            {
                autoSendConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                autoSendConfigPanel.Visibility = Visibility.Collapsed;
                if (IsCompactViewMode())
                {
                    autoSendConfigPanel.Visibility = Visibility.Visible;
                    EnterCompactViewMode();
                }
            }

            autoSendDataSettingViewMenuItem.IsChecked = !state;
        }

        /// <summary>
        /// 串口通信设置视图菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void serialCommunicationSettingViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool state = serialCommunicationSettingViewMenuItem.IsChecked;

            if (state == false)
            {
                serialCommunicationConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                serialCommunicationConfigPanel.Visibility = Visibility.Collapsed;

                if (IsCompactViewMode())
                {
                    serialCommunicationConfigPanel.Visibility = Visibility.Visible;
                    EnterCompactViewMode();
                }
            }

            serialCommunicationSettingViewMenuItem.IsChecked = !state;
        }

        /// <summary>
        /// 简洁视图菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void compactViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (IsCompactViewMode())
            {
                RestoreViewMode();
            }
            else
            {
                EnterCompactViewMode();
            }
        }

        /// <summary>
        /// 关于菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WPFSerialAssistant.About about = new About();
            about.ShowDialog();            
        }

        /// <summary>
        /// 帮助菜单项点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void helpMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Event handler for buttons and so on.
        /// <summary>
        /// 打开或关闭串口按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void openClosePortButton_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                if (ClosePort())
                {
                    openClosePortButton.Content = "打开";
                }
            }
            else
            {
                if (OpenPort())
                {
                    openClosePortButton.Content = "关闭";
                }
            }
        }

        /// <summary>
        /// 查找串口按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void findPortButton_Click(object sender, RoutedEventArgs e)
        {
            FindPorts();
        }

        /// <summary>
        /// 自动发送使能复选框点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void autoSendEnableCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (autoSendEnableCheckBox.IsChecked == true)
            {
                Information(string.Format("使能串口自动发送功能，发送间隔：{0} {1}。", autoSendIntervalTextBox.Text, timeUnitComboBox.Text.Trim()));
            }
            else
            {
                Information("禁用串口自动发送功能。");
                StopAutoSendDataTimer();
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 发送数据按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void sendDataButton_Click(object sender, RoutedEventArgs e)
        {
            string text = sendDataTextBox.Text.Trim();

            if (autoSendEnableCheckBox.IsChecked == true)
            {
                AutoSendData();
            }
            else
            {
                SendData();

                sendHistory.Add(text);
                if (sendHistory.Count > MaxHistoryCount)
                    sendHistory.RemoveAt(0);

                // 生成发送显示文本：时间戳 + 字节 + 换行
                string timestamp = GetTimeData();
                string byteText = GetSendData(); ;
                string fullText = timestamp + byteText + "\n";

                // 使用 Run + Paragraph 追加，设置颜色为蓝色
                AppendColoredText(recvDataRichTextBox, fullText, Brushes.RoyalBlue);

                recvDataRichTextBox.ScrollToEnd();
            }
        }

        /// <summary>
        /// 查看历史数据按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void reviewDataButton_Click(object sender, RoutedEventArgs e)
        {
            historyListBox.ItemsSource = null; // 刷新绑定
            historyListBox.ItemsSource = sendHistory;

            historyPopup.IsOpen = true;
        }

        /// <summary>
        /// 历史数据列表双击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">鼠标事件参数</param>
        private void historyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (historyListBox.SelectedItem != null)
            {
                sendDataTextBox.Text = historyListBox.SelectedItem.ToString();
                historyPopup.IsOpen = false; // 选完自动关闭
            }
        }

        /// <summary>
        /// 保存接收数据按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void saveRecvDataButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData(GetSaveDataPath());
        }

        /// <summary>
        /// 清空接收数据区按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void clearRecvDataBoxButton_Click(object sender, RoutedEventArgs e)
        {
            recvDataRichTextBox.Document.Blocks.Clear();
        }

        /// <summary>
        /// 接收模式单选按钮选中事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void recvModeButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (recvDataRichTextBox == null)
            {
                return;
            }

            if (rb != null)
            {
                //
                // TO-DO:
                // 可以将已经存在在文本框中的内容全部转换成指定形式显示，而不是简单地清空
                //
                recvDataRichTextBox.Document.Blocks.Clear();

                switch (rb.Tag.ToString())
                {
                    case "char":
                        receiveMode = ReceiveMode.Character;
                        break;
                    case "hex":
                        receiveMode = ReceiveMode.Hex;
                        break;
                    default:
                        break;
                }
            }
        }

        private void recvModeButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (recvDataRichTextBox == null)
            {
                return;
            }

            if (rb != null)
            {
                recvDataRichTextBox.Document.Blocks.Clear();

                switch (rb.Tag.ToString())
                {
                    case "char":
                        Information("提示：字符显示模式。");
                        break;
                    case "hex":
                        Information("提示：十六进制显示模式。");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 给 Modbus RTU 数据帧添加 CRC16 校验码。
        /// </summary>
        /// <param name="data">原始数据帧</param>
        /// <param name="pos">需要添加校验的位置</param>
        /// <returns>带 CRC16 的新数据帧</returns>
        public static byte[] AddCRC16(byte[] data, CheckPos pos)
        {
            ushort crc = 0xFFFF;
            int length = 0;

            if (pos == CheckPos.LastTow)
                length = data.Length - 2;
            else
                length = data.Length;
            // 计算 CRC16
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];

                for (int j = 0; j < 8; j++)
                { 
                    if ((crc & 0x0001) != 0) 
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            // 将 CRC 拆分为低字节和高字节（Modbus RTU 低字节在前）
            byte crcLow = (byte)(crc & 0xFF);
            byte crcHigh = (byte)((crc >> 8) & 0xFF);

            // 创建新数组并追加 CRC
            byte[] newFrame = new byte[data.Length + 2];
            Array.Copy(data, newFrame, data.Length);

            switch (pos)
            {
                case CheckPos.Last:
                    newFrame[newFrame.Length - 2] = crcHigh;
                    newFrame[newFrame.Length - 1] = crcLow;

                    return newFrame;
                case CheckPos.LastTow:
                    newFrame[newFrame.Length - 2] = newFrame[newFrame.Length - 4];
                    newFrame[newFrame.Length - 1] = newFrame[newFrame.Length - 3];
                    newFrame[newFrame.Length - 4] = crcHigh;
                    newFrame[newFrame.Length - 3] = crcLow;

                    return newFrame;
                case CheckPos.None:
                default:
                    return data;
            }
        }

        /// <summary>
        /// 将十六进制字符串数组添加CRC校验后返回新字符串。
        /// </summary>
        /// <param name="text">十六进制字符串数组</param>
        /// <returns>添加CRC后的十六进制字符串</returns>
        public string AddHexBytesCrc(string[] text)
        {
            byte[] bytes = new byte[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                bytes[i] = Convert.ToByte(text[i], 16);
            }

            byte[] newbyte = AddCRC16(bytes, checkpos);

            StringBuilder result = new StringBuilder();

            foreach (var item in newbyte)
            {
                result.Append(item.ToString("X2").ToUpper());
                result.Append(" ");
            }

            return result.ToString();
        }

        /// <summary>
        /// 移除十六进制字符串中的CRC校验码。
        /// </summary>
        /// <param name="hexFrame">带CRC的十六进制字符串</param>
        /// <param name="checkpos">校验码位置</param>
        /// <returns>移除CRC后的十六进制字符串</returns>
        public string RemoveHexBytesCrc(string hexFrame, CheckPos checkpos)
        {
            int removeIndex = 0;
            if (string.IsNullOrWhiteSpace(hexFrame)) return hexFrame;

            // 拆分成字节字符串数组
            string[] hexParts = hexFrame.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            switch (checkpos)
            {
                case CheckPos.LastTow:
                    removeIndex = -4;
                    break;
                case CheckPos.Last:
                    removeIndex = -2;
                    break;
                default:
                    break;
            }

            // 处理负索引（-1 表示最后一个字节）
            if (removeIndex < 0)
            {
                removeIndex = hexParts.Length + removeIndex;
            }

            // 检查范围
            if (removeIndex < 0 || removeIndex + 2 > hexParts.Length)
                throw new ArgumentOutOfRangeException("删除范围超出帧的长度。");

            // 转成 List 并删除
            List<string> list = hexParts.ToList();
            list.RemoveRange(removeIndex, 2);

            // 拼接回字符串
            return string.Join(" ", list);
        }

        /// <summary>
        /// 校验模式单选按钮选中事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void CheckModeButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "None":
                        checkmode = CheckMode.None;

                        noCheckPositionRadioButton.IsChecked = true;
                        noCheckPositionRadioButton.IsEnabled = false;
                        lastCheckPositionRadioButton.IsEnabled = false;
                        lasttwoCheckPositionRadioButton.IsEnabled = false;
                        break;
                    case "CRC16":
                        checkmode = CheckMode.Crc16;

                        noCheckPositionRadioButton.IsEnabled = true;
                        lastCheckPositionRadioButton.IsEnabled = true;
                        lasttwoCheckPositionRadioButton.IsEnabled = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void CheckModeButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "None":
                        Information("提示：不添加校验。");
                        break;
                    case "CRC16":
                        Information("提示：添加CRC-16校验，请选择校验位置。");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 校验位置单选按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void CheckPosButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            string text = sendDataTextBox.Text.Trim();
            string[] hexValues = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "None":
                        checkpos = CheckPos.None;
                        break;
                    case "Last":
                        checkpos = CheckPos.Last;
                        Information("提示：在发送末尾添加校验。");
                        break;
                    case "LastTow":
                        checkpos = CheckPos.LastTow;
                        Information("提示：在发送倒数第二个字节前添加校验（校验内容为之前所有字节）。");
                        break;
                    default:
                        break;
                }
            }

            text = AddHexBytesCrc(hexValues);

            sendDataTextBox.Text = text;
        }

        private void NOnePosButton_Click(object sender, RoutedEventArgs e)
        {
            Information("提示：不添加校验。");
        }

        /// <summary>
        /// 校验位置单选按钮取消选中事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void CheckPosButton_Unchecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "Last":
                        sendDataTextBox.Text = RemoveHexBytesCrc(sendDataTextBox.Text, checkpos);
                        break;
                    case "LastTow":
                        sendDataTextBox.Text = RemoveHexBytesCrc(sendDataTextBox.Text, checkpos);
                        break;
                    default:
                        break;
                }
            }
        }

        private bool showReceiveData = true;

        /// <summary>
        /// 是否显示接收数据按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void showRecvDataCheckBox_Click(object sender, RoutedEventArgs e)
        {
            showReceiveData = (bool)showRecvDataCheckBox.IsChecked;

            if (showReceiveData)
            {
                recvCharacterRadioButton.IsEnabled = true;
                recvHexRadioButton.IsEnabled = true;
            }
            else
            {
                recvCharacterRadioButton.IsEnabled = false;
                recvHexRadioButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// 发送数据模式单选按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void sendDataModeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "char":
                        sendMode = SendMode.Character;
                        Information("提示：发送字符文本。");
                        // 将文本框中内容转换成char
                        sendDataTextBox.Text = Utilities.ToSpecifiedText(sendDataTextBox.Text, SendMode.Character, serialPort.Encoding);
                        break;
                    case "hex":
                        // 将文本框中的内容转换成hex
                        sendMode = SendMode.Hex;
                        Information("提示：发送十六进制。输入十六进制数据之间用空格隔开，如：1D 2A 38。");
                        sendDataTextBox.Text = Utilities.ToSpecifiedText(sendDataTextBox.Text, SendMode.Hex, serialPort.Encoding);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 手动输入单选按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void manualInputRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 加载文件单选按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void loadFileRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 清空发送数据文本框按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void clearSendDataTextBox_Click(object sender, RoutedEventArgs e)
        {
            sendDataTextBox.Clear();
        }

        /// <summary>
        /// 发送追加内容单选按钮点击事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void appendRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "none":
                        appendContent = "";
                        break;
                    case "return":
                        appendContent = "\r";
                        break;
                    case "newline":
                        appendContent = "\n";
                        break;
                    case "retnewline":
                        appendContent = "\r\n";
                        break;
                    default:
                        break;
                }
                Information("发送追加：" + rb.Content.ToString());
            }
        }
        #endregion

        #region Event handler for timers
        /// <summary>
        /// 时钟定时器Tick事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeDate();
        }

        /// <summary>
        /// 自动发送数据定时器Tick事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void AutoSendDataTimer_Tick(object sender, EventArgs e)
        {
            bool ret = false;
            ret = SendData();

            if (ret == false)
            {
                StopAutoSendDataTimer();
            }
        }

        /// <summary>
        /// 窗口关闭前事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">取消事件参数</param>
        public void Port_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 释放没有关闭的端口资源
            if (serialPort.IsOpen)
            {
                ClosePort();
            }

            // 提示是否需要保存配置到文件中
            if (MessageBox.Show("是否在退出前保存软件配置？", "小贴士", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                SaveConfig();
            }
        }

        /// <summary>
        /// 窗口按键按下事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">按键事件参数</param>
        public void Port_Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+S保存数据
            if (e.Key == Key.S && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                SaveData(GetSaveDataPath());
            }

            // Ctrl+Enter 进入/退出简洁视图模式
            if (e.Key == Key.Enter && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                if (IsCompactViewMode())
                {
                    RestoreViewMode();
                }
                else
                {
                    EnterCompactViewMode();
                }
            }

            // Enter发送数据
            if (e.Key == Key.Enter)
            {
                SendData();
            }
        }

        #endregion

        #region EventHandler for serialPort
        
        // 数据接收缓冲区
        private List<byte> receiveBuffer = new List<byte>();

        // 一个阈值，当接收的字节数大于这么多字节数之后，就将当前的buffer内容交由数据处理的线程
        // 分析。这里存在一个问题，假如最后一次传输之后，缓冲区并没有达到阈值字节数，那么可能就
        // 没法启动数据处理的线程将最后一次传输的数据处理了。这里应当设定某种策略来保证数据能够
        // 在尽可能短的时间内得到处理。
        private const int THRESH_VALUE = 128;

        private bool shouldClear = true;

        /// <summary>
        /// 串口数据接收事件处理。
        /// </summary>
        /// <param name="sender">串口对象</param>
        /// <param name="e">串口数据接收事件参数</param>
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = sender as System.IO.Ports.SerialPort;

            if (sp != null)
            {
                // 临时缓冲区将保存串口缓冲区的所有数据
                int bytesToRead = sp.BytesToRead;
                byte[] tempBuffer = new byte[bytesToRead];

                // 将缓冲区所有字节读取出来
                sp.Read(tempBuffer, 0, bytesToRead);

                // 检查是否需要清空全局缓冲区先
                if (shouldClear)
                {
                    receiveBuffer.Clear();
                    shouldClear = false;
                }

                // 暂存缓冲区字节到全局缓冲区中等待处理
                receiveBuffer.AddRange(tempBuffer);

                if (receiveBuffer.Count >= THRESH_VALUE)
                {
                    //Dispatcher.Invoke(new Action(() =>
                    //{
                    //    recvDataRichTextBox.AppendText("Process data.\n");
                    //}));
                    // 进行数据处理，采用新的线程进行处理。
                    Thread dataHandler = new Thread(new ParameterizedThreadStart(ReceivedDataHandler));
                    dataHandler.Start(receiveBuffer);
                }

                // 启动定时器，防止因为一直没有到达缓冲区字节阈值，而导致接收到的数据一直留存在缓冲区中无法处理。
                StartCheckTimer();

                this.Dispatcher.Invoke(new Action(() =>
                {   
                    if (autoSendEnableCheckBox.IsChecked == false)
                    {
                        Information("");
                    }                                 
                    dataRecvStatusBarItem.Visibility = Visibility.Visible;
                }));
            }
        }

        #endregion

        #region 数据处理

        /// <summary>
        /// 检查定时器Tick事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            // 触发了就把定时器关掉，防止重复触发。
            StopCheckTimer();

            // 只有没有到达阈值的情况下才会强制其启动新的线程处理缓冲区数据。
            if (receiveBuffer.Count < THRESH_VALUE)
            {
                //recvDataRichTextBox.AppendText("Timeout!\n");
                // 进行数据处理，采用新的线程进行处理。
                Thread dataHandler = new Thread(new ParameterizedThreadStart(ReceivedDataHandler));
                dataHandler.Start(receiveBuffer);
            }
        }

        /// <summary>
        /// 向 RichTextBox 追加带颜色的文本。
        /// </summary>
        /// <param name="box">RichTextBox 控件</param>
        /// <param name="text">要追加的文本</param>
        /// <param name="color">显示颜色</param>
        private void AppendColoredText(RichTextBox box, string text, Brush color)
        {
            if (box == null || string.IsNullOrEmpty(text)) return;

            Paragraph para;

            // 获取最后一个 Paragraph，如果没有则新建一个
            if (box.Document.Blocks.LastBlock is Paragraph lastPara)
            {
                para = lastPara;
            }
            else
            {
                para = new Paragraph();
                box.Document.Blocks.Add(para);
            }

            // 创建 Run 并设置颜色
            Run run = new Run(text)
            {
                Foreground = color
            };

            para.Inlines.Add(run);

            // 滚动到末尾
            box.ScrollToEnd();
        }

        /// <summary>
        /// 数据接收处理线程方法。
        /// </summary>
        /// <param name="obj">接收缓冲区对象</param>
        private void ReceivedDataHandler(object obj)
        {
            List<byte> recvBuffer = new List<byte>();
            recvBuffer.AddRange((List<byte>)obj);

            if (recvBuffer.Count == 0) return;

            // 必须应当保证全局缓冲区的数据能够被完整地备份出来，这样才能进行进一步的处理。
            shouldClear = true;

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (showReceiveData)
                {
                    // 生成接收显示文本：时间戳 + 字节 + 换行
                    string timestamp = GetTimeData();
                    string byteText = Utilities.BytesToText(recvBuffer, receiveMode, serialPort.Encoding);
                    string fullText = timestamp + byteText + "\n";

                    // 使用 Run + Paragraph 追加，设置颜色为绿色
                    AppendColoredText(recvDataRichTextBox, fullText, Brushes.Green);
                }

                dataRecvStatusBarItem.Visibility = Visibility.Collapsed;
            }));

            // TO-DO: 解析数据等逻辑
        }
        #endregion
    }
}
