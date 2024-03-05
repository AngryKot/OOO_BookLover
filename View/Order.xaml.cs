using BooksSharp.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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
using ZXing;


namespace BooksSharp.View
{
    /// <summary>
    /// Interaction logic for Order.xaml
    /// </summary>
    public partial class Order : Window
    {
        List<Classes.ProductInOrder> userOrder;
        private ImageSource QRCode;
        public Order()
        {
            InitializeComponent();
        }

        public Order(List<Classes.ProductInOrder> userOrder)
        {
            InitializeComponent();
            this.userOrder = userOrder;
            //MessageBox.Show("" + userOrder.Count);
            ShowInfo();

            cbPoint.ItemsSource = Helper.DB.Point.ToList();

        }


        //отображать инфу о товаре
        private void ShowInfo()
        {
            listBoxProductsInOrder.ItemsSource = userOrder;
            CalcOrder();
        }


        //расчеты по заказу
        private void CalcOrder()
        {
            double summaTotal = 0;
            double summaWithDiscount = 0;
            double summaDiscount = 0;
            foreach (var item in userOrder)
            {
                summaTotal += (double)item.ProductExtendedInOrder.Product.ProductCost * item.countProductInOrder;
                summaWithDiscount += (double)item.ProductExtendedInOrder.ProductCostWithDiscount * item.countProductInOrder;
            }
            tbSummaTotal.Text = "Стоимость заказа: " + summaTotal.ToString();
            tbSummaWithDiscount.Text = "Стоимость со скидкой: " + summaWithDiscount.ToString();
            summaDiscount = summaTotal - summaWithDiscount;
            tbSummaDiscount.Text = "Сумма скидки: " + summaDiscount.ToString();

        
           

           
        }

       

        private void buttonNavigation_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// оформить заказ - сохранение в БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (userOrder.Count > 0) //есть товары
            {
                Model.Order order = new Model.Order();
                order.OrderClient = tbFIO.Text;

                //выбор id пункта выдачи
                int pointId = (cbPoint.SelectedItem as Model.Point).PointID;
                order.OrderPoint = pointId;
                order.OrderDate = DateTime.Now;
                order.OrderCode = new Random().Next(100, 1000);

                var moreThanExists = false;
                foreach (var item in userOrder)
                {
                    if (item.countProductInOrder > item.ProductExtendedInOrder.Product.ProductCount)
                    {
                        moreThanExists = true;
                        break;
                    }
                }
                if (moreThanExists)
                    order.OrderDelivery = DateTime.Now.AddDays(6);
                else
                    order.OrderDelivery = DateTime.Now.AddDays(3);

                order.OrderStatus = 1;
                Helper.DB.Order.Add(order);
                try
                {
                    Helper.DB.SaveChanges();
                    foreach (var item in userOrder)
                    {
                        Model.OrderProduct orderProduct = new Model.OrderProduct();
                        orderProduct.OrderID = order.OrderID;
                        orderProduct.ProductArticle = item.ProductExtendedInOrder.Product.ProductArticle;
                        orderProduct.ProductCount = item.countProductInOrder;
                        Helper.DB.OrderProduct.Add(orderProduct);
                        Helper.DB.SaveChanges();

                    }
                    MessageBox.Show("Заказ оформлен");

                
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Сохранять нечего");
            }
        }

        private void butDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as ProductInOrder;
            userOrder.Remove(item);
            //сброс листбокса чтобы потом обновить в нем данные
            listBoxProductsInOrder.ItemsSource = null;
            ShowInfo();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var item = (sender as TextBox).DataContext as ProductInOrder;
            //получаем индекс самого найденного элемента
            var replaceIndex = userOrder.IndexOf(item);
            try
            {
                item.countProductInOrder = int.Parse((sender as TextBox).Text);
            }
            catch
            {
                item.countProductInOrder = 0;
            }
            //перезапись данных
            userOrder[replaceIndex] = item;

            ShowInfo();
        }
    }
}
