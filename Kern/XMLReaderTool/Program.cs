using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLReaderTool
{
    public readonly struct Knoten{ 
        public Knoten(uint id, double lat, double lon)
        {
            Id = id;
            Breite = lat;
            Laenge = lon;
        }
        public uint Id { get; }
        public double Breite { get;}
        public double Laenge { get;}
        public override string ToString() => $"{Id}:({Breite}, {Laenge})";
    };

    public readonly struct BoundingBox
    {
        public BoundingBox(Tuple<double, double> suedwestecke, Tuple<double, double> nordostecke)
        {
            Suedwestecke = suedwestecke;
            Nordostecke = nordostecke;
        }
        public Tuple<double, double> Suedwestecke { get; }
        public Tuple<double, double> Nordostecke { get; }
        public override string ToString() => $"({Suedwestecke}, {Nordostecke})";
    };

    internal class Program
    {
        static void Main(string[] args)
        {
            var Haupteingang_BUW = new Knoten(2344738309, 51.2457103, 7.1499000);
            var Box_100 = new BoundingBox(new Tuple<double, double>(50.346226, 5.72198), new Tuple<double, double>(52.1452, 8.58682));

            XmlReaderSettings settings = new XmlReaderSettings();

            //using (XmlReader reader = XmlReader.Create(new FileStream(@"D:\nordrhein-westfalen-latest.osm", FileMode.Open), settings))
            using (XmlReader reader = XmlReader.Create(new FileStream(@"C:\Users\Subira\Documents\test.osm", FileMode.Open), settings))
            {
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
                FileStream ziel = new FileStream(@"D:\kartenausschnitt.txt", FileMode.Create, FileAccess.ReadWrite);
                FileStream log = new FileStream(@"D:\log.txt", FileMode.Create, FileAccess.ReadWrite);
                int i = 0;

                while (reader.Read())
                {
                    Console.WriteLine(reader.Name);
                    Console.WriteLine(reader.Depth);
                    string toWrite = "";
                    if (reader.Name.Equals("node"))
                    {
                        if (reader.MoveToAttribute("lat"))
                        {
                            double lat = double.Parse(reader.Value, CultureInfo.InvariantCulture);
                            if (lat <= Box_100.Suedwestecke.Item1 || lat >= Box_100.Nordostecke.Item1)
                            {
                                continue;
                            }

                            if (reader.MoveToAttribute("lon"))
                            {
                                double lon = double.Parse(reader.Value, CultureInfo.InvariantCulture);
                                if (lon <= Box_100.Suedwestecke.Item2 || lon >= Box_100.Nordostecke.Item2)
                                {
                                    continue;
                                }
                                if (reader.MoveToAttribute("id"))
                                {
                                    toWrite += "n:"+reader.Value+":("+lat+","+lon+");";
                                    Console.WriteLine(toWrite);
                                    ziel.Write(uniEncoding.GetBytes(toWrite), 0, uniEncoding.GetByteCount(toWrite));
                                }
                            }
                        }
                    }
                    if (reader.Name.Equals("way"))
                    {
                        if (toWrite.Length > 0)
                        {
                            string fehler = "DATENFEHLER: " + toWrite;
                            log.Write(uniEncoding.GetBytes(toWrite), 0, uniEncoding.GetByteCount(toWrite));
                        }
                        if (reader.MoveToAttribute("id"))
                        {
                            /*toWrite += "w:" + reader.Value + ":[";
                            if (reader.MoveToAttribute("nodes"))
                            {
                                toWrite += reader.Value+"]";
                                if (reader.MoveToAttribute("tags"))
                                {
                                    //innerorts???, außerorts?, autobahn?, highway, oneway, maxspeed, minspeed
                                    toWrite += reader.Value;
                                    Console.WriteLine(toWrite);
                                    ziel.Write(uniEncoding.GetBytes(toWrite), 0, uniEncoding.GetByteCount(toWrite));
                                }
                            }*/
                        }
                    }
                }
                Console.WriteLine(i);
                Console.ReadLine();
            }
        }
    }
}
