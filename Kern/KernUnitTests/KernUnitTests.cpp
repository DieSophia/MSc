#include "pch.h"
#include "CppUnitTest.h"
#include "../Kern/Header.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace KernUnitTests
{
	TEST_CLASS(KernUnitTests)
	{
	public:

		int testid_1 = 109067;
		double testlat_1 = 24.3;
		double testlon_1 = 7.3;

		int testid_2 = 111111;
		double testlat_2 = 87.0;
		double testlon_2 = -10.8;

		int testid_3 = 113311;
		double testlat_3 = 37.0;
		double testlon_3 = -30.8;

		int testid_4 = 411111;
		double testlat_4 = 47.0;
		double testlon_4 = -40.8;

		IGraphData::Knoten testknoten1 = IGraphData::Knoten{ testid_1, testlat_1, testlon_1 };
		IGraphData::Knoten testknoten2 = IGraphData::Knoten{ testid_2, testlat_2, testlon_2 };
		IGraphData::Knoten testknoten3 = IGraphData::Knoten{ testid_3, testlat_3, testlon_3 };
		IGraphData::Knoten testknoten4 = IGraphData::Knoten{ testid_4, testlat_4, testlon_4 };

		double gewicht_kante12 = 0.12;
		double gewicht_kante21 = 0.21;
		double gewicht_kante13 = 0.13;
		double gewicht_kante31 = 0.31;
		double gewicht_kante14 = 0.14;
		double gewicht_kante41 = 0.41;
		double gewicht_kante23 = 0.23;
		double gewicht_kante32 = 0.32;
		double gewicht_kante24 = 0.24;
		double gewicht_kante42 = 0.42;
		double gewicht_kante34 = 0.34;
		double gewicht_kante43 = 0.43;

		IGraphData::Kanteneigenschaften kanteneigenschaften12 = { gewicht_kante12 };
		IGraphData::Kanteneigenschaften kanteneigenschaften21 = { gewicht_kante21 };
		IGraphData::Kanteneigenschaften kanteneigenschaften13 = { gewicht_kante13 };
		IGraphData::Kanteneigenschaften kanteneigenschaften31 = { gewicht_kante31 };
		IGraphData::Kanteneigenschaften kanteneigenschaften14 = { gewicht_kante14 };
		IGraphData::Kanteneigenschaften kanteneigenschaften41 = { gewicht_kante41 };
		IGraphData::Kanteneigenschaften kanteneigenschaften23 = { gewicht_kante23 };
		IGraphData::Kanteneigenschaften kanteneigenschaften32 = { gewicht_kante32 };
		IGraphData::Kanteneigenschaften kanteneigenschaften24 = { gewicht_kante24 };
		IGraphData::Kanteneigenschaften kanteneigenschaften42 = { gewicht_kante42 };
		IGraphData::Kanteneigenschaften kanteneigenschaften34 = { gewicht_kante34 };
		IGraphData::Kanteneigenschaften kanteneigenschaften43 = { gewicht_kante43 };

		void assertGraphEnthaeltTestknoten(IGraphData::Graph g, int id, double lat, double lon) {
			Assert::AreEqual(id, g.knotenmap[id].id);
			Assert::AreEqual(lat, g.knotenmap[id].lat);
			Assert::AreEqual(lon, g.knotenmap[id].lon);
		}


		void assertGraphEnthaeltNichtKnoten(IGraphData::Graph g, IGraphData::Knoten knoten) {
			Assert::IsFalse(g.knotenmap.count(knoten.id));
		}

		void assertGraphEnthaeltTestkante(IGraphData::Graph g, IGraphData::Knoten kanteneingang, IGraphData::Knoten kantenausgang, double gewicht) {
			std::map<int, IGraphData::Kanteneigenschaften> adj = g.adj[kanteneingang.id];
			std::map<int, IGraphData::Kanteneigenschaften>::iterator it = adj.begin();
			while (it != adj.end()) {
				//Wenn die Kante enthalten ist und das Gewicht übereinstimmt, war der Test erfolgreich.
				if ((*it).first == kantenausgang.id) {
					Assert::AreEqual(gewicht, (*it).second.gewicht);
					return;
				}
				it++;
			}
			//Hier angekommen ist die Kante nicht enthalten.
			Assert::IsTrue(false);
		}

		void assertGraphEnthaeltNichtKante(IGraphData::Graph g, IGraphData::Knoten kanteneingang, IGraphData::Knoten kantenausgang) {
			std::map<int, IGraphData::Kanteneigenschaften> adj = g.adj[kanteneingang.id];
			std::map<int, IGraphData::Kanteneigenschaften>::iterator it = adj.begin();
			while (it != adj.end()) {
				Assert::AreNotEqual(kantenausgang.id, (*it).first);
				it++;
			}
		}
		TEST_METHOD(TestKnotenHinzufuegen)
		{
			std::string testtagname = "irgendein doofer Tag";
			std::string testtagbeschreibung = "doofe Beschreibung";
			IGraphData::Knoten testknoten = IGraphData::Knoten{ testid_1, testlat_1, testlon_1, {{testtagname, testtagbeschreibung} } };
			IGraphData::Graph g = IGraphData::Graph{ 1, {}, {} };
			IGraphData::addKnoten(&g, testknoten);

			assertGraphEnthaeltTestknoten(g, testid_1, testlat_1, testlon_1);
			Assert::AreEqual(testtagbeschreibung, g.knotenmap[testid_1].tags[testtagname]);
			Assert::AreEqual(1, (int)g.knotenmap.size());
		}


		TEST_METHOD(TestKanteHinzufuegen)
		{
			IGraphData::Kanteneigenschaften kanteneigenschaften = { gewicht_kante12 };

			IGraphData::Graph g = IGraphData::Graph{ 2, {}, {} };
			IGraphData::Graph* gptr = &g;
			IGraphData::addKante(gptr, testknoten1, testknoten2, kanteneigenschaften);

			assertGraphEnthaeltTestknoten(g, testknoten1.id, testknoten1.lat, testknoten1.lon);
			assertGraphEnthaeltTestknoten(g, testknoten2.id, testknoten2.lat, testknoten2.lon);
			Assert::AreEqual((size_t) 1, g.adj[testknoten1.id].size());

			assertGraphEnthaeltTestkante(g, testknoten1, testknoten2, gewicht_kante12);
			Assert::AreEqual(gewicht_kante12, g.adj[testknoten1.id][testknoten2.id].gewicht);
			//So ist implizit mitgetestet, dass die Kante nur in eine Richtung initialisiert wurde (Digraph-Eigenschaft).
			Assert::AreEqual((size_t) 0, g.adj[testknoten2.id].size());
		}

		TEST_METHOD(TestMehrereKantenHinzufuegen)
		{
			IGraphData::Graph g = IGraphData::Graph{ 3, {}, {} };
			IGraphData::Graph* gptr = &g;
			IGraphData::addKante(gptr, testknoten1, testknoten2, kanteneigenschaften12);
			IGraphData::addKante(gptr, testknoten2, testknoten1, kanteneigenschaften21);
			IGraphData::addKante(gptr, testknoten1, testknoten3, kanteneigenschaften13);
			IGraphData::addKante(gptr, testknoten3, testknoten1, kanteneigenschaften31);
			IGraphData::addKante(gptr, testknoten3, testknoten2, kanteneigenschaften32);

			assertGraphEnthaeltTestknoten(g, testknoten1.id, testknoten1.lat, testknoten1.lon);
			assertGraphEnthaeltTestknoten(g, testknoten2.id, testknoten2.lat, testknoten2.lon);
			assertGraphEnthaeltTestknoten(g, testknoten3.id, testknoten3.lat, testknoten3.lon);
			assertGraphEnthaeltNichtKnoten(g, testknoten4);
			Assert::AreEqual((size_t) 2, g.adj[testknoten1.id].size());
			Assert::AreEqual((size_t) 1, g.adj[testknoten2.id].size());
			Assert::AreEqual((size_t) 2, g.adj[testknoten3.id].size());
			Assert::AreEqual((size_t) 0, g.adj[testknoten4.id].size());

			assertGraphEnthaeltTestkante(g, testknoten1, testknoten2, gewicht_kante12);
			assertGraphEnthaeltTestkante(g, testknoten2, testknoten1, gewicht_kante21);
			assertGraphEnthaeltTestkante(g, testknoten1, testknoten3, gewicht_kante13);
			assertGraphEnthaeltTestkante(g, testknoten3, testknoten1, gewicht_kante31);
			assertGraphEnthaeltTestkante(g, testknoten3, testknoten2, gewicht_kante32);
			assertGraphEnthaeltNichtKante(g, testknoten2, testknoten3);
		}

		TEST_METHOD(TestCalculateIsochroneEinfacheKante) {
			IGraphData::Graph g = IGraphData::Graph{ 4, {}, {} };
			IGraphData::Graph* gptr = &g;
			IGraphData::addKante(gptr, testknoten1, testknoten2, kanteneigenschaften12);
			IGraphData::addKante(gptr, testknoten2, testknoten1, kanteneigenschaften21);
			IGraphData::addKante(gptr, testknoten1, testknoten3, kanteneigenschaften13);
			IGraphData::addKante(gptr, testknoten3, testknoten1, kanteneigenschaften31);
			IGraphData::addKante(gptr, testknoten3, testknoten2, kanteneigenschaften32);
		}
	};
}
