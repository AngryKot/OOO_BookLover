using Aspose.BarCode.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;

namespace BooksSharp.Classes
{
    public class ProductExtended
    {
        public ProductExtended(Model.Product product)
        {
            this.Product = product;
        }
        public Model.Product Product { get; set; }

        //строка с заглушкой или фото+папка
        public string ProductPhotoPath
        {
            get
            {
                string temp = Environment.CurrentDirectory + "/Images/" + Product.ProductPhoto;
                if (!File.Exists(temp))
                {
                    temp = "/Resources/picture.png";

                }
                return temp;
            }

        }

        //цена со скидкой
        private double productCostWithDiscount;
        public double ProductCostWithDiscount
        {
            get
            {
                double temp = 0.0;
                double discount = (double)Product.ProductCost * (double)Product.ProductDiscount / 100;
                temp = (double)Product.ProductCost - discount;
                return temp;
            }
            set
            {
                productCostWithDiscount = value;
            }
        }

        //public ImageSource ProductBarcode
        //{
        //    get
        //    {
        //        return CreateBarcode(Product.ProductArticle);
        //    }
        //}

        //private ImageSource CreateBarcode(string barcodeText)
        //{
        //    var imageType = "Png"; //тип сохраненной картинки
        //    var imageFormat = (BarCodeImageFormat)Enum.Parse(typeof(BarCodeImageFormat), imageType.ToString());
        //    // Set default as Code128
        //    var encodeType = EncodeTypes.Code128;
        //    // Initalize barcode object
        //    BarCode barcode = new BarCode /*создаем объект класса штрихкод*/
        //    {
        //        Text = barcodeText,
        //        BarcodeType = encodeType,
        //        ImageType = imageFormat
        //    };

        //    string imagePath = "";
        //    // Generate barcode and get image path
        //    imagePath = GenerateBarcode(barcode);
        //    // Display the image
        //    Uri fileUri = new Uri(imagePath);
        //    return new BitmapImage(fileUri); //вставка кода в image
        //}

        //private string GenerateBarcode(BarCode barcode)
        //{
        //    //// Image path
        //    //string imagePath = @"C:\Users\Polina\Documents\FlowersSharp\Images\" + barcode.Text + "." + barcode.ImageType;

        //    //if (!File.Exists(imagePath))
        //    //{
        //    //    // Initilize barcode generator
        //    //    BarcodeGenerator generator = new BarcodeGenerator(barcode.BarcodeType, barcode.Text);

        //    //    // Save the image
        //    //    generator.Save(imagePath, barcode.ImageType);
        //    //}


        //    //return imagePath;
        //}
    }
}
