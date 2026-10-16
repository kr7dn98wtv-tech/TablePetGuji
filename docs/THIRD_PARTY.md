# 第三方与构建依赖说明

本清单区分工作室项目内容、AI辅助内容及第三方依赖，不将第三方权利归于噜噜或噜噜工作室。

## Microsoft .NET Framework

程序以.NET Framework 4.6为API基线，使用Windows已有的C#编译器、WPF、Windows Forms、System.Drawing等系统组件。仓库与当前便携包不包含或重新分发.NET Framework运行环境及系统字体文件。

这些组件及运行环境属于其各自权利人，按微软相应条款使用。

## 编译参考程序集

- 包名：Microsoft.NETFramework.ReferenceAssemblies.net46
- 固定版本：1.0.3
- 用途：仅编译时引用；不随当前便携包交付。
- 来源：[微软NuGet包页面](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net46/1.0.3)
- 固定包SHA256：DCBB79BB3868DBFBB64C643116FAA63D888EE9ECCBA0C4E965E9992ED7C4E35D
- 包内nuspec标注：© Microsoft Corporation. All rights reserved.
- 包内nuspec提供的许可链接：[Microsoft/dotnet LICENSE](https://github.com/Microsoft/dotnet/blob/master/LICENSE)

上面的许可链接按包元数据原样记录，不据此为本项目套用相同许可证。依赖恢复脚本保留下载包内容。

## 美术与文档素材

咕叽的身体、头饰、翅膀、包包、服装、食物和道具图集使用AI辅助生成，并由项目代码完成切片、定位和局部变形。作者的设计选择、修改与编排和AI原始输出应分别识别，不能统一宣称所有原始输出均享有独占著作权。

文档中的功能截图来自程序实际绘制；新增参考素材或外部图库内容时，需另外核实来源、使用权限和署名要求。

## 后续新增依赖

引入新的库、字体文件、音效或图片时，发布前补充名称、版本、来源、适用条款及是否随包分发。此清单不替代第三方原始条款。
