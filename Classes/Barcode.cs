using Aspose.BarCode.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace BooksSharp.Classes
{
    public class BarCode
    {
        public string Text { get; set; }
        public BaseEncodeType BarcodeType { get; set; }
        public BarCodeImageFormat ImageType { get; set; }

    }
}
