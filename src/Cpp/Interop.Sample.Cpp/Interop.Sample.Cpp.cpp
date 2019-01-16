// Interop.Sample.Cpp.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "stdio.h"
#include <windows.h>

extern "C" __declspec(dllexport) void test(char *data)
{
	printf("%s\n", data);
	fflush(stdout);
}


