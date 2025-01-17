#pragma once

#ifdef JA_C_EXPORTS
#define JC_C_API __declspec(dllexport)
#else
#define JA_C_API __declspec(dllimport)
#endif
