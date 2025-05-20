# OpenHarmony.Avalonia

English | [简体中文](README.md)

## Usage

See [guide](https://openharmony-net.github.io/docs/docs/avalonia/introduction.html)

## Development

Folder structure:
```bash
|-- OHOS_Project             <-- DevEco Project directory
|-- OpenHarmony.Avalonia.sln <-- Visual Studio solution file
|-- README.md
|-- Src                      <-- Project source code
|   |-- Avalonia.OpenHarmony <-- Avalonia platform API implementation
|   |-- Entry                <-- Avalonia entry
                                 (register as RegisterEntryModule
                                  and initialize as XComponent)
|   |-- Example              <-- Example project
|-- ThirdParty
```

## Contributing

We encourage you to participate in the development of OpenHarmony.Avalonia! Please see [contributing guide](CONTRIBUTING.md) for how to proceed.

## Status

.NET runtime and GPU rendering have been resolved.

## 计划

- [ ] Keyboard events
- [ ] Mouse events
- [ ] Multi-window
- [x] Font
