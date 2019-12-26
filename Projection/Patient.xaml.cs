/// <summary>
/// 病人页面
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Data;
using DataBase;
using System.Text.RegularExpressions;

namespace EyeMedicine
{
    public partial class Patient : UserControl
    {
        int count;
        static string a;
        static string patientId1;
        static string patientName1;
        static string createDate1;
        static string patientSex1;
        static string patientPhone1;
        static string birthDate1;
        static string deleteoneid;

        public delegate void TopButtonAndMessageChangeHandler();
        public event TopButtonAndMessageChangeHandler TopButtonAndMessageChange;

        public Patient()
        {
            InitializeComponent();
            PatientGridWidget.ItemsSource = OCTPatient.ShowAllPatient().DefaultView;
        }

        /// <summary>
        ///显示所有用户
        /// </summary>
        public void ShowPatient()
        {
            PatientGridWidget.ItemsSource = OCTPatient.ShowAllPatient().DefaultView;
        }



        /// <summary>
        /// 选择病人列表时的点击事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        private void PatientGridWidget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var userName = ((((DataGrid)PatientGridWidget).SelectedItem) as DataRowView);

            if (userName != null)
            {
                a = userName.Row.ItemArray[0].ToString();
                OCTPatient temp = new OCTPatient();
                string s = userName.Row.ItemArray[2].ToString();
                temp.InitData(userName.Row.ItemArray[0].ToString(), userName.Row.ItemArray[1].ToString(), DateTime.Parse(userName.Row.ItemArray[5].ToString()), userName.Row.ItemArray[2].ToString(),
                    userName.Row.ItemArray[3].ToString(), DateTime.Parse(userName.Row.ItemArray[4].ToString()));

                GlobalData.GetPatient = temp;
                GlobalData.GetExitPatient = 1;

                patientId1 = userName.Row.ItemArray[0].ToString();
                patientName1 = userName.Row.ItemArray[1].ToString();
                patientSex1 = userName.Row.ItemArray[2].ToString();
                patientPhone1 = userName.Row.ItemArray[3].ToString();
                createDate1 = userName.Row.ItemArray[5].ToString();
                birthDate1 = userName.Row.ItemArray[4].ToString();

                GlobalData.GetExitCase = 0;

                CaseGridWidget.ItemsSource = OCTCase.ShowOnePatientCase(patientId1).DefaultView;

                TopButtonAndMessageChange();
            }

        }

        /// <summary>
        /// 选择病例列表时的点击事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void CaseGridWidget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //count = CaseGrid.SelectedItems.Count;

            var userName1 = ((((DataGrid)CaseGridWidget).SelectedItem) as DataRowView);

            if (userName1 != null)
            {
                deleteoneid = userName1.Row.ItemArray[2].ToString();
                OCTCase temp = new OCTCase();
                temp.InitData(userName1.Row.ItemArray[4].ToString(), DateTime.Parse(userName1.Row.ItemArray[0].ToString()), userName1.Row.ItemArray[2].ToString(), userName1.Row.ItemArray[5].ToString(), userName1.Row.ItemArray[3].ToString());
                GlobalData.GetExitCase = 1;
                GlobalData.GetCase = temp;
                GlobalData.HEIGHT = Convert.ToInt32(userName1.Row.ItemArray[6].ToString());
                GlobalData.WIDTH = Convert.ToInt32(userName1.Row.ItemArray[7].ToString());
                TopButtonAndMessageChange();
            }
        }

        /// <summary>
        /// 删除病人事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void DeletePatientWidget_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("是否删除病人信息", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                GlobalData.GetExitPatient = 0;
                GlobalData.GetExitCase = 0;
                TopButtonAndMessageChange();
                OCTPatient.DeleteOnePatient(a);
                PatientGridWidget.ItemsSource = OCTPatient.ShowAllPatient().DefaultView;
            }
        }

        /// <summary>
        /// 修改病人事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void ModifyPatientWidget_Click(object sender, RoutedEventArgs e)
        {
            stackpanel_hidden.Visibility = Visibility.Visible;
            Modify r = new Modify();
            r.PatientIdWidget.Text = patientId1;
            r.PatientNameWidget.Text = patientName1;
            r.PatientPhoneWidget.Text = patientPhone1;
            r.PatientSexWidget.Text = patientSex1;
            r.CreatDateWidget.Text = createDate1;
            r.BirthDateWidget.Text = birthDate1;
            r.DatalistChange += new Modify.DataChangeHandler(ShowPatient);
            r.ShowDialog();
            stackpanel_hidden.Visibility = Visibility.Hidden ;
        }

        /// <summary>
        /// 病例删除事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void DeleteCaseWidget_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("是否删除病人病例", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                OCTCase.DeleteOneCase(deleteoneid);
                GlobalData.GetExitCase = 0;
                TopButtonAndMessageChange();
                CaseGridWidget.ItemsSource = OCTCase.ShowOnePatientCase(patientId1).DefaultView;
            }

        }

        /// <summary>
        ///搜索按钮事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        public void SearchWidget_Click(object sender, RoutedEventArgs e)
        {

            stackpanel_hidden.Visibility = Visibility.Visible;
            Search pa = new Search();
            pa.ShowDialog();
            stackpanel_hidden.Visibility = Visibility.Hidden;

        }

        /// <summary>
        /// 显示所有用户事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void ShowAllUserWidget_Click(object sender, RoutedEventArgs e)
        {
            PatientGridWidget.ItemsSource = OCTPatient.ShowAllPatient().DefaultView;
        }

        /// <summary>
        /// 注册按钮事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        private void RegisterWidget_Click(object sender, RoutedEventArgs e)
        {
            stackpanel_hidden.Visibility = Visibility;
            Register r = new Register();
            r.DatalistChange += new Register.DataChangeHandler(ShowPatient);
            r.ShowDialog();
            stackpanel_hidden.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 鼠标滑入按钮区域事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        public void BorderMouseEnter(object sender, MouseEventArgs e)
        {
            Border bor = new Border();
            bor = (Border)sender;
            bor.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }

        /// <summary>
        /// 鼠标滑出按钮区域事件
        /// </summary>
        /// <param name="sender">控件本身的对象</param>
        /// <param name="e">事件本身</param>
        public void BorderMouseLeave(object sender, MouseEventArgs e)
        {
            Border bor1 = new Border();
            bor1 = (Border)sender;
            bor1.Background = new SolidColorBrush(Color.FromRgb(235, 234, 233));

        }

    }

}
