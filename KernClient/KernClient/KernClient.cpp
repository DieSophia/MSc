// KernClient.cpp : Diese Datei enthält die Funktion "main". Hier beginnt und endet die Ausführung des Programms.
//

#include <iostream>
#include "Header.h"

int main()
{
    printf("STARTE BERECHNUNG");

    addKante(Knoten{ 1 }, Knoten{ 2 }, Kanteneigenschaften{.1});
    addKante(Knoten{ 2 }, Knoten{ 3 }, Kanteneigenschaften{.003});
    addKante(Knoten{ 3 }, Knoten{ 4 }, Kanteneigenschaften{.04});
    addKante(Knoten{ 1 }, Knoten{ 4 }, Kanteneigenschaften{ .2});
    addKante(Knoten{ 1 }, Knoten{ 10 }, Kanteneigenschaften{.020});
    addKante(Knoten{ 3 }, Knoten{ 2 }, Kanteneigenschaften{ .010 });

    calculateKuerzestenWeg(2);
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
