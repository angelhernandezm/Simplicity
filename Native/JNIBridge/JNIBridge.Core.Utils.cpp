#include "stdafx.h"
#include "JNIBridge.Core.Utils.h"

using namespace std;

std::string JNIBridge::Core::Utils::Common::ConvertWstringToChar(const std::wstring& original) {
	std::string retval;
	size_t cntConverted;

	if (!original.empty()) {
		wcstombs_s(&cntConverted, nullptr, NULL, original.c_str(), original.size());
		auto buffer = std::make_unique<char[]>(cntConverted);
		memset(buffer.get(), ' ', original.size());
		wcstombs_s(&cntConverted, buffer.get(), strlen(buffer.get()) + 1, original.c_str(), original.size());
		retval.append(buffer.get());
	}

	return retval;
}

bool JNIBridge::Core::Utils::Common::DoesFileExist(const char* fileToCheck) {
	auto retval = false;
	struct stat buffer;

	if (fileToCheck != nullptr && strlen(fileToCheck) > 0)
		retval = stat(fileToCheck, &buffer) == 0;

	return retval;
}

std::wstring JNIBridge::Core::Utils::Common::ConvertCharToWstring(const char* original) {
	std::wstring retval;
	size_t cntConverted;

	if (original != nullptr && strlen(original) > 0) {
		mbstowcs_s(&cntConverted, nullptr, NULL, original, strlen(original));
		auto buffer = std::make_unique<wchar_t[]>(cntConverted);
		memset(buffer.get(), ' ', strlen(original));
		auto sizeInWords = wcslen(buffer.get());
		mbstowcs_s(&cntConverted, buffer.get(), cntConverted, original, cntConverted);
		retval.append(buffer.get());
	}

	return retval;
}

std::vector<std::wstring> JNIBridge::Core::Utils::Common::GetModuleName(const std::wstring& fullPath) {
	wchar_t szDir[_MAX_DIR];
	wchar_t szExt[_MAX_EXT];
	wchar_t szFName[_MAX_FNAME];
	wchar_t szDrive[_MAX_DRIVE];
	std::vector<std::wstring> retval;

	if (!fullPath.empty()) {
		_wsplitpath_s(fullPath.c_str(), szDrive, Array_Size(szDrive), szDir, Array_Size(szDir), szFName, Array_Size(szFName), szExt, Array_Size(szExt));
		// Item 0 contains filename (incl. Extension) and item 1 contains the path (Drive + Folder)
		retval.push_back(std::wstring(szFName).append(szExt));
		retval.push_back(std::wstring(szDrive).append(szDir));
	} else throw std::exception("fullPath cannot be empty.");

	return retval;
}