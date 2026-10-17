using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnviImageConverter
{
    internal class BandDataProcessor// 用于处理和转换波段数据的类
    {
        public List<List<char>> Band { get; private set; }
        public int columns;
        public int rows;
        public int bands;
        public int totalPixels;// 每个波段的像素总数
        public byte[] data;

        // 构造函数，初始化波段列表
        public BandDataProcessor(byte[] data, int columns, int rows, int bands)
        {
            this.data = data;
            this.columns = columns;
            this.rows = rows;
            this.bands = bands;
            this.totalPixels = columns * rows; // 每个波段的像素总数

            // 按波段个数动态建立波段列表
            this.Band = new List<List<char>>();
            for (int i = 0; i < bands; i++)
            {
                this.Band.Add(new List<char>());
            }
        }

        public void ProcessBSQFormat()// 处理BSQ格式数据
        {
            // 确保有足够的波段存储空间
            while (Band.Count < bands)
            {
                Band.Add(new List<char>());
            }

            // 读取每个波段的数据
            for (int bandIndex = 0; bandIndex < bands; bandIndex++)
            {
                int startIndex = bandIndex * totalPixels;// 计算当前波段在数据中的起始位置
                if (startIndex + totalPixels > data.Length)
                {
                    break; // 防止数组越界
                }
                Band[bandIndex].Clear();
                for (int i = 0; i < totalPixels; i++)
                {
                    Band[bandIndex].Add((char)data[startIndex + i]);
                }
            }
        }
        public void BIP2BSQ()// BIP转BSQ
        {
            // 确保有足够的波段存储空间
            while (Band.Count < bands)
            {
                Band.Add(new List<char>());
            }

            int totalPixels = columns * rows;
            int bytesPerPixel = 1; // 假设每个像素1字节

            // 验证数据长度是否足够
            if (data.Length < totalPixels * bands * bytesPerPixel)
            {
                MessageBox.Show("数据长度不足，无法完成BIP格式解析！");
                return;
            }

            // BIP转BSQ的核心算法
            for (int col = 0; col < columns; col++)  // 外循环n次(列数)
            {
                for (int row = 0; row < rows; row++) // 内循环m次(行数)
                {
                    int pixelIndex = row * columns + col; // 当前像素的线性索引

                    for (int band = 0; band < bands; band++)
                    {
                        // 计算BIP格式中的字节位置
                        int bipIndex = (pixelIndex * bands) + band;

                        if (bipIndex < data.Length)
                        {
                            Band[band].Add((char)data[bipIndex]);
                        }
                    }
                }
            }
        }
        public void BIL2BSQ()// BIL转BSQ
        {
            // 清空并初始化波段列表（动态适应波段数）
            Band.Clear();
            for (int i = 0; i < bands; i++)
            {
                Band.Add(new List<char>());
            }

            int bytesPerPixel = 1; // 假设每个像素1字节
            int lineSize = columns * bytesPerPixel; // 每波段每行的数据量
            int totalLineSize = lineSize * bands;   // 每行所有波段的总数据量

            // 验证数据长度
            if (data.Length < rows * totalLineSize)
            {
                MessageBox.Show("数据长度不足！");
                return;
            }

            // 按行处理（你的直接索引思路）
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // 动态处理所有波段（不再硬编码3个）
                    for (int band = 0; band < bands; band++)
                    {
                        int dataIndex = col + row * totalLineSize + band * columns;
                        if (dataIndex < data.Length)
                        {
                            Band[band].Add((char)data[dataIndex]);
                        }
                        else
                        {
                            Band[band].Add('\0'); // 越界时填充空字符
                        }
                    }
                }
            }
        }
        public void DisplayBandData(string dataFormat, RichTextBox richTextBox)// 在RichTextBox中显示波段数据预览
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"数据格式: {dataFormat.ToUpper()}");
            sb.AppendLine($"波段数据预览（前{columns * 5}个像素，十进制显示）：");

            for (int bandIndex = 0; bandIndex < Band.Count; bandIndex++)
            {
                if (Band[bandIndex].Count == 0) continue;

                sb.AppendLine($"\n波段 {bandIndex + 1}:");
                int displayCount = Math.Min(columns * 5, Band[bandIndex].Count);

                for (int i = 0; i < displayCount; i++)
                {
                    int decimalValue = (int)Band[bandIndex][i];
                    sb.Append(decimalValue.ToString().PadLeft(4) + " ");

                    if ((i + 1) % columns == 0)
                    {
                        sb.AppendLine();
                    }
                }
            }
            richTextBox.Text = sb.ToString();
        }
    }
}
