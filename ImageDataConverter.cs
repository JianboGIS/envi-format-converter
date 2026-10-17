using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviImageConverter
{
    /// <summary>
    /// 图像数据格式转换类：把已按 BSQ 组织好的波段数据重排为 BIP / BIL
    /// </summary>
    internal class ImageDataConverter
    {
        // 新增：用于存储各波段数据的属性
        public List<List<char>> Band { get; set; }
        public int columns;
        public int rows;
        public int bands;
        public int totalPixels;// 每个波段的像素总数

        public ImageDataConverter(int columns, int rows, int bands)
        {
            this.columns = columns;
            this.rows = rows;
            this.bands = bands;
            this.totalPixels = columns * rows; // 每个波段的像素总数
            this.Band = new List<List<char>>();
        }

        public byte[] BSQToBIP()// BSQ 转 BIP 方法
        {
            byte[] bipData = new byte[totalPixels * bands];

            for (int pixel = 0; pixel < totalPixels; pixel++)// 遍历每个像素
            {
                for (int band = 0; band < bands; band++)// 遍历每个波段
                {
                    if (band < Band.Count && pixel < Band[band].Count)// 确保索引不越界
                    {
                        bipData[pixel * bands + band] = (byte)Band[band][pixel];// 计算BIP格式中的索引并赋值
                    }
                }
            }

            return bipData;
        }
        public byte[] BSQToBIL()// BSQ 转 BIL 方法
        {
            byte[] bilData = new byte[totalPixels * bands];

            for (int row = 0; row < rows; row++)
            {
                for (int band = 0; band < bands; band++)
                {
                    for (int col = 0; col < columns; col++)
                    {
                        int pixelIndex = row * columns + col;
                        if (band < Band.Count && pixelIndex < Band[band].Count)
                        {
                            bilData[row * bands * columns + band * columns + col] = (byte)Band[band][pixelIndex];
                        }
                    }
                }
            }
            return bilData;
        }
    }
}
