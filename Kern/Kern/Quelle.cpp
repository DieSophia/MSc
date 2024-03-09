#include "pch.h"
#include "Header.h"
#include <typeinfo>


void addKnoten(Knoten u) {
    graph.knotenmap.insert({ u.id, u });
}

void addKante(Knoten kanteneingang, Knoten kantenausgang, Kanteneigenschaften eigenschaften)
{
    addKnoten(kanteneingang);
    addKnoten(kantenausgang);
    std::map<int, Kanteneigenschaften> adj_knoten = graph.adj[kanteneingang.id];
    adj_knoten.insert({ kantenausgang.id, eigenschaften });
    printf("Kanteneingangs-ID %d Kantenausgangs-ID %d Laenge adj_knoten %d\n", kanteneingang.id, kantenausgang.id, (int)adj_knoten.size());
    std::map<int, Kanteneigenschaften>::iterator it = adj_knoten.begin();
    while (it != adj_knoten.end()) {
        printf("%d\n", (*it).first);
        it++;
    }
    graph.adj[kanteneingang.id] = adj_knoten;

}

void printKuerzesteWege(int src, std::map<int, double> dist) {
    // Gib die kürzesten Distanzen aus dist aus.
    printf("Knotenabstand von Quellknoten %d\n", src);
    std::map<int, double>::iterator it = dist.begin();
    while (it != dist.end()) {
        printf("%d \t\t %f\n", (*it).first, dist[(*it).first]);
        it++;
    }
}

// Gibt den kürzesten Weg von dem Knoten mit der übergebenen ID zu allen anderen Knoten aus.
void calculateKuerzestenWeg(int src)
{
    // Priority Queue erzeugen, um vorprozessierte Knoten zu speichern. (SPÄTER ERSETZEN DURCH FIBONACCI-HEAP?)
    std::priority_queue< std::pair<double, int>, std::vector<std::pair<double, int>>, std::greater<std::pair<double, double>> >
        pq;

    // Initialisiere die Knotendistanzen für alle Knoten, die Ausgangskanten haben, als unendlich.
    std::map<int, double> dist;
    std::map<int, Knoten>::iterator i = graph.knotenmap.begin();
    while (i != graph.knotenmap.end()) {
        dist[(*i).first] = INF;
        i++;
    }

    // Füge den Quellknoten hinzu und setze seine Distanz zu sich selbst auf 0.
    pq.push(std::make_pair(.0, src));
    dist[src] = .0;

    // Dijkstra endet erst, wenn keine Knoten mehr übrig oder alle übrigen unerreichbar sind.
    while (!pq.empty()) {
        int u = pq.top().second;
        pq.pop();

        std::map<int, Kanteneigenschaften>::iterator i;
        for (i = graph.adj[u].begin(); i != graph.adj[u].end(); ++i) {
            // Bestimme den nächsten zu betrachtenden Kantenausgang und das Gewicht der Kante u -> v.
            int v = (*i).first;
            double gewicht = (*i).second.gewicht;

            // Wenn der Weg durch den Knoten der ID u kürzer ist als alle bisher bekannten Wege.
            if (dist[v] > dist[u] + gewicht) {
                dist[v] = dist[u] + gewicht;
                pq.push(std::make_pair(dist[v], v));
            }
        }
    }

    printKuerzesteWege(src, dist);
}
