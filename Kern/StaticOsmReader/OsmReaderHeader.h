#pragma once

#include <stdio.h>
#include "expat.h"

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
startElement(void* userData, const XML_Char* name, const XML_Char** atts);

void XMLCALL
endElement(void* userData, const XML_Char* name);
