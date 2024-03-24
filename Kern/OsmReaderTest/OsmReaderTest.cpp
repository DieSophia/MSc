#include "pch.h"
#include "CppUnitTest.h"
#include <stdio.h>
#include <expat.h>
#include "../OsmReader/OsmReaderDefinitionen.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace OsmReaderTest
{
	TEST_CLASS(OsmReaderTest)
	{
	public:
        XML_Parser parser = XML_ParserCreate(NULL);
	};
}