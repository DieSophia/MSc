// StaticOsmReader.cpp : Hiermit werden die Funktionen für die statische Bibliothek definiert.
//

#pragma once

#include "pch.h"
#include "OsmReaderHeader.h"

#ifdef XML_LARGE_SIZE
#  define XML_FMT_INT_MOD "ll"
#else
#  define XML_FMT_INT_MOD "l"
#endif

#ifdef XML_UNICODE_WCHAR_T
#  define XML_FMT_STR "ls"
#else
#  define XML_FMT_STR "s"
#endif

void XMLCALL
startElement(void* userData, const XML_Char* name, const XML_Char** atts) {
    int i;
    int* const depthPtr = (int*)userData;
    (void)atts;

    for (i = 0; i < *depthPtr; i++)
        putchar('\t');
    printf("%" XML_FMT_STR "\n", name);
    *depthPtr += 1;
}

void XMLCALL
endElement(void* userData, const XML_Char* name) {
    int* const depthPtr = (int*)userData;
    (void)name;

    *depthPtr -= 1;
}