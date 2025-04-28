cd %cd%
dotnet publish  ./Src/Entry/Entry.csproj -c Release -r linux-musl-arm64 -p:PublishAot=true  -o OHOS_Project/entry/libs/arm64-v8a
del  %cd%\OHOS_Project\entry\libs\arm64-v8a\*.pdb
del  %cd%\OHOS_Project\entry\libs\arm64-v8a\*.json
rmdir  /s /q %cd%\OHOS_Project\entry\libs\x86_64
pause