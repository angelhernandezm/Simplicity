#include "stdafx.h"
#include "CppUnitTest.h"
#include <memory>
#include <Rpc.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;


#pragma  region "Callbacks"
typedef void(*ptrShutdownJvm) ();
typedef HRESULT(*ptrLoadJVMAndJar) (const char* configFile);
typedef HRESULT(*ptrAddPath) (const char* jarFile, char* result);
typedef HRESULT(*ptrSerializeMethodsInJar) (const char* jarFile, const char* xmlPath);

#pragma endregion



namespace JNIBridge_Tests {
	TEST_CLASS(HostingJVMTests) {
public:

	/// <summary>
	/// Loads the JVM and jar.
	/// </summary>
	TEST_METHOD(LoadJvmAndJar) {
		HRESULT retval;
		ptrLoadJVMAndJar jvmLoadCallback;
		HINSTANCE hInstance, hJvmInstance;

		if ((hJvmInstance = LoadLibrary(JavaVMLib)) != nullptr) {
			if ((hInstance = LoadLibrary(JNIBridgeLib)) != nullptr) {
				if ((jvmLoadCallback = (ptrLoadJVMAndJar)GetProcAddress(hInstance, "LoadJVMAndJar")) != nullptr) {
					retval = jvmLoadCallback(ConfigFile);
					Assert::IsTrue(SUCCEEDED(retval));
					(ptrShutdownJvm)GetProcAddress(hInstance, "ShutdownJvm")();
					FreeLibrary(hInstance);
					FreeLibrary(hJvmInstance);
				}
			}
		}
	}

	/// <summary>
	/// Tests the singleton.
	/// </summary>
	TEST_METHOD(TestSingleton) {
		HRESULT retvalOne, retvalTwo;
		ptrLoadJVMAndJar jvmLoadCallback;
		HINSTANCE hInstance, hJvmInstance;

		if ((hJvmInstance = LoadLibrary(JavaVMLib)) != nullptr) {
			if ((hInstance = LoadLibrary(JNIBridgeLib)) != nullptr) {
				if ((jvmLoadCallback = (ptrLoadJVMAndJar)GetProcAddress(hInstance, "LoadJVMAndJar")) != nullptr) {
					retvalOne = jvmLoadCallback(ConfigFile);
					retvalTwo = jvmLoadCallback(ConfigFile);
					Assert::IsTrue(SUCCEEDED(retvalOne) && SUCCEEDED(retvalTwo));
					(ptrShutdownJvm)GetProcAddress(hInstance, "ShutdownJvm")();
					FreeLibrary(hInstance);
					FreeLibrary(hJvmInstance);
				}
			}
		}
	}


	/// <summary>
	/// Adds the path.
	/// </summary>
	TEST_METHOD(AddPath) {
		HRESULT retval;
		ptrAddPath addPathCallback;
		ptrLoadJVMAndJar jvmLoadCallback;
		char buffer[MAX_PATH] = { '\0' };
		HINSTANCE hInstance, hJvmInstance;

		if ((hJvmInstance = LoadLibrary(JavaVMLib)) != nullptr) {
			if ((hInstance = LoadLibrary(JNIBridgeLib)) != nullptr) {
				if ((jvmLoadCallback = (ptrLoadJVMAndJar)GetProcAddress(hInstance, "LoadJVMAndJar")) != nullptr) {
					retval = jvmLoadCallback(ConfigFile);
					Assert::IsTrue(SUCCEEDED(retval));
					if ((addPathCallback = (ptrAddPath)GetProcAddress(hInstance, "AddPath")) != nullptr) {
						retval = addPathCallback("c:\\Somepath\\MyJarFile.jar", buffer);
						Assert::IsTrue(SUCCEEDED(retval));
					}
					(ptrShutdownJvm)GetProcAddress(hInstance, "ShutdownJvm")();
					FreeLibrary(hInstance);
					FreeLibrary(hJvmInstance);
				}
			}
		}

	}

	/// <summary>
	/// Serializes the methods in jar.
	/// </summary>
	TEST_METHOD(SerializeMethodsInJar) {
		HRESULT retval;
		ptrLoadJVMAndJar jvmLoadCallback;
		ptrSerializeMethodsInJar serializeMethodsCallback;
		HINSTANCE hInstance, hJvmInstance;

		if ((hJvmInstance = LoadLibrary(JavaVMLib)) != nullptr) {
			if ((hInstance = LoadLibrary(JNIBridgeLib)) != nullptr) {
				if ((jvmLoadCallback = (ptrLoadJVMAndJar)GetProcAddress(hInstance, "LoadJVMAndJar")) != nullptr) {
					retval = jvmLoadCallback(ConfigFile);
					Assert::IsTrue(SUCCEEDED(retval));
					if ((serializeMethodsCallback = (ptrSerializeMethodsInJar)GetProcAddress(hInstance, "SerializeMethodsInJar")) != nullptr) {
						retval = serializeMethodsCallback("C:\\Temp\\JNIBridge\\SimpleCalc.jar", "c:\\Temp\\work\\output.xml");
						Assert::IsTrue(SUCCEEDED(retval));
					}
					(ptrShutdownJvm)GetProcAddress(hInstance, "ShutdownJvm")();
					FreeLibrary(hInstance);
					FreeLibrary(hJvmInstance);
				}
			}
		}

	}

	};
}