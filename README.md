# OpenHarmony.Avalonia

简体中文 | [English](README.en.md)

## 使用

请参考[文档](https://openharmony-net.github.io/docs/docs/avalonia/introduction.html)

## 开发

目录结构
```bash
|-- OHOS_Project             <-- DevEco项目目录
|-- OpenHarmony.Avalonia.sln <-- 解决方案文件
|-- README.md
|-- Src                      <-- 项目源代码
|   |-- Avalonia.OpenHarmony <-- Avalonia 的平台API的实现
|   |-- Entry                <-- Avalonia 的入口
                                 (注册为RegisterEntryModule，
                                  以及注册为XComponent的初始化操作)
|   |-- Example              <-- 示例项目
|-- ThirdParty
```

## 如何参与项目

我们鼓励您参与 OpenHarmony.Avalonia 的开发！请查看[贡献指南](CONTRIBUTING.md)以了解如何进行。

## 状态
.NET运行时和GPU渲染均已解决

## 计划

- [ ] 键盘事件
- [ ] 鼠标事件
- [ ] 多窗口
- [x] 字体
