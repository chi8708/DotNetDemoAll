using System;
using System.Text;

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
            if (amount<0)
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


            if (fractionPart!=0)
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

            if (orgAmount<0)
            {
                result.Insert(0, "（负数）");
            }
            return result.ToString();
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
    }

}
