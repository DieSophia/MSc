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


class IGraphData {
public:
    using name = std::string;
    using beschreibung = std::string;

    struct Knoten {
        int id;
        double lat;
        double lon;

        std::map<name, beschreibung> tags;
    };

    struct Kanteneigenschaften {
        double gewicht;
        std::map<name, beschreibung> tags;
    };


    // Die Knotenliste entält jeweils Paare von Knoten und deren Ausgangsknoten samt den Eigenschaften der zugehörigen Kante.
    using adjazenzMap = std::map<int, std::map<int, Kanteneigenschaften>>;

    struct Graph {
        int id;
        adjazenzMap adj;
        std::map<int, Knoten> knotenmap;
    };

    static void addKante(Graph* g, Knoten eingangsknoten, Knoten ausgangsknoten, Kanteneigenschaften eigenschaften) {
        addKnoten(g, eingangsknoten);
        addKnoten(g, ausgangsknoten);
        std::map<int, Kanteneigenschaften> adj_knoten = (*g).adj[eingangsknoten.id];
        adj_knoten.insert({ ausgangsknoten.id, eigenschaften });
        std::map<int, Kanteneigenschaften>::iterator it = adj_knoten.begin();
        while (it != adj_knoten.end()) {
            printf("%d\n", (*it).first);
            it++;
        }
        (*g).adj[eingangsknoten.id] = adj_knoten;
    }
    
     static void addKnoten(Graph* g, Knoten k) {
        (*g).knotenmap.insert({ k.id, k });
    }
};

extern KERN_API void calculateIsochrone(IGraphData::Graph g, int quellknoten, double isochronenlinienDistanz);// Gibt kürzesten Weg von s aus


