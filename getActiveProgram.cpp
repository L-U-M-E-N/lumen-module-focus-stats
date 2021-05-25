#include <stdlib.h>
#include <iostream>
#include <node.h>
#include <windows.h>
#include "libloaderapi.h"
#include <psapi.h>
#include <tchar.h>
#include <stdio.h>
#pragma comment(lib, "psapi.lib")

namespace demo {

using v8::FunctionCallbackInfo;
using v8::Isolate;
using v8::Local;
using v8::Object;
using v8::String;
using v8::Value;

void GetActiveProgramName(const FunctionCallbackInfo<Value>& args) {
	HWND fgWin = GetForegroundWindow();
	std::wstring title(GetWindowTextLength(fgWin) + 1, L'\0');
	GetWindowTextW(fgWin, &title[0], title.size()); //note: C++11 only

	char *str = new char[title.size()];
	wcstombs(str, title.c_str(), title.size());

	/**
	 * Return
	 */
	Isolate* isolate = args.GetIsolate();
	args.GetReturnValue().Set(
		String::NewFromUtf8(
			isolate,
			str
		).ToLocalChecked()
	);
}

void GetActiveProgramExe(const FunctionCallbackInfo<Value>& args) {
	TCHAR szAppPath[MAX_PATH];			
	HWND hFG = GetForegroundWindow();
	DWORD  dwId;
	HANDLE hProc;

	GetWindowThreadProcessId(hFG, &dwId);
	hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, dwId);

	if (hProc) {
		DWORD dwSize = ARRAYSIZE(szAppPath);
		if (!QueryFullProcessImageName(hProc, 0, szAppPath, &dwSize))
		CloseHandle(hProc);
	}

	/**
	 * Return
	 */
	Isolate* isolate = args.GetIsolate();
	args.GetReturnValue().Set(
		String::NewFromUtf8(
			isolate,
			szAppPath
		).ToLocalChecked()
	);
}

void Initialize(Local<Object> exports) {
  NODE_SET_METHOD(exports, "getActiveProgramName", GetActiveProgramName);
  NODE_SET_METHOD(exports, "getActiveProgramExe", GetActiveProgramExe);
}

NODE_MODULE(NODE_GYP_MODULE_NAME, Initialize)

}  // namespace demo