// XMLReader.cpp : Diese Datei enthält die Funktion "main". Hier beginnt und endet die Ausführung des Programms.
//

#include <iostream>
#include <vector>
#include "rapidxml.hpp"
#include "rapidxml_iterators.hpp"
#include "rapidxml_print.hpp"
#include "rapidxml_utils.hpp"

void outputAddress(const rapidxml::xml_node<>& addressNode)
{
    std::cout << addressNode.first_node("Recipient")->value() << std::endl;
    std::cout << addressNode.first_node("House")->value()
        << " " << addressNode.first_node("Street")->value() << std::endl;
    std::cout << addressNode.first_node("Town")->value() << std::endl;
    std::cout << addressNode.first_node("PostCode")->value() << std::endl;
    std::cout << addressNode.first_node("Country")->value() << std::endl;

}

int main()
{// ---- parse from file and stream ----

// need to convert file and stream to cstring before parsing
// as rapidxml needs a null terminated cstring for parsing

// file to string
    std::ifstream fin("address.xml");
    std::stringstream ss;
    ss << fin.rdbuf();
    std::string xml = ss.str();

    // string to dynamic cstring
    std::vector<char> stringCopy(xml.length(), '\0');
    std::copy(xml.begin(), xml.end(), stringCopy.begin());
    char* cstr = &stringCopy[0];

    // create xml document object and parse cstring
    // character type defaults to char
    rapidxml::xml_document<> parsedFromFile;
    // 0 means default parse flags
    try
    {
        parsedFromFile.parse<0>(cstr);

        rapidxml::xml_node<>* addressNode = parsedFromFile.first_node("Address");
        outputAddress(*addressNode);

        // Print to stream using operator <<
        std::cout << parsedFromFile;

        // Print to stream using print function, specifying printing flags
        // 0 means default printing flags
        rapidxml::print(std::cout, parsedFromFile, 0);
    }
    catch (const rapidxml::parse_error& e)
    {
        std::cout << "Parse error due to " << e.what() << std::endl;
    }

    // ---- create from scratch ----

    rapidxml::xml_node<>* addressNode = rapidxml::all
        fromScratch.allocate_node(rapidxml::node_element, "Address");
    rapidxml::xml_node<>* recipientNode =
        fromScratch.allocate_node(rapidxml::node_element, "Recipient", "Mr Malcolm Reynolds");
    rapidxml::xml_node<>* houseNode =
        fromScratch.allocate_node(rapidxml::node_element, "House", "3");
    rapidxml::xml_node<>* streetNode =
        fromScratch.allocate_node(rapidxml::node_element, "Street", "Serenity");
    rapidxml::xml_node<>* townNode =
        fromScratch.allocate_node(rapidxml::node_element, "Town", "Space");
    rapidxml::xml_node<>* postCodeNode =
        fromScratch.allocate_node(rapidxml::node_element, "PostCode", "DE18 5HI");
    rapidxml::xml_document<> fromScratch;

    addressNode->append_node(recipientNode);
    addressNode->append_node(houseNode);
    addressNode->append_node(streetNode);
    addressNode->append_node(townNode);
    addressNode->append_node(postCodeNode);

    fromScratch.append_node(addressNode);

    outputAddress(*addressNode);

    // ---- output to file ----

    std::ofstream fout("test.xml");
    fout << fromScratch;
}

// Programm ausführen: STRG+F5 oder Menüeintrag "Debuggen" > "Starten ohne Debuggen starten"
// Programm debuggen: F5 oder "Debuggen" > Menü "Debuggen starten"

// Tipps für den Einstieg: 
//   1. Verwenden Sie das Projektmappen-Explorer-Fenster zum Hinzufügen/Verwalten von Dateien.
//   2. Verwenden Sie das Team Explorer-Fenster zum Herstellen einer Verbindung mit der Quellcodeverwaltung.
//   3. Verwenden Sie das Ausgabefenster, um die Buildausgabe und andere Nachrichten anzuzeigen.
//   4. Verwenden Sie das Fenster "Fehlerliste", um Fehler anzuzeigen.
//   5. Wechseln Sie zu "Projekt" > "Neues Element hinzufügen", um neue Codedateien zu erstellen, bzw. zu "Projekt" > "Vorhandenes Element hinzufügen", um dem Projekt vorhandene Codedateien hinzuzufügen.
//   6. Um dieses Projekt später erneut zu öffnen, wechseln Sie zu "Datei" > "Öffnen" > "Projekt", und wählen Sie die SLN-Datei aus.
