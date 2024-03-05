using BooksSharp.Classes;
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
    /// Interaction logic for WorkOrder.xaml
    /// </summary>
    public partial class WorkOrder : Window
    {
        public WorkOrder()
        {
            InitializeComponent();
        }
        //Список всех заказов из БД
        List<Model.Order> listOrders = new List<Model.Order>();
        //Список заказанных товаров
        List<Model.OrderProduct> listProductsInOrders = new List<Model.OrderProduct>();
        //Список заказанных товаров с дополнительными свойствами
        List<Classes.OrderExtended> listExtendedOrders;

        Classes.OrderExtended selectOrder;		//Выбранный заказ

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Список статусов заказа
            cbStatus.ItemsSource = Helper.DB.Status.ToList();
            cbStatus.DisplayMemberPath = "StatusName";
            cbStatus.SelectedValuePath = "StatusID";
            cbSort.SelectedIndex = 0;
            //cbFilterDiscount.ItemsSource = new List<string>() { "Все диапазоны", "До 3%", "От 3 до 5%", "От 5%" };
            //cbFilterDiscount.SelectedIndex = 0;
            ShowOrders();
        }

        /// Метод отображает информацию о заказах с дополнительными свойствами
        /// </summary>
        private void ShowOrders()
        {
            listOrders = Helper.DB.Order.ToList();	//Получить все заказы
            int totalCount = listOrders.Count;		//Их общее количество
            listProductsInOrders = Helper.DB.OrderProduct.ToList(); //Получить все заказанные товары
            //Создание списка заказанных товаров с расширенными свойствами
            listExtendedOrders = new List<OrderExtended>();
            foreach (var order in listOrders)
            {
                Classes.OrderExtended orderExtended = new OrderExtended();
                orderExtended.Order = order;		//Заполнить данные о заказе из БД
                //Вызов методов класса через объект для вычисления дополнительных свойств
                orderExtended.TotalSumma = orderExtended.CalcTotalSummma(listProductsInOrders);
                orderExtended.TotalDiscount = orderExtended.CalcTotalDiscount(listProductsInOrders);
                listExtendedOrders.Add(orderExtended);
            }

            if (cbFilterDiscount != null)
            {
                if (cbFilterDiscount.SelectedIndex == 1)
                    listExtendedOrders = listExtendedOrders.Where(order => order.TotalDiscountPercent < 3).ToList();
                else if (cbFilterDiscount.SelectedIndex == 2)
                    listExtendedOrders = listExtendedOrders.Where(order => order.TotalDiscountPercent > 2.99 && order.TotalDiscountPercent < 5).ToList();
                else if (cbFilterDiscount.SelectedIndex == 3)
                    listExtendedOrders = listExtendedOrders.Where(order => order.TotalDiscountPercent > 4.99).ToList();
            }

            //Сортировка
            switch (cbSort.SelectedIndex)
            {
                case 1:
                    listExtendedOrders = listExtendedOrders.OrderBy(or => or.TotalSumma).ToList();
                    break;
                case 2:
                    listExtendedOrders =
                                       listExtendedOrders.OrderByDescending(or => or.TotalSumma).ToList();
                    break;
            }
            //Отображение информации о заказах
            int filterCount = listExtendedOrders.Count;
            listViewOrders.ItemsSource = null;
            listViewOrders.ItemsSource = listExtendedOrders;
            tbCount.Text = filterCount + " из " + totalCount;
        }

        /// Метод создает список заказанных товаров только для конкретного заказа
        /// <param name="orderId"> Номер заказа</param>
        /// <returns>Построенный список из товаров в заказе</returns>
        public List<Model.OrderProduct> ListProductsInOrderId(int orderId)
        {
            List<Model.OrderProduct> list = new List<Model.OrderProduct>();
            //Поиск всех товаров для выбранного номера заказа
            list = listProductsInOrders.FindAll(pr => pr.OrderID == orderId).ToList();
            return list;
        }

        private void buttonNavigate_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Сохранение в таблицу Order БД изменений о заказе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butEditOrder_Click(object sender, RoutedEventArgs e)
        {
            //Есть выбранный заказ для редактирования
            if (selectOrder != null)
            {
                //Получить объект модели заказа
                Model.Order order = selectOrder.Order;
                //Задать новые значения для даты доставки и статуса
                order.OrderDelivery = (DateTime)dateDelivery.SelectedDate;
                order.OrderStatus = (int)cbStatus.SelectedValue;
                try
                {
                    Helper.DB.SaveChanges();		//Сохранение в БД
                    MessageBox.Show("Данные успешно обновлены");
                    ShowOrders();
                }
                catch
                {
                    MessageBox.Show("При обновлении данных возникли проблемы");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Не выбран редактируемый заказ");
            }
        }

        /// <summary>
        /// Выбор заказа в списке заказов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Проверка, что есть выбранный заказ
            if (listViewOrders.SelectedItem == null)
                return;
            //Получение всей информации о выбранном заказе
            selectOrder = listViewOrders.SelectedItem as Classes.OrderExtended;
            //Формирование списка товаров в этом заказе по разработанному методу
            List<Model.OrderProduct> listTemp = ListProductsInOrderId(selectOrder.Order.OrderID);
            //Отображение товаров выбранного заказа
            listViewOrder.ItemsSource = listTemp;
            //Отобразить статус заказа
            cbStatus.SelectedValue = selectOrder.Order.OrderStatus;
            //Отобразить дату доставки заказа
            dateDelivery.SelectedDate = selectOrder.Order.OrderDelivery;
            ShowOrders();
        }

        /// <summary>
        /// Выбор условия фильтрации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFilterDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowOrders();
        }

        /// <summary>
        /// Выбор направления сортировки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowOrders();
        }
    }
}
