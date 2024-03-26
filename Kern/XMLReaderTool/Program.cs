using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLReaderTool
{
    public readonly struct BoundingBox
    {
        public BoundingBox(Tuple<double, double> suedwestecke, Tuple<double, double> nordostecke)
        {
            Suedwestecke = suedwestecke;
            Nordostecke = nordostecke;
        }
        /*public BoundingBox(Tuple<double, double> zentrum, double nordSuedInKm, double ostWestInKm)
        {
            // Folgende Gleichung ist nach x umzustellen und dann der naechstgelegene Wert zu nehmen (Periodizität!)
            1/2*nordsuedInKm = Math.Sqrt((Math.Cos(x)-Math.Cos(zentrum.Item1))^2 + (Math.Sin(x)-Math.Sin(zentrum.Item1))^2)
        }*/
        public Tuple<double, double> Suedwestecke { get; }
        public Tuple<double, double> Nordostecke { get; }
        public override string ToString() => $"({Suedwestecke}, {Nordostecke})";
    };

    internal class Program
    {
        private static BoundingBox Box_100 = new BoundingBox(new Tuple<double, double>(50.346226, 5.72198), new Tuple<double, double>(52.1452, 8.58682));

        private static void schreibeKnoten(XmlReader reader, FileStream ziel)
        {
            if (reader.MoveToAttribute("lat"))
            {
                double lat = double.Parse(reader.Value, CultureInfo.InvariantCulture);
                if (lat <= Box_100.Suedwestecke.Item1 || lat >= Box_100.Nordostecke.Item1)
                {
                    return;
                }

                if (reader.MoveToAttribute("lon"))
                {
                    double lon = double.Parse(reader.Value, CultureInfo.InvariantCulture);
                    if (lon <= Box_100.Suedwestecke.Item2 || lon >= Box_100.Nordostecke.Item2)
                    {
                        return;
                    }
                    if (reader.MoveToAttribute("id"))
                    {
                        string toWrite = "n" + reader.Value + ":" + lat + ";" + lon + "\n";
                        byte[] b = new UTF8Encoding(true).GetBytes(toWrite);
                        ziel.Write(b, 0, b.Length);
                        //ziel.Write(uniEncoding.GetBytes(toWrite), 0, uniEncoding.GetByteCount(toWrite));
                        ziel.Flush();
                    }
                }
            }
        }

        private static FileStream log = new FileStream(@".\exportlog.txt", FileMode.Create, FileAccess.ReadWrite);

        static void Main(string[] args)
        {

            XmlReaderSettings settings = new XmlReaderSettings();

            //using (XmlReader reader = XmlReader.Create(new FileStream(@"D:\nordrhein-westfalen-latest.osm", FileMode.Open), settings))
            using (XmlReader reader = XmlReader.Create(new FileStream(@"..\..\export.osm", FileMode.Open), settings))
            {
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
                //FileStream ziel = new FileStream(@"D:\kartenausschnitt.dat", FileMode.Create, FileAccess.ReadWrite);
                //FileStream log = new FileStream(@"D:\log.dat", FileMode.Create, FileAccess.ReadWrite);
                FileStream ziel = new FileStream(@".\exported.osm", FileMode.Create, FileAccess.ReadWrite);
                int i = 0;

                while (reader.Read())
                {
                    //Console.WriteLine(reader.Name);
                    //Console.WriteLine(reader.Depth);
                    if (reader.Name.Equals("node"))
                    {
                        schreibeKnoten(reader, ziel);
                    }
                    if (reader.Name.Equals("way"))
                    {
                        if (reader.MoveToAttribute("id"))
                        {
                            int suchtiefeKnoten = reader.Depth;

                            //Die vorlaeufige Liste enthält Knotenids
                            List<long> weg = new List<long>();
                            bool einbahnAuto = false;
                            bool einbahnRad = false;
                            bool explizitNichtEinbahnRad = false;
                            bool isAndersherumAuto = true;
                            bool isAndersherumRad = true;
                            double maxspeedVorwaertsAuto = 99999999.9;
                            double maxspeedRueckwaertsAuto = 99999999.9;
                            double maxspeedVorwaertsRad = 99999999.9;
                            double maxspeedRueckwaertsRad = 99999999.9;
                            double maxspeedVorwaertsFuss = 99999999.9;
                            double maxspeedRueckwaertsFuss = 99999999.9;
                            bool autoVorw = true;
                            bool autoRueckw = true;
                            bool radVorw = true;
                            bool radRueckw = true;
                            bool fussVorw = true;
                            bool fussRueckw = true;
                            bool autoVorwExplizitErlaubt = false;
                            bool autoRueckwExplizitErlaubt = false;
                            bool radVorwExplizitErlaubt = false;
                            bool radRueckwExplizitErlaubt = false;
                            bool fussVorwExplizitErlaubt = false;
                            bool fussRueckwExplizitErlaubt = false;
                            //Nur, wenn irgendwo ein Highway-Tag gefunden wurde, wird der Wert true und damit ein Weg geschrieben.
                            //Alle anderen "Wege" sind nur Linien, z.B. Gebaeude o.ä.
                            bool istWeg = false;

                            while (reader.Read() && reader.Depth == suchtiefeKnoten)
                            {
                                if (reader.Name.Equals("nd") && reader.MoveToAttribute("ref"))
                                {
                                    weg.Append(long.Parse(reader.Value));
                                }
                                // Dieser Abschnitt ausgefuehrt, um die korrekte Richtung und Gewichtung der Knoten zu ermitteln.

                                if (reader.Name.Equals("tag"))
                                {
                                    if (reader.MoveToAttribute("k"))
                                    {
                                        //Umgang mit Einbahnstraßentags
                                        if (reader.Value.Equals("oneway") && reader.MoveToAttribute("v"))
                                        {
                                            if (reader.Value.Equals("yes"))
                                            {
                                                einbahnAuto = true;
                                                isAndersherumAuto = false;
                                                if (!explizitNichtEinbahnRad)
                                                {
                                                    einbahnRad = true;
                                                    isAndersherumRad = false;
                                                }
                                            }
                                            else if (reader.Value.Equals(-1))
                                            {
                                                einbahnAuto = true;
                                                if (!explizitNichtEinbahnRad)
                                                {
                                                    einbahnRad = true;
                                                }
                                            }
                                        }
                                        if (reader.Value.Equals("oneway:bicycle") && reader.MoveToAttribute("v") && !reader.Value.Equals("yes"))
                                        {
                                            einbahnAuto = true;
                                            einbahnRad = false;
                                            isAndersherumAuto = false;
                                            isAndersherumRad = false;
                                            explizitNichtEinbahnRad = true;
                                        }
                                        if (reader.Value.Equals("cycleway") && reader.MoveToAttribute("v"))
                                        {
                                            if (reader.Value.Equals("opposite") || reader.Value.Equals("opposite_lane") ||
                                                reader.Value.Equals("track") || reader.Value.Equals("opposite_track"))
                                            {
                                                einbahnAuto = true;
                                                einbahnRad = false;
                                                isAndersherumAuto = false;
                                                isAndersherumRad = false;
                                                explizitNichtEinbahnRad = true;
                                            }
                                        }
                                        //Umgang mit Geschwindigkeitstags. Der Eintrag "default" würde unbegrenzt bedeuten.
                                        else if ((reader.Value.Equals("maxspeed") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none")) ||
                                            (reader.Value.Equals("maxspeed:variable") && reader.MoveToAttribute("v") && !reader.Value.Equals("no")))
                                        {
                                            maxspeedVorwaertsAuto = calculateVorlaeufigeMaxspeed(reader, maxspeedVorwaertsAuto);
                                            maxspeedVorwaertsRad = calculateVorlaeufigeMaxspeed(reader, maxspeedVorwaertsRad);
                                            maxspeedVorwaertsFuss = calculateVorlaeufigeMaxspeed(reader, maxspeedVorwaertsFuss);
                                            maxspeedRueckwaertsAuto = calculateVorlaeufigeMaxspeed(reader, maxspeedRueckwaertsAuto);
                                            maxspeedRueckwaertsRad = calculateVorlaeufigeMaxspeed(reader, maxspeedRueckwaertsRad);
                                            maxspeedRueckwaertsFuss = calculateVorlaeufigeMaxspeed(reader, maxspeedRueckwaertsFuss);
                                        }
                                        else if ((reader.Value.Equals("maxspeed:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none")) ||
                                            (reader.Value.Equals("maxspeed:variable:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no")))
                                        {
                                            maxspeedVorwaertsAuto = calculateVorlaeufigeMaxspeed(reader, maxspeedVorwaertsAuto);
                                            maxspeedVorwaertsRad = calculateVorlaeufigeMaxspeed(reader, maxspeedVorwaertsRad);
                                            maxspeedVorwaertsFuss = calculateVorlaeufigeMaxspeed(reader, maxspeedVorwaertsFuss);
                                        }
                                        else if ((reader.Value.Equals("maxspeed:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none")) ||
                                            (reader.Value.Equals("maxspeed:variable:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no")))
                                        {
                                            maxspeedRueckwaertsAuto = calculateVorlaeufigeMaxspeed(reader, maxspeedRueckwaertsAuto);
                                            maxspeedRueckwaertsRad = calculateVorlaeufigeMaxspeed(reader, maxspeedRueckwaertsRad);
                                            maxspeedRueckwaertsFuss = calculateVorlaeufigeMaxspeed(reader, maxspeedRueckwaertsFuss);
                                        }
                                        //In dem Fall soll beispielsweise ein Fußgänger oder Fahrrad ausgeschlossen werden.
                                        else if (reader.Value.Equals("minspeed") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                        {
                                            double min = getMindestgeschwindigkeit(reader);
                                            if (min > 5)
                                            {
                                                if (!fussVorwExplizitErlaubt)
                                                {
                                                    fussVorw = false;
                                                }
                                                if (!fussRueckwExplizitErlaubt)
                                                {
                                                    fussRueckw = false;
                                                }
                                                if (min > 20)
                                                {
                                                    if (!radVorwExplizitErlaubt)
                                                    {
                                                        radVorw = false;
                                                    }
                                                    if (!radRueckwExplizitErlaubt)
                                                    {
                                                        radRueckw = false;
                                                    }
                                                }
                                            }
                                        }
                                        else if (reader.Value.Equals("minspeed:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                        {
                                            double min = getMindestgeschwindigkeit(reader);
                                            if (min > 5 && !fussVorwExplizitErlaubt)
                                            {
                                                fussVorw = false;
                                                if (min > 20 && !radVorwExplizitErlaubt)
                                                {
                                                    radVorw = false;
                                                }
                                            }
                                        }
                                        else if (reader.Value.Equals("minspeed:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                        {
                                            double min = getMindestgeschwindigkeit(reader);
                                            if (min > 5 && !fussRueckwExplizitErlaubt)
                                            {
                                                fussRueckw = false;
                                                if (min > 20 && !radRueckwExplizitErlaubt)
                                                {
                                                    radRueckw = false;
                                                }
                                            }
                                        }
                                        else if (reader.Value.Equals("bicycle") && reader.MoveToAttribute("v") && !reader.Value.Equals("no"))
                                        {
                                            radVorwExplizitErlaubt = true;
                                            radRueckwExplizitErlaubt = true;
                                            if (reader.Value.Equals("dismount"))
                                            {
                                                maxspeedVorwaertsRad = 5;
                                                maxspeedRueckwaertsRad = 5;
                                            }
                                        }
                                        else if (reader.Value.Equals("bicycle:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no"))
                                        {
                                            radVorwExplizitErlaubt = true;
                                            if (reader.Value.Equals("dismount"))
                                            {
                                                maxspeedVorwaertsRad = 5;
                                            }
                                        }
                                        else if (reader.Value.Equals("bicycle:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no"))
                                        {
                                            radRueckwExplizitErlaubt = true;
                                            if (reader.Value.Equals("dismount"))
                                            {
                                                maxspeedRueckwaertsRad = 5;
                                            }
                                        }
                                        else if (reader.Value.Equals("vehicle") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            autoVorwExplizitErlaubt = true;
                                            autoRueckwExplizitErlaubt = true;
                                            radVorwExplizitErlaubt = true;
                                            radRueckwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("motor_vehicle") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            autoVorwExplizitErlaubt = true;
                                            autoRueckwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("vehicle:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            autoVorwExplizitErlaubt = true;
                                            radVorwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("motor_vehicle:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            autoVorwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("vehicle:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            autoRueckwExplizitErlaubt = true;
                                            radRueckwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("motor_vehicle:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            autoRueckwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("foot") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            fussVorwExplizitErlaubt = true;
                                            fussRueckwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("foot:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            fussVorwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("foot:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                        {
                                            fussRueckwExplizitErlaubt = true;
                                        }
                                        else if (reader.Value.Equals("highway") && reader.MoveToAttribute("v"))
                                        {
                                            istWeg = true;
                                            if (reader.Value.Equals("pedestrian"))
                                            {
                                                if (!autoVorwExplizitErlaubt)
                                                {
                                                    autoVorw = false;
                                                }
                                                else
                                                {
                                                    maxspeedVorwaertsAuto = 5;
                                                }
                                                if (!autoRueckwExplizitErlaubt)
                                                {
                                                    autoRueckw = false;
                                                }
                                                else
                                                {
                                                    maxspeedRueckwaertsAuto = 5;
                                                }
                                                if (!radVorwExplizitErlaubt)
                                                {
                                                    radVorw = false;
                                                }
                                                else
                                                {
                                                    maxspeedVorwaertsRad = 5;
                                                }
                                                if (!radRueckwExplizitErlaubt)
                                                {
                                                    radRueckw = false;
                                                }
                                                else
                                                {
                                                    maxspeedRueckwaertsRad = 5;
                                                }
                                            }
                                            else if (reader.Value.Equals("cycleway"))
                                            {
                                                if (!autoVorwExplizitErlaubt)
                                                {
                                                    autoVorw = false;
                                                }
                                                else
                                                {
                                                    maxspeedVorwaertsAuto = 20;
                                                }
                                                if (!autoRueckwExplizitErlaubt)
                                                {
                                                    autoRueckw = false;
                                                }
                                                else
                                                {
                                                    maxspeedRueckwaertsAuto = 20;
                                                }
                                                if (!fussVorwExplizitErlaubt)
                                                {
                                                    fussVorw = false;
                                                }
                                                if (!fussRueckwExplizitErlaubt)
                                                {
                                                    fussRueckw = false;
                                                }
                                            }
                                            else if (reader.Value.Equals("motorway"))
                                            {
                                                radVorw = false;
                                                radRueckw = false;
                                                fussVorw = false;
                                                fussRueckw = false;
                                            }
                                        }
                                        else
                                        {
                                            Log("Nicht verarbeiteter Tag: " + reader.Name + " " + reader.Value + "\n");
                                        }
                                    }
                                }
                            }
                            //Nur wenn es sich bei Way um einen Highway handelt, ist es eine Straße, ein Pfad, Weg o.ä. im engeren Sinne.
                            if (istWeg)
                            {
                                schreibeWeg(reader, ziel, weg, einbahnAuto, einbahnRad, isAndersherumAuto, isAndersherumRad, maxspeedVorwaertsAuto,
                                maxspeedRueckwaertsAuto, maxspeedVorwaertsRad, maxspeedRueckwaertsRad, maxspeedVorwaertsFuss, maxspeedRueckwaertsFuss,
                                autoVorw, autoRueckw, radVorw, radRueckw, fussVorw, fussRueckw);
                            }
                        }
                    }
                    //Ist der Knoten weder Weg noch Kartenknoten, so können er selbst und all seine Untereintraege uebersprungen werden.
                    //else
                    //{
                    //    reader.Skip();
                    //}
                }
                Console.WriteLine(i);
                Console.ReadLine();
            }
        }

        private static void Log(string text)
        {
            byte[] b = new UTF8Encoding(true).GetBytes(text);
            log.Write(b, 0, b.Length);
            log.Flush();
        }

        private static void schreibeWeg(XmlReader reader, FileStream ziel, List<long> weg, bool einbahnAuto, bool einbahnRad, bool isAndersherumAuto, 
            bool isAndersherumRad, double maxspeedVorwAuto, double maxspeedRueckwAuto, double maxspeedVorwRad, double maxspeedRueckwRad, double maxspeedVorwFuss, 
            double maxspeedRueckwFuss, bool autoVorw, bool autoRueckw, bool radVorw, bool radRueckw, bool fussVorw, bool fussRueckw)
        {
            //Annahme: Liste Weg ist in der richtigen Reihenfolge!! Die Knoten werden immer in der richtigen Reihenfolge abgespeichert samt der Information,
            //ob der Weg umgekehrt passierbar ist.
            if (einbahnAuto && einbahnRad)
            {
                if (isAndersherumAuto && isAndersherumRad)
                {
                    weg.Reverse();
                }
            }
            
            string toWrite = "w:  a:"+maxspeedVorwAuto+";"+ maxspeedRueckwAuto + ";"+einbahnAuto+";"+autoVorw+ ";" + autoRueckw +"\n" +
                "    r:"+maxspeedVorwRad+";"+ maxspeedRueckwRad + ";"+einbahnRad+";"+radVorw+ ";" + radRueckw +"\n" +
                "    f:" + maxspeedVorwFuss + ";" + maxspeedRueckwFuss + ";" + fussVorw + ";" + fussRueckw 
                + "\n\t{\n";
            foreach(long knotenid in weg)
            {
                toWrite += "\t"+knotenid+"\n";
            }
            toWrite += "\t}\n";

            byte[] b = new UTF8Encoding(true).GetBytes(toWrite);
            ziel.Write(b, 0, b.Length);
            //ziel.Write(uniEncoding.GetBytes(toWrite), 0, uniEncoding.GetByteCount(toWrite));
            ziel.Flush();
        }

        private static double getMindestgeschwindigkeit(XmlReader reader)
        {
            double min = 0;
            if (reader.Value.Equals("DE:Autobahn"))
            {
                min = 60;
            }
            else
            {
                min = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            }

            return min;
        }

        //Berechnet das vorlaeufige Kantengewicht als Kehrwert der aktuell minimal bekannten Maximalgeschwindigkeit.
        private static double calculateVorlaeufigeMaxspeed(XmlReader reader, double aktMax)
        {
            double max = 120;
            if (reader.Value.Equals("DE:Landstraße") || reader.Value.Equals("DE:rural"))
            {
                max = 100;
            }
            else if (reader.Value.Equals("NL:Landstraße") || reader.Value.Equals("NL:rural"))
            {
                max = 80;
            }
            else if (reader.Value.Equals("DE:Autobahn") || reader.Value.Equals("DE:motorway"))
            {
                max = 100;
            }
            else if (reader.Value.Equals("DE:bicycle_road"))
            {
                max = 30;
            }
            else if (reader.Value.Equals("DE:Innerorts") || reader.Value.Equals("NL:Innerorts") || reader.Value.Equals("DE:urban") || reader.Value.Equals("NL:urban"))
            {
                max = 50;
            }
            else if (reader.Value.Equals("walk") || reader.Value.Equals("DE:living_street"))
            {
                max = 15;
            }
            //Tritt auf, wenn maxspeed:variable o.ä. Dann muss das Gewicht anderweitig definiert werden, tue also nichts.
            else if (reader.Value.Equals("yes"))
            {
            }
            else
            {
                max = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            }
            return Math.Min(max, aktMax);
        }
    }
}
