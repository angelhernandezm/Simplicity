#include <iostream>
#include <string>
#include <memory>
#include <algorithm>
#include <vector>
#include <functional>
#include "stdafx.h"
#include "..\Dependencies\jni.h"
#include "ConfigReader.h"

using namespace std;

#pragma region "Interop class"
namespace JNIBridge {
	namespace Core {
		class JNIBRIDGE_API Interop {
		private:
			/// <summary>
			/// Runs the java env checks.
			/// </summary>
			void RunJavaEnvChecks();

			/// <summary>
			/// Gets the jar files.
			/// </summary>
			/// <param name="directoryPath">The directory path.</param>
			/// <param name="extension">The extension.</param>
			/// <returns></returns>
			std::vector<std::wstring> GetJarFiles(std::wstring directoryPath, std::wstring extension);

		protected:
			/// <summary>
			/// The m java vm
			/// </summary>
			JavaVM* m_JavaVm;

			/// <summary>
			/// Checks for java env variables.
			/// </summary>
			/// <returns></returns>
			bool CheckForJavaEnvVariables();

			/// <summary>
			/// The m configuration reader
			/// </summary>
			mutable JNIBridge::Config::ConfigReader m_configReader;

			/// <summary>
			/// The m jar files
			/// </summary>
			mutable std::vector<std::wstring> m_jarFiles;

			/// <summary>
			/// Checks the required jre sym links.
			/// </summary>
			/// <param name="deleteAndCreateIfExist">if set to <c>true</c> [delete and create if exist].</param>
			/// <returns></returns>
			bool CheckRequiredJreSymLinks(bool deleteAndCreateIfExist);

			/// <summary>
			/// The m java home
			/// </summary>
			std::wstring m_javaHome;

			/// <summary>
			/// The m jre home
			/// </summary>
			std::wstring m_jreHome;

		public:

			/// <summary>
			/// Initializes a new instance of the <see cref="Interop" /> class.
			/// </summary>
			Interop();

			/// <summary>
			/// Finalizes an instance of the <see cref="Interop" /> class.
			/// </summary>
			~Interop();

			/// <summary>
			/// Javas the env get.
			/// </summary>
			/// <returns></returns>
			JNIEnv* JavaEnv_get() const;

			/// <summary>
			/// Javas the vm get.
			/// </summary>
			/// <returns></returns>
			JavaVM* JavaVM_get() const;

			/// <summary>
			/// Jnis the version get.
			/// </summary>
			/// <returns></returns>
			jint JniVersion_get() const;

			/// <summary>
			/// Initializes the JVM.
			/// </summary>
			/// <param name="configFile">The configuration file.</param>
			/// <returns></returns>
			bool InitializeJvm(const char* configFile);

			/// <summary>
			/// Configurations the get.
			/// </summary>
			/// <returns></returns>
			JNIBridge::Config::ConfigReader& Config_get() const;

			/// <summary>
			/// Loadeds the jars get.
			/// </summary>
			/// <returns></returns>
			std::vector<std::wstring>& LoadedJars_get() const;

			/// <summary>
			/// Javas the home get.
			/// </summary>
			/// <returns></returns>
			std::wstring JavaHome_get() const;

			/// <summary>
			/// Jres the home get.
			/// </summary>
			/// <returns></returns>
			std::wstring  JreHome_get() const;

			/// <summary>
			/// Gets the jni expection details.
			/// </summary>
			/// <param name="e">The e.</param>
			/// <returns></returns>
			static std::string GetJniExpectionDetails(const jthrowable& e, JNIEnv** env);

			/// <summary>
			/// Checks for exceptions in JVM.
			/// </summary>
			/// <param name="env">The env.</param>
			/// <param name="exceptionsThrown">The exceptions thrown.</param>
			static void CheckForExceptionsInJvm(JNIEnv* env, std::vector<std::string>& exceptionsThrown);

			/// <summary>
			/// Prepares the arguments.
			/// </summary>
			/// <param name="params">The parameters.</param>
			/// <param name="paramType">Type of the parameter.</param>
			/// <param name="args">The arguments.</param>
			static void PrepareArgs(SAFEARRAY& params, SAFEARRAY& paramType, std::vector<jvalue>& args);
		};
	}
}
#pragma endregion

#pragma region "Exported functions"
extern "C"
{
	JNIBRIDGE_API void ForceGC();

	JNIBRIDGE_API void ShutdownJvm();

	JNIBRIDGE_API HRESULT LoadJVMAndJar(const char* configFile);

	JNIBRIDGE_API HRESULT AddPath(const char* jarFile, char* result, char* exceptions);

	JNIBRIDGE_API HRESULT SerializeMethodsInJar(const char* jarFile, const char* xmlPath, char* exceptions);

	JNIBRIDGE_API HRESULT RunMethodInJar(const char* className, const char* methodName, BOOL isStatic, const char* methodProto,
										SAFEARRAY params, SAFEARRAY paramType, int arrayLength, const void* returnValue, 
										char* exceptions, char* returnType);

}
#pragma endregion