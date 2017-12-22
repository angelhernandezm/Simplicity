#include "stdafx.h"
#include "ConfigReader.h"
#include "JNIBridge.Core.Utils.h"

using namespace std;
using namespace JNIBridge::Core::Utils;


#pragma region "Constructor & Destructor"

JNIBridge::Config::ConfigReader::ConfigReader() {
	CoInitialize(NULL);
}

JNIBridge::Config::ConfigReader::~ConfigReader() {
	CoUninitialize();
}

#pragma endregion 


void JNIBridge::Config::ConfigReader::ReadConfig(const char* configFile) {
	if (configFile != nullptr) {
		if (Common::DoesFileExist(configFile)) {
			if (!ParseConfigFile(Common::ConvertCharToWstring(configFile)))
				throw std::exception("Unable to parse config file.");
		} else throw std::exception("Config file not found in specified path.");
	} else if (!LocateConfigFile())
		throw std::exception("Config file not found. Unable to proceed.");
}

void JNIBridge::Config::ConfigReader::ExtractInformationFromElement(IXMLDOMNodePtr& node) {
	VARIANT value;
	std::wstring key;
	BSTR nodeContent;
	DOMNodeType nodeType;
	std::string szBuffer;
	WCHAR szNodeText[512] = {0};

	if (SUCCEEDED(node->get_nodeType(&nodeType)) && nodeType == DOMNodeType::NODE_ELEMENT) {
		nodeContent = SysAllocString(szNodeText);
		auto pElement = (IXMLDOMElementPtr)node;
		pElement->get_tagName(&nodeContent);

		if (wcscmp(nodeContent, L"key") == 0) {
			pElement->getAttribute(_bstr_t(L"name"), &value);

			if (value.vt != VT_NULL)
				key.assign(value.bstrVal);

			pElement->getAttribute(_bstr_t(L"value"), &value);

			if (value.vt != VT_NULL && !key.empty()) {
				Properties.insert(std::make_pair(key.c_str(), value.bstrVal));
				std::string name(Common::ConvertWstringToChar(key));
				std::string keyValue(Common::ConvertWstringToChar(std::wstring(value.bstrVal)));
				m_settings.push_back(JNIBridge::Structs::ConfigSetting(name, keyValue));
			}
		}
		SysFreeString(nodeContent);
	}
}


const std::wstring JNIBridge::Config::ConfigReader::GetSetting(const wchar_t* key) {
	std::wstring retval;

	if (Properties.size() > 0 && key != nullptr && wcslen(key) > 0) {
		typedef std::pair<const std::wstring, const std::wstring> item;

		auto result = std::find_if(Properties.begin(), Properties.end(), [&](item i) {
			auto ret = FALSE;

			if (retval.size() == 0) {
				if (wcscmp(i.first.data(), key) == 0) {
					retval.assign(i.second);
					ret = TRUE;
				}
			}
			return ret;
		});
	}


	return retval;
}

void JNIBridge::Config::ConfigReader::ProcessElementRecursively(IXMLDOMNodePtr& node) {
	long childrenCount = 0;
	IXMLDOMNodePtr childNode;
	IXMLDOMNodeListPtr children;

	if (SUCCEEDED(node->get_childNodes(&children)) && SUCCEEDED(children->get_length(&childrenCount)) && childrenCount > 0) {
		for (auto nCount = 0; nCount < childrenCount; nCount++) {
			if (SUCCEEDED(children->get_item(nCount, &childNode))) {
				ExtractInformationFromElement(childNode);
				ProcessElementRecursively(childNode);
			}
		}
	}
}


BOOL JNIBridge::Config::ConfigReader::ParseConfigFile(const std::wstring& configFile) {
	auto retval = FALSE;
	VARIANT_BOOL success;
	IXMLDOMDocumentPtr pDocPtr;
	IXMLDOMNodePtr selectedNode;

	pDocPtr.CreateInstance("Msxml2.DOMDocument.6.0");

	if (SUCCEEDED(pDocPtr->load(_variant_t(configFile.c_str()), &success))) {
		if (SUCCEEDED(pDocPtr->selectSingleNode(_bstr_t(XmlRootNode), &selectedNode))) {
			m_settings.clear();
			ProcessElementRecursively(selectedNode);
			retval = TRUE;
		}
	}

	return retval;
}

