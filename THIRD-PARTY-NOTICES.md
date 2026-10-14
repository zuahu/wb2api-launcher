# Third-party notices

本仓库分发的二进制依赖及其许可证。**许可证文本以各上游仓库的 `LICENSE` 文件为准**，下面只做归集与指引。

## 界面库（本项目的能力来源）

| 组件 | 许可证 | 出处 |
| --- | --- | --- |
| **AntdUI** 2.3.2 | **Apache License 2.0** | <https://github.com/AntdUI/AntdUI> |
| **AntdUI.EmojiFluentFlat** | Apache License 2.0（AntdUI 组织下的表情资源包） | 同上（AntdUI 组织） |

主界面所有控件（`Input` / `Button` / `Checkbox` / `Label` / 主题与 DPI 适配）都由 AntdUI 提供。
AntdUI 在其官方文档《鸣谢》页明确「本项目采用 Apache License 2.0 开源协议」，因此：

- `lib\AntdUI.dll` 与 `lib\AntdUI.EmojiFluentFlat.dll` 以**未修改、未合并**的原始形式随仓库分发；
- 本项目在 `README.md` 与本文档中显著署名致谢，符合 Apache-2.0「保留版权声明与许可证文本」的要求；
- 本项目自己的源码与 `new.framework.dll` 按 MIT 提供，与 AntdUI 的 Apache-2.0 组合分发互不冲突
  （Apache-2.0 组件仍受其自身条款约束）。

完整许可文本见 AntdUI 仓库根目录的 `LICENSE` 与其官方文档的 Thanks 页。
AntdUI 自身还引用了 STTextBox（MIT）、Vanara（MIT）、ChineseCalendar（MIT）、SVG.NET（MS-PL），
详见其官方文档《鸣谢》。

## 被合并进 new.framework.dll 的第三方库

`lib\new.framework.dll` 是自研框架 6.new.framework 的合并混淆产物（见 README「关于 lib/new.framework.dll」），
它编译时引用了下列库；这些库**没有被合并进去**，仍以独立 DLL 形式随仓库分发：

| 组件 | 许可证 | 出处 |
| --- | --- | --- |
| Newtonsoft.Json | MIT | <https://github.com/JamesNK/Newtonsoft.Json> |
| log4net | Apache-2.0 | <https://logging.apache.org/log4net/> |
| BouncyCastle.Crypto | MIT | <https://www.bouncycastle.org/csharp/> |
| HtmlAgilityPack | MIT | <https://github.com/zzzprojects/html-agility-pack> |
| ICSharpCode.SharpZipLib | GPL-2.0 with classpath exception | <https://github.com/icsharpcode/SharpZipLib> |
| Downloader | 见其程序集元数据与上游仓库（随框架附带） | — |
| EventSource4Net | 见其程序集元数据与上游仓库（随框架附带） | — |
| System.Memory / System.Buffers / System.Numerics.Vectors / System.Threading.Tasks.Extensions / System.Resources.Extensions | MIT（.NET 官方仓库发布的兼容包） | dotnet 官方仓库 |

## 运行时

- .NET Framework 4.8（Windows 自带组件，需 Targeting Pack 才能编译）— <https://dotnet.microsoft.com/download/dotnet-framework/net48>

## 构建工具（不随仓库分发）

- ILMerge 3.0.41（Microsoft，MIT）— 把 5 个框架程序集合并成 `new.framework.dll`
- SmartAssembly 8（Red Gate，商业软件）— 控制流混淆、字符串加密、`SuppressIldasm`

## 上游项目

- [workbuddy2api-panel](https://github.com/linguo2625469/workbuddy2api-panel) —— 本启动器要拉起的网关。
  本仓库不含它的任何代码或二进制，也不构成对它许可状态的改变；使用它请遵循它自己的许可证与使用限制。
