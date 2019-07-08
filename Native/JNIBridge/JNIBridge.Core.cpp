#include "stdafx.h"
#include "JNIBridge.Core.h"
#include "JNIBridge.Core.Utils.h"

using namespace JNIBridge::Core::Utils;

/// <summary>
/// The g singleton
/// </summary>
JNIBridge::Core::Interop* g_Singleton;

typedef jint(*ptrLoadJvm) (JavaVM** pvm, void** penv, void* args);

#pragma region "Generic lambdas"

auto dynamicallyLoadJvm = [&](JavaVM** pvm, void** penv, void* args, std::wstring selectedJvm) -> jint {
	auto retval = 0;
	ptrLoadJvm loadJvm;
	HINSTANCE hInstance;

	if ((hInstance = LoadLibrary(selectedJvm.c_str())) != nullptr)  {
		if ((loadJvm = (ptrLoadJvm)GetProcAddress(hInstance, "JNI_CreateJavaVM")) != nullptr) {
			retval = loadJvm(pvm, penv, args);
		}
		FreeLibrary(hInstance);
	}
	

	  
// auto jvm = JNI_CreateJavaVM(&m_JavaVm, (void**)&env, &initArgs);

	return retval;
};

/// <summary>
/// The get jni versionfrom string
/// </summary>
auto getJniVersionfromString = [=](const std::wstring& versionAsString) -> int {
	auto retval = 0;
	auto selectedVersion = versionAsString.c_str();

	if (wcscmp(selectedVersion, L"JNI_VERSION_1_1") == 0)
		retval = JNI_VERSION_1_1;
	else if (wcscmp(selectedVersion, L"JNI_VERSION_1_2") == 0)
		retval = JNI_VERSION_1_2;
	else if (wcscmp(selectedVersion, L"JNI_VERSION_1_4") == 0)
		retval = JNI_VERSION_1_4;
	else if (wcscmp(selectedVersion, L"JNI_VERSION_1_6") == 0)
		retval = JNI_VERSION_1_6;
	else if (wcscmp(selectedVersion, L"JNI_VERSION_1_8") == 0)
		retval = JNI_VERSION_1_8;

	return retval;
};

/// <summary>
/// The get safe array element count
/// </summary>
auto getSafeArrayElementCount = [&](SAFEARRAY& arr) -> long {
	long lowerBound, upperBound, retval;

	SafeArrayGetLBound(&arr, 1, &lowerBound);
	SafeArrayGetUBound(&arr, 1, &upperBound);
	retval = upperBound - lowerBound + 1;

	return retval;
};

/// <summary>
/// The get j value from argument type
/// </summary>
auto getJValueFromArgType = [&](BSTR& type, VARIANT& arg) -> jvalue {
	jvalue retval;

	if (wcscmp(type, L"single") == 0 || wcscmp(type, L"float") == 0) {
		retval.f = arg.fltVal;
	}
	else if (wcscmp(type, L"integer") == 0) {
		retval.i = arg.intVal;
	}
	else if (wcscmp(type, L"byte") == 0) {
		retval.b = arg.bVal;
	}
	else if (wcscmp(type, L"char") == 0) {
		retval.c = arg.cVal;
	}
	else if (wcscmp(type, L"short") == 0) {
		retval.s = arg.iVal;
	}
	else if (wcscmp(type, L"long") == 0) {
		retval.j = arg.lVal;
	}
	else if (wcscmp(type, L"double") == 0) {
		retval.d = arg.dblVal;
	}
	else if (wcscmp(type, L"bool") == 0) {
		retval.z = (jboolean)arg.boolVal;
	}
	else { // We'll default to "object" (void*)
	 // [CONSIDER] Other reference data types (e.g.: string) [CONSIDER]
		retval.l = *(reinterpret_cast<jobject*>(arg.byref));
	}

	return retval;
};

