using BooksSharp.Classes;
using System;
using System.Drawing;
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
using Word = Microsoft.Office.Interop.Word;


namespace BooksSharp.View
{
    /// <summary>
    /// Interaction logic for Order.xaml
    /// </summary>
    /// 


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

           //оформление заказа если у любого из товаров кол-во больше 0
                listBoxProductsInOrder.ItemsSource = userOrder;
                butCreateOrder.IsEnabled = userOrder.FindAll(it => it.countProductInOrder > 0).Any();


            
        }

        private ImageSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
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

            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            // Установка кодировки на UTF-8
            var encodingOptions = new ZXing.Common.EncodingOptions
            {
                Width = 100,
                Height = 100,
                Margin = 0
            };

            encodingOptions.Hints.Add(ZXing.EncodeHintType.CHARACTER_SET, "UTF-8");
            barcodeWriter.Options = encodingOptions;
            var bitmap = barcodeWriter.Write(tbSummaWithDiscount.Text);

            QRCode = BitmapToImageSource(bitmap);

            imgQRCode.Source = QRCode;
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
            try
            {
                if (userOrder.Count > 0) //есть товары
                {
                    Model.Order order = new Model.Order();
                    order.OrderClient = tbFIO.Text;

                    try
                    {

                        //выбор id пункта выдачи
                        int pointId = (cbPoint.SelectedItem as Model.Point).PointID;
                        order.OrderPoint = pointId;
                        order.OrderDate = DateTime.Now;
                        order.OrderCode = new Random().Next(100, 1000);
                    }
                    catch
                    {
                        MessageBox.Show("введите корректные данные");
                    }
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

                        //создание чека заказа
                        //объявление необходимых величин
                        Word.Application wordApp;   //сервер Word
                        Word.Document wordDoc;
                        Word.Paragraph wordPar;     //абзац документа
                        Word.Range wordRange;       //тест абзаца
                        Word.Table wordTable;
                        Word.InlineShape wordShape; //рисунок
                                                    //создание сервера Word
                        try
                        {
                            wordApp = new Word.Application();
                            wordApp.Visible = false;
                        }
                        catch
                        {
                            MessageBox.Show("Товарный чек в Word создать не удалось");
                            return;
                        }
                        //Создание документа Word
                        wordDoc = wordApp.Documents.Add(); //добавить новый пустой документ
                        wordDoc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;

                        //**************Первый параграф - заголовок документа: логотип и дата
                        wordPar = wordDoc.Paragraphs.Add();
                        wordPar.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        wordRange = wordPar.Range;
                        wordPar.set_Style("Заголовок 1"); //Стиль взятый из Word
                                                          //Текст первого абзаца - заголовка документа
                        wordRange.Text = "Дата заказа: " + DateTime.Now.ToLongDateString();
                        //Добавить логотип
                        wordShape = wordDoc.InlineShapes.AddPicture(@"D:\УчПрактикаПриложение\BooksSharp\BooksSharp\Resources\logo.png", Type.Missing, Type.Missing);
                        wordShape.Width = 100;
                        wordShape.Height = 100;
                        //************Второй параграф - текст
                        wordRange.InsertParagraphAfter();
                        wordPar = wordDoc.Paragraphs.Add();
                        wordPar.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        wordRange = wordPar.Range;
                        wordRange.Font.Size = 16;
                        wordRange.Font.Color = Word.WdColor.wdColorBlue;
                        wordRange.Font.Name = "Times New Roman";
                        wordRange.Text = "Список купленных товаров";
                        //**********Третий параграф - таблица
                        wordRange = wordPar.Range;
                        //Число строк в таблице совпадает с числом строк в таблице заказов формы
                        wordTable = wordDoc.Tables.Add(wordRange, userOrder.Count + 1, 4);
                        wordTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        wordTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleDouble;
                        //Заголовок таблицы из DataGrid
                        Word.Range cellRange;
                        var columnHeaders = new List<string>() { "Название", "Цена", "Количество", "Итого" };
                        for (int col = 1; col <= columnHeaders.Count; col++)
                        {
                            cellRange = wordTable.Cell(1, col).Range;
                            cellRange.Text = columnHeaders[col - 1];
                        }
                        ////Заливка заголовка таблицы
                        wordTable.Rows[1].Shading.ForegroundPatternColor = Word.WdColor.wdColorLightYellow;
                        wordTable.Rows[1].Shading.BackgroundPatternColorIndex = Word.WdColorIndex.wdBlue;
                        wordTable.Rows[1].Range.Bold = 1;
                        wordTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        wordRange.Font.Size = 14;
                        wordRange.Font.Color = Word.WdColor.wdColorBlue;
                        wordRange.Font.Name = "Times New Roman";
                        //wordRange.Font.Italic= 1;
                        //Заполнение ячеек таблицы из списка заказов
                        wordPar.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                        //wordPar.set_Style("Заголовок 2");
                        for (int row = 2; row <= userOrder.Count + 1; row++)
                        {
                            cellRange = wordTable.Cell(row, 1).Range;
                            cellRange.Text = userOrder[row - 2].ProductExtendedInOrder.Product.ProductName;
                            wordRange.Font.Size = 14;
                            wordRange.Font.Color = Word.WdColor.wdColorBlack;
                            wordRange.Font.Name = "Times New Roman";
                            //wordRange.Font.Italic= 0;
                            cellRange = wordTable.Cell(row, 2).Range;
                            cellRange.Text = userOrder[row - 2].ProductExtendedInOrder.ProductCostWithDiscount.ToString();
                            cellRange = wordTable.Cell(row, 3).Range;
                            cellRange.Text = userOrder[row - 2].countProductInOrder.ToString();
                            cellRange = wordTable.Cell(row, 4).Range;
                            var productFinalCost = userOrder[row - 2].ProductExtendedInOrder.ProductCostWithDiscount * userOrder[row - 2].countProductInOrder;
                            cellRange.Text = productFinalCost.ToString();

                        }
                        //***********Четвертый параграф - итоги
                        wordRange.InsertParagraphAfter();
                        wordPar = wordDoc.Paragraphs.Add();
                        wordRange = wordPar.Range;
                        wordPar.set_Style("Заголовок 1"); //Стиль взятый из Word
                        wordRange.Font.Size = 20;
                        wordRange.Font.Color = Word.WdColor.wdColorRed;
                        wordRange.Bold = 1;
                        wordRange.Text = tbSummaWithDiscount.Text;
                        wordApp.Visible = true;
                        //Сохранение документа
                        string fileName = @"D:\УчПрактикаПриложение\BooksSharp\BooksSharp\bin\Debug\Чек";
                        wordDoc.SaveAs2(fileName + ".docx");
                        wordDoc.SaveAs2(fileName + ".pdf", Word.WdExportFormat.wdExportFormatPDF);
                        //Завершение работы с Word
                        wordDoc.Close(true, null, null); //сначала закрыть документ
                        wordApp.Quit(); //выход из Word
                                        //Вызвать свою подпрограмму уничтожения процессов
                                        //releaseObject(wordPar);         //уничтожить абзац
                                        //releaseObject(wordDoc);          //уничтожить документ
                                        //releaseObject(wordApp);                                    //удалить из Диспетчера задач
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
            catch
            {
                MessageBox.Show  ("Введите корректное число товаров");
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
