using BooksSharp.Classes;
using BooksSharp.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Interaction logic for Catalog.xaml
    /// </summary>
    public partial class Catalog : Window
    {
        //товары в заказе
        List<Classes.ProductInOrder> productInOrders = new List<Classes.ProductInOrder>();

        public Catalog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Не видно меню и кнопки, связанные с ролью
            listBoxProducts.Visibility = Visibility.Hidden;
            butViewOrder.Visibility = Visibility.Hidden;
            butAddProduct.Visibility = Visibility.Hidden;
            butWorkOrder.Visibility = Visibility.Hidden;
            //Информация о пользователе
            if (Helper.User != null)    //Авторизованный
            {
                tbFIO.Text = Helper.User.UserFullName;
                switch (Helper.User.UserRole)
                {
                    case 1: //Пользователь
                        listBoxProducts.Visibility = Visibility.Visible;
                        btnDelete.Visibility = Visibility.Hidden;
                        break;
                    case 2: //Менеджер
                        butWorkOrder.Visibility = Visibility.Visible;
                        listBoxProducts.Visibility = Visibility.Visible;
                        btnDelete.Visibility = Visibility.Visible;
                        break;
                    case 3: //Администратор
                        butWorkOrder.Visibility = Visibility.Visible;
                        butAddProduct.Visibility = Visibility.Visible;
                        listBoxProducts.Visibility = Visibility.Visible;
                        btnDelete.Visibility = Visibility.Visible;
                        break;
                }
            }
            else //Гость
            {
                tbFIO.Text = "Гость";
                listBoxProducts.Visibility = Visibility.Visible;
            }

            //видимость от роли
            cmAddInOrder.Visibility = Visibility.Hidden;
            butViewOrder.Visibility = Visibility.Hidden;
            if (Helper.User == null || Helper.User.UserRole == 1)
            {
                cmAddInOrder.Visibility = Visibility.Visible;
            }

            List<Model.Category> categories = new List<Model.Category>();
            categories = Helper.DB.Category.ToList();
            Model.Category category = new Model.Category();
            category.CategoryID = 0;
            category.CategoryName = "Все категории";
            categories.Insert(0, category);

            cbFilterCategory.DisplayMemberPath = "CategoryName";
            cbFilterCategory.SelectedValuePath = "CategoryID";
            cbFilterCategory.ItemsSource = categories;

            cbSort.SelectedIndex = 0;
            cbFilterDiscount.SelectedIndex = 0;
            cbFilterCategory.SelectedIndex = 0;

            ShowProducts();
        }

        private void ShowProducts()
        {
            List<ProductExtended> extendedProducts =
                Helper.DB.Product.ToList().ConvertAll<ProductExtended>(p => new ProductExtended(p));

            int totalCount = extendedProducts.Count;
            //Сортировка
            switch (cbSort.SelectedIndex)
            {
                case 0:
                    extendedProducts = extendedProducts.OrderBy(pr => pr.Product.ProductCost).ToList();
                    break;
                case 1:
                    extendedProducts = extendedProducts.OrderByDescending(pr => pr.Product.ProductCost).ToList();
                    break;
            }
            int max = 100, min = 0;
            //Фильтр по скидке
            if (cbFilterDiscount.SelectedIndex > 0)
            {
                switch (cbFilterDiscount.SelectedIndex)
                {
                    case 1:
                        min = 0; max = 9;
                        break;
                    case 2:
                        min = 10; max = 14;
                        break;
                    case 3:
                        min = 15; max = 100;
                        break;
                }
                extendedProducts = extendedProducts.Where(pr => pr.Product.ProductDiscountMax >= min && pr.Product.ProductDiscountMax <= max).ToList();
            }
            //фильтр по категории
            if (cbFilterCategory.SelectedIndex > 0)
            {
                extendedProducts = extendedProducts.Where(pr => pr.Product.ProductCategory == (int)cbFilterCategory.SelectedValue).ToList();
            }
            //поиск по названию товара
            string search = tbSearch.Text;
            if (!string.IsNullOrEmpty(search))
            {
                //extendedProducts = extendedProducts.Where(pr => pr.Product.ProductName.Contains(search)).ToList();
                extendedProducts = extendedProducts.Where(pr => pr.Product.ProductName.ToLower().Contains(search.ToLower())).ToList();
            }


            int filterCount = extendedProducts.Count;
            tbCount.Text = filterCount + " из" + totalCount;
            listBoxProducts.ItemsSource = extendedProducts;
        }

        /// <summary>
        /// Возврат на авторизацию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNavigation_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// переход в окно оформления заказа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butViewOrder_Click(object sender, RoutedEventArgs e)
        {
            //окну передали заказ
            Order order = new Order(productInOrders);
            this.Hide();
            order.ShowDialog();
            this.ShowDialog();
        }

        private void butWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            WorkOrder workOrder = new WorkOrder();
            this.Hide();
            workOrder.ShowDialog();
            this.ShowDialog();
        }


        /// <summary>
        /// Добавление товара
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butAddProduct_Click(object sender, RoutedEventArgs e)
        {
            //Вызов конструктора без параметров
            View.EditCatalog editCatalog = new View.EditCatalog();
            this.Hide();
            editCatalog.ShowDialog();
            this.ShowDialog();
        }

        /// <summary>
        /// удалить товар 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Classes.ProductExtended productSelected = null;
            if (listBoxProducts.SelectedItem == null)	//Проверка, что есть выбранный товар в списке
            {
                MessageBox.Show("Выберите удаляемый товар");
                return;
            }
            //Выбранный товар в каталоге
            productSelected = listBoxProducts.SelectedItem as Classes.ProductExtended;
            if (MessageBox.Show("Вы действительно хотите удалить этот товар?",
                        "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                //Выбранный товар
                Model.Product product = productSelected.Product;
                //Поиск его среди заказанных товаров для правильного удаления
                Model.OrderProduct orderProduct = Helper.DB.OrderProduct.
                                                  FirstOrDefault(pr => pr.ProductArticle == product.ProductArticle);
                if (orderProduct == null)				//Товар еще не заказывали
                {
                    Helper.DB.Product.Remove(product); 	//Можно удалять
                    try
                    {
                        Helper.DB.SaveChanges(); 		//Фиксация изменений в БД
                        MessageBox.Show("Товар успешно удален");
                        ShowProducts();
                    }
                    catch
                    {
                        MessageBox.Show("При удалении возникли проблемы");
                        return;
                    }
                }
                else 					//Товар присутствует в заказах - удалять нельзя
                {
                    MessageBox.Show("Удалить нельзя, т.к. товар есть в заказах");
                    return;
                }
            }
        }

        /// <summary>
        /// Выбор направления сортировки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowProducts();
        }

        /// <summary>
        /// фильтрация по скидкам
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFilterDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowProducts();
        }

        /// <summary>
        /// фильтрация по категории
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFilterCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowProducts();
        }

        /// <summary>
        /// ввод строки поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowProducts();
        }

        /// <summary>
        /// Для редактирования товара - двойной клик по товару
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (Helper.User != null && Helper.User.UserRole == 3) 		//Только для роли администратора 
            {
                //Выбранный товар в каталоге
                Classes.ProductExtended productSelected =
                                                     listBoxProducts.SelectedItem as Classes.ProductExtended;
                //Вызов конструктора с параметром - выбранный товар для редактирования
                View.EditCatalog editCatalog = new View.EditCatalog(productSelected);
                this.Hide();
                editCatalog.ShowDialog();
                this.ShowDialog();
            }
        }

        /// <summary>
        /// Добавить в заказ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miAddInOrder_Click(object sender, RoutedEventArgs e)
        {
            //видимость кнопки "Оформить заказ"
            butViewOrder.Visibility = Visibility.Visible;
            //выбранный товар
            Classes.ProductExtended productSelected = listBoxProducts.SelectedItem as Classes.ProductExtended;
            //артикул выбранного товара 
            string art = productSelected.Product.ProductArticle;
            //результат поиска в заказе товара по артиклю
            Classes.ProductInOrder productSearch = productInOrders.FirstOrDefault(pr => pr.ProductExtendedInOrder.Product.ProductArticle == art);
            //анализ результата поиска
            if (productSearch != null) //нашли
            {
                productSearch.countProductInOrder++;
            }
            else //не нашли
            {
                //создали новый продукт на основе выбранного
                Classes.ProductInOrder productNew = new Classes.ProductInOrder(productSelected);
                //добавили в звказ
                productInOrders.Add(productNew);
            }
        }

        /// <summary>
        /// При активизации окна - переход на это окно - обновить каталог при отображении
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, EventArgs e)
        {
            ShowProducts();
        }
    }
}