/// <summary>
/// The format exceptions as string
/// </summary>
auto formatExceptionsAsString = [=](const std::vector<std::string>& exceptions) -> std::string {
	auto count = 0;
	std::string retval;

	std::for_each(exceptions.begin(), exceptions.end(), [&](std::string ex) {
		char buffer[50] = { '\0' };
		_itoa_s(++count, buffer, sizeof(char[50]), 10);
		auto exCount = std::string("Exception # ").append(buffer);
		retval += exCount + "\n" + ex + "[----------------------]";
	});

	return retval;
};

#pragma endregion

#pragma region "Interop class"


/// <summary>
/// Initializes a new instance of the <see cref="Interop"/> class.
/// </summary>
JNIBridge::Core::Interop::Interop() {

}

/// <summary>
/// Finalizes an instance of the <see cref="Interop"/> class.
/// </summary>
JNIBridge::Core::Interop::~Interop() {
	if (m_JavaVm != nullptr)
		m_JavaVm->DestroyJavaVM();
}


/// <summary>
/// Runs the java env checks.
/// </summary>
void JNIBridge::Core::Interop::RunJavaEnvChecks() {
	// For JVM to load properly, it's required to have two SymLinks pointing to bin & lib folders respectively.
	if (!CheckRequiredJreSymLinks(true))
		throw std::exception("SymLinks to JRE aren't available. Unable to continue.");

	if (!CheckForJavaEnvVariables())
		throw std::exception("Make sure JAVA_HOME and JRE_HOME environment variables exist. Unable to continue.");
}

/// <summary>
/// Checks for java env variables.
/// </summary>
/// <returns></returns>
bool JNIBridge::Core::Interop::CheckForJavaEnvVariables() {
	auto count = 0;
	auto retval = false;
	std::map<std::wstring, std::wstring&> var{ {L"JAVA_HOME", m_javaHome},{ L"JRE_HOME", m_jreHome } };

	for (std::map<std::wstring, std::wstring&>::iterator it = var.begin(); it != var.end(); ++it) {
		wchar_t buffer[MAX_PATH];
		if (GetEnvironmentVariable((*it).first.c_str(), buffer, MAX_PATH) > 0) {
			(*it).second = std::wstring(buffer);
			count++;
		}
	}

	retval = var.size() == count;

	return retval;
}

/// <summary>
/// Checks the required jre sym links.
/// </summary>
/// <param name="deleteAndCreateIfExist">if set to <c>true</c> [delete and create if exist].</param>
/// <returns></returns>
bool JNIBridge::Core::Interop::CheckRequiredJreSymLinks(bool deleteAndCreateIfExist) {
	auto retval = false;
	auto libSymLinkName = m_configReader.GetSetting(L"SymLinkLibName");
	auto binSymLinkName = m_configReader.GetSetting(L"SymLinkBinName");
	auto libSymLinkTarget = m_configReader.GetSetting(L"SymLinkLibTarget");
	auto binSymLinkTarget = m_configReader.GetSetting(L"SymLinkBinTarget");
	std::map<std::wstring, std::wstring> symLinks = { { libSymLinkName , libSymLinkTarget }, { binSymLinkName, binSymLinkTarget } };

	// Let's first check if SymLinks exist, otherwise we'll create them (Lambda to re-use later on)
	auto checkFunc = [&](std::map<std::wstring, std::wstring> symLinks, bool deleteAndCreateIfExists)->bool {
		auto index = 0;
		auto exists = true;
		auto successCount = 0;

		for (auto &x : symLinks) {
			if (!deleteAndCreateIfExists) {
				if (!(GetFileAttributes(x.first.c_str()) & FILE_ATTRIBUTE_REPARSE_POINT)) {
					exists = false;
					break;
				}
			}
			else {
				if (GetFileAttributes(x.first.c_str()) & FILE_ATTRIBUTE_REPARSE_POINT)
					RemoveDirectory(x.first.c_str());

				if (CreateSymbolicLink(x.first.c_str(), x.second.c_str(), SYMBOLIC_LINK_FLAG_DIRECTORY))
					successCount++;
			}
			index++;
		}
		return  deleteAndCreateIfExists ? successCount == symLinks.size() : exists;
	};

	retval = checkFunc(symLinks, deleteAndCreateIfExist);

	return retval;
}

