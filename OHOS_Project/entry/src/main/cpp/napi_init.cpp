#include "napi/native_api.h"
#include <cassert>
#include <dlfcn.h>
#include <node_api.h>

#include "napi/native_api.h"
#include <thread>
#include <uv.h>

/**
 * 返回测试
 */
static napi_value resultMsg(napi_env env, napi_callback_info info) {
    size_t argc = 2;
    napi_value args[2] = {nullptr};

    napi_get_cb_info(env, info, &argc, args, nullptr, nullptr);

    napi_valuetype valuetype0;
    napi_typeof(env, args[0], &valuetype0);

    napi_valuetype valuetype1;
    napi_typeof(env, args[1], &valuetype1);

    double value0;
    napi_get_value_double(env, args[0], &value0);

    double value1;
    napi_get_value_double(env, args[1], &value1);

    napi_value sum;
    napi_create_double(env, value0 + value1, &sum);

    return sum;
}

typedef struct CallbackContext {
    napi_env env = nullptr;
    napi_ref callbackRef = nullptr;
    int progress = 0;
} CallbackContext;

auto asyncContext = new CallbackContext();
 
// 回调ts侧函数，将进度信息通知到ts侧
static void callTS(napi_env env, napi_value jsCb, void *context, void *data) {
    CallbackContext *arg = (CallbackContext *)data;
    napi_value progress;
    napi_create_int32(arg->env, arg->progress, &progress);
    napi_call_function(arg->env, nullptr, jsCb, 1, &progress, nullptr);
}

void callBackTask() {
    // 创建线程安全函数
    napi_value workName;
    napi_create_string_utf8(asyncContext->env, "callBack", NAPI_AUTO_LENGTH, &workName);
    napi_value jsCb;
    napi_get_reference_value(asyncContext->env, asyncContext->callbackRef, &jsCb);
    napi_threadsafe_function tsfn;
    napi_create_threadsafe_function(asyncContext->env, jsCb, nullptr, workName, 0, 1, nullptr, nullptr, nullptr, callTS,
                                    &tsfn);
    while (asyncContext && asyncContext->progress < 10) {
        asyncContext->progress += 1;
        napi_acquire_threadsafe_function(tsfn);
        napi_call_threadsafe_function(tsfn, (void *)asyncContext, napi_tsfn_blocking);
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
};

/**
 * 设置回调方法
 */
static napi_value setOnCallBack(napi_env env, napi_callback_info info) {
    size_t argc = 1;
    napi_value args[1] = {nullptr};
    napi_get_cb_info(env, info, &argc, args, nullptr, nullptr);
    asyncContext->env = env;
    napi_create_reference(env, args[0], 1, &asyncContext->callbackRef);
    return nullptr;
}

/**
 * 测试数据回调到TS端展现出来
 */
static napi_value nativeToTSCallBack(napi_env env, napi_callback_info info) {

    size_t argc = 1;
    napi_value args[1] = {nullptr};

    napi_get_cb_info(env, info, &argc, args, nullptr, nullptr);

    napi_valuetype valuetype0;
    napi_typeof(env, args[0], &valuetype0);

    int value0;
    napi_get_value_int32(env, args[0], &value0);

    asyncContext->progress = value0;
    std::thread callBackThread(callBackTask);
    callBackThread.detach();
    return nullptr;
}

EXTERN_C_START
static napi_value Init(napi_env env, napi_value exports) {
    napi_property_descriptor desc[] = {
        {"resultMsg", nullptr, resultMsg, nullptr, nullptr, nullptr, napi_default, nullptr},
        {"setOnCallBack", nullptr, setOnCallBack, nullptr, nullptr, nullptr, napi_default, nullptr},
        {"nativeToTSCallBack", nullptr, nativeToTSCallBack, nullptr, nullptr, nullptr, napi_default, nullptr}};
    napi_define_properties(env, exports, sizeof(desc) / sizeof(desc[0]), desc);

    return exports;
}
EXTERN_C_END
 
// 准备模块加载相关信息，将上述Init函数与本模块名等信息记录下来。
static napi_module testModule = {
    .nm_version = 1,
    .nm_flags = 0,
    .nm_filename = nullptr,
    .nm_register_func = Init,
    .nm_modname = "entry",
    .nm_priv = ((void *)0),
    .reserved = {0},
};
#include <hilog/log.h>
#include <native_drawing/drawing_font_collection.h>
#include <native_drawing/drawing_register_font.h>
#include <native_drawing/drawing_text_typography.h>
extern "C" __attribute__((constructor)) void RegisterEntryModule(void) {
    OH_Drawing_FontConfigInfoErrorCode fontConfigInfoErrorCode; // 用于接收错误代码
    OH_Drawing_FontConfigInfo *fontConfigInfo = OH_Drawing_GetSystemFontConfigInfo(&fontConfigInfoErrorCode);
    if (fontConfigInfoErrorCode != SUCCESS_FONT_CONFIG_INFO) {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, "DrawingSample", "获取系统信息失败，错误代码为： %{public}d",
                     fontConfigInfoErrorCode);
    } // 获取系统字体配置信息示例
    if (fontConfigInfo != nullptr) {
        // 获取字体文件路径数量，打印日志
        size_t fontDirCount = fontConfigInfo->fontDirSize;
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, "DrawingSample", "字体文件路径数量为: %{public}zu\n", fontDirCount);
        // 遍历字体文件路径列表，打印日志
        for (size_t i = 0; i < fontDirCount; ++i) {
            OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, "DrawingSample", "字体文件路径为: %{public}s\n",
                         fontConfigInfo->fontDirSet[i]);
        }
        // 获取通用字体集数量，打印日志
        size_t genericCount = fontConfigInfo->fontGenericInfoSize;
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, "DrawingSample", "通用字体集数量为: %{public}zu\n", genericCount);
        // 遍历获取每个通用字体集中的字体家族名（例如 HarmonyOS Sans），打印日志
        for (size_t i = 0; i < genericCount; ++i) {
            OH_Drawing_FontGenericInfo &genericInfo = fontConfigInfo->fontGenericInfoSet[i];
            OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, "DrawingSample",
                         "获取第%{public}zu个通用字体集中的字体家族名为: %{public}s", i, genericInfo.familyName);
        }
    }
    
    auto handle = dlopen("libavalonia.so", RTLD_NOW);
    assert(handle != nullptr);
    auto func = (void (*)())dlsym(handle, "RegisterEntryModule");
    assert(func != nullptr);
    func();
}