using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfCommandBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            //check if there is an argument.
            if(args.Length < 1)
            {
                Console.WriteLine("Please provide a pdf file as parameter");
                Environment.Exit(2);
            }

            string filename = args[0];
            if (!File.Exists(filename))
            {
                Console.WriteLine("The file provided does not exist");
                Environment.Exit(1);
            }

            string parsed = ReadPdfFile(filename);

            if (string.IsNullOrEmpty(parsed))
            {
                Console.WriteLine("No Text was extracted");
                Environment.Exit(3);
            }

            //Console.WriteLine("extracted text:");
            //Console.WriteLine(parsed);

            //Read Json config
            Settings settings = new Settings()
            {
                command = "",
                arguments = "",
                testmode = true
            };

            if (File.Exists("settings.json"))
            {
                var filecontent = File.ReadAllText(@"settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(filecontent);
            }
            else
            {
                File.WriteAllText(@"settings.json",JsonConvert.SerializeObject(settings));
            }
            

            var extracted = ExtractControlstrings(parsed);

            if (!extracted.ContainsKey("%%FullPath%%"))
            {
                extracted.Add("%%FullPath%%", System.IO.Path.GetFullPath(filename));
            }           


            Console.WriteLine("Extracted commands");
            foreach (var command in extracted)
            {
                Console.WriteLine(string.Concat(command.Key, " -> ", command.Value));
            }
            Console.WriteLine("You can also use all Environment variables");

            var finalCommand = ReplaceCommand(settings.command, extracted);
            var finalArguments = ReplaceCommand(settings.arguments, extracted);

            Console.WriteLine("Command to run:");
            Console.WriteLine(finalCommand);
            Console.WriteLine("Parameters:");
            Console.WriteLine(finalArguments);

            if (settings.testmode)
            {
                Console.WriteLine("Testmode, press enter to exit");
                Console.ReadLine();
            }
            else
            {
                System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo(finalCommand, finalArguments);
                System.Diagnostics.Process.Start(pi);
            }
        }

        static string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            PdfReader pdfReader = new PdfReader(fileName);

            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                text.Append(currentText);
            }
            pdfReader.Close();

            return text.ToString();
        }

        static Dictionary<string,string> ExtractControlstrings(string input)
        {
            Regex rx = new Regex(@"\%\%(\w+)\:\s*(.*?)%%", RegexOptions.CultureInvariant);
            var result = new Dictionary<string, string>();

            foreach (Match match in rx.Matches(input))
            {
                if(match.Groups.Count > 2)
                {
                    var key = string.Concat("%%", match.Groups[1].Value, "%%");
                    if (! result.ContainsKey(key))
                    {
                        result.Add(key, match.Groups[2].Value);
                    }                    
                }
            }

            return result;
        }

        static string ReplaceCommand (string command, Dictionary<string,string> parameters)
        {
            var result = command;
            Regex rx = new Regex(@"(%%\w+%%)", RegexOptions.CultureInvariant);
            foreach (Match match in rx.Matches(command))
            {
                var variable = match.Groups[1].Value;
                if (parameters.ContainsKey(variable))
                {
                    result = result.Replace(variable, parameters[variable]);
                }
                else
                {
                    var envvalue = Environment.ExpandEnvironmentVariables(variable.Replace("%%", "%"));
                    if (envvalue.Contains("%"))
                    {
                        result = result.Replace(variable, "");
                    }
                    else
                    {
                        result = result.Replace(variable, envvalue);
                    }                    
                }
            }
                return result;
        }
    }
}
