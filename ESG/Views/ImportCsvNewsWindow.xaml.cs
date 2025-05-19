using ESG.Data;
using ESG.Models;
using ESG.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class ImportCsvNewsWindow : Window
    {
        private readonly AddNewsWindow _parentWindow;
        private readonly DatabaseService _dbService;
        private string _filePath;
        public List<News> AddedOrUpdatedNews { get; private set; }
        public string StatusMessage { get; private set; }

        public ImportCsvNewsWindow(AddNewsWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            _dbService = new DatabaseService();
            AddedOrUpdatedNews = new List<News>();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Выберите CSV-файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                FilePathTextBox.Text = _filePath;
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = ""; // Очищаем предыдущее сообщение

            if (string.IsNullOrWhiteSpace(_filePath) || !File.Exists(_filePath))
            {
                StatusMessage = "Пожалуйста, выберите корректный файл.";
                StatusTextBlock.Text = StatusMessage;
                return;
            }

            try
            {
                var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
                if (lines.Length < 2)
                {
                    StatusMessage = "Файл пуст или содержит только заголовок.";
                    StatusTextBlock.Text = StatusMessage;
                    return;
                }

                var headers = lines[0].Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(h => h.Trim().ToLower())
                                    .ToArray();

                bool hasCompanyHeader = headers.Contains("компания");
                bool hasTitleHeader = headers.Contains("заголовок");
                bool hasDateHeader = headers.Contains("дата");
                bool hasSourceHeader = headers.Contains("источник");

                if (!hasCompanyHeader || !hasTitleHeader || !hasDateHeader || !hasSourceHeader)
                {
                    StatusMessage = "Отсутствуют обязательные столбцы: 'компания', 'заголовок', 'дата', 'источник'.";
                    StatusTextBlock.Text = StatusMessage;
                    return;
                }

                int companyIndex = Array.IndexOf(headers, "компания");
                int titleIndex = Array.IndexOf(headers, "заголовок");
                int dateIndex = Array.IndexOf(headers, "дата");
                int sourceIndex = Array.IndexOf(headers, "источник");
                int? contentIndex = headers.Contains("содержание") ? Array.IndexOf(headers, "содержание") : (int?)null;

                var newsAdded = 0;
                var newsUpdated = 0;
                var allNews = _dbService.GetNews(null, null, null, null).ToList();
                var allCompanies = _dbService.GetCompanies().ToList();

                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(c => c.Trim())
                                        .ToArray();

                    if (columns.Length <= Math.Max(companyIndex, Math.Max(titleIndex, Math.Max(dateIndex, sourceIndex)))) continue;

                    var companyName = columns[companyIndex];
                    var title = columns[titleIndex];
                    var dateStr = columns[dateIndex];
                    var source = columns[sourceIndex];
                    var content = contentIndex.HasValue && contentIndex < columns.Length ? columns[contentIndex.Value] : null;

                    if (!DateTime.TryParse(dateStr, out var date))
                    {
                        StatusMessage = $"Недействительная дата в строке {i + 1}: {dateStr}. Ожидается формат YYYY-MM-DD.";
                        StatusTextBlock.Text = StatusMessage;
                        return;
                    }

                    var company = allCompanies.FirstOrDefault(c => c.Name.ToLower() == companyName.ToLower());
                    if (company == null)
                    {
                        company = new Company { Name = companyName };
                        company = _dbService.AddCompany(company);
                        allCompanies.Add(company);
                    }

                    var normalizedTitle = title.ToLower();
                    var existingNews = allNews.FirstOrDefault(n =>
                        n.CompanyId == company.CompanyId && n.Title.ToLower() == normalizedTitle);

                    if (existingNews != null)
                    {
                        existingNews.Title = title;
                        existingNews.Content = content;
                        existingNews.Date = date;
                        existingNews.Source = source;
                        _dbService.UpdateNews(existingNews);
                        AddedOrUpdatedNews.Add(existingNews);
                        newsUpdated++;
                    }
                    else
                    {
                        var news = new News
                        {
                            CompanyId = company.CompanyId,
                            Title = title,
                            Content = content,
                            Date = date,
                            Source = source
                        };
                        news = _dbService.AddNews(news);
                        allNews.Add(news);

                        if (news.NewsId == 0)
                        {
                            StatusMessage = $"Ошибка: NewsId = 0 после добавления. Title: {news.Title}, Content: {news.Content}";
                            StatusTextBlock.Text = StatusMessage;
                            return;
                        }

                        AddedOrUpdatedNews.Add(news);
                        newsAdded++;
                    }
                }

                StatusMessage = $"Успешно добавлено {newsAdded} новостей, обновлено {newsUpdated}.";
                StatusTextBlock.Text = StatusMessage;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при обработке файла: {ex.Message}";
                StatusTextBlock.Text = StatusMessage;
            }
        }
    }
}