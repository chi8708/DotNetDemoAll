using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public class MoneyToChinese
    {
        private static readonly string[] ChineseNumbers = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        private static readonly string[] ChineseUnits = { "", "拾", "佰", "仟" };
        private static readonly string[] ChineseSections = { "", "万", "亿", "兆" };

        public static string ConvertToChinese(decimal amount)
        {
            var orgAmount = amount;
            StringBuilder result = new StringBuilder();
            if (amount < 0)
            {
                amount = -amount;
            }

            if (amount < 0 || amount > 999999999999.99m)
                throw new ArgumentOutOfRangeException("金额超出范围");

            // 整数和小数部分分离
            long integerPart = (long)amount;
            int fractionPart = (int)((amount - integerPart) * 100);



            // 整数部分转大写
            if (integerPart == 0)
            {
                result.Append("零圆");
            }
            else
            {
                int sectionCount = 0;
                while (integerPart > 0)
                {
                    int section = (int)(integerPart % 10000);
                    if (section != 0)
                    {
                        result.Insert(0, SectionToChinese(section) + ChineseSections[sectionCount]);
                    }
                    else if (result.Length > 0 && result[0] != '零')
                    {
                        result.Insert(0, "零");
                    }

                    integerPart /= 10000;
                    sectionCount++;
                }

                result.Append("圆");
            }


            if (fractionPart != 0)
            {
                int jiao = fractionPart / 10;
                int fen = fractionPart % 10;

                if (jiao > 0)
                {
                    result.Append(ChineseNumbers[jiao] + "角");
                }

                if (fen > 0)
                {
                    result.Append(ChineseNumbers[fen] + "分");
                }
            }

            var amountStr = amount.ToString("0.00");
            if (amountStr.Substring(amountStr.Length - 1) == "0")
            {
                result.Append("整");
            }

            if (orgAmount < 0)
            {
                result.Insert(0, "（负数）");
            }
            return result.Replace("零圆整", "圆整").Replace("零万圆整", "万圆整").ToString();
        }

        private static string SectionToChinese(int section)
        {
            StringBuilder sectionResult = new StringBuilder();
            int unitPos = 0;
            bool zeroFlag = false;

            while (section > 0)
            {
                int digit = section % 10;
                if (digit == 0)
                {
                    zeroFlag = true;
                }
                else
                {
                    if (zeroFlag)
                    {
                        sectionResult.Insert(0, "零");
                        zeroFlag = false;
                    }
                    sectionResult.Insert(0, ChineseNumbers[digit] + ChineseUnits[unitPos]);
                }

                unitPos++;
                section /= 10;
            }

            return sectionResult.ToString();
        }


        #region 方法2 人民币小写金额转大写金额 推荐
        /// <summary>
        /// 小写金额转大写金额
        /// </summary>
        /// <param name="Money">接收需要转换的小写金额</param>
        /// <returns>返回大写金额</returns>
        public static string ConvertMoney(Decimal Money)
        {
            //金额转换程序
            string MoneyNum = "";//记录小写金额字符串[输入参数]
            string MoneyStr = "";//记录大写金额字符串[输出参数]
            string BNumStr = "零壹贰叁肆伍陆柒捌玖";//模
            string UnitStr = "万仟佰拾亿仟佰拾万仟佰拾圆角分厘毫";//模
            bool minus = Money < 0;
            if (minus) Money = Math.Abs(Money);
            MoneyNum = ((long)(Money * 10000)).ToString();
            for (int i = 0; i < MoneyNum.Length; i++)
            {
                string DVar = "";//记录生成的单个字符(大写)
                string UnitVar = "";//记录截取的单位
                for (int n = 0; n < 10; n++)
                {
                    //对比后生成单个字符(大写)
                    if (Convert.ToInt32(MoneyNum.Substring(i, 1)) == n)
                    {
                        DVar = BNumStr.Substring(n, 1);//取出单个大写字符
                        //给生成的单个大写字符加单位
                        UnitVar = UnitStr.Substring(17 - (MoneyNum.Length)).Substring(i, 1);
                        n = 10;//退出循环
                    }
                }
                //生成大写金额字符串
                MoneyStr = MoneyStr + DVar + UnitVar;
            }
            //二次处理大写金额字符串
            MoneyStr = MoneyStr + "整";
            while (MoneyStr.Contains("零分") || MoneyStr.Contains("零角") || MoneyStr.Contains("零佰") || MoneyStr.Contains("零仟")
                || MoneyStr.Contains("零万") || MoneyStr.Contains("零亿") || MoneyStr.Contains("零零") || MoneyStr.Contains("零圆")
                || MoneyStr.Contains("亿万") || MoneyStr.Contains("零整") || MoneyStr.Contains("分整") || MoneyStr.Contains("厘整") || MoneyStr.Contains("毫整"))
            {

                MoneyStr = MoneyStr.Replace("零毫", "零");
                MoneyStr = MoneyStr.Replace("零厘", "零");


                MoneyStr = MoneyStr.Replace("零分", "零");
                MoneyStr = MoneyStr.Replace("零角", "零");
                MoneyStr = MoneyStr.Replace("零拾", "零");
                MoneyStr = MoneyStr.Replace("零佰", "零");
                MoneyStr = MoneyStr.Replace("零仟", "零");
                MoneyStr = MoneyStr.Replace("零万", "万");
                MoneyStr = MoneyStr.Replace("零亿", "亿");
                MoneyStr = MoneyStr.Replace("亿万", "亿");
                MoneyStr = MoneyStr.Replace("零零", "零");
                MoneyStr = MoneyStr.Replace("零圆", "圆零");
                MoneyStr = MoneyStr.Replace("零整", "整");
                MoneyStr = MoneyStr.Replace("角整", "角");
                MoneyStr = MoneyStr.Replace("分整", "分");
                MoneyStr = MoneyStr.Replace("厘整", "厘");
                MoneyStr = MoneyStr.Replace("毫整", "毫");
                MoneyStr = MoneyStr.Replace("圆零", "圆");





            }
            if (MoneyStr == "整" || MoneyStr == "")
            {
                MoneyStr = "零圆整";
            }
            if (minus) MoneyStr = "负" + MoneyStr;
            if (Regex.IsMatch(MoneyStr, @"圆零[壹贰叁肆伍陆柒捌玖]角"))
            {
                MoneyStr = MoneyStr.Replace("圆零", "圆");
            }

            return MoneyStr;
        }
        #endregion
    }


}
