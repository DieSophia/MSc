using System;
using System.CodeDom;
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
            if (!reader.MoveToAttribute("id"))
            {
                Log($"Datenfehler: Knoten ohne ID gefunden!");
                return;
            }
            string knotenid = reader.Value;

            if (!reader.MoveToAttribute("lat"))
            {
                Log($"Datenfehler: Knoten {knotenid} ohne lat gefunden.");
                return;
            }
            double lat = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (lat <= Box_100.Suedwestecke.Item1 || lat >= Box_100.Nordostecke.Item1)
            {
                Log($"Knoten {knotenid} aussortiert wegen {lat}");
                return;
            }

            if (!reader.MoveToAttribute("lon"))
            {
                Log($"Datenfehler: Knoten {knotenid} ohne lon gefunden.");
                return;
            }
            double lon = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (lon <= Box_100.Suedwestecke.Item2 || lon >= Box_100.Nordostecke.Item2)
            {
                Log($"Knoten {knotenid} aussortiert wegen {lon}");
                return;
            }

            string toWrite = "n:\t" + knotenid + ":" + lat + ";" + lon + "\n";
            byte[] b = new UTF8Encoding(true).GetBytes(toWrite);
            ziel.Write(b, 0, b.Length);
            ziel.Flush();
        }

        private static double MAX_AUTO = 120d;
        private static double MAX_RAD = 25d;
        private static double MAX_FUSS = 5d;

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
                int anzahlKnoten = 0;

                while (reader.Read())
                {
                    if (reader.Name.Equals("node") && reader.IsStartElement())
                    {
                        anzahlKnoten++;
                        schreibeKnoten(reader, ziel);
                    }
                    if (reader.Name.Equals("way") && reader.IsStartElement())
                    {
                        if (!reader.MoveToAttribute("id"))
                        {
                            Log("Datenfehler: Weg ohne ID gefunden.");
                            continue;
                        }
                        string wegid = reader.Value;

                        int suchtiefeKnoten = reader.Depth;

                        List<string> unverarbeiteteTags = new List<string>();
                        //Die vorlaeufige Liste enthält Knotenids
                        List<string> weg = new List<string>();
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

                        while (reader.Read() && reader.IsStartElement() && reader.Depth == suchtiefeKnoten)
                        {
                            if (reader.Name.Equals("nd"))
                            {
                                if (!reader.MoveToAttribute("ref"))
                                {
                                    Log("Datenfehler: Knoten ohne ref-Wert in Weg " + wegid + " gefunden.");
                                    continue;
                                }
                                weg.Add(reader.Value);
                                Log($"{wegid} mit {weg.Count} Knoten gefunden");
                            }
                            // Dieser Abschnitt ausgefuehrt, um die korrekte Richtung und Gewichtung der Knoten zu ermitteln.

                            if (reader.Name.Equals("tag"))
                            {
                                Log($"{wegid} tag erreicht");
                                if (!reader.MoveToAttribute("k"))
                                {
                                    Log($"Datenfehler: Tag {reader.Value} ohne key an Weg {wegid} gefunden.");
                                    continue;
                                }
                                /********************************************************************************
                                 ********************** READER STEHT NUN AUF DEM TAG k = ?  *********************
                                 ********************************************************************************/
                                if (reader.Value.Equals("building"))
                                {
                                    break;
                                }
                                Log($"{wegid} {reader.Value}");

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
                                    maxspeedVorwaertsAuto = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                    maxspeedVorwaertsRad = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                    maxspeedVorwaertsFuss = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                    maxspeedRueckwaertsAuto = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                    maxspeedRueckwaertsRad = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                    maxspeedRueckwaertsFuss = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                }
                                else if ((reader.Value.Equals("maxspeed:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none")) ||
                                    (reader.Value.Equals("maxspeed:variable:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no")))
                                {
                                    maxspeedVorwaertsAuto = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                    maxspeedVorwaertsRad = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                    maxspeedVorwaertsFuss = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                }
                                else if ((reader.Value.Equals("maxspeed:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("default") && !reader.Value.Equals("none")) ||
                                    (reader.Value.Equals("maxspeed:variable:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no")))
                                {
                                    maxspeedRueckwaertsAuto = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_AUTO);
                                    maxspeedRueckwaertsRad = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_RAD);
                                    maxspeedRueckwaertsFuss = calculateVorlaeufigeMaxspeed(reader, wegid, MAX_FUSS);
                                }
                                //In dem Fall soll beispielsweise ein Fußgänger oder Fahrrad ausgeschlossen werden.
                                else if (reader.Value.Equals("minspeed") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                {
                                    double min = getMindestgeschwindigkeit(reader);
                                    if (min > MAX_FUSS)
                                    {
                                        if (!fussVorwExplizitErlaubt)
                                        {
                                            fussVorw = false;
                                        }
                                        if (!fussRueckwExplizitErlaubt)
                                        {
                                            fussRueckw = false;
                                        }
                                        if (min > MAX_RAD)
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
                                else if (reader.Value.Equals("minspeed:forward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                {
                                    double min = getMindestgeschwindigkeit(reader);
                                    if (min > MAX_FUSS && !fussVorwExplizitErlaubt)
                                    {
                                        fussVorw = false;
                                        if (min > MAX_RAD && !radVorwExplizitErlaubt)
                                        {
                                            radVorw = false;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("minspeed:backward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("default") && !reader.Value.Equals("none"))
                                {
                                    double min = getMindestgeschwindigkeit(reader);
                                    if (min > MAX_FUSS && !fussRueckwExplizitErlaubt)
                                    {
                                        fussRueckw = false;
                                        if (min > MAX_RAD && !radRueckwExplizitErlaubt)
                                        {
                                            radRueckw = false;
                                        }
                                    }
                                }
                                else if (reader.Value.Equals("bicycle") && reader.MoveToAttribute("v") && !reader.Value.Equals("no"))
                                {
                                    radVorwExplizitErlaubt = true;
                                    radRueckwExplizitErlaubt = true;
                                    maxspeedVorwaertsRad = MAX_RAD ;
                                    maxspeedRueckwaertsRad = MAX_RAD;
                                    if (reader.Value.Equals("dismount"))
                                    {
                                        maxspeedVorwaertsRad = MAX_FUSS;
                                        maxspeedRueckwaertsRad = MAX_FUSS;
                                    }
                                }
                                else if (reader.Value.Equals("bicycle:forward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no"))
                                {
                                    radVorwExplizitErlaubt = true;
                                    maxspeedVorwaertsRad = MAX_RAD;
                                    if (reader.Value.Equals("dismount"))
                                    {
                                        maxspeedVorwaertsRad = MAX_FUSS;
                                    }
                                }
                                else if (reader.Value.Equals("bicycle:backward") && reader.MoveToAttribute("v") && !reader.Value.Equals("no"))
                                {
                                    radRueckwExplizitErlaubt = true;
                                    maxspeedRueckwaertsRad = MAX_RAD;
                                    if (reader.Value.Equals("dismount"))
                                    {
                                        maxspeedRueckwaertsRad = MAX_FUSS;
                                    }
                                }
                                else if (reader.Value.Equals("vehicle") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    autoVorwExplizitErlaubt = true;
                                    autoRueckwExplizitErlaubt = true;
                                    radVorwExplizitErlaubt = true;
                                    radRueckwExplizitErlaubt = true;
                                }
                                else if (reader.Value.Equals("motor_vehicle") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    autoVorwExplizitErlaubt = true;
                                    autoRueckwExplizitErlaubt = true;
                                }
                                else if (reader.Value.Equals("vehicle:forward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    autoVorwExplizitErlaubt = true;
                                    radVorwExplizitErlaubt = true;
                                }
                                else if (reader.Value.Equals("motor_vehicle:forward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    autoVorwExplizitErlaubt = true;
                                }
                                else if (reader.Value.Equals("vehicle:backward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    autoRueckwExplizitErlaubt = true;
                                    radRueckwExplizitErlaubt = true;
                                }
                                else if (reader.Value.Equals("motor_vehicle:backward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    autoRueckwExplizitErlaubt = true;
                                }
                                else if (reader.Value.Equals("foot") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    fussVorwExplizitErlaubt = true;
                                    fussRueckwExplizitErlaubt = true;
                                    maxspeedVorwaertsFuss = MAX_FUSS;
                                    maxspeedRueckwaertsFuss = MAX_FUSS;
                                }
                                else if (reader.Value.Equals("foot:forward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    fussVorwExplizitErlaubt = true;
                                    maxspeedVorwaertsFuss = MAX_FUSS;
                                }
                                else if (reader.Value.Equals("foot:backward") && reader.MoveToAttribute("v") && 
                                    !reader.Value.Equals("no") && !reader.Value.Equals("private"))
                                {
                                    fussRueckwExplizitErlaubt = true;
                                    maxspeedRueckwaertsFuss = MAX_FUSS;
                                }
                                else if (reader.Value.Equals("highway") && reader.MoveToAttribute("v"))
                                {
                                    istWeg = true;
                                    if (reader.Value.Equals("footway"))
                                    {
                                        maxspeedVorwaertsFuss = MAX_FUSS;
                                        maxspeedRueckwaertsFuss = MAX_FUSS;

                                        if (!autoVorwExplizitErlaubt)
                                        {
                                            autoVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsAuto = MAX_FUSS;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsAuto = MAX_FUSS;
                                        }
                                        if (!radVorwExplizitErlaubt)
                                        {
                                            radVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsRad = MAX_FUSS;
                                        }
                                        if (!radRueckwExplizitErlaubt)
                                        {
                                            radRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsRad = MAX_FUSS;
                                        }
                                    }
                                    if (reader.Value.Equals("pedestrian"))
                                    {
                                        if (!autoVorwExplizitErlaubt)
                                        {
                                            autoVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsAuto = MAX_FUSS;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsAuto = MAX_FUSS;
                                        }
                                        if (!radVorwExplizitErlaubt)
                                        {
                                            radVorw = false;
                                        }
                                        else
                                        {
                                            maxspeedVorwaertsRad = MAX_FUSS;
                                        }
                                        if (!radRueckwExplizitErlaubt)
                                        {
                                            radRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsRad = MAX_FUSS;
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
                                            maxspeedVorwaertsAuto = MAX_RAD;
                                        }
                                        if (!autoRueckwExplizitErlaubt)
                                        {
                                            autoRueckw = false;
                                        }
                                        else
                                        {
                                            maxspeedRueckwaertsAuto = MAX_RAD;
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
                                    if (!unverarbeiteteTags.Contains(reader.Name+" "+reader.Value))
                                    {
                                        unverarbeiteteTags.Add(reader.Name);
                                        Log("Nicht verarbeiteter Tag: " + reader.Name + " " + reader.Value);
                                    }
                                }
                            }
                        }//Ende while (reader.Read() && reader.IsStartElement() && reader.Depth == suchtiefeKnoten)

                        /*
                         * WURDE BIS HIERHER KEINE GESCHWINDIGKEIT ERMITTELT, MUSS DIESE AUS DER LAGE (INNERORTS, AUSSERORTS) ODER DEM STRASSENTYP HERVORGEHEN.
                         */


                        //Nur wenn es sich bei Way um einen Highway handelt, ist es eine Straße, ein Pfad, Weg o.ä. im engeren Sinne.
                        if (istWeg)
                        {
                            SchreibeWeg(reader, ziel, wegid, weg, einbahnAuto, einbahnRad, isAndersherumAuto, isAndersherumRad, maxspeedVorwaertsAuto,
                            maxspeedRueckwaertsAuto, maxspeedVorwaertsRad, maxspeedRueckwaertsRad, maxspeedVorwaertsFuss, maxspeedRueckwaertsFuss,
                            autoVorw, autoRueckw, radVorw, radRueckw, fussVorw, fussRueckw);
                        }
                    }// Ende if reader.Name.Equals("way")

                    //Ist der Knoten weder Weg noch Kartenknoten, so können er selbst und all seine Untereintraege uebersprungen werden.
                    //else
                    //{
                    //    reader.ReadToNextSibling();
                    //}
                }
                Console.WriteLine(anzahlKnoten+" Knoten geschrieben.");
                Console.ReadLine();
            }
        }

        private static void Log(string text)
        {
            byte[] b = new UTF8Encoding(true).GetBytes(text+"\n");
            log.Write(b, 0, b.Length);
            log.Flush();
        }

        private static void SchreibeWeg(XmlReader reader, FileStream ziel, string wegid, List<string> weg, bool einbahnAuto, bool einbahnRad, bool isAndersherumAuto, 
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
            
            string toWrite = "w:  "+wegid+" a:"+maxspeedVorwAuto+";"+ maxspeedRueckwAuto + ";"+einbahnAuto+";"+autoVorw+ ";" + autoRueckw +"\n" +
                "    r:"+maxspeedVorwRad+";"+ maxspeedRueckwRad + ";"+einbahnRad+";"+radVorw+ ";" + radRueckw +"\n" +
                "    f:" + maxspeedVorwFuss + ";" + maxspeedRueckwFuss + ";" + fussVorw + ";" + fussRueckw 
                + "\n\t{\n";
            if(weg.Count == 0)
            {
                Log($"Datenfehler: Der Weg {wegid} enthaelt keine Knoten.");
            }
            foreach(string knotenid in weg)
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
        private static double calculateVorlaeufigeMaxspeed(XmlReader reader, string wegid, double aktMax)
        {
            double max = 120;
            switch (reader.Value)
            {
                case "DE:Landstraße":
                case "DE:rural":
                    max = 100;
                    break;
                case "NL:Landstraße":
                case "NL:rural":
                    max = 80;
                    break;
                case "DE:Autobahn":
                case "DE:motorway":
                    max = 100;
                    break;
                case "DE:bicycle_road":
                    max = 30;
                    break;
                case "DE:Innerorts":
                case "NL:Innerorts":
                case "DE:urban":
                case "NL:urban":
                    max = 50;
                    break;
                case "walk":
                case "DE:living_street":
                    max = 15;
                    break;
                //Tritt auf, wenn maxspeed:variable o.ä. Dann muss das Gewicht anderweitig definiert werden, tue also nichts.
                case "yes":
                    Log($"Case yes bei Wegid {wegid}");
                    break;
                default:
                    max = double.Parse(reader.Value, CultureInfo.InvariantCulture);
                    Log($"Case default es bei Wegid {wegid}. Errechneter Wert {max}");
                    break;
            }
            return Math.Min(max, aktMax);
        }
    }
}
