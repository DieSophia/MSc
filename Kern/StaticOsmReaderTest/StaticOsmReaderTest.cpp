#pragma warning(disable : 4996) //Ist in VS2022 unter Eigenschaften/Konf./C++/Erweitert/Bestimmte Warnungen ausschalten

#include "pch.h"
#include "CppUnitTest.h"
#include "../StaticOsmReader/OsmReaderHeader.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace StaticOsmReaderTest
{
	TEST_CLASS(StaticOsmReaderTest)
	{
	public:
		
		TEST_METHOD(TestMethod1)
		{
            XML_Parser parser = XML_ParserCreate(NULL);
            int done;
            int depth = 0;

            if (!parser) {
                fprintf(stderr, "Couldn't allocate memory for parser\n");
                Assert::IsFalse(true);
            }

            XML_SetUserData(parser, &depth);
            XML_SetElementHandler(parser, startElement, endElement);

            do {
                void* const buf = XML_GetBuffer(parser, BUFSIZ);
                if (!buf) {
                    fprintf(stderr, "Couldn't allocate memory for buffer\n");
                    XML_ParserFree(parser);
                    Assert::IsFalse(true);
                }
                
                FILE* fp;
                errno_t err;
                const char* dateiname = "C:/Users/Subira/source/repos/Kern/Debug/simple.xml";
                if ((err = fopen_s(&fp, dateiname, "r")) != 0) {
                    fprintf(stderr, "Kann folgende Datei '%s' nicht öffnen: %s\n", dateiname, strerror(err));
                }
                else {
                    const size_t len = fread(buf, 1, BUFSIZ, fp);
                    if (ferror(fp)) {
                        fprintf(stderr, "Read error\n");
                        XML_ParserFree(parser);
                        Assert::IsFalse(true);
                    }

                    done = feof(fp);

                    if (XML_ParseBuffer(parser, (int)len, done) == XML_STATUS_ERROR) {
                        fprintf(stderr,
                            "Parse error at line %" XML_FMT_INT_MOD "u:\n%" XML_FMT_STR "\n",
                            XML_GetCurrentLineNumber(parser),
                            XML_ErrorString(XML_GetErrorCode(parser)));
                        XML_ParserFree(parser);
                        Assert::IsFalse(true);
                    }
                    fclose(fp);
                }
            } while (!done);

            XML_ParserFree(parser);
		}
	};
}
