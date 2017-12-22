#include <iostream>
#include <string>


namespace JNIBridge {
	namespace Structs {
		typedef struct _ConfigSetting {
		protected:
			std::string m_name;
			std::string m_value;

		public:
			_ConfigSetting(std::string name, std::string value) {
				m_name = name;
				m_value = value;
			}

			std::string Name_get() const {
				return m_name;
			}

			std::string Value_get() const {
				return m_value;
			}

		} ConfigSetting;
	}
}
