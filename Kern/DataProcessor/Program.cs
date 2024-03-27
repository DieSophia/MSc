using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
    internal class Program
    {
        private static readonly string pfad = @"..\..\..\XMLReaderTool\bin\Debug\exported.txt";
        //Die ID des Knotens am Haupteingang der Bergischen Universität Wuppertal.
        private static string id_quellknoten = "1446853352"; //Zu (51.2456660, 7.1497853)
        static void Main(string[] args)
        {
            StreamReader datei = File.OpenText(pfad);
            using (FileStream dat = new FileStream(@".\dataPrepared.txt", FileMode.Create, FileAccess.Write))
            {
                string aktId = id_quellknoten;
                int i = 1;
                while (!datei.EndOfStream)
                {   
                    string s = datei.ReadLine();
                    if (s.StartsWith($"n:\t{aktId}"))
                    {
                        SetAndWriteAusgehendeKanten(aktId, dat);
                        datei.Close();
                        //Anweisung hier essenziell, da sonst versucht wird, weiterzulesen.
                        break;
                    }
                    i++;
                }
                Console.WriteLine(i);
                Console.ReadLine();
            }
        }

        private static void SetAndWriteAusgehendeKanten(string aktId, FileStream dat)
        {
            bool saveNextId = false;
            string aktWegId = null;
            bool checkNextNodes = false;
            StreamReader datei = File.OpenText(pfad);
            while (!datei.EndOfStream)
            {
                string s = datei.ReadLine();
                if (saveNextId)
                {
                    aktWegId = s;
                    saveNextId = false;
                }
                if (s.StartsWith($"w:\t"))
                {
                    saveNextId = true;
                }
                if (aktWegId != null && s.StartsWith("\t{"))
                {
                    checkNextNodes = true;
                }
                if (aktWegId != null && s.StartsWith("\t}"))
                {
                    checkNextNodes = false;
                }
            }
        }
    }
}