/// <summary>
/// Initializes the JVM.
/// </summary>
/// <param name="configFile">The configuration file.</param>
/// <returns></returns>
bool JNIBridge::Core::Interop::InitializeJvm(const char* configFile) {
	JNIEnv* env;
	auto retval = false;
	JavaVMInitArgs initArgs;
	m_configReader.ReadConfig(configFile);
	unique_ptr<JavaVMOption[]> javaVmOption;
	auto selectedJvm = m_configReader.GetSetting(L"SelectedJvm");

	auto formatJarPathFromVector = [&](std::vector<std::wstring> jars) -> std::wstring {
		std::wstring jarPath;
		std::for_each(jars.begin(), jars.end(), [&](std::wstring jar) { jarPath += jar + L"; "; });
		jarPath = jarPath.substr(0, jarPath.length() - 2);
		return jarPath;
	};

	RunJavaEnvChecks(); // Let's check environment variables and symlinks exist
	m_jarFiles = GetJarFiles(JavaHome_get().append(L"\\jre\\lib\\"), L"jar");
	auto nEnvVarPos = m_configReader.GetSetting(L"jvmOptions").find(L"%java_home%");
	auto originalOption = m_configReader.GetSetting(L"jvmOptions");
	auto allJars = originalOption.replace(nEnvVarPos, wcslen(L"%java_home%"), formatJarPathFromVector(m_jarFiles));
	auto version = getJniVersionfromString(m_configReader.GetSetting(L"jniVersion"));
	auto optionCount = Common::ConvertWstringToChar(m_configReader.GetSetting(L"jvmOptionCount"));
	auto nOptions = atoi(optionCount.c_str());
	javaVmOption = make_unique<JavaVMOption[]>(nOptions);
	auto options = Common::ConvertWstringToChar(allJars);
	javaVmOption[0].optionString = const_cast<char*>(options.c_str());
	javaVmOption[1].optionString = const_cast<char*>(Common::ConvertWstringToChar(m_configReader.GetSetting(L"jvmInitialHeapSize")).c_str()); // 1MB
	javaVmOption[2].optionString = const_cast<char*>(Common::ConvertWstringToChar(m_configReader.GetSetting(L"jvmMaximumHeapSize")).c_str());; // 1GB 
	initArgs.version = (jint)version;
	initArgs.nOptions = nOptions;
	initArgs.options = javaVmOption.get();
	initArgs.ignoreUnrecognized = JNI_TRUE;

	try {
		auto jvm = dynamicallyLoadJvm(&m_JavaVm, (void**)&env, &initArgs, selectedJvm);
		retval = jvm == JNI_OK;
	}
	catch (exception &ex) {
		auto p = ex.what();
	}

	return retval;
}

/// <summary>
/// Jnis the version get.
/// </summary>
/// <returns></returns>
jint JNIBridge::Core::Interop::JniVersion_get() const {
	return m_configReader.Settings_get().size() > 0 ?
		(int)getJniVersionfromString(m_configReader.GetSetting(L"jniVersion")) : -1;

}

/// <summary>
/// Javas the env get.
/// </summary>
/// <returns></returns>
JNIEnv* JNIBridge::Core::Interop::JavaEnv_get() const {
	jint result;
	JNIEnv* retval = nullptr;

	if (m_JavaVm != nullptr) {
		// If current thread is detached from JVM, we'll attempt to re-attach it one more time
		if (result = m_JavaVm->GetEnv((void**)&retval, JniVersion_get()) == JNI_EDETACHED) {
			if (m_JavaVm->AttachCurrentThread((void**)&retval, nullptr) != JNI_OK)
				retval = nullptr;
		}
	}

	return retval;
}


