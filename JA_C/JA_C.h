#pragma once

#ifdef JA_C_EXPORTS
#define JC_C_API __declspec(dllexport)
#else
#define JA_C_API __declspec(dllimport)
#endif

#include <string>
#include <fstream>
#include <iostream>
#include <vector>

extern "C" JA_C_API int test(int a, int b);
