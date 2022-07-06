using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console.MakeFile;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using static System.Console;

namespace Console.MakeFile
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Initialize HTML to PDF converter 
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
                BlinkConverterSettings settings = new BlinkConverterSettings();

                //Assign Blink settings to HTML converter
                htmlConverter.ConverterSettings = settings;

                //Convert HTML to PDF
                PdfDocument document = htmlConverter.Convert("http://www.google.com/");
                //Save and Close the PDF document 
                document.Save("HTMLToPDF.pdf");
                document.Close(true);

                //This will open the PDF file so, the result will be seen in default PDF viewer
                System.Diagnostics.Process.Start("HTMLToPDF.pdf");

            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
            finally
            {
                Write("Done");
                Read();
            }
        }
    }
}