/// <summary>
/// Gets the jar files.
/// </summary>
/// <param name="directoryPath">The directory path.</param>
/// <param name="extension">The extension.</param>
/// <returns></returns>
std::vector<std::wstring> JNIBridge::Core::Interop::GetJarFiles(std::wstring directoryPath, std::wstring extension) {
	std::vector<std::wstring> retval;

	auto getFileExt = [&](const std::wstring s) -> std::wstring {
		std::wstring retval;
		auto nPos = s.rfind('.', s.length());

		if (nPos != std::wstring::npos)
			retval = s.substr(nPos + 1, s.length() - nPos);

		return retval;
	};

	std::function<void(std::wstring)> fileEnum = [&](std::wstring path) -> void {
		WIN32_FIND_DATA fi;
		auto searchPattern = std::wstring(path.c_str()).append(L"*.*");
		auto found = FindFirstFile(searchPattern.c_str(), &fi);

		while (FindNextFile(found, &fi) != 0) {
			if (fi.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
				if (wcscmp(fi.cFileName, L".") == 0 || wcscmp(fi.cFileName, L"..") == 0)
					continue;
				else fileEnum(path + fi.cFileName + L"\\");
			}
			else if (getFileExt(fi.cFileName) == extension) {
				retval.push_back(path + fi.cFileName);
			}
		}
	};

	if (directoryPath.size() > 0 && extension.size() > 0)
		fileEnum(directoryPath);

	return retval;
}

/// <summary>
/// Javas the vm get.
/// </summary>
/// <returns></returns>
JavaVM* JNIBridge::Core::Interop::JavaVM_get() const {
	return m_JavaVm;
}

/// <summary>
/// Configurations the get.
/// </summary>
/// <returns></returns>
JNIBridge::Config::ConfigReader& JNIBridge::Core::Interop::Config_get() const {
	return m_configReader;
}

/// <summary>
/// Loadeds the jars get.
/// </summary>
/// <returns></returns>
std::vector<std::wstring>& JNIBridge::Core::Interop::LoadedJars_get() const {
	return m_jarFiles;
}

/// <summary>
/// Javas the home get.
/// </summary>
/// <returns></returns>
std::wstring JNIBridge::Core::Interop::JavaHome_get() const {
	return m_javaHome;
}

/// <summary>
/// Javas the home get.
/// </summary>
/// <returns></returns>
std::wstring JNIBridge::Core::Interop::JreHome_get() const {
	return m_jreHome;
}

/// <summary>
/// Gets the jni expection details.
/// </summary>
/// <param name="e">The e.</param>
/// <param name="env">The env.</param>
/// <returns></returns>
std::string JNIBridge::Core::Interop::GetJniExpectionDetails(const jthrowable& e, JNIEnv** env) {
	jobject res;
	jclass clazz;
	jmethodID method;
	std::string retval;

	if (env != nullptr) {
		clazz = (*env)->FindClass("com/simplicity/Inspector");
		method = (*env)->GetStaticMethodID(clazz, "extractExceptionDetails", "(Ljava/lang/Object;)Ljava/lang/String;");
		res = (*env)->CallStaticObjectMethod(clazz, method, e);
		const char* val = (*env)->GetStringUTFChars((jstring)res, nullptr);
		retval = std::string(val);

		// Release JNI resources
		(*env)->ReleaseStringUTFChars((jstring)res, val);
		(*env)->DeleteLocalRef(res);
		(*env)->DeleteLocalRef(clazz);
	}

	return retval;
}

