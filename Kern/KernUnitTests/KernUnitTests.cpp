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

		void assertGraphEnthaeltTestknoten(IGraphData::Graph g, int id, double lat, double lon) {
			Assert::AreEqual(id, g.knotenmap[id].id);
			Assert::AreEqual(lat, g.knotenmap[id].lat);
			Assert::AreEqual(lon, g.knotenmap[id].lon);
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

		void assertGraphEnthaeltTestkante(IGraphData::Graph g, IGraphData::Knoten kanteneingang, IGraphData::Knoten kantenausgang) {
			Assert::IsNull(&g.adj[kanteneingang.id]);
		}

		void assertGraphEnthaeltNichtKante(IGraphData::Knoten kanteneingang, IGraphData::Knoten kantenausgang) {

		}

		TEST_METHOD(TestKanteHinzufuegen)
		{
			IGraphData::Knoten kanteneingang = IGraphData::Knoten{ testid_1, testlat_1, testlon_1 };
			IGraphData::Knoten kantenausgang = IGraphData::Knoten{ testid_2, testlat_2, testlon_2 };
			IGraphData::Kanteneigenschaften kanteneigenschaften = { 0.7 };

			IGraphData::Graph g = IGraphData::Graph{ 2, {}, {} };
			IGraphData::Graph* gptr = &g;
			IGraphData::addKante(gptr, kanteneingang, kantenausgang, kanteneigenschaften);

			assertGraphEnthaeltTestknoten(g, kanteneingang.id, kanteneingang.lat, kanteneingang.lon);
			assertGraphEnthaeltTestknoten(g, kantenausgang.id, kantenausgang.lat, kantenausgang.lon);
			(*gptr).adj[kantenausgang.id];
		}
	};
}
