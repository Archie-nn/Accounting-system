using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final
{
    public class AccountItem
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }       // "收入" 或 "支出"
        public string Category { get; set; }   // 例如：餐飲、薪資、娛樂等
        public int Amount { get; set; }
        public string Note { get; set; }

        public AccountItem() { }

        /// <summary>
        /// 解析檔案中以 Tab 分隔的字串行
        /// </summary>
        public AccountItem(string line)
        {
            string[] tokens = line.Split('\t');
            if (tokens.Length >= 5)
            {
                Date = DateTime.Parse(tokens[0]);
                Type = tokens[1];
                Category = tokens[2];
                Amount = int.Parse(tokens[3]);
                Note = tokens[4];
            }
        }

        /// <summary>
        /// 轉回符合檔案儲存的 Tab 分隔格式
        /// </summary>
        public string ToLineString()
        {
            return $"{Date:yyyy-MM-dd HH:mm:ss}\t{Type}\t{Category}\t{Amount}\t{Note}";
        }
    }
}

