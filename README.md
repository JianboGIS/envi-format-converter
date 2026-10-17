# 遥感图像格式转换工具

> ENVI 影像三种存储布局 **BSQ / BIL / BIP** 互转 + 头文件 `interleave` 字段同步改写

C# WinForms 桌面程序（.NET Framework 4.7.2 / Visual Studio 2022）。

本工具读入 ENVI 头文件与影像数据文件：解析 `samples / lines / bands / data type / interleave`，
在界面中预览各波段数据，再把 BSQ 数据重排为 BIP 或 BIL 写出，
并生成与新数据文件同名的 `.hdr`（`interleave` 字段同步更新）。

---

## 功能

| 界面元素 | 方法 | 说明 |
| --- | --- | --- |
| **数据输入 → 输入头文件 → 选择文件** | `button1_Click_1` | 选择 ENVI `.hdr` 头文件；路径无效时输入框变粉 |
| **数据输入 → 输入源文件 → 选择文件** | `button5_Click` | 选择影像数据文件（`.dat`） |
| **数据信息 → 自动获取** | `button2_Click` | 逐行解析头文件，回填像素列数 / 像素行数 / 波段个数 / 数据格式 / 解译格式 |
| **数据信息 → 开始解译** | `button6_Click` | 按解译格式读取源文件（BSQ / BIP / BIL 三种都支持），统一还原成 BSQ 波段列表并在下方预览 |
| **数据解译（预览框）** | `DisplayBandData` | 按波段输出前 `列数 × 5` 个像素的十进制值，每行刚好 `列数` 个 |
| **数据输出 → 选择路径** | `button3_Click` | 指定输出 `.dat` 路径 |
| **数据输出 → 开始转换** | `button4_Click` | 按单选按钮（BSQ / BIP / BIL）执行转换、写出数据文件、改写并保存同名 `.hdr` |

## 目录结构

```
遥感图像格式转换工具/
├─ 遥感图像格式转换工具.sln      ← VS2022 解决方案
├─ EnviImageConverter.csproj     ← 程序集 EnviImageConverter
├─ Program.cs                    ← 入口
├─ Form1.cs                      ← 主窗体逻辑（6 个按钮 / 8 个文本框 / 3 个单选）
├─ Form1.Designer.cs             ← 界面布局
├─ Form1.resx
├─ EnviHeaderParser.cs           ← 头文件解析与改写
├─ ImageDataConverter.cs         ← BSQ → BIP / BIL
├─ BandDataProcessor.cs          ← 读入 BSQ / BIP / BIL → 统一 BSQ 波段列表 + 预览
├─ Properties/                   ← AssemblyInfo / Resources / Settings
├─ App.config
├─ .gitignore
└─ data/
   ├─ BSQ.dat                    ← 示例影像（200×204×3，122 400 字节 = 120 KB）
   ├─ BSQ.hdr                    ← 示例头文件
   └─ reference/                 ← 用等价实现算出的参考输出，便于核对
      ├─ BIP.dat / BIP.hdr
      ├─ BIL.dat / BIL.hdr
      └─ preview_BSQ.txt         ← 预览框应有的内容
```

## 类结构

| 类 | 文件 | 职责 |
| --- | --- | --- |
| `Form1` | Form1.cs / Form1.Designer.cs | 图形界面：文件选择、参数读取、转换与显示 |
| `EnviHeaderParser` | EnviHeaderParser.cs | `ModifyAndSaveHeader()`：读原头文件 → 改 `interleave = ...` → 按输出数据文件名写出 `.hdr` |
| `ImageDataConverter` | ImageDataConverter.cs | `BSQToBIP()` / `BSQToBIL()`：把 BSQ 波段列表重排为 BIP / BIL 字节流 |
| `BandDataProcessor` | BandDataProcessor.cs | `ProcessBSQFormat()` / `BIP2BSQ()` / `BIL2BSQ()` 读入；`DisplayBandData()` 预览 |

关系：`Form1` →（读源文件）`BandDataProcessor` → 得到 `Band`（各波段像素列表）
→（转换）`ImageDataConverter` → 字节数组 →（写盘 + 改头文件）`EnviHeaderParser`。

## 三种存储布局的索引关系

设 `samples = columns`（列数）、`lines = rows`（行数）、`bands`（波段数）、
`totalPixels = columns × rows`，当前像素线性号 `p = row × columns + col`。

| 布局 | 含义 | 第 `band` 波段、像素 `p` 的字节位置 |
| --- | --- | --- |
| **BSQ** | 波段顺序（Band Sequential） | `band × totalPixels + p` |
| **BIL** | 波段按行交替（Band Interleaved by Line） | `row × bands × columns + band × columns + col` |
| **BIP** | 波段按像素交替（Band Interleaved by Pixel） | `p × bands + band` |

程序里 `char` 与字节一一对应（`(char)data[i]` / `(byte)Band[b][p]` 在 0~255 范围内无损往返），
因此波段数据用 `List<List<char>>` 承载；`data type = 1`（8 位无符号整型）时一个像素占 1 字节。

## 使用步骤

1. 用 VS2022 打开 `遥感图像格式转换工具.sln`，直接 F5 运行。
2. 「输入头文件 → 选择文件」选 `data/BSQ.hdr`；「输入源文件 → 选择文件」选 `data/BSQ.dat`。
3. 点「自动获取」→ 像素列数 200、像素行数 204、波段个数 3、数据格式 1、解译格式 BSQ。
4. 点「开始解译」→ 预览框出现三个波段各 1000 个像素的十进制值。
   可把结果与 `data/reference/preview_BSQ.txt` 比对。
