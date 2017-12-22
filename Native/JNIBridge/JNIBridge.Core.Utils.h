#include <iostream>
#include <vector>
#include <memory>
#include <sys/stat.h>

namespace JNIBridge {
	namespace Core {
		namespace Utils {
			class Common {
			public:
				static bool DoesFileExist(const char* fileToCheck);
				static std::wstring ConvertCharToWstring(const char* original);
				static std::string ConvertWstringToChar(const std::wstring& original);
				static std::vector<std::wstring> GetModuleName(const std::wstring& fullPath);
			};
		}
	}
}