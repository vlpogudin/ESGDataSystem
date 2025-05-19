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
    public partial class ImportCsvInstructionWindow : Window
    {
        private readonly AddWebsiteWindow _parentWindow;
        private readonly DatabaseService _dbService;
        private string _filePath;
        public List<Website> AddedOrUpdatedWebsites { get; private set; }
        public string StatusMessage { get; private set; }

        public ImportCsvInstructionWindow(AddWebsiteWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            _dbService = new DatabaseService();
            AddedOrUpdatedWebsites = new List<Website>();
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
            if (string.IsNullOrWhiteSpace(_filePath) || !File.Exists(_filePath))
            {
                StatusMessage = "Пожалуйста, выберите корректный файл.";
                DialogResult = false;
                return;
            }

            try
            {
                var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
                if (lines.Length < 2)
                {
                    StatusMessage = "Файл пуст или содержит только заголовок.";
                    DialogResult = false;
                    return;
                }

                var headers = lines[0].Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(h => h.Trim().ToLower())
                                    .ToArray();

                bool hasCompanyHeader = headers.Contains("компания");
                bool hasWebsiteHeader = headers.Contains("веб-сайт") || headers.Contains("веб-сайт (оф. сайт, главная страница)");

                if (!hasCompanyHeader || !hasWebsiteHeader)
                {
                    StatusMessage = "Отсутствуют обязательные столбцы: 'компания', 'веб-сайт'.";
                    DialogResult = false;
                    return;
                }

                int companyIndex = Array.IndexOf(headers, "компания");
                int websiteIndex = headers.Contains("веб-сайт")
                    ? Array.IndexOf(headers, "веб-сайт")
                    : Array.IndexOf(headers, "веб-сайт (оф. сайт, главная страница)");

                var websitesAdded = 0;
                var websitesUpdated = 0;
                var allWebsites = _dbService.GetWebsites().ToList();
                var allCompanies = _dbService.GetCompanies().ToList();

                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(c => c.Trim())
                                        .ToArray();

                    if (columns.Length <= Math.Max(companyIndex, websiteIndex)) continue;

                    var companyName = columns[companyIndex];
                    var url = columns[websiteIndex];

                    // Формируем описание
                    var descriptionParts = new List<string> { "Официальный сайт" };
                    for (int j = 0; j < headers.Length; j++)
                    {
                        if (j == companyIndex || j == websiteIndex) continue;
                        var header = headers[j];
                        descriptionParts.Add(header);
                    }
                    var description = string.Join("; ", descriptionParts.Where(p => !string.IsNullOrWhiteSpace(p)));

                    if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        StatusMessage = $"Недействительный URL в строке {i + 1}: {url}";
                        DialogResult = false;
                        return;
                    }

                    // Проверяем или добавляем компанию
                    var company = allCompanies.FirstOrDefault(c => c.Name.ToLower() == companyName.ToLower());
                    if (company == null)
                    {
                        company = new Company { Name = companyName };
                        company = _dbService.AddCompany(company); // Получаем обновлённую компанию
                        allCompanies.Add(company);
                    }

                    var normalizedUrl = url.ToLower().Replace("https://", "").Replace("http://", "");
                    var existingWebsite = allWebsites.FirstOrDefault(w =>
                        w.CompanyId == company.CompanyId &&
                        w.Url.ToLower().Replace("https://", "").Replace("http://", "") == normalizedUrl);

                    if (existingWebsite != null)
                    {
                        existingWebsite.Url = url;
                        existingWebsite.Description = description;
                        existingWebsite.LastUpdated = DateTime.Now;
                        _dbService.UpdateWebsite(existingWebsite);
                        AddedOrUpdatedWebsites.Add(existingWebsite);
                        websitesUpdated++;
                    }
                    else
                    {
                        var website = new Website
                        {
                            CompanyId = company.CompanyId,
                            Url = url,
                            Description = description,
                            LastUpdated = DateTime.Now
                        };
                        website = _dbService.AddWebsite(website); // Получаем обновлённый объект
                        allWebsites.Add(website);

                        // Отладочная информация
                        if (website.WebsiteId == 0)
                        {
                            MessageBox.Show($"Ошибка: WebsiteId = 0 после добавления. URL: {website.Url}, Description: {website.Description}");
                        }
                        else
                        {
                            MessageBox.Show($"Добавлен веб-сайт: ID={website.WebsiteId}, URL={website.Url}, Description={website.Description}, LastUpdated={website.LastUpdated}");
                        }

                        AddedOrUpdatedWebsites.Add(website);
                        websitesAdded++;
                    }
                }

                StatusMessage = $"Успешно добавлено {websitesAdded} веб-сайтов, обновлено {websitesUpdated}.";
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при обработке файла: {ex.Message}";
                DialogResult = false;
            }
        }
    }
}