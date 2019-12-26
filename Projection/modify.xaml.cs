using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 修改病人信息页面
/// </summary>
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataBase;
namespace EyeMedicine
{
    public partial class Modify : Window
    {
        public Modify()
        {
            InitializeComponent();
        }

        public delegate void DataChangeHandler();
        public event DataChangeHandler DatalistChange;

        /// <summary>
        /// 修改病人后提交按钮
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void FinishWidget_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            int Count;
            Regex rx = new Regex("^1[34578]\\d{9}$");
            bool a = PatientNameWidget.Text == "";
            bool b = PatientSexWidget.Text == "";
            bool c = PatientPhoneWidget.Text == "";
            bool d = BirthDateWidget.Text == "";
            if (a || b || c || d)
            {
                MessageBox.Show("请输入完整信息！");
            }
            else
                if (!rx.IsMatch(PatientPhoneWidget.Text)) //不匹配
            {

                MessageBox.Show("手机号格式不对，请重新输入！");    //弹框提示
            }
            else
            {
                String ID = PatientIdWidget.Text;
                String Name = PatientNameWidget.Text;
                String Sex = PatientSexWidget.Text;
                String Phone = PatientPhoneWidget.Text;
                DateTime Birth = Convert.ToDateTime(BirthDateWidget.Text);
                DateTime Creat = Convert.ToDateTime(CreatDateWidget.Text);

                OCTPatient temp = new DataBase.OCTPatient();
                temp.InitData(ID, Name, Creat, Sex, Phone, Birth);
                Count=temp.UpdatePatient();

                if (Count==1)
                {
                    MessageBox.Show("手机号重复，请重新输入！");
                }
                else
                {
                    this.Close();
                    DatalistChange();
                }

            }
        }

        /// <summary>
        /// 修改病人页面的取消按钮事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void CancelWidget_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 鼠标滑入按钮区域事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        public void BorderMouseEnter(object sender, MouseEventArgs e)
        {
            Border border = new Border();
            border = (Border)sender;
            border.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }


        /// <summary>
        /// 鼠标滑出按钮区域事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        public void BorderMouseLeave(object sender, MouseEventArgs e)
        {
            Border border = new Border();
            border = (Border)sender;
            border.Background = new SolidColorBrush(Color.FromRgb(235, 234, 233));

        }
    }
}
