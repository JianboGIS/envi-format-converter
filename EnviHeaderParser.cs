using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnviImageConverter
{
    /// <summary>
    /// ENVI头文件解析与生成工具类
    /// </summary>
    internal class EnviHeaderParser
    {
        /// <summary>
        /// 修改ENVI头文件中的interleave字段，并保存为与输出数据文件同名的头文件，扩展名为.hdr
        /// </summary>
        /// <param name="sourceHeaderPath">原始头文件路径</param>
        /// <param name="outputDataPath">输出数据文件路径（用于生成新头文件名）</param>
        /// <param name="newInterleave">新的interleave值（如"bsq"、"bil"、"bip"）</param>
        public static void ModifyAndSaveHeader(string sourceHeaderPath, string outputDataPath, string newInterleave)
        {
            try
            {
                // 生成输出头文件路径（与数据文件同名，扩展名为.hdr）
                string outputHeaderPath = Path.ChangeExtension(outputDataPath, ".hdr");

                // 读取原始头文件内容
                string[] headerLines = File.ReadAllLines(sourceHeaderPath);

                // 查找并修改interleave字段
                for (int i = 0; i < headerLines.Length; i++)//查找并修改interleave字段
                {
                    if (headerLines[i].StartsWith("interleave =", StringComparison.OrdinalIgnoreCase))//忽略大小写比较
                    {
                        headerLines[i] = $"interleave = {newInterleave}";//修改为新的interleave值
                    }
                }
                // 写入新头文件
                File.WriteAllLines(outputHeaderPath, headerLines);
            }
            catch (Exception ex)
            {
                // 异常时弹窗提示
                MessageBox.Show($"头文件修改失败: {ex.Message}");
            }
        }
    }
}
