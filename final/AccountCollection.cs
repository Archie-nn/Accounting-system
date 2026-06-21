using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final
{
    public class AccountCollection : Collection<AccountItem>
    {
        // 載入文字檔資料
        public void LoadFromFile(string filePath)
        {
            this.Clear();
            if (!File.Exists(filePath)) return;

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                this.Add(new AccountItem(line));
            }
        }

        // 儲存資料回文字檔
        public void SaveToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (AccountItem item in this)
                {
                    writer.WriteLine(item.ToLineString());
                }
            }
        }

        // --- 報表篩選邏輯 ---

        // 日報表：篩選指定日期的資料
        public AccountItem[] GetDailyReport(DateTime targetDate)
        {
            return this.Where(item => item.Date.Date == targetDate.Date).ToArray();
        }

        // 週報表：篩選與指定日期在同一個星期之內的資料（以週日為一週第一天為例）
        public AccountItem[] GetWeeklyReport(DateTime targetDate)
        {
            int diff = (7 + (targetDate.Date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            DateTime startOfWeek = targetDate.Date.AddDays(-diff);
            DateTime endOfWeek = startOfWeek.AddDays(7);

            return this.Where(item => item.Date.Date >= startOfWeek && item.Date.Date < endOfWeek).ToArray();
        }

        // 月報表：篩選指定年與月的資料
        public AccountItem[] GetMonthlyReport(DateTime targetDate)
        {
            return this.Where(item => item.Date.Year == targetDate.Year && item.Date.Month == targetDate.Month).ToArray();
        }
    }
}