/// <summary>
/// Checks for exceptions in JVM.
/// </summary>
/// <param name="env">The env.</param>
/// <param name="exceptionsThrown">The exceptions thrown.</param>
void JNIBridge::Core::Interop::CheckForExceptionsInJvm(JNIEnv* env, std::vector<std::string>& exceptionsThrown) {
	jthrowable ex = nullptr;

	if (env->ExceptionCheck() && (ex = env->ExceptionOccurred()) != nullptr) {
		exceptionsThrown.push_back(JNIBridge::Core::Interop::GetJniExpectionDetails(ex, &env));
		env->ExceptionClear();
		env->DeleteLocalRef(ex);
	}
}


/// <summary>
/// Prepares the arguments.
/// </summary>
/// <param name="params">The parameters.</param>
/// <param name="paramType">Type of the parameter.</param>
/// <param name="args">The arguments.</param>
void JNIBridge::Core::Interop::PrepareArgs(SAFEARRAY& params, SAFEARRAY& paramType, std::vector<jvalue>& args) {
	BSTR *pArgTypes;
	VARIANT *pArgValues;

	if (SUCCEEDED(SafeArrayAccessData(&params, (void**)&pArgValues)) &&
		SUCCEEDED(SafeArrayAccessData(&paramType, (void**)&pArgTypes))) {
		auto cArgElements = getSafeArrayElementCount(params);
		auto cTypeElements = getSafeArrayElementCount(paramType);

		// Element count must match for both safearrays (1-1 ratio)
		if (cArgElements == cTypeElements) {
			for (auto idx = 0; idx < cArgElements; ++idx) {
				auto argType = pArgTypes[idx];
				auto argument = pArgValues[idx];
				args.push_back(getJValueFromArgType(argType, argument));
			}
		}
	}
}

#pragma endregion


#pragma region "Exported functions"


/// <summary>
/// Loads the JVM and jar.
/// </summary>
/// <param name="configFile">The configuration file.</param>
/// <returns></returns>
JNIBRIDGE_API HRESULT LoadJVMAndJar(const char* configFile) {
	HRESULT retval;

	if (g_Singleton == nullptr) {
		g_Singleton = new JNIBridge::Core::Interop;
		retval = g_Singleton->InitializeJvm(configFile) ? S_OK : S_FALSE;
	}
	else retval = S_OK;

	return retval;
}


/// <summary>
/// Shutdowns the JVM.
/// </summary>
/// <returns></returns>
JNIBRIDGE_API void ShutdownJvm() {
	if (g_Singleton != nullptr)
		delete g_Singleton;
}

/// <summary>
/// Forces the gc.
/// </summary>
/// <returns></returns>
JNIBRIDGE_API void ForceGC() {
	JNIEnv* env;
	jclass clazz;
	jmethodID method;

	if (g_Singleton != nullptr && (env = g_Singleton->JavaEnv_get()) != nullptr) {
		clazz = env->FindClass("java/lang/System");
		method = env->GetStaticMethodID(clazz, "gc", "()V");
		env->CallStaticObjectMethod(clazz, method);
		env->DeleteLocalRef(clazz);
	}
}