std::wstring JNIBridge::Config::ConfigReader::DoesConfigFileExist(const HANDLE& hProcess, std::vector<HMODULE>& hModules, const wchar_t* targetImage) {
	BOOL found = FALSE;
	std::wstring retval;
	wchar_t szBuffer[MAX_PATH];

	auto result = std::find_if(hModules.begin(), hModules.end(), [&, this](HMODULE hModule) {
		auto ret = FALSE;

		if (!found && hModule != nullptr && (GetModuleFileNameEx(hProcess, hModule, szBuffer, Array_Size(szBuffer))) != NULL) {
			auto filePath = Common::GetModuleName(std::wstring(szBuffer));
			auto imageName = filePath.at(0);
			auto configPath = filePath.at(1).assign(ConfigFileName);
			auto szAnsiPath = Common::ConvertWstringToChar(configPath);
			std::ifstream configFile(szAnsiPath);

			if (wcscmp(targetImage, imageName.data()) == 0 && configFile.good()) {
				configFile.close();
				retval.assign(configPath);
				found = TRUE;
			}
		}
		return ret;
	});

	return retval;
}

HANDLE JNIBridge::Config::ConfigReader::GetThreadToken() {
	HANDLE retval;
	auto flags = TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY;

	if (!OpenThreadToken(GetCurrentThread(), flags, FALSE, &retval)) {
		if (GetLastError() == ERROR_NO_TOKEN) {
			if (ImpersonateSelf(SecurityImpersonation) &&
				!OpenThreadToken(GetCurrentThread(), flags, FALSE, &retval))
				retval = NULL;
		}
	}
	return retval;
}

BOOL JNIBridge::Config::ConfigReader::SetPrivilege(HANDLE& hToken, LPCTSTR Privilege, BOOL bEnablePrivilege) {
	LUID luid;
	auto retval = FALSE;
	TOKEN_PRIVILEGES tp = {0};
	DWORD cb = sizeof(TOKEN_PRIVILEGES);

	if (LookupPrivilegeValue(NULL, Privilege, &luid)) {
		tp.PrivilegeCount = 1;
		tp.Privileges[0].Luid = luid;
		tp.Privileges[0].Attributes = bEnablePrivilege ? SE_PRIVILEGE_ENABLED : 0;
		AdjustTokenPrivileges(hToken, FALSE, &tp, cb, NULL, NULL);

		if (GetLastError() == ERROR_SUCCESS)
			retval = TRUE;
	}
	return retval;
}

BOOL JNIBridge::Config::ConfigReader::LocateConfigFile() {
	auto retval = FALSE;
	wchar_t buffer[MAX_PATH];
	IXMLDOMDocumentPtr pDocPtr;
	MODULEINFO moduleDetails = {0};
	HANDLE hToken = NULL, hProcess = NULL;
	DWORD nModuleCount = 0, szProcessName = 0;
	HMODULE hLoadedModules[Max_Loaded_Modules];

	if ((hToken = GetThreadToken()) != NULL && SetPrivilege(hToken, SE_DEBUG_NAME, TRUE)) {
		if ((hProcess = OpenProcess(PROCESS_ALL_ACCESS, TRUE, GetCurrentProcessId())) != NULL) {
			if ((EnumProcessModulesEx(hProcess, hLoadedModules, sizeof(hLoadedModules), &nModuleCount, LIST_MODULES_ALL)) != NULL) {
				auto modules = std::vector<HMODULE>(std::begin(hLoadedModules), std::end(hLoadedModules));

#ifdef _WIN64
				nModuleCount = Item_Count(nModuleCount) / 2;
#else
				nModuleCount = Item_Count(nModuleCount);
#endif

				auto result = QueryFullProcessImageName(GetCurrentProcess(), NULL, buffer, &szProcessName);
				auto targetImageName = Common::GetModuleName(std::wstring(buffer)).at(0);
				auto config = DoesConfigFileExist(hProcess, modules, targetImageName.c_str());

				if (!config.empty())
					retval = ParseConfigFile(config);
			}
			CloseHandle(hProcess);
		}
		SetPrivilege(hToken, SE_DEBUG_NAME, FALSE);
		CloseHandle(hToken);
	}

	return retval;
}

std::vector<JNIBridge::Structs::ConfigSetting>& JNIBridge::Config::ConfigReader::Settings_get() {
	return m_settings;
}