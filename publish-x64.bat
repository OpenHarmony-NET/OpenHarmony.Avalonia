cd %cd%
dotnet publish  ./Src/Entry/Entry.csproj -c Release -r linux-musl-x64 -p:PublishAot=true  -o OHOS_Project/entry/libs/x86_64
del  %cd%\OHOS_Project\entry\libs\x86_64\*.pdb
del  %cd%\OHOS_Project\entry\libs\x86_64\*.json
rmdir  /s /q %cd%\OHOS_Project\entry\libs\arm64-v8a
pause