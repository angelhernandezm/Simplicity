#include <stdlib.h>
#include <iostream>
#include <fstream>
#include <memory>
#include <typeinfo.h>
#include <msxml6.h>
#include <vector>
#include <exception>
#include <Psapi.h>
#include <algorithm>
#include <map>
#include "ConfigSetting.h"
#include "stdafx.h"
#import "msxml6.dll"

namespace JNIBridge {
	namespace Config {
		class ConfigReader {
		private:
			HANDLE GetThreadToken();
			BOOL LocateConfigFile();
			void ProcessElementRecursively(IXMLDOMNodePtr& node);
			BOOL ParseConfigFile(const std::wstring& configFile);
			std::vector<JNIBridge::Structs::ConfigSetting> m_settings;
			void ExtractInformationFromElement(IXMLDOMNodePtr& node);
			std::map<const std::wstring, const std::wstring> Properties;
			BOOL SetPrivilege(HANDLE& hToken, LPCTSTR Privilege, BOOL bEnablePrivilege);
			std::wstring DoesConfigFileExist(const HANDLE& hProcess, std::vector<HMODULE>& hModules, const wchar_t* targetImage);

		protected:
			

		public:
			ConfigReader();
			~ConfigReader();
			void ReadConfig(const char* configFile);
			const std::wstring GetSetting(const wchar_t* key);
			std::vector<JNIBridge::Structs::ConfigSetting>& Settings_get();
		};
	}
}

