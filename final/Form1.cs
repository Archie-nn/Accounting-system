using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace final
{
    public partial class Form1 : Form
    {
        private AccountCollection _accounts = new AccountCollection();
        private readonly string _dataFilePath = Path.Combine(Application.StartupPath, "finance_data.txt");
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. 初始化下拉選單
            cmbReportView.SelectedIndex = 0; // 預設顯示日報
            rdoExpense.Checked = true; // 預設為支出模式

            // 2. 初始化記帳類別選單
            UpdateCategorySelection();

            // 3. 初始化 ListView 欄位
            lvwAccounts.Columns.Add("日期時間", 112);
            lvwAccounts.Columns.Add("類型", 40);
            lvwAccounts.Columns.Add("類別", 64);
            lvwAccounts.Columns.Add("金額", 56);
            lvwAccounts.Columns.Add("備註", 120);
            lvwAccounts.View = View.Details;
            lvwAccounts.FullRowSelect = true;

            // 4. 自動讀取歷史帳目資料
            if (File.Exists(_dataFilePath))
            {
                _accounts.LoadFromFile(_dataFilePath);
            }

            // 5. 刷新報表檢視與總計金額
            RefreshReport();
        }

        private void rdoExpense_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCategorySelection();
        }

        private void UpdateCategorySelection()
        {
            cmbCategory.Items.Clear();
            if (rdoIncome.Checked)
            {
                cmbCategory.Items.AddRange(new string[] { "薪資", "獎金", "投資收益", "零用錢", "其他收入" });
            }
            else
            {
                cmbCategory.Items.AddRange(new string[] { "居住費用","交通","通訊與訂閱","保險與稅務","飲食","生活日常","娛樂交際","債務攤還","投資儲蓄", "其他支出" });
            }
            cmbCategory.SelectedIndex = 0;
        }

        /// <summary>
        /// 核心：依據條件篩選資料，並重繪更新 ListView 與總金額
        /// </summary>
        private void RefreshReport()
        {
            lvwAccounts.BeginUpdate();
            lvwAccounts.Items.Clear();

            DateTime selectedDate = dtpDate.Value;
            AccountItem[] filteredItems = null;

            // 根據選取的報表模式呼叫 Collection 的篩選函式
            switch (cmbReportView.SelectedIndex)
            {
                case 0: // 日報表
                    filteredItems = _accounts.GetDailyReport(selectedDate);
                    break;
                case 1: // 週報表
                    filteredItems = _accounts.GetWeeklyReport(selectedDate);
                    break;
                case 2: // 月報表
                    filteredItems = _accounts.GetMonthlyReport(selectedDate);
                    break;
            }

            if (filteredItems == null) filteredItems = new AccountItem[0];

            int totalIncome = 0;
            int totalExpense = 0;

            // 填入 ListView 項目
            foreach (var item in filteredItems)
            {
                ListViewItem lvi = new ListViewItem(item.Date.ToString("yyyy-MM-dd HH:mm"));
                lvi.SubItems.Add(item.Type);
                lvi.SubItems.Add(item.Category);
                lvi.SubItems.Add(item.Amount.ToString("N0")); // 加上千分位展示
                lvi.SubItems.Add(item.Note);

                // 計算當前報表小計
                if (item.Type == "收入") totalIncome += item.Amount;
                else totalExpense += item.Amount;

                lvwAccounts.Items.Add(lvi);
            }

            lvwAccounts.EndUpdate();

            // 更新結餘狀態 Label (顯示報表小計)
            int balance = totalIncome - totalExpense;
            lblSummary.Text = $"當期統計 -> 總收入: ${totalIncome:N0}  |  總支出: ${totalExpense:N0}  |  本期淨結餘: ${balance:N0}";

            // 淨結餘為負時顯示紅色字體警告
            lblSummary.ForeColor = balance >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _accounts.SaveToFile(_dataFilePath);
                MessageBox.Show("所有資料已成功儲存至內部檔案中！", "儲存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存檔案時發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("關閉程式前，是否確認將今天的記帳資料更新儲存？", "關閉確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _accounts.SaveToFile(_dataFilePath);
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true; // 取消關閉表單
            }
        }

        private void cmbReportView_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            RefreshReport();
        }

        private void dtpDate_ValueChanged_1(object sender, EventArgs e)
        {
            RefreshReport();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (numAmount.Value <= 0)
            {
                MessageBox.Show("請輸入大於 0 的有效金額！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 建立帳目物件
            AccountItem newItem = new AccountItem
            {
                Date = dtpDate.Value, // 採用 DateTimePicker 所選之日期與時間
                Type = rdoIncome.Checked ? "收入" : "支出",
                Category = cmbCategory.SelectedItem.ToString(),
                Amount = (int)numAmount.Value,
                Note = txtNote.Text.Trim().Replace("\t", " ") // 避免備註內含 Tab 破壞檔案排版
            };

            // 加入記憶體集合
            _accounts.Add(newItem);

            // 清空輸入欄位方便下一筆輸入
            numAmount.Value = 0;
            txtNote.Clear();

            // 重新整理 UI 報表
            RefreshReport();
            MessageBox.Show("帳目已成功新增！記得存檔喔。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
