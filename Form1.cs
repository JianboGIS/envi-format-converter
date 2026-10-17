using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EnviImageConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // 初始化波段列表
            Band = new List<List<char>>();
            Band.Add(new List<char>()); // 波段1
            Band.Add(new List<char>()); // 波段2
            Band.Add(new List<char>()); // 波段3
        }

        public int Rows, Columns, Bands; // 行数、列数、波段数
        List<List<char>> Band; //存储各波段数据

        private void button1_Click_1(object sender, EventArgs e) //选择头文件
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "HDR文件 (*.hdr)|*.hdr|所有文件 (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) //读取头文件
        {
            string headerPath = textBox1.Text;
            if (!File.Exists(headerPath))
            {
                MessageBox.Show("头文件不存在！");
                return;
            }
            try
            {
                using (StreamReader sr = new StreamReader(headerPath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("samples ="))
                        {
                            string value = line.Split('=')[1].Trim();
                            textBox2.Text = value; // 显示像素列数
                            int.TryParse(value, out Columns); // 赋值给Columns
                        }
                        else if (line.StartsWith("lines ="))
                        {
                            string value = line.Split('=')[1].Trim();
                            textBox3.Text = value; // 显示像素行数
                            int.TryParse(value, out Rows); // 赋值给Rows
                        }
                        else if (line.StartsWith("bands ="))
                        {
                            string value = line.Split('=')[1].Trim();
                            textBox5.Text = value; // 显示波段个数
                            int.TryParse(value, out Bands); // 赋值给Bands
                        }
                        else if (line.StartsWith("data type ="))
                            textBox6.Text = line.Split('=')[1].Trim(); // 显示数据格式
                        else if (line.StartsWith("interleave ="))
                            textBox4.Text = line.Split('=')[1].Trim().ToUpper(); // 显示解译格式
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取头文件失败：" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e) //选择输出文件
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "DAT文件 (*.dat)|*.dat|所有文件 (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    textBox7.Text = sfd.FileName;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e) //开始转换
        {
            if (string.IsNullOrEmpty(textBox7.Text))
            {
                MessageBox.Show("请选择输出文件路径！");
                return;
            }
            try
            {
                byte[] outputData = null;
                string newInterleave = "";

                // 根据选择的格式进行转换
                if (radioButton1.Checked) // BSQ
                {
                    MessageBox.Show("数据已经是BSQ格式，无需转换");
                    return;
                }
                else if (radioButton2.Checked) // BIP
                {
                    var converter = new ImageDataConverter(Columns, Rows, Bands);
                    converter.Band = this.Band; // 关键：赋值波段数据
                    outputData = converter.BSQToBIP();
                    newInterleave = "bip";
                }
                else if (radioButton3.Checked) // BIL
                {
                    var converter = new ImageDataConverter(Columns, Rows, Bands);
                    converter.Band = this.Band; // 关键：赋值波段数据
                    outputData = converter.BSQToBIL();
                    newInterleave = "bil";
                }

                // 写入输出文件
                File.WriteAllBytes(textBox7.Text, outputData);

                // 修改并保存头文件
                if (File.Exists(textBox1.Text)) // 检查输入头文件是否存在
                {
                    EnviHeaderParser.ModifyAndSaveHeader(textBox1.Text, textBox7.Text, newInterleave);
                    MessageBox.Show("转换完成！已生成数据文件和修改后的头文件");
                }
                else
                {
                    MessageBox.Show("转换完成！但未找到输入头文件，无法生成输出头文件");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"转换失败：{ex.Message}");
            }
        }

        private void button5_Click(object sender, EventArgs e) //选择源文件
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DAT文件 (*.dat)|*.dat|所有文件 (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox8.Text = ofd.FileName;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e) //读取源文件并显示波段数据
        {
            foreach (var band in Band)
            {
                band.Clear();
            }

            string srcPath = textBox8.Text;

            if (!File.Exists(srcPath))
            {
                MessageBox.Show("源文件不存在！");
                return;
            }

            // 验证必要的输入参数
            if (!int.TryParse(textBox2.Text, out int columns) ||
                !int.TryParse(textBox3.Text, out int rows) ||
                !int.TryParse(textBox5.Text, out int bands))
            {
                MessageBox.Show("请先正确设置列数、行数和波段数！");
                return;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(srcPath);
                var bandDataProcessor = new BandDataProcessor(fileBytes, columns, rows, bands);
                switch (textBox4.Text.ToUpper())
                {
                    case "BSQ":
                        bandDataProcessor.ProcessBSQFormat();
                        break;
                    case "BIP":
                        bandDataProcessor.BIP2BSQ();
                        break;
                    case "BIL":
                        bandDataProcessor.BIL2BSQ();
                        break;
                    default:
                        MessageBox.Show("不支持的格式类型！");
                        return;
                }

                // 显示波段数据
                bandDataProcessor.DisplayBandData(textBox6.Text, richTextBox1);
                this.Band = bandDataProcessor.Band;
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取文件失败：" + ex.Message);
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e) //头文件路径变化
        {
            if (File.Exists(textBox1.Text))
            {
                textBox1.BackColor = Color.White;
            }
            else
            {
                textBox1.BackColor = Color.MistyRose;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e) //像素列数变化
        {
            if (!int.TryParse(textBox2.Text, out _))
            {
                textBox2.BackColor = Color.MistyRose;
            }
            else
            {
                textBox2.BackColor = Color.White;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e) //像素行数变化
        {
            if (!int.TryParse(textBox3.Text, out _))
            {
                textBox3.BackColor = Color.MistyRose;
            }
            else
            {
                textBox3.BackColor = Color.White;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e) //解译格式变化
        {
            // 可选：可做格式校验
        }

        private void textBox5_TextChanged(object sender, EventArgs e) //波段个数变化
        {
            if (!int.TryParse(textBox5.Text, out _))
            {
                textBox5.BackColor = Color.MistyRose;
            }
            else
            {
                textBox5.BackColor = Color.White;
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e) //数据格式变化
        {
            // 可选：可做格式校验
        }

        private void textBox7_TextChanged(object sender, EventArgs e) //输出文件路径变化
        {
            // 可选：校验路径是否合法
        }

        private void textBox8_TextChanged(object sender, EventArgs e) //源文件路径变化
        {
            // 可选：校验路径是否合法
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) //选择BSQ格式
        {
            if (radioButton1.Checked)
            {
                textBox4.Text = "BSQ";
                label8.Text = "将数据转换为BSQ格式";
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) //选择BIP格式
        {
            if (radioButton2.Checked)
            {
                textBox4.Text = "BIP";
                label8.Text = "将数据从BSQ转换为BIP格式";
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e) //选择BIL格式
        {
            if (radioButton3.Checked)
            {
                textBox4.Text = "BIL";
                label8.Text = "将数据从BSQ转换为BIL格式";
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) //显示波段数据变化
        {
            // 显示解译结果
        }
    }
}