/// <summary>
/// Runs the method in jar.
/// </summary>
/// <param name="className">Name of the class.</param>
/// <param name="methodName">Name of the method.</param>
/// <param name="isStatic">The is static.</param>
/// <param name="methodProto">The method proto.</param>
/// <param name="params">The parameters.</param>
/// <param name="paramType">Type of the parameter.</param>
/// <param name="arrayLength">Length of the array.</param>
/// <param name="returnValue">The return value.</param>
/// <param name="exceptions">The exceptions.</param>
/// <param name="returnType">Type of the return.</param>
/// <returns></returns>
JNIBRIDGE_API HRESULT RunMethodInJar(const char* className, const char* methodName, BOOL isStatic, const char* methodProto,
	SAFEARRAY params, SAFEARRAY paramType, int arrayLength, const void* returnValue, char* exceptions, char* returnType) {

	JNIEnv* env;
	jclass clazz;
	va_list parameters;
	jobject res, object;
	HRESULT retval = S_FALSE;
	std::vector<jvalue> args;
	jmethodID method, constructor;
	std::vector<std::string> exceptionsThrown;

	if (g_Singleton != nullptr && (env = g_Singleton->JavaEnv_get()) != nullptr &&
		className != nullptr && strlen(className) > 0 && methodName != nullptr && strlen(methodName) &&
		methodProto != nullptr && strlen(methodProto) > 0 && returnValue != nullptr) {
		try {
			clazz = env->FindClass(className);
			constructor = env->GetMethodID(clazz, "<init>", "()V");
			object = env->NewObject(clazz, constructor);
			method = env->GetMethodID(clazz, methodName, methodProto);

			// Let's call right method based on parameter count
			if (arrayLength == 0) {
				res = env->CallObjectMethod(object, method);
			}
			else {
				JNIBridge::Core::Interop::PrepareArgs(params, paramType, args);
				res = env->CallObjectMethodA(object, method, &args[0]);
			}

			// Let's get exception details (if any)
			JNIBridge::Core::Interop::CheckForExceptionsInJvm(env, exceptionsThrown);
			auto exceptionAsStr = formatExceptionsAsString(exceptionsThrown);
			strcpy_s(exceptions, sizeof(char[Max_BufferSizeForMessagesFromJni]), exceptionAsStr.c_str());
			retval = exceptionsThrown.size() == 0 ? S_OK : S_FALSE;

			// Return result to CLR via a callback
			if (strcmp(returnType, "single") == 0 || strcmp(returnType, "float") == 0) {
				((ptrClrDelegateForFloat)returnValue)(*(reinterpret_cast<jfloat*>(&res)));
			}
			else if (strcmp(returnType, "integer") == 0) {
				((ptrClrDelegateForInteger)returnValue)(*(reinterpret_cast<jint*>(&res)));
			}
			else if (strcmp(returnType, "byte") == 0) {
				((ptrClrDelegateForByte)returnValue)(*(reinterpret_cast<jbyte*>(&res)));
			}
			else if (strcmp(returnType, "char") == 0) {
				((ptrClrDelegateForChar)returnValue)(*(reinterpret_cast<jchar*>(&res)));
			}
			else if (strcmp(returnType, "short") == 0) {
				((ptrClrDelegateForShort)returnValue)(*(reinterpret_cast<jshort*>(&res)));
			}
			else if (strcmp(returnType, "long") == 0) {
				((ptrClrDelegateForLong)returnValue)(*(reinterpret_cast<jlong*>(&res)));
			}
			else if (strcmp(returnType, "double") == 0) {
				((ptrClrDelegateForDouble)returnValue)(*(reinterpret_cast<jdouble*>(&res)));
			}
			else if (strcmp(returnType, "bool") == 0) {
				((ptrClrDelegateForBoolean)returnValue)(*(reinterpret_cast<jboolean*>(&res)));
			}
			else { // We'll default to "object" (void*)
				   // [CONSIDER] Other reference data types (e.g.: string) [CONSIDER]
				((ptrClrDelegateForObject)returnValue)(*(reinterpret_cast<jobject*>(&res)));
			}

			// Release JNI Resources
			env->DeleteLocalRef(object);
			env->DeleteLocalRef(clazz);
		}
		catch (std::exception& e) {
			auto p = e.what();
		}
	}

	return retval;
}