5. 「需转换为」勾 **BIL**（或 BIP）→「选择路径」指到任意 `xxx.dat` →「开始转换」。
   程序会写出 `xxx.dat` 与 `xxx.hdr`（`interleave = bil`）。
6. 把生成的 `.dat` 与 `data/reference/BIL.dat` 做二进制比对，应当完全一致。

> 说明：原程序要求输入为 BSQ（选中 BSQ 会提示「数据已经是BSQ格式，无需转换」直接返回）。
> 若用 BIP / BIL 文件作为源文件，程序会先经 `BIP2BSQ` / `BIL2BSQ` 还原成 BSQ 再转换。

## 实现说明与偏差记录

下面记录本工具在实现过程中确定的若干取舍。主要涉及命名空间与程序集命名、窗体标题、
波段数据的元素类型、界面布局代码、构造函数体补全以及示例数据的生成方式。

| 项 | 常见写法 | 本工具 | 原因 |
| --- | --- | --- | --- |
| 命名空间 / 程序集 | `test` | `EnviImageConverter` | 命名与工程语义一致 |
| 窗体标题 | `Form1` | `遥感图像格式转换工具` | 同上 |
| `Band` 元素类型 | `List<List<byte>>` | 统一为 `List<List<char>>` | 各处的 `List<List<char>>` 写法互相一致；若改用 `byte`，则 `converter.Band = this.Band` 无法通过编译 |
| `Form1.Designer.cs` | 未附界面源码 | 按界面设计写出布局代码 | 界面源码需与实际界面一致 |
| 个别构造函数体 | 为折叠状态 | 按字段语义补全（`data/columns/rows/bands/totalPixels` 赋值 + 按 `bands` 初始化波段列表） | 折叠处无源码 |
| `ImageDataConverter` / `BandDataProcessor` 的 `using` | 含 `System.Windows.Forms.VisualStyles...` 两行 | 已去除 | VS 误加、与本程序无关 |
| 示例数据 | 原 `BSQ.dat`（120 KB） | 按已知规格 `200×204×3`、`data type = 1`、`interleave = bsq` 生成合成数据 122 400 字节 | 原始数据文件缺失，仅规格可考 |

界面部分保留了一个行为：单选按钮的处理函数把提示文字写进了 `label8`，
而 `label8` 正是「输出路径」标签，勾选格式后该标签文字会被替换为
「将数据从BSQ转换为BIL格式」——此现象与界面上的实际表现一致，故按原样保留。

## 已知问题（代码遗留，未改动）

`BandDataProcessor.BIP2BSQ()` 的循环是「列在外、行在内」：

```csharp
for (int col = 0; col < columns; col++)   // 外循环 n 次(列数)
    for (int row = 0; row < rows; row++)  // 内循环 m 次(行数)
    {
        int pixelIndex = row * columns + col;   // 行优先索引
        ...
    }
```

`pixelIndex` 是**行优先**索引（`row × columns + col`），但外层循环走的是**列**，
于是 `Band[band]` 中的元素实际按**列优先**顺序排列，与 `BIL2BSQ()`（行优先，正确）不对称。
后果：把 BIP 文件当源文件读入时，预览与后续转换的结果是转置的。

本工具保留了该行为（见「验证」第 4 项）。若需要修正，把两个 `for` 的顺序对调即可：

```csharp
for (int row = 0; row < rows; row++)
    for (int col = 0; col < columns; col++)
```

## 验证

本机无法编译 C#（`MSBuild.exe` 被沙箱策略拦截），因此采用**按 C# 源码逐行等价翻译为 Python**
的方式做端到端校验，对 `ProcessBSQFormat / BSQToBIP / BSQToBIL / BIP2BSQ / BIL2BSQ /
DisplayBandData / ModifyAndSaveHeader` 全部 7 个方法逐一比对（含 `char`/`byte` 往返、
`Split('=')`、`PadLeft(4)`、`% columns` 换行等细节）：

```
=== 1) BSQ 读取（ProcessBSQFormat）===
  [PASS] 波段数 = 3
  [PASS] 每波段像素数 = 40800
  [PASS] 波段1首个像素 = 45
  [PASS] 预览首行文案  数据格式: 1
  [PASS] 预览第2行文案  波段数据预览（前1000个像素，十进制显示）：
  [PASS] 预览行数 = 2 + 3*(1+1+5) = 23
=== 2) BSQ -> BIP ===
  [PASS] 长度 = 122400
  [PASS] 全量 40800 像素 BIP 布局正确
=== 3) BSQ -> BIL ===
  [PASS] 全量 BIL 布局正确
=== 4) 回读还原（BIP/BIL -> BSQ）===
  [PASS] BIL->BSQ 还原一致（行优先）
  [PASS] BIP->BSQ 元素按【列优先】排列（原代码行为）
        BIP->BSQ 是否等价于正确的行优先还原: 否 —— 原代码此处存在转置缺陷
=== 5) 头文件 interleave 字段修改（EnviHeaderParser）===
  [PASS] BIP.hdr 的 interleave = bip
  [PASS] BIL.hdr 的 interleave = bil
  [PASS] 保留 samples/lines/bands
=== 6) 规模统计 ===
  [PASS] BSQ.dat 大小 = cols*rows*bands  122400 字节 (120 KB)
```

参考输出（`data/reference/`）由同一份等价实现生成，程序跑出来的 `.dat` 应与它逐字节相同：

| 文件 | 大小 | MD5 |
| --- | --- | --- |
| `data/BSQ.dat` | 122 400 B | `e23cda3b2db7ff8818a5059f444bbe47` |
| `data/reference/BIP.dat` | 122 400 B | `3345d6240b901f9144cfb3544a6dd849` |
| `data/reference/BIL.dat` | 122 400 B | `21a48fefd849616d97f657ee6aba1f71` |
