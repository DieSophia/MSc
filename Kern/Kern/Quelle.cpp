#include "pch.h"
#include "Header.h"
#include <typeinfo>

IGraphData* op;

static void printKuerzesteWege(int src, std::map<int, double> dist) {
    // Gib die kürzesten Distanzen aus dist aus.
    printf("Knotenabstand von Quellknoten %d\n", src);
    std::map<int, double>::iterator it = dist.begin();
    while (it != dist.end()) {
        printf("%d \t\t %f\n", (*it).first, dist[(*it).first]);
        it++;
    }
}

// Gibt den kürzesten Weg von dem Knoten mit der übergebenen ID zu allen anderen Knoten aus.
void calculateIsochrone(IGraphData::Graph g, int src, double isochronenlinienDistanz)
{
    // Priority Queue erzeugen, um vorprozessierte Knoten zu speichern. (SPÄTER ERSETZEN DURCH FIBONACCI-HEAP?)
    std::priority_queue< std::pair<double, int>, std::vector<std::pair<double, int>>, std::greater<std::pair<double, double>> >
        pq;

    // Initialisiere die Knotendistanzen für alle Knoten, die Ausgangskanten haben, als unendlich.
    std::map<int, double> dist;
    std::map<int, IGraphData::Knoten>::iterator i = g.knotenmap.begin();
    while (i != g.knotenmap.end()) {
        dist[(*i).first] = INF;
        i++;
    }

    std::map<int, int> vorgaenger = {};
    // Füge den Quellknoten hinzu und setze seine Distanz zu sich selbst auf 0.
    pq.push(std::make_pair(.0, src));
    dist[src] = .0;
    vorgaenger[src] = -1; //Das soll per definitionem bedeuten, dass es keinen Vorgaenger gibt.
    std::map<int, IGraphData::Knoten> isochronenkandidaten;

    // Dijkstra endet erst, wenn das Abbruchkriterium für alle übrigen Knoten erfüllt ist (d.h. isochronenlinienDistanz wird überschritten) oder keine Knoten mehr übrig sind.
    while (!pq.empty()) {
        int u = pq.top().second;
        pq.pop();

        std::map<int, IGraphData::Kanteneigenschaften>::iterator i;
        for (i = g.adj[u].begin(); i != g.adj[u].end(); ++i) {
            // Bestimme den nächsten zu betrachtenden Kantenausgang und das Gewicht der Kante u -> v.
            int v = (*i).first;
            double gewicht = (*i).second.gewicht;

            // Wenn der Weg durch den Knoten der ID u kürzer ist als alle bisher bekannten Wege.
            if (dist[v] > dist[u] + gewicht) {
                dist[v] = dist[u] + gewicht;
                vorgaenger[v] = u;
                //Der Knoten soll nur dann in der Priority Queue landen, wenn seine Nachbarn auch noch auf dem Polygonrand oder im Polgon selbst liegen könnten.
                if (dist[v] < isochronenlinienDistanz) {
                    pq.push(std::make_pair(dist[v], v));
                }
                else if (dist[v] == isochronenlinienDistanz) {
                    isochronenkandidaten[v] = g.knotenmap[v];
                }
                else if (dist[v] > isochronenlinienDistanz) {
                    isochronenkandidaten[u] = g.knotenmap[u];
                }
            }
        }
    }

    //Aus den Isochronenkandidaten müssen auch interne Zyklen (innere Ränder) nicht aussortiert werden.
    //Nächste Schwierigkeit: welche der Kandidaten bilden jeweils einen gemeinsamen Zyklus?

    printKuerzesteWege(src, dist);
}
