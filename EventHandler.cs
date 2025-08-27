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
        Hex,        //十六进制
    }

    public enum SendMode
    {
        Character,  //字符
        Hex         //十六进制
    }

    public enum CheckMode
    {
        None,
        Crc16,
    }

    public enum CheckPos
    {
        None,
        Last,
        LastTow,
    }
    public partial class MainWindow : Window
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
        private void saveSerialDataMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            // 状态栏显示保存成功
            Information("配置信息保存成功。");
        }

        private void loadConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            // 状态栏显示加载成功
            Information("配置信息加载成功。");
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

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

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WPFSerialAssistant.About about = new About();
            about.ShowDialog();            
        }

        private void helpMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Event handler for buttons and so on.
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

        private void findPortButton_Click(object sender, RoutedEventArgs e)
        {
            FindPorts();
        }

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

                // 发送字节显示
                string sTemp = GetSendData();

                string sDateTemp = "";
                sDateTemp = GetTimeData();

                recvDataRichTextBox.AppendText(sDateTemp);
                recvDataRichTextBox.AppendText(sTemp);
                recvDataRichTextBox.AppendText("\n");

                TextPointer pTemp = recvDataRichTextBox.Document.ContentEnd.GetPositionAtOffset(-72);

                // 更改接收显示字节的字节颜色
                TextRange dataTextRange = new TextRange(pTemp, recvDataRichTextBox.Document.ContentEnd);
                dataTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));

                recvDataRichTextBox.ScrollToEnd();
            }
        }

        private void reviewDataButton_Click(object sender, RoutedEventArgs e)
        {
            historyListBox.ItemsSource = null; // 刷新绑定
            historyListBox.ItemsSource = sendHistory;

            historyPopup.IsOpen = true;
        }
        private void historyListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (historyListBox.SelectedItem != null)
            {
                sendDataTextBox.Text = historyListBox.SelectedItem.ToString();
                historyPopup.IsOpen = false; // 选完自动关闭
            }
        }

        private void saveRecvDataButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData(GetSaveDataPath());
        }

        private void clearRecvDataBoxButton_Click(object sender, RoutedEventArgs e)
        {
            recvDataRichTextBox.Document.Blocks.Clear();
        }

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
                        Information("提示：字符显示模式。");
                        break;
                    case "hex":
                        receiveMode = ReceiveMode.Hex;
                        Information("提示：十六进制显示模式。");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 给 Modbus RTU 数据帧添加 CRC16 校验码
        /// </summary>
        /// <param name="data">原始数据帧</param>
        /// <returns>带 CRC16 的新数据帧</returns>
        public static byte[] AddCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;

            // 计算 CRC16
            for (int i = 0; i < data.Length; i++)
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
            newFrame[newFrame.Length - 2] = crcHigh;
            newFrame[newFrame.Length - 1] = crcLow;

            return newFrame;
        }

        private void CheckModeButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "None":
                        checkmode = CheckMode.None;
                        Information("提示：不添加校验。");
                        break;
                    case "CRC16":
                        checkmode = CheckMode.Crc16;
                        Information("提示：添加CRC-16校验。");
                        //string text = sendDataTextBox.Text.Trim();
                        //string[] hexValues = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //byte[] bytes = new byte[hexValues.Length];

                        //for (int i = 0; i < hexValues.Length; i++)
                        //{
                        //    bytes[i] = Convert.ToByte(hexValues[i], 16);
                        //}

                        //byte[] newbyte = AddCRC16(bytes);

                        //StringBuilder result = new StringBuilder();

                        //foreach (var item in newbyte)
                        //{
                        //    result.Append(item.ToString("X2").ToUpper());
                        //    result.Append(" ");
                        //}

                        //sendDataTextBox.Text = result.ToString();
                        break;
                    default:
                        break;
                }
            }
        }

        private void CheckPosButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.Tag != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "None":
                        checkpos = CheckPos.None;
                        Information("提示：不添加校验。");
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
        }

        private bool showReceiveData = true;
        private void showRecvDataCheckBox_Click(object sender, RoutedEventArgs e)
        {
            showReceiveData = (bool)showRecvDataCheckBox.IsChecked;
        }

        private void sendDataModeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb != null)
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

        private void manualInputRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void loadFileRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void clearSendDataTextBox_Click(object sender, RoutedEventArgs e)
        {
            sendDataTextBox.Clear();
        }

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
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeDate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// 窗口关闭前拦截
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
        /// 捕获窗口按键。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
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
        /// 更新：采用一个缓冲区，当有数据到达时，把字节读取出来暂存到缓冲区中，缓冲区到达定值
        /// 时，在显示区显示数据即可。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void ReceivedDataHandler(object obj)
        {
            List<byte> recvBuffer = new List<byte>();
            recvBuffer.AddRange((List<byte>)obj);

            if (recvBuffer.Count == 0)
            {
                return;
            }

            // 必须应当保证全局缓冲区的数据能够被完整地备份出来，这样才能进行进一步的处理。
            shouldClear = true;

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (showReceiveData)
                {
                    // 增加接收时间戳
                    string sTemp = "";
                    sTemp = GetTimeData();
                    recvDataRichTextBox.AppendText(sTemp);
                    // 根据显示模式显示接收到的字节.
                    recvDataRichTextBox.AppendText(Utilities.BytesToText(recvBuffer, receiveMode, serialPort.Encoding));

                    // 增加换行符
                    recvDataRichTextBox.AppendText("\n");

                    TextPointer pTemp = recvDataRichTextBox.Document.ContentEnd.GetPositionAtOffset(-72);   // 强行更改颜色显示，后续进行调整

                    // 更改接收显示字节的字节颜色
                    TextRange dataTextRange = new TextRange(pTemp, recvDataRichTextBox.Document.ContentEnd);
                    dataTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Green));  // 设置为绿色

                    recvDataRichTextBox.ScrollToEnd();
                }

                dataRecvStatusBarItem.Visibility = Visibility.Collapsed;
            }));

            // TO-DO：
            // 处理数据，比如解析指令等等
        }
        #endregion
    }
}
