using BooksSharp.Classes;
using EasyCaptcha.Wpf;
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
using System.Windows.Threading;

namespace BooksSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isFirst = true;
        private string captchaText = "";
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Helper.DB = new Model.DBbooksShopEntities(); //связь с бд
                                                          // MessageBox.Show("Связь с бд");
            }
            catch
            {
                MessageBox.Show("Прроблема связи с базой");
                return;
            }


            const int lengthCaptcha = 4;        //Длина каптчи
            captcha.CreateCaptcha(Captcha.LetterOption.Alphanumeric, lengthCaptcha);
            captchaText = captcha.CaptchaText;	//Сохранение строки каптчи
        }

        /// <summary>
        /// Завершение приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNavigation_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// переход в каталог для гостя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGuest_Click(object sender, RoutedEventArgs e)
        {
            Helper.User = null;
            goCatalog();
        }

        /// <summary>
        /// переход в каталог для авторизации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text;
            string password = passwordBoxPassword.Password;

            StringBuilder sb = new StringBuilder();
            if (String.IsNullOrEmpty(login))
            {
                sb.AppendLine("Вы забыли ввести логин");
            }
            if (String.IsNullOrEmpty(password))
            {
                sb.AppendLine("Вы забыли ввести пароль");
            }
            if (!isFirst)
            {
                if (textBoxCaptcha.Text != captchaText)
                {
                    sb.AppendLine("Каптча введена неверно");
                    //MessageBox.Show(captcha.CaptchaText);
                }
            }
            //поиск в бд пользователя с таким логином и паролем

            if (sb.Length == 0)
            {
                List<Model.User> users = new List<Model.User>();
                //получить все данные их таблицы
                //users = Helper.DB.User.ToList();
                users = Helper.DB.User.Where(u => u.UserLogin == login && u.UserPassword == password).ToList();
                //Model.User user = users.FirstOrDefault();
                Helper.User = users.FirstOrDefault();
                if (Helper.User != null)
                {
                    sb.AppendLine("Здравствуйте " + Helper.User.UserFullName);
                    sb.AppendLine("Вы вошли с ролью " + Helper.User.Role.RoleName);
                    MessageBox.Show(sb.ToString());
                    goCatalog();
                }
                else
                {
                    sb.AppendLine("Такой пользователь не зарегистрирован");
                }

            }

            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
                if (isFirst)
                {
                    captcha.Visibility = Visibility.Visible;
                    textBoxCaptcha.Visibility = Visibility.Visible;
                }
                if (!isFirst)
                {
                    buttonEnter.IsEnabled = false;
                    captcha.CreateCaptcha(Captcha.LetterOption.Alphanumeric, 4);
                    captchaText = captcha.CaptchaText;	//Сохранение строки каптчи
                    timer.Tick += new EventHandler(TimerTick);
                    timer.Interval = new TimeSpan(40000000);
                    timer.Start();
                }
                isFirst = false;
                return;
            }

        }

        private void TimerTick(object sender, EventArgs e)
        {
            buttonEnter.IsEnabled = true;
            timer.Stop();
        }


        private void isVisiblePassword_Checked(object sender, RoutedEventArgs e)
        {
            tbPassword.Text = passwordBoxPassword.Password;
            tbPassword.Visibility = Visibility.Visible;
            passwordBoxPassword.Visibility = Visibility.Hidden;
        }

        private void isVisiblePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            passwordBoxPassword.Password = tbPassword.Text;
            tbPassword.Visibility = Visibility.Hidden;
            passwordBoxPassword.Visibility = Visibility.Visible;
        }

        private void goCatalog()
        {
            View.Catalog catalog = new View.Catalog();
            this.Hide();
            catalog.ShowDialog();
            this.Show();
        }
    }
}
