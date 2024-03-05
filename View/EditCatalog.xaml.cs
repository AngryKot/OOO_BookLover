using BooksSharp.Classes;
using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace BooksSharp.View
{
    /// <summary>
    /// Interaction logic for EditCatalog.xaml
    /// </summary>
    public partial class EditCatalog : Window
    {
        OpenFileDialog dlg = new OpenFileDialog();
        bool isPhoto = false;		//Наличие фото
        string filePath;			//Путь к фото из диалога
        //Путь к папке с фотографиями
        string pathPhoto = System.IO.Directory.GetCurrentDirectory() + @"\Images\";
        Model.Product product;	//Товар, с которым сейчас работаю

        /// <summary>
        /// Конструктор окна без параметров - при добавлении нового товара
        /// </summary>
        public EditCatalog()
        {
            InitializeComponent();
            tbTitle.Text = "Добавление товара";
            tbArt.IsEnabled = true;	//Доступен артикль для заполнения
            tbDiscr.Clear();
        }

        /// Конструктор окна с параметром - при редактировании товара
        /// <param name="productSelected">Переданный товар</param>
        public EditCatalog(Classes.ProductExtended productSelected)
        {
            InitializeComponent();
            tbTitle.Text = "Редактирование товара";
            //Отобразить фото
            imagePhoto.Source = new BitmapImage(new
                                Uri(productSelected.ProductPhotoPath, UriKind.RelativeOrAbsolute));
            //Блокировать кнопку изменения фото
            butSelectPhoto.Visibility = Visibility.Collapsed;
            //Информация о продукте
            product = productSelected.Product;
            tbArt.Text = product.ProductArticle;
            tbArt.IsEnabled = false;		//Блокировать артикль
            //Все остальные поля товара вывести в элементы интерфейса
            tbName.Text = product.ProductName;
            cbManuf.SelectedValue = product.ProductManufacturer;
            cbProv.SelectedValue = product.ProductManufacturer;
            cbCat.SelectedValue = product.ProductUnit;
            tbCost.Text = product.ProductCost.ToString();
            tbCount.Text = product.ProductCount.ToString();
            tbMaxDisc.Text = product.ProductDiscountMax.ToString();
            tbCurDisc.Text = product.ProductDiscount.ToString();
            tbDiscr.Text = product.ProductDiscription;
        }

        /// <summary>
        /// Подготовительные действия
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Заполнение и настройка списков из БД

            cbManuf.DisplayMemberPath = "ManufacturerName";
            cbManuf.SelectedValuePath = "ManufacturerID";
            cbManuf.ItemsSource = Helper.DB.Manufacturer.ToList();
            cbManuf.SelectedIndex = 0;

            cbProv.DisplayMemberPath = "ProviderName";
            cbProv.SelectedValuePath = "ProviderID";
            cbProv.ItemsSource = Helper.DB.Provider.ToList();
            cbProv.SelectedIndex = 0;

            cbCat.DisplayMemberPath = "CategoryName";
            cbCat.SelectedValuePath = "CategoryID";
            cbCat.ItemsSource = Helper.DB.Category.ToList();
            cbCat.SelectedIndex = 0;

            cbUnit.DisplayMemberPath = "UnitName";
            cbUnit.SelectedValuePath = "UnitID";
            cbUnit.ItemsSource = Helper.DB.Unit.ToList();
            cbUnit.SelectedIndex = 0;
            //Настройка диалога
            dlg.InitialDirectory = @"C:\Users\Polina\Documents\FlowersSharp\flowerShop\flowerShop\bin\Debug\Images";
            dlg.Filter = "Image files (*.jpg)|*.jpg";
        }

        private void buttonNavigation_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Выбрать фото
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (dlg.ShowDialog() == true)
            {
                filePath = dlg.FileName;
                //Отобразить фото в компоненте
                imagePhoto.Source = new BitmapImage(new Uri(filePath, UriKind.Absolute));
                isPhoto = true;		 //Есть фото
            }
        }

        /// <summary>
        /// Сохранить в БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butSaveInDB_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();	//Строка с сообщениями
            sb.Clear();
            //Проверка пустоты всех полей
            if (tbArt.IsEnabled)				//При добавлении проверяем артикль
            {
                if (String.IsNullOrEmpty(tbArt.Text))
                {
                    sb.Append("Вы не ввели артикль." + Environment.NewLine);
                }
                else
                {
                    //Проверка отсутствия повторения артикля
                    Model.Product product = Helper.DB.Product.
                                                             FirstOrDefault(pr => pr.ProductArticle == tbArt.Text);
                    if (product != null)
                    { sb.Append("Такой артикль существует." + Environment.NewLine); }
                }
            }
            //Проверка непустых значений для обязательных полей
            if (String.IsNullOrEmpty(tbName.Text))
            { sb.Append("Вы не ввели название." + Environment.NewLine); }
            if (String.IsNullOrEmpty(tbCost.Text))
            { sb.Append("Вы не ввели цену." + Environment.NewLine); }
            if (String.IsNullOrEmpty(tbMaxDisc.Text))
            { sb.Append("Вы не ввели максимальную скидку." + Environment.NewLine); }
            if (String.IsNullOrEmpty(tbCurDisc.Text))
            { sb.Append("Вы не ввели действующую скидку." + Environment.NewLine); }
            if (String.IsNullOrEmpty(tbCount.Text))
            { sb.Append("Вы не ввели количество." + Environment.NewLine); }
            if (sb.Length > 0)			//Есть сообщения об ошибках
            {
                MessageBox.Show(sb.ToString());
            }
            else					//Ошибок нет
            {
                if (tbArt.IsEnabled)		//При добавлении
                {
                    product = new Model.Product();		//Создаем добавляемый объект
                    product.ProductArticle = tbArt.Text;		//Внимание на артикль и фото
                    //Есть фото
                    if (isPhoto)
                    {
                        product.ProductPhoto = product.ProductArticle + ".jpg";	 //Для записи в БД
                        string newPath = pathPhoto + product.ProductPhoto;	//Полное имя файла цели
                        System.IO.File.Copy(filePath, newPath, true); //Копирование из диалога в целевую
                    }
                }
                //Получаем значения для всех остальных полей при добавлении/редактировании
                product.ProductName = tbName.Text;
                product.ProductManufacturer = (int)cbManuf.SelectedValue;
                product.ProductProvider = (int)cbProv.SelectedValue;
                product.ProductCategory = (int)cbCat.SelectedValue;
                product.ProductUnit = (int)cbUnit.SelectedValue;
                product.ProductCost = (decimal)Convert.ToDouble(tbCost.Text);
                product.ProductCount = Convert.ToInt32(tbCount.Text);
                product.ProductDiscountMax = Convert.ToInt32(tbMaxDisc.Text);
                product.ProductDiscount = Convert.ToInt32(tbCurDisc.Text);
                product.ProductDiscription = tbDiscr.Text;
                if (tbArt.IsEnabled)				//При добавлении
                {
                    Helper.DB.Product.Add(product);		//Добавить в кэш новую запись
                }
                try
                {
                    Helper.DB.SaveChanges();		//Обновить БД и при добавлении/редактировании
                    MessageBox.Show("БД успешно обновлена");
                }
                catch
                {
                    MessageBox.Show("При обновлении БД возникли проблемы");
                }
                this.Close();
            }
        }
    }
}
