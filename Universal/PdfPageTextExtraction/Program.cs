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
            using (var pdf =
                new PdfLoadedDocument(
                    @"C:\Users\shiv\GitHub\Source\Repos\UniversalApps\Universal\PdfPageTextExtraction\Dump\test.pdf"))
            {
                foreach (PdfPageBase page in pdf.Pages)
                {
                    page.Rotation = PdfPageRotateAngle.RotateAngle90;
                    List<TextData> textDataCollection;
                    string extractedText = page.ExtractText(out textDataCollection);
                    page.Rotation = PdfPageRotateAngle.RotateAngle0;
                    List<TextData> textDataCollection1;
                    string extractedText1 = page.ExtractText(out textDataCollection1);

                    var t = textDataCollection.FirstOrDefault(x => x.Text.Contains("CUSTOMER"));
                    var t1 = textDataCollection1.FirstOrDefault(x => x.Text.Contains("CUSTOMER"));
                }

                pdf.Save();
            }
        }
    }
}
