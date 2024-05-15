using System;
using System.IO;
using Tesseract;

namespace podautomation
{
    internal class Program
    {
        public static int successInvoice = 0;
        public static int failureInvoice = 0;

        static void Main(string[] args)
        {
            string folderPath = @"D:\pods\"; // Path to the folder containing the invoices
            string[] files = Directory.GetFiles(folderPath);
            string sourceFile = @"D:\pods\";
            string destinationSuccess = @"D:\pods\Success\";
            string destinationFailed = @"D:\pods\Failed\";
            int totalInvoices = files.Length;

            Console.WriteLine("Total Invoices Found " +totalInvoices);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string invoiceNumber = ExtractInvoiceNumber(file, sourceFile, destinationSuccess, destinationFailed, fileName);
                Console.WriteLine("Extracted Invoice Number from : " + fileName + " is " + invoiceNumber);
            }

            Console.WriteLine("Successful Invoices " + successInvoice);
            Console.WriteLine("Failed Invoices " + failureInvoice);

        }

        public static string ExtractInvoiceNumber(string imagePath, string sourceFile, string destinationSuccess, string destinationFailed, string fileName)
        {
            try
            {
                // Initialize Tesseract engine
                using (var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        // Perform OCR on the image
                        using (var page = engine.Process(img))
                        {
                            string text = page.GetText();

                            // Regular expression pattern to search for invoice number
                            var invoiceNumberPattern = @"\bIN\d{6}\b";

                            // Search for invoice number in the extracted text
                            var match = System.Text.RegularExpressions.Regex.Match(text, invoiceNumberPattern);

                            if (match.Success)
                            {
                                successInvoice++;
                                //Block to move invoices status to Success/Failed invoices
                                try
                                {
                                    string destinationDirectory = Path.GetDirectoryName(destinationSuccess);
                                    if (!Directory.Exists(destinationDirectory))
                                    {
                                        Directory.CreateDirectory(destinationSuccess);
                                    }
                                    // Move the file
                                    string fileToMove = Path.Combine(destinationDirectory, match.Value + ".jpg");
                                    File.Move(imagePath, fileToMove);
                                    Console.WriteLine($"File moved from {imagePath} to {fileToMove}");
                                }
                                catch (IOException ioEx)
                                {
                                    Console.WriteLine($"IO Exception: {ioEx.Message}");
                                }
                                catch (UnauthorizedAccessException uaEx)
                                {
                                    Console.WriteLine($"Unauthorized Access Exception: {uaEx.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"General Exception: {ex.Message}");
                                }

                                return match.Value;
                            }
                            else
                            {
                                failureInvoice++;
                                try
                                {
                                    string destinationDirectory = Path.GetDirectoryName(destinationFailed);
                                    if (!Directory.Exists(destinationDirectory))
                                    {
                                        Directory.CreateDirectory(destinationFailed);
                                    }
                                    // Move the file
                                    string fileToMove = Path.Combine(destinationDirectory, fileName);
                                    File.Move(imagePath, fileToMove);
                                    Console.WriteLine($"File moved from {imagePath} to {fileToMove}");
                                }
                                catch (IOException ioEx)
                                {
                                    Console.WriteLine($"IO Exception: {ioEx.Message}");
                                }
                                catch (UnauthorizedAccessException uaEx)
                                {
                                    Console.WriteLine($"Unauthorized Access Exception: {uaEx.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"General Exception: {ex.Message}");
                                }

                                return "Invoice number not found";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}
