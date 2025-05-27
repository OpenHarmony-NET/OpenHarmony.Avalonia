#include "napi/native_api.h"
#include <cassert>
#include <cstdint>
#include <dlfcn.h>
#include <node_api.h>
#include "napi/native_api.h"
#include <thread>
#include <hilog/log.h>
#include <native_drawing/drawing_text_typography.h>
#include "unistd.h"

#undef LOG_DOMAIN
#undef LOG_TAG
#define LOG_DOMAIN 0x0 // 全局domain宏，标识业务领域
#define LOG_TAG "CSharp" // 全局tag宏，标识模块日志tag
extern "C" __attribute__((constructor)) void RegisterEntryModule(void) {
    auto temp=getpagesize();
    void* ptr=calloc(16, 1);
    OH_LOG_INFO(LOG_APP,"鸿蒙内存页大小：%s",temp);
    auto handle = dlopen("libavalonia.so", RTLD_NOW);
    assert(handle != nullptr);
    auto func = (void (*)())dlsym(handle, "RegisterEntryModule");
    auto fcalloc=dlsym(handle,"SystemNative_Calloc");
    free(ptr);
    assert(func != nullptr);
    func();
}