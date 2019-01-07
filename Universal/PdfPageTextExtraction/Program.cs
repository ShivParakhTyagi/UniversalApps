using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;

namespace PdfPageTextExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = "";
            file = @"C:\Users\shiv\GitHub\Source\Repos\UniversalApps\Universal\PdfPageTextExtraction\Dump\test.pdf";
            file = @"C:\IOCheck_DRAWINGS2\Drawing_rptate1\IO Check Tool_rptate1_53 - Copy.pdf";
            file = @"C:\IOCheck_DRAWINGS2\Drawing_4Page\IO Check Tool_4Page_1.pdf";
            file = @"C:\Users\shiv\Desktop\bin\Combined3000.pdf";
            using (var pdf =
                new PdfLoadedDocument(file))
            {
                foreach (PdfPageBase page in pdf.Pages)
                {
                    //page.Rotation = PdfPageRotateAngle.RotateAngle90;
                    //List<TextData> textDataCollection;
                    //string extractedText = page.ExtractText(out textDataCollection);
                    //page.Rotation = PdfPageRotateAngle.RotateAngle0;
                    List<TextData> textDataCollection1;
                    string extractedText1 = page.ExtractText(out textDataCollection1);

                    //var t = textDataCollection.FirstOrDefault(x => x.Text.Contains("CUSTOMER"));
                    var t1 = textDataCollection1.FirstOrDefault(x => x.Text.Contains("CUSTOMER"));
                    foreach (var textData in textDataCollection1)
                    {
                        Console.WriteLine(textData.Text);
                    }
                    Console.WriteLine("press Enter to continue...");
                    Console.ReadLine();
                }

                Console.ReadLine();
                //pdf.Save();
            }
        }
    }
}
