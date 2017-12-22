// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <string.h>


// TODO: reference additional headers your program requires here

#include <combaseapi.h>
#include <comutil.h>
#include <atlbase.h>
#include <OaIdl.h>

typedef void(*ptrClrDelegateForByte) (BYTE results);
typedef void(*ptrClrDelegateForChar) (char results);
typedef void(*ptrClrDelegateForLong) (long results);
typedef void(*ptrClrDelegateForFloat) (float results);
typedef void(*ptrClrDelegateForShort) (short results);
typedef void(*ptrClrDelegateForInteger) (int results);
typedef void(*ptrClrDelegateForObject) (void* results);
typedef void(*ptrClrDelegateForString) (char* results);
typedef void(*ptrClrDelegateForBoolean) (bool results);
typedef void(*ptrClrDelegateForDouble) (double results);

#define XmlRootNode L"config"
#define Max_Loaded_Modules 0x0000100
#define ConfigFileName  L"config.xml"
#define Max_BufferSizeForMessagesFromJni 4096
#define Item_Count(x) ( x > 0 ? (x/sizeof(x)) : 0)
#define Array_Size(array) (sizeof(array) / sizeof(array[0]))


#pragma warning(disable:4192 4251)

#ifdef JNIBRIDGE_EXPORTS
#define JNIBRIDGE_API __declspec(dllexport)
#else
#define JNIBRIDGE_API __declspec(dllimport)
#endif