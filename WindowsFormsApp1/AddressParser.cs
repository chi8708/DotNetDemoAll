using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddressParser : Form
    {
        public AddressParser()
        {
            InitializeComponent();
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            // 测试用例
            string address = "盘龙区测润悦府小区东郊路1号";
            var result = ParseAddress(address);
            MessageBox.Show($"所属县/区：{result.District}\n小区名：{result.CommunityName}");
        }

        // 方法：提取小区名和所属县/区
        public static (string District, string CommunityName) ParseAddress(string address)
        {
            // 定义正则表达式匹配模式
            // 匹配所属县/区（可选）和包含“小区”或“大厦”的小区名
            string pattern = @"(?<district>[^\s]+区|[^\s]+县)?(?<community>.+?(小区|大厦))";

            // 使用正则表达式匹配
            Match match = Regex.Match(address, pattern);

            if (match.Success)
            {
                // 提取所属县/区（如果存在）
                string district = match.Groups["district"].Value;
                if (string.IsNullOrEmpty(district))
                {
                    district = "未知"; // 如果未找到县/区，设置为默认值
                }

                // 提取小区名
                string communityName = match.Groups["community"].Value.Trim();

                return (district, communityName);
            }
            else
            {
                // 如果匹配失败，返回默认值
                return ("未知", null);
            }
        }
    }
}
