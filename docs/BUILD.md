# 构建与验证

- Windows 10/11，Windows PowerShell 5.1 或兼容的 PowerShell。
- 系统 .NET Framework C# 编译器（优先 Framework64，缺失时改用 Framework）。
- 首次构建需要网络下载 Microsoft.NETFramework.ReferenceAssemblies.net46 1.0.3。

从仓库根目录运行 build.ps1；-Test 启用自检。编译参数使用 /nostdlib+，明确引用 4.6 参考程序集，避免误用本机较高版本 API。

依赖下载在 build-deps/net46，临时目录 app/work，程序 app/Guji.exe。所有路径相对仓库定位，没有固定盘符要求。构建不把用户文件写到系统盘。

参考包官方地址：
https://api.nuget.org/v3-flatcontainer/microsoft.netframework.referenceassemblies.net46/1.0.3/microsoft.netframework.referenceassemblies.net46.1.0.3.nupkg

固定 SHA256：DCBB79BB3868DBFBB64C643116FAA63D888EE9ECCBA0C4E965E9992ED7C4E35D

自检会启动 WPF 测试窗口，适合已登录的 Windows 桌面会话。结果 app/work/test-report.txt，Failures=0 表示检查通过。测试设置使用独立目录，不覆盖用户 data。

scripts/package.ps1 从明确的程序、配置、素材和说明文件列表生成 dist/Guji-版本号-Windows.zip，不包含 data、work、参考程序集或源码。

如对应版本的打包目录已存在，脚本会停止，避免覆盖；可修改版本号后再打包。运行 Guji.exe --soak 可执行15秒运行检查，--endurance 可执行约95秒检查。这些检查不能替代真实鼠标、多屏与目标电脑验收。
