#include "napi/native_api.h"
#include <cassert>
#include <dlfcn.h>
#include <node_api.h>
#include "napi/native_api.h"
#include <thread>
#include <hilog/log.h>
#include <native_drawing/drawing_text_typography.h>

extern "C" __attribute__((constructor)) void RegisterEntryModule(void) {
    auto handle = dlopen("libavalonia.so", RTLD_NOW);
    assert(handle != nullptr);
    auto func = (void (*)())dlsym(handle, "RegisterEntryModule");
    assert(func != nullptr);
    func();
}