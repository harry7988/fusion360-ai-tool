# Fusion CLI

A native .NET 10 AOT command-line tool for controlling Autodesk Fusion 360.

通过 .NET 10 AOT 原生命令行工具控制 Autodesk Fusion 360。

## Architecture / 架构

```
Terminal (fusion)  ────HTTP────▶  Fusion 360 Python Add-in (FusionMCP.py)
   .NET 10 AOT                          localhost:7432
```

## Prerequisites / 前置条件

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Autodesk Fusion 360
- [FusionMCP add-in](https://github.com/user/fusion-mcp) installed and running

## Quick Start / 快速开始

```bash
# Build / 构建
dotnet build

# Run in dev mode / 开发模式运行
dotnet run -- status
dotnet run -- bodies

# AOT native publish (no .NET runtime needed) / AOT 原生发布（无需 .NET 运行时）
dotnet publish -c Release -r osx-arm64
./bin/Release/net10.0/osx-arm64/publish/fusion status
```

## Install Add-in / 安装插件

The CLI needs the FusionMCP add-in running inside Fusion 360. Use the `install` command to deploy it:
CLI 需要在 Fusion 360 内部运行 FusionMCP 插件。使用 `install` 命令一键部署：

```bash
# Install from a local fusion-mcp directory / 从本地 fusion-mcp 目录安装
dotnet run -- install ../fusion-mcp

# Or after AOT publish / AOT 发布后
fusion install ../fusion-mcp
```

After installation, follow the prompts to enable the add-in in Fusion 360:
安装完成后，按提示在 Fusion 360 中启用插件：

1. Open Fusion 360 / 打开 Fusion 360
2. Press **Shift+S** to open the Scripts and Add-Ins panel / 按 **Shift+S** 打开脚本和插件面板
3. Switch to the **Add-Ins** tab / 切换到 **Add-Ins** 标签页
4. Click **+** to add the FusionMCP folder / 点击 **+** 添加 FusionMCP 文件夹
5. Select **FusionMCP** and click **Run** / 选中 **FusionMCP** 并点击 **Run**
6. Wait for the popup: "FusionMCP bridge is running on port 7432" / 等待弹出 "FusionMCP bridge is running on port 7432"

Then verify connection / 然后验证连接：

```bash
fusion status
```

## Usage / 使用方法

Start the FusionMCP add-in in Fusion 360, then run in terminal:
启动 Fusion 360 中的 FusionMCP 插件后，在终端运行：

```bash
# Check connection / 检查连接
fusion status

# View design info / 查看设计信息
fusion info
fusion bodies
fusion faces 0
fusion edges 0
fusion timeline

# Create sketch & extrude / 创建草图并拉伸
fusion sketch XY MySketch
fusion circle 0 0 5
fusion finish-sketch
fusion extrude 2.0

# Fillet edges / 倒圆角
fusion fillet 0 0.5 0 1 2 3

# Undo / 撤销
fusion undo 2

# Export / 导出
fusion stl
fusion step

# Screenshot / 截图
fusion screenshot ~/Desktop/shot.png
```

## All Commands / 全部命令

### Status & Info / 状态 & 信息

| Command / 命令 | Description / 说明 |
|------|------|
| `status` | Check connection to Fusion 360 / 检查与 Fusion 360 的连接 |
| `info` | Get full design info / 获取完整设计信息 |
| `bodies` | List all bodies / 列出所有实体 |
| `faces [body]` | List faces of a body / 列出实体面信息 |
| `edges [body]` | List edges of a body / 列出实体边信息 |
| `sketch-info [sketch]` | Get sketch details / 获取草图详情 |
| `timeline` | Get feature timeline / 获取特征时间线 |
| `measure [body]` | Measure body (dimensions, volume) / 测量实体 |
| `distance <e1> <e2>` | Measure distance between entities / 测量距离 |

### Document / 文档

| Command / 命令 | Description / 说明 |
|------|------|
| `new-doc [name]` | Create new document / 创建新文档 |
| `clear` | Clear current design / 清空当前设计 |

### Sketch / 草图

| Command / 命令 | Description / 说明 |
|------|------|
| `sketch [plane] [name]` | Create sketch on plane (XY/XZ/YZ) / 在平面创建草图 |
| `sketch-face <body> <face>` | Create sketch on body face / 在实体面上创建草图 |
| `finish-sketch [sketch]` | Finish/deactivate sketch / 完成/停用草图 |
| `delete-sketch [sketch]` | Delete sketch / 删除草图 |
| `rect <x1> <y1> <x2> <y2>` | Draw rectangle / 画矩形 |
| `center-rect <cx> <cy> <w> <h>` | Draw center rectangle / 画中心矩形 |
| `circle <cx> <cy> <r>` | Draw circle / 画圆 |
| `line <x1> <y1> <x2> <y2>` | Draw line / 画线 |
| `arc <cx> <cy> <r> <start> <sweep>` | Draw arc / 画弧 |
| `polygon <cx> <cy> <r> <sides>` | Draw polygon / 画多边形 |
| `ellipse <cx> <cy> <rx> <ry>` | Draw ellipse / 画椭圆 |
| `spline <x1 y1 x2 y2 ...>` | Draw spline / 画样条曲线 |
| `slot <x1> <y1> <x2> <y2> <w>` | Draw slot / 画槽 |
| `text <str> [x] [y] [h]` | Draw text / 画文字 |
| `sketch-fillet <l1> <l2> <r>` | Sketch fillet / 草图圆角 |
| `offset <dist> <dx> <dy>` | Offset sketch / 偏移草图 |
| `mirror <axis>` | Mirror sketch / 镜像草图 |
| `pattern <xn> <yn> <xs> <ys>` | Rectangular pattern / 矩形阵列草图 |

### Constraints / 约束

| Command / 命令 | Description / 说明 |
|------|------|
| `constraint <type> [e1 e2]` | Add geometric constraint / 添加几何约束 |
| `dimension <type> <val> [e1 e2]` | Add dimensional constraint / 添加尺寸约束 |

### Features / 特征

| Command / 命令 | Description / 说明 |
|------|------|
| `extrude <dist> [op] [prof]` | Extrude sketch / 拉伸 |
| `revolve <angle> [prof] [axis]` | Revolve profile / 旋转 |
| `loft <i1> <i2> ...` | Loft through profiles / 放样 |
| `sweep <profile> <path>` | Sweep along path / 扫掠 |
| `helix <pitch> <height>` | Create helix/coil / 创建螺旋 |
| `pipe <path> <size> [wall]` | Create pipe / 创建管道 |
| `hole <body> <face> <x> <y> <dia> <depth>` | Create hole / 创建孔 |
| `shell <body> <thick> [faces...]` | Shell body / 抽壳 |
| `fillet <body> <r> [edges...]` | Fillet edges / 倒圆角 |
| `chamfer <body> <d> [edges...]` | Chamfer edges / 倒角 |
| `mirror-body <body> [plane]` | Mirror body / 镜像实体 |
| `pattern-body <body> <xn> <xs>` | Rectangular pattern / 矩形阵列 |
| `circular <body> <count> [axis]` | Circular pattern / 环形阵列 |
| `combine <target> <tools...> [op]` | Boolean operation / 布尔运算 |
| `scale <body> <sx> [sy] [sz]` | Scale body / 缩放 |
| `move <body> <dx> <dy> <dz>` | Move body / 移动 |
| `rotate <body> [axis] [angle]` | Rotate body / 旋转 |
| `press-pull <body> <face> <dist>` | Press/pull face / 按/拉面 |
| `thicken <thickness>` | Thicken sketch / 加厚 |
| `draft <body> <face> <angle>` | Draft face / 拔模 |
| `thread <body> <face>` | Add thread / 添加螺纹 |

### Assembly / 装配

| Command / 命令 | Description / 说明 |
|------|------|
| `component [name]` | Create component / 创建组件 |
| `move-to-comp <body> <comp>` | Move body to component / 移动实体到组件 |
| `joint <c1> <c2> <type>` | Create joint / 创建关节 |
| `as-built <c1> <c2> <type>` | Create as-built joint / 创建就位关节 |

### Body Management / 实体管理

| Command / 命令 | Description / 说明 |
|------|------|
| `delete [body]` | Delete body / 删除实体 |
| `rename <body> <name>` | Rename body / 重命名实体 |
| `copy [body] [name] [dx dy dz]` | Copy body / 复制实体 |
| `visibility [body]` | Toggle visibility / 切换可见性 |

### Construction / 构造几何

| Command / 命令 | Description / 说明 |
|------|------|
| `plane [base] [offset]` | Add construction plane / 添加构造平面 |
| `axis [type] [body]` | Add construction axis / 添加构造轴 |

### Parameters / 参数

| Command / 命令 | Description / 说明 |
|------|------|
| `params` | List parameters / 列出所有参数 |
| `add-param <name> <val> [unit]` | Add parameter / 添加参数 |
| `set-param <name> <val>` | Update parameter / 更新参数 |

### Appearance / 外观

| Command / 命令 | Description / 说明 |
|------|------|
| `appearances [search]` | List appearances / 列出可用外观 |
| `apply <body> <appearance>` | Apply appearance / 应用外观 |
| `color <body> <r> <g> <b> [a]` | Set RGB color / 设置颜色 |

### Export / 导出

| Command / 命令 | Description / 说明 |
|------|------|
| `stl [path]` | Export as STL / 导出 STL |
| `step [path]` | Export as STEP / 导出 STEP |
| `export-3mf [path]` | Export as 3MF / 导出 3MF |
| `f3d [path]` | Export as F3D / 导出 F3D |
| `screenshot [path] [w] [h]` | Capture screenshot / 截图 |

### History & Script / 历史 & 脚本

| Command / 命令 | Description / 说明 |
|------|------|
| `undo [steps]` | Undo features / 撤销 |
| `redo [steps]` | Redo / 重做 |
| `save [desc]` | Save design / 保存 |
| `save-as <name> [desc]` | Save as new document / 另存为 |
| `script <code>` | Execute Python in Fusion / 在 Fusion 中执行脚本 |

## Project Structure / 项目结构

```
FusionCli/
├── FusionCli.csproj         # .NET 10 project config (AOT) / 项目配置
├── Program.cs               # Entry point / 入口点
├── FusionClient.cs          # HTTP client (AOT-safe JSON) / HTTP 客户端
├── CommandDispatcher.cs     # 70+ command dispatch / 命令分发
└── JsonContext.cs            # JSON source generator / JSON 源生成器
```

## Tech Stack / 技术栈

- **.NET 10** + AOT native compilation / AOT 原生编译
- **Zero dependencies** — no third-party libraries / 零外部依赖
- Manual JSON serialization — fully AOT compatible / 手动 JSON 序列化，完全 AOT 兼容
- Produces a ~5.5MB native binary, no runtime required / 产物约 5.5MB，无需运行时

## Cross-Platform Publish / 跨平台发布

```bash
# macOS ARM (Apple Silicon)
dotnet publish -c Release -r osx-arm64

# macOS Intel
dotnet publish -c Release -r osx-x64

# Windows 64-bit
dotnet publish -c Release -r win-x64

# Linux 64-bit
dotnet publish -c Release -r linux-x64
```

## License / 许可证

MIT

## Acknowledgments / 致谢

The Fusion 360 add-in (`plugin/FusionMCP.py`) is derived from the [fusion-mcp](https://github.com/Anonimus124/fusion-mcp) project. It acts as an HTTP bridge running inside Fusion 360, exposing the full Fusion API as JSON commands over `localhost:7432`. This CLI communicates with that bridge to provide a native command-line experience.

Fusion 360 插件（`plugin/FusionMCP.py`）源自 [fusion-mcp](https://github.com/Anonimus124/fusion-mcp) 项目。该插件作为 HTTP 桥接运行在 Fusion 360 内部，将完整的 Fusion API 以 JSON 命令方式暴露在 `localhost:7432`。本 CLI 通过与该桥接通信，提供原生命令行体验。

Key contributions from the original project / 原项目的主要贡献：
- Complete Fusion 360 API command dispatcher / 完整的 Fusion 360 API 命令分发器
- Sketch, feature, assembly, export, and parameter operations / 草图、特征、装配、导出和参数操作
- Thread-safe event handling via custom events / 通过自定义事件实现线程安全的事件处理
- Screenshot capture with base64 encoding / 带 base64 编码的截图功能
