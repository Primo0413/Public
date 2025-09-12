using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFSerialAssistant
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialAssistantPage serialPage;
        private UserControl calculatorPage;

        public MainWindow()
        {
            InitializeComponent();
            //InitCore();

            // 初始化页面
            serialPage = new SerialAssistantPage();
            calculatorPage = new UserControl(); // 这里先用空的 UserControl 代替计算器

            // 默认显示串口助手
            MainContentArea.Children.Clear();
            MainContentArea.Children.Add(serialPage);
        }

        private void LeftSideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e)
        {
            var selectedItem = e.Info as HandyControl.Controls.SideMenuItem;
            if (selectedItem != null)
            {
                string header = selectedItem.Header.ToString();
                switch (header)
                {
                    case "串口助手":
                        // MainContentArea.Children.Clear();
                        MainContentArea.Children.Add(new SerialAssistantPage());
                        break;
                    case "计算器":
                        // MainContentArea.Children.Clear();
                        MainContentArea.Children.Add(new CalculatorPage());
                        break;
                }
            }
        }


        /// <summary>
        /// 窗口关闭前事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">取消事件参数</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            serialPage.Port_Window_Closing(sender,e);
        }

        /// <summary>
        /// 窗口按键按下事件处理。
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">按键事件参数</param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            serialPage.Port_Window_KeyDown(sender, e);
        }

    }
}
