#pragma once //Der Include Guard analog zu #ifndef STRUKTUREN_H #define STRUKTUREN_H mit #endif am Ende der Headerfile

#ifdef KERN_EXPORTS
#define KERN_API __declspec(dllexport)
#else
#define KERN_API __declspec(dllimport)
#endif

#define INF 0x3f3f3f3f
#include <list>
#include <queue>
#include <map>
#include <string>


struct Tag {
    std::string tagname;
    std::string beschreibung;
};

struct Kanteneigenschaften {
    double gewicht;
    std::list<Tag> tags;
};

struct Knoten {
    int id;
    double lat;
    double lon;
    std::list<Tag> tags;
};

// Die Knotenliste entält jeweils Paare von Knoten und deren Ausgangsknoten samt den Eigenschaften der zugehörigen Kante.
using adjazenzMap = std::map<int, std::map<int, Kanteneigenschaften>>;

struct Graph {
    adjazenzMap adj;
    std::map<int, Knoten> knotenmap;
} graph = { {},{} };


// Beim Hinzufügen einer Kante werden automatisch die Knoten hinzugefügt.
extern KERN_API void addKante(Knoten eingangsknoten, Knoten ausgangsknoten, Kanteneigenschaften eigenschaften);
extern KERN_API void addKnoten(Knoten v);
extern KERN_API void calculateKuerzestenWeg(int quellknoten);// Gibt kürzesten Weg von s aus