/// <summary>
/// Serializes the methods in jar.
/// </summary>
/// <param name="jarFile">The jar file.</param>
/// <param name="xmlPath">The XML path.</param>
/// <param name="exceptions">The exceptions.</param>
/// <returns></returns>
JNIBRIDGE_API HRESULT SerializeMethodsInJar(const char* jarFile, const char* xmlPath, char* exceptions) {
	JNIEnv* env;
	jclass clazz;
	jstring jFile;
	jobject res, object;
	HRESULT retval = S_FALSE;
	jmethodID method, constructor;
	std::vector<std::string> exceptionsThrown;

	if (g_Singleton != nullptr && (env = g_Singleton->JavaEnv_get()) != nullptr &&
		jarFile != nullptr && strlen(jarFile) > 0 && xmlPath != nullptr && strlen(xmlPath)) {
		try {
			// Create instance of Inspector and call getMethodsInClasses
			jFile = env->NewStringUTF(jarFile);
			clazz = env->FindClass("com/simplicity/Inspector");
			constructor = env->GetMethodID(clazz, "<init>", "()V");
			object = env->NewObject(clazz, constructor);
			method = env->GetMethodID(clazz, "getMethodsInClasses", "(Ljava/lang/String;)Ljava/util/Map;");
			res = env->CallObjectMethod(object, method, jFile);
			JNIBridge::Core::Interop::CheckForExceptionsInJvm(env, exceptionsThrown);

			// Create instance of InspectorSerialized and serialize methods information
			jFile = env->NewStringUTF(xmlPath);
			clazz = env->FindClass("com/simplicity/InspectorSerializer");
			constructor = env->GetMethodID(clazz, "<init>", "()V");
			object = env->NewObject(clazz, constructor);
			method = env->GetMethodID(clazz, "serializeReflectedMethods", "(Ljava/util/Map;Ljava/lang/String;)Z");
			retval = (jboolean)env->CallObjectMethod(object, method, res, jFile) == 1 ? S_OK : S_FALSE;
			JNIBridge::Core::Interop::CheckForExceptionsInJvm(env, exceptionsThrown);
			auto exceptionAsStr = formatExceptionsAsString(exceptionsThrown);
			strcpy_s(exceptions, sizeof(char[Max_BufferSizeForMessagesFromJni]), exceptionAsStr.c_str());

			// Release JNI resources
			env->DeleteLocalRef(res);
			env->DeleteLocalRef(object);
			env->DeleteLocalRef(clazz);
			env->ReleaseStringUTFChars(jFile, nullptr);
		}
		catch (std::exception& e) {
			auto p = e.what();
		}
	}

	return retval;
}

/// <summary>
/// Adds the path.
/// </summary>
/// <param name="jarFile">The jar file.</param>
/// <param name="result">The result.</param>
/// <param name="exceptions">The exceptions.</param>
/// <returns></returns>
JNIBRIDGE_API HRESULT AddPath(const char* jarFile, char* result, char* exceptions) {
	JNIEnv* env;
	jobject res;
	jclass clazz;
	jstring jFile;
	jmethodID method;
	HRESULT retval = S_FALSE;
	std::vector<std::string> exceptionsThrown;

	if (g_Singleton != nullptr && (env = g_Singleton->JavaEnv_get()) != nullptr &&
		jarFile != nullptr && strlen(jarFile) > 0) {
		try {
			jFile = env->NewStringUTF(jarFile);
			clazz = env->FindClass("com/simplicity/Inspector");
			method = env->GetStaticMethodID(clazz, "addPath", "(Ljava/lang/String;)Ljava/lang/String;");
			res = env->CallStaticObjectMethod(clazz, method, jFile);
			const char* val = env->GetStringUTFChars((jstring)res, nullptr);
			auto resLength = strlen(val);
			strcpy_s(result, sizeof(char[MAX_PATH]), val);
			JNIBridge::Core::Interop::CheckForExceptionsInJvm(env, exceptionsThrown);
			auto exceptionAsStr = formatExceptionsAsString(exceptionsThrown);
			strcpy_s(exceptions, sizeof(char[Max_BufferSizeForMessagesFromJni]), exceptionAsStr.c_str());

			// Release JNI resources
			env->ReleaseStringUTFChars(jFile, val);
			env->DeleteLocalRef(res);
			env->DeleteLocalRef(clazz);
			env->DeleteLocalRef(jFile);
			retval = resLength > 0 ? S_OK : S_FALSE;
		}
		catch (std::exception& e) {
			auto p = e.what();
		}
	}

	return retval;
}

#pragma endregion