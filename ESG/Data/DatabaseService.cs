using ESG.Models;
using Npgsql;
using System.Windows;

namespace ESG.Data
{
    /// <summary>
    /// Класс взаимодействия с базой данных
    /// </summary>
    public class DatabaseService
    {
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Подключение и инициализация базы данных
        /// </summary>
        public DatabaseService()
        {
            _connectionString = "Host=localhost;Port=5432;Database=test_esg_data;Username=postgres;Password=30101983Fyyf";
            InitializeDatabase(); // Инициализация базы данных
        }

        #region Инициализация базы данных

        /// <summary>
        /// Инициализация базы данных
        /// </summary>
        private void InitializeDatabase()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open(); // Открытие соединения с базой данных
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = connection; // Создание необходимых таблиц
                    command.CommandText = @"
                        -- Создаём таблицу Industries
                        CREATE TABLE IF NOT EXISTS Industries (
                            industry_id SERIAL PRIMARY KEY,
                            industry_name VARCHAR(100) NOT NULL UNIQUE
                        );

                        -- Создаём таблицу Companies
                        CREATE TABLE IF NOT EXISTS Companies (
                            company_id SERIAL PRIMARY KEY,
                            name VARCHAR(100) NOT NULL,
                            country VARCHAR(100),
                            is_selected BOOLEAN DEFAULT FALSE
                        );

                        -- Создаём промежуточную таблицу CompanyIndustry
                        CREATE TABLE IF NOT EXISTS CompanyIndustry (
                            company_id INT NOT NULL,
                            industry_id INT NOT NULL,
                            PRIMARY KEY (company_id, industry_id),
                            FOREIGN KEY (company_id) REFERENCES Companies(company_id) ON DELETE CASCADE,
                            FOREIGN KEY (industry_id) REFERENCES Industries(industry_id) ON DELETE CASCADE
                        );

                        -- Создаём таблицу Reports
                        CREATE TABLE IF NOT EXISTS Reports (
                            report_id SERIAL PRIMARY KEY,
                            company_id INT NOT NULL,
                            title VARCHAR(200) NOT NULL,
                            year INT,
                            language VARCHAR(50),
                            file_path VARCHAR(500),
                            FOREIGN KEY (company_id) REFERENCES Companies(company_id) ON DELETE CASCADE
                        );

                        -- Создаём таблицу Roles
                        CREATE TABLE IF NOT EXISTS Roles (
                            role_id SERIAL PRIMARY KEY,
                            role_name VARCHAR(50) NOT NULL,
                            description VARCHAR(200)
                        );

                        -- Создаём таблицу Users
                        CREATE TABLE IF NOT EXISTS Users (
                            user_id SERIAL PRIMARY KEY,
                            username VARCHAR(50) NOT NULL UNIQUE,
                            password_hash VARCHAR(100) NOT NULL,
                            role_id INT NOT NULL,
                            FOREIGN KEY (role_id) REFERENCES Roles(role_id) ON DELETE CASCADE
                        );

                        -- Создаём таблицу News
                        CREATE TABLE IF NOT EXISTS News (
                            news_id SERIAL PRIMARY KEY,
                            company_id INT NOT NULL,
                            title VARCHAR(200) NOT NULL,
                            content TEXT,
                            date DATE,
                            source VARCHAR(200),
                            FOREIGN KEY (company_id) REFERENCES Companies(company_id) ON DELETE CASCADE
                        );

                        -- Создаём таблицу Websites
                        CREATE TABLE IF NOT EXISTS Websites (
                            website_id SERIAL PRIMARY KEY,
                            company_id INT NOT NULL,
                            url VARCHAR(200) NOT NULL,
                            description VARCHAR(500),
                            last_updated DATE,
                            FOREIGN KEY (company_id) REFERENCES Companies(company_id) ON DELETE CASCADE
                        );

                        -- Создаём таблицу Permissions
                        CREATE TABLE IF NOT EXISTS Permissions (
                            permission_id SERIAL PRIMARY KEY,
                            role_id INT NOT NULL,
                            permission_name VARCHAR(100) NOT NULL,
                            description VARCHAR(200),
                            FOREIGN KEY (role_id) REFERENCES Roles(role_id) ON DELETE CASCADE
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Проверка успешности подключения к базе данных
        /// </summary>
        /// <returns>Подключение к базе данных корерктно или нет</returns>
        public bool IsConnected()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();  // Попытка открыть соединение
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Работа с компаниями

        /// <summary>
        /// Добавление компании
        /// </summary>
        /// <param name="company">Новая компания</param>
        /// <returns>Новый объект компании</returns>
        public Company AddCompany(Company company)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO Companies (name, country, is_selected) " +
                    "VALUES (@name, @country, @is_selected) RETURNING company_id", connection))
                {
                    command.Parameters.AddWithValue("name", company.Name);
                    command.Parameters.AddWithValue("country", (object)company.Country ?? DBNull.Value);
                    command.Parameters.AddWithValue("is_selected", company.IsSelected);
                    company.CompanyId = (int)command.ExecuteScalar();
                }
                UpdateCompanyIndustries(company.CompanyId, company.SelectedIndustries);
                return GetCompanyById(company.CompanyId);
            }
        }

        /// <summary>
        /// Обновление компании
        /// </summary>
        /// <param name="company">Наименование компании</param>
        public void UpdateCompany(Company company)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE Companies SET name = @name, country = @country, is_selected = @is_selected " +
                    "WHERE company_id = @company_id", connection))
                {
                    command.Parameters.AddWithValue("company_id", company.CompanyId);
                    command.Parameters.AddWithValue("name", company.Name);
                    command.Parameters.AddWithValue("country", (object)company.Country ?? DBNull.Value);
                    command.Parameters.AddWithValue("is_selected", company.IsSelected);
                    command.ExecuteNonQuery();
                }
                UpdateCompanyIndustries(company.CompanyId, company.SelectedIndustries);
            }
        }

        /// <summary>
        /// Удаление компании
        /// </summary>
        /// <param name="companyId">Код компании</param>
        public void DeleteCompany(int companyId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM Companies WHERE company_id = @company_id", connection))
                {
                    command.Parameters.AddWithValue("company_id", companyId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение списка компаний
        /// </summary>
        /// <returns>Список компаний</returns>
        public List<Company> GetCompanies()
        {
            var companies = new List<Company>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT c.company_id, c.name, c.country, c.is_selected, " +
                    "STRING_AGG(i.industry_name, ', ') AS industries " +
                    "FROM Companies c " +
                    "LEFT JOIN CompanyIndustry ci ON c.company_id = ci.company_id " +
                    "LEFT JOIN Industries i ON ci.industry_id = i.industry_id " +
                    "GROUP BY c.company_id, c.name, c.country, c.is_selected", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var company = new Company
                            {
                                CompanyId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Country = reader.IsDBNull(2) ? null : reader.GetString(2),
                                IsSelected = reader.GetBoolean(3)
                            };
                            var industries = reader.IsDBNull(4) ? null : reader.GetString(4);
                            if (!string.IsNullOrEmpty(industries))
                            {
                                company.SelectedIndustries = industries.Split(", ").ToList();
                            }
                            companies.Add(company);
                        }
                    }
                }
            }
            return companies;
        }

        /// <summary>
        /// Получение компании по коду компании
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Company GetCompanyById(int companyId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT c.company_id, c.name, c.country, c.is_selected, " +
                    "STRING_AGG(i.industry_name, ', ') AS industries " +
                    "FROM Companies c " +
                    "LEFT JOIN CompanyIndustry ci ON c.company_id = ci.company_id " +
                    "LEFT JOIN Industries i ON ci.industry_id = i.industry_id " +
                    "WHERE c.company_id = @companyId " +
                    "GROUP BY c.company_id, c.name, c.country, c.is_selected", connection))
                {
                    command.Parameters.AddWithValue("companyId", companyId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var company = new Company
                            {
                                CompanyId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Country = reader.IsDBNull(2) ? null : reader.GetString(2),
                                IsSelected = reader.GetBoolean(3)
                            };
                            var industries = reader.IsDBNull(4) ? null : reader.GetString(4);
                            if (!string.IsNullOrEmpty(industries))
                            {
                                company.SelectedIndustries = industries.Split(", ").ToList();
                            }
                            return company;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Получение наименования компании по коду компании
        /// </summary>
        /// <param name="companyId">Код компании</param>
        /// <returns>Наименование компании</returns>
        public string GetCompanyNameById(int companyId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT name FROM Companies WHERE company_id = @companyId", connection))
                {
                    command.Parameters.AddWithValue("companyId", companyId);
                    var result = command.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        /// <summary>
        /// Получение сводки по компаниям по выбранным фильтрам
        /// </summary>
        /// <param name="selectedIndustry">Выбранная отрасль</param>
        /// <param name="companyIds">Список кодов компаний</param>
        /// <param name="startYear">Начала периода</param>
        /// <param name="endYear">Конец периода</param>
        /// <returns>Сводка компаний по выбранным фильтрам</returns>
        public List<CompanySummary> GetCompanySummary(string selectedIndustry, List<int>? companyIds, int? startYear, int? endYear)
        {
            var summaries = new List<CompanySummary>();
            var companies = new List<Company>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT DISTINCT c.company_id, c.name, " +
                    "STRING_AGG(i.industry_name, ', ') AS industries " +
                    "FROM Companies c " +
                    "LEFT JOIN CompanyIndustry ci ON c.company_id = ci.company_id " +
                    "LEFT JOIN Industries i ON ci.industry_id = i.industry_id " +
                    "LEFT JOIN Reports r ON c.company_id = r.company_id " +
                    "WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();
                if (selectedIndustry != null && selectedIndustry != "Все отрасли")
                {
                    query += " AND i.industry_name = @industry";
                    parameters.Add(new NpgsqlParameter("industry", selectedIndustry));
                }
                if (companyIds != null && companyIds.Any())
                {
                    query += " AND c.company_id = ANY (@companyIds)";
                    parameters.Add(new NpgsqlParameter("companyIds", companyIds.ToArray()));
                }
                if (startYear.HasValue)
                {
                    query += " AND (r.year >= @startYear OR r.year IS NULL)";
                    parameters.Add(new NpgsqlParameter("startYear", startYear.Value));
                }
                if (endYear.HasValue)
                {
                    query += " AND (r.year <= @endYear OR r.year IS NULL)";
                    parameters.Add(new NpgsqlParameter("endYear", endYear.Value));
                }
                query += " GROUP BY c.company_id, c.name";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var company = new Company
                            {
                                CompanyId = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            };
                            var industries = reader.IsDBNull(2) ? null : reader.GetString(2);
                            if (!string.IsNullOrEmpty(industries))
                            {
                                company.SelectedIndustries = industries.Split(", ").ToList();
                            }
                            companies.Add(company);
                        }
                    }
                }
                foreach (var company in companies)
                {
                    var summary = new CompanySummary
                    {
                        CompanyId = company.CompanyId,
                        CompanyName = company.Name,
                        Industries = company.SelectedIndustries.Any() ? string.Join(", ", company.SelectedIndustries) : "N/A"
                    };

                    var reportQuery = @"
                        SELECT file_path, title, year
                        FROM Reports
                        WHERE company_id = @companyId";
                    if (startYear.HasValue)
                        reportQuery += " AND year >= @startYear";
                    if (endYear.HasValue)
                        reportQuery += " AND year <= @endYear";

                    var reportParams = new List<NpgsqlParameter>
                    {
                        new NpgsqlParameter("companyId", company.CompanyId)
                    };
                    if (startYear.HasValue)
                        reportParams.Add(new NpgsqlParameter("startYear", startYear.Value));
                    if (endYear.HasValue)
                        reportParams.Add(new NpgsqlParameter("endYear", endYear.Value));

                    var reportFiles = new List<string>();
                    var reportInfo = new List<string>();
                    using (var reportCmd = new NpgsqlCommand(reportQuery, connection))
                    {
                        reportCmd.Parameters.AddRange(reportParams.ToArray());
                        using (var reader = reportCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var filePath = reader.IsDBNull(0) ? "N/A" : reader.GetString(0);
                                var title = reader.GetString(1);
                                var year = reader.IsDBNull(2) ? "N/A" : reader.GetInt32(2).ToString();
                                reportFiles.Add(filePath);
                                reportInfo.Add($"{title} ({year})");
                            }
                        }
                    }
                    summary.ReportFiles = reportFiles.Any() ? string.Join("; ", reportFiles) : "N/A";
                    summary.ReportInfo = reportInfo.Any() ? string.Join("; ", reportInfo) : "N/A";

                    var newsQuery = @"
                        SELECT title, date, content, source
                        FROM News
                        WHERE company_id = @companyId";
                    var newsParams = new List<NpgsqlParameter>
                    {
                        new NpgsqlParameter("companyId", company.CompanyId)
                    };

                    var newsTitles = new List<string>();
                    var newsInfo = new List<string>();
                    using (var newsCmd = new NpgsqlCommand(newsQuery, connection))
                    {
                        newsCmd.Parameters.AddRange(newsParams.ToArray());
                        using (var reader = newsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var title = reader.GetString(0);
                                var date = reader.IsDBNull(1) ? "N/A" : reader.GetDateTime(1).ToString("yyyy-MM-dd");
                                var content = reader.IsDBNull(2) ? "N/A" : reader.GetString(2).Replace("\n", " ");
                                var source = reader.IsDBNull(3) ? "N/A" : reader.GetString(3);
                                newsTitles.Add($"{title} ({date})");
                                newsInfo.Add($"{content} [{source}]");
                            }
                        }
                    }
                    summary.NewsTitles = newsTitles.Any() ? string.Join("; ", newsTitles) : "N/A";
                    summary.NewsInfo = newsInfo.Any() ? string.Join("; ", newsInfo) : "N/A";

                    var websiteQuery = @"
                        SELECT url, description
                        FROM Websites
                        WHERE company_id = @companyId";
                    var websiteParams = new List<NpgsqlParameter>
                    {
                        new NpgsqlParameter("companyId", company.CompanyId)
                    };

                    var websiteUrls = new List<string>();
                    var websiteInfo = new List<string>();
                    using (var websiteCmd = new NpgsqlCommand(websiteQuery, connection))
                    {
                        websiteCmd.Parameters.AddRange(websiteParams.ToArray());
                        using (var reader = websiteCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var url = reader.GetString(0);
                                var description = reader.IsDBNull(1) ? "N/A" : reader.GetString(1).Replace("\n", " ");
                                websiteUrls.Add(url);
                                websiteInfo.Add(description);
                            }
                        }
                    }
                    summary.WebsiteUrls = websiteUrls.Any() ? string.Join("; ", websiteUrls) : "N/A";
                    summary.WebsiteInfo = websiteInfo.Any() ? string.Join("; ", websiteInfo) : "N/A";
                    summaries.Add(summary);
                }
            }
            return summaries;
        }

        #endregion

        #region Работа с новостями

        /// <summary>
        /// Добавление новости
        /// </summary>
        /// <param name="news">Новости для добавления</param>
        /// <returns>Обновленный объект новостей</returns>
        public News AddNews(News news)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO News (company_id, title, content, date, source) " +
                    "VALUES (@company_id, @title, @content, @date, @source) RETURNING news_id", connection))
                {
                    command.Parameters.AddWithValue("company_id", news.CompanyId);
                    command.Parameters.AddWithValue("title", news.Title);
                    command.Parameters.AddWithValue("content", (object)news.Content ?? DBNull.Value);
                    command.Parameters.AddWithValue("date", (object)news.Date ?? DBNull.Value);
                    command.Parameters.AddWithValue("source", (object)news.Source ?? DBNull.Value);
                    news.NewsId = (int)command.ExecuteScalar();
                }
                return news;
            }
        }

        /// <summary>
        /// Обновление новостей
        /// </summary>
        /// <param name="news">Новости для обновления</param>
        public void UpdateNews(News news)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE News SET company_id = @company_id, title = @title, content = @content, " +
                    "date = @date, source = @source " +
                    "WHERE news_id = @news_id", connection))
                {
                    command.Parameters.AddWithValue("news_id", news.NewsId);
                    command.Parameters.AddWithValue("company_id", news.CompanyId);
                    command.Parameters.AddWithValue("title", news.Title);
                    command.Parameters.AddWithValue("content", (object)news.Content ?? DBNull.Value);
                    command.Parameters.AddWithValue("date", (object)news.Date ?? DBNull.Value);
                    command.Parameters.AddWithValue("source", (object)news.Source ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаление новостей
        /// </summary>
        /// <param name="newsId">Код новости для удаления</param>
        public void DeleteNews(int newsId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM News WHERE news_id = @news_id", connection))
                {
                    command.Parameters.AddWithValue("news_id", newsId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение списка новостей
        /// </summary>
        /// <param name="selectedIndustry">Выбранная отрасль</param>
        /// <param name="selectedCompany">Выбранная компания</param>
        /// <param name="startYear">Начало периода</param>
        /// <param name="endYear">Конец периода</param>
        /// <returns>Список новостей по выбранным фильтрам</returns>
        public List<News> GetNews(string selectedIndustry, Company selectedCompany, int? startYear, int? endYear)
        {
            var newsItems = new List<News>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT n.news_id, n.company_id, n.title, n.content, n.date, n.source
                    FROM News n
                    JOIN Companies c ON n.company_id = c.company_id
                    LEFT JOIN CompanyIndustry ci ON c.company_id = ci.company_id
                    LEFT JOIN Industries i ON ci.industry_id = i.industry_id
                    WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();
                if (selectedIndustry != null && selectedIndustry != "Все отрасли")
                {
                    query += " AND i.industry_name = @industry";
                    parameters.Add(new NpgsqlParameter("industry", selectedIndustry));
                }
                if (selectedCompany != null)
                {
                    query += " AND c.company_id = @companyId";
                    parameters.Add(new NpgsqlParameter("companyId", selectedCompany.CompanyId));
                }
                if (startYear.HasValue)
                {
                    query += " AND EXTRACT(YEAR FROM n.date) >= @startYear";
                    parameters.Add(new NpgsqlParameter("startYear", startYear.Value));
                }
                if (endYear.HasValue)
                {
                    query += " AND EXTRACT(YEAR FROM n.date) <= @endYear";
                    parameters.Add(new NpgsqlParameter("endYear", endYear.Value));
                }
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            newsItems.Add(new News
                            {
                                NewsId = reader.GetInt32(0),
                                CompanyId = reader.GetInt32(1),
                                Title = reader.GetString(2),
                                Content = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Date = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                Source = reader.IsDBNull(5) ? null : reader.GetString(5)
                            });
                        }
                    }
                }
            }
            return newsItems;
        }

        #endregion

        #region Работа с веб-сайтами

        /// <summary>
        /// Добавление веб-сайтов
        /// </summary>
        /// <param name="website">Новый веб-сайт</param>
        /// <returns>Добавленный объект веб-сайта</returns>
        public Website AddWebsite(Website website)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                int newWebsiteId;
                using (var command = new NpgsqlCommand(
                    "INSERT INTO Websites (company_id, url, description, last_updated) " +
                    "VALUES (@company_id, @url, @description, @last_updated) RETURNING website_id", connection))
                {
                    DateTime? lastUpdated = website.LastUpdated ?? DateTime.Now;
                    command.Parameters.AddWithValue("company_id", website.CompanyId);
                    command.Parameters.AddWithValue("url", website.Url);
                    command.Parameters.AddWithValue("description", (object)website.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("last_updated", lastUpdated);
                    newWebsiteId = (int)command.ExecuteScalar();
                }
                using (var command = new NpgsqlCommand(
                    "SELECT website_id, company_id, url, description, last_updated " +
                    "FROM Websites WHERE website_id = @website_id", connection))
                {
                    command.Parameters.AddWithValue("website_id", newWebsiteId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Website
                            {
                                WebsiteId = reader.GetInt32(0),
                                CompanyId = reader.GetInt32(1),
                                Url = reader.GetString(2),
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                LastUpdated = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
                            };
                        }
                    }
                }
            }
            return website;
        }

        /// <summary>
        /// Обновление веб-сайтов
        /// </summary>
        /// <param name="website">Веб-сайт для обновления</param>
        public void UpdateWebsite(Website website)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE Websites SET company_id = @company_id, url = @url, " +
                    "description = @description, last_updated = @last_updated " +
                    "WHERE website_id = @website_id", connection))
                {
                    command.Parameters.AddWithValue("website_id", website.WebsiteId);
                    command.Parameters.AddWithValue("company_id", website.CompanyId);
                    command.Parameters.AddWithValue("url", website.Url);
                    command.Parameters.AddWithValue("description", (object)website.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("last_updated", (object)website.LastUpdated ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаление веб-сайта
        /// </summary>
        /// <param name="websiteId">Код веб-сайта</param>
        public void DeleteWebsite(int websiteId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM Websites WHERE website_id = @website_id", connection))
                {
                    command.Parameters.AddWithValue("website_id", websiteId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение списка веб-сайтов
        /// </summary>
        /// <returns>Список веб-сайтов</returns>
        public List<Website> GetWebsites()
        {
            var websites = new List<Website>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT website_id, company_id, url, description, last_updated FROM Websites", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            websites.Add(new Website
                            {
                                WebsiteId = reader.GetInt32(0),
                                CompanyId = reader.GetInt32(1),
                                Url = reader.GetString(2),
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                LastUpdated = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            return websites;
        }

        /// <summary>
        /// Получение списка веб-сайтов по коду компании
        /// </summary>
        /// <param name="companyId">Код компании</param>
        /// <returns>Список веб-сайтов</returns>
        public List<Website> GetWebsitesForCompany(int companyId)
        {
            var websites = new List<Website>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT website_id, company_id, url, description, last_updated " +
                    "FROM Websites WHERE company_id = @companyId", connection))
                {
                    command.Parameters.AddWithValue("companyId", companyId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            websites.Add(new Website
                            {
                                WebsiteId = reader.GetInt32(0),
                                CompanyId = reader.GetInt32(1),
                                Url = reader.GetString(2),
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                LastUpdated = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            return websites;
        }

        #endregion

        #region Работа с отчетами

        /// <summary>
        /// Добавление нового отчета
        /// </summary>
        /// <param name="report">Новый отчет</param>
        public void AddReport(Report report)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO Reports (company_id, title, year, language, file_path) " +
                    "VALUES (@company_id, @title, @year, @language, @file_path) RETURNING report_id", connection))
                {
                    command.Parameters.AddWithValue("company_id", report.CompanyId);
                    command.Parameters.AddWithValue("title", report.Title);
                    command.Parameters.AddWithValue("year", (object)report.Year ?? DBNull.Value);
                    command.Parameters.AddWithValue("language", (object)report.Language ?? DBNull.Value);
                    command.Parameters.AddWithValue("file_path", (object)report.FilePath ?? DBNull.Value);
                    report.ReportId = (int)command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Обновление отчета
        /// </summary>
        /// <param name="report">Отчет для обновления</param>
        public void UpdateReport(Report report)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE Reports SET company_id = @company_id, title = @title, year = @year, " +
                    "language = @language, file_path = @file_path " +
                    "WHERE report_id = @report_id", connection))
                {
                    command.Parameters.AddWithValue("report_id", report.ReportId);
                    command.Parameters.AddWithValue("company_id", report.CompanyId);
                    command.Parameters.AddWithValue("title", report.Title);
                    command.Parameters.AddWithValue("year", (object)report.Year ?? DBNull.Value);
                    command.Parameters.AddWithValue("language", (object)report.Language ?? DBNull.Value);
                    command.Parameters.AddWithValue("file_path", (object)report.FilePath ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаление отчета
        /// </summary>
        /// <param name="reportId">Код отчета</param>
        public void DeleteReport(int reportId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM Reports WHERE report_id = @report_id", connection))
                {
                    command.Parameters.AddWithValue("report_id", reportId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение списка отчетов по фильтрам
        /// </summary>
        /// <param name="selectedIndustry">Выбранная отрасль</param>
        /// <param name="selectedCompany">Выбранная компания</param>
        /// <param name="startYear">Начало периода</param>
        /// <param name="endYear">Конец периода</param>
        /// <returns>Список отчетов по выбранным фильтрам</returns>
        public List<Report> GetReports(string selectedIndustry, Company selectedCompany, int? startYear, int? endYear)
        {
            var reports = new List<Report>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT r.report_id, r.company_id, r.title, r.year, r.language, r.file_path, r.date
                    FROM Reports r
                    JOIN Companies c ON r.company_id = c.company_id
                    LEFT JOIN CompanyIndustry ci ON c.company_id = ci.company_id
                    LEFT JOIN Industries i ON ci.industry_id = i.industry_id
                    WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();
                if (selectedIndustry != null && selectedIndustry != "Все отрасли")
                {
                    query += " AND i.industry_name = @industry";
                    parameters.Add(new NpgsqlParameter("industry", selectedIndustry));
                }
                if (selectedCompany != null)
                {
                    query += " AND c.company_id = @companyId";
                    parameters.Add(new NpgsqlParameter("companyId", selectedCompany.CompanyId));
                }
                if (startYear.HasValue)
                {
                    query += " AND r.year >= @startYear";
                    parameters.Add(new NpgsqlParameter("startYear", startYear.Value));
                }
                if (endYear.HasValue)
                {
                    query += " AND r.year <= @endYear";
                    parameters.Add(new NpgsqlParameter("endYear", endYear.Value));
                }
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reports.Add(new Report
                            {
                                ReportId = reader.GetInt32(0),
                                CompanyId = reader.GetInt32(1),
                                Title = reader.GetString(2),
                                Year = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                Language = reader.IsDBNull(4) ? null : reader.GetString(4),
                                FilePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                            });
                        }
                    }
                }
            }
            return reports;
        }

        #endregion

        #region Работа с ролями

        /// <summary>
        /// Получение списка ролей
        /// </summary>
        /// <returns>Список ролей</returns>
        public List<Role> GetRoles()
        {
            var roles = new List<Role>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT role_id, role_name, description FROM Roles", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(new Role
                            {
                                RoleId = reader.GetInt32(0),
                                RoleName = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return roles;
        }

        #endregion

        #region Работа с разрешениями

        /// <summary>
        /// Получение списка разрешений по коду роли
        /// </summary>
        /// <param name="roleId">Код роли</param>
        /// <returns>Список разрешений роли</returns>
        public List<Permission> GetPermissionsByRoleId(int roleId)
        {
            var permissions = new List<Permission>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT permission_id, role_id, permission_name, description FROM Permissions WHERE role_id = @roleId", connection))
                {
                    command.Parameters.AddWithValue("roleId", roleId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(new Permission
                            {
                                PermissionId = reader.GetInt32(0),
                                RoleId = reader.GetInt32(1),
                                PermissionName = reader.GetString(2),
                                Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return permissions;
        }

        #endregion

        #region Работа с пользователями

        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        /// <param name="user">Новый пользователь</param>
        public void AddUser(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password); // Хэшируем пароль перед сохранением
                using (var command = new NpgsqlCommand(
                    "INSERT INTO Users (username, password_hash, role_id) " +
                    "VALUES (@username, @password_hash, @role_id) RETURNING user_id", connection))
                {
                    command.Parameters.AddWithValue("username", user.Username);
                    command.Parameters.AddWithValue("password_hash", passwordHash);
                    command.Parameters.AddWithValue("role_id", user.RoleId);
                    user.UserId = (int)command.ExecuteScalar();
                    user.Password = passwordHash; // Обновляем Password в объекте на хэш
                }
            }
        }

        /// <summary>
        /// Обновление данных пользователя
        /// </summary>
        /// <param name="user">Пользователь для обновления данных</param>
        public void UpdateUser(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string passwordHash = string.IsNullOrWhiteSpace(user.Password) ? user.Password : BCrypt.Net.BCrypt.HashPassword(user.Password);
                using (var command = new NpgsqlCommand(
                    "UPDATE Users SET username = @username, password_hash = @password_hash, role_id = @role_id " +
                    "WHERE user_id = @user_id", connection))
                {
                    command.Parameters.AddWithValue("user_id", user.UserId);
                    command.Parameters.AddWithValue("username", user.Username);
                    command.Parameters.AddWithValue("password_hash", passwordHash ?? user.Password);
                    command.Parameters.AddWithValue("role_id", user.RoleId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="userId">Код пользователя</param>
        public void DeleteUser(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM Users WHERE user_id = @user_id", connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение списка пользователя
        /// </summary>
        /// <returns>Список пользователей</returns>
        public List<User> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT user_id, username, password_hash, role_id FROM Users", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                RoleId = reader.GetInt32(3)
                            };
                            user.Permissions = GetPermissionsByRoleId(user.RoleId);
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        /// <param name="username">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Найденный пользователь</returns>
        public (User user, string role_name) AuthenticateUser(string username, string password)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT u.user_id, u.username, u.password_hash, u.role_id, r.role_name " +
                    "FROM Users u " +
                    "JOIN Roles r ON u.role_id = r.role_id " +
                    "WHERE u.username = @username", connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader.GetString(2);
                            try
                            {
                                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                                {
                                    var user = new User
                                    {
                                        UserId = reader.GetInt32(0),
                                        Username = reader.GetString(1),
                                        Password = storedHash,
                                        RoleId = reader.GetInt32(3)
                                    };
                                    user.Permissions = GetPermissionsByRoleId(user.RoleId);
                                    return (user, reader.GetString(4));
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при входе в аккаунт: {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            return (null, null);
        }

        #endregion

        #region Работа с отраслями

        /// <summary>
        /// Добавление отрасли
        /// </summary>
        /// <param name="industryName">Наименование отрасли</param>
        /// <returns>Добавленный объект отрасли</returns>
        public Industry AddIndustry(string industryName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO Industries (industry_name) VALUES (@industryName) ON CONFLICT (industry_name) DO NOTHING RETURNING industry_id, industry_name",
                    connection))
                {
                    command.Parameters.AddWithValue("industryName", industryName);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Industry
                            {
                                IndustryId = reader.GetInt32(0),
                                IndustryName = reader.GetString(1)
                            };
                        }
                    }
                }
                using (var command = new NpgsqlCommand(
                    "SELECT industry_id, industry_name FROM Industries WHERE industry_name = @industryName", connection))
                {
                    command.Parameters.AddWithValue("industryName", industryName);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Industry
                            {
                                IndustryId = reader.GetInt32(0),
                                IndustryName = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Обновление отрасли
        /// </summary>
        /// <param name="oldIndustryName">Старое наименование отрасли</param>
        /// <param name="newIndustryName">Новое наименование отрасли</param>
        /// <exception cref="Exception">Ошибка при обновлении несуществующей отрасли</exception>
        public void UpdateIndustry(string oldIndustryName, string newIndustryName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE Industries SET industry_name = @newIndustryName WHERE industry_name = @oldIndustryName",
                    connection))
                {
                    command.Parameters.AddWithValue("oldIndustryName", oldIndustryName);
                    command.Parameters.AddWithValue("newIndustryName", newIndustryName);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Отрасль с именем '{oldIndustryName}' не найдена для обновления.");
                    }
                }
            }
        }

        /// <summary>
        /// Удаление отрасли
        /// </summary>
        /// <param name="industryName">Наименование отрасли</param>
        public void DeleteIndustry(string industryName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "DELETE FROM Industries WHERE industry_name = @industryName",
                    connection))
                {
                    command.Parameters.AddWithValue("industryName", industryName);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение списка отраслей, отсортированных по наименованию
        /// </summary>
        /// <returns>Отсортированный список отраслей</returns>
        public IEnumerable<Industry> GetIndustries()
        {
            var industries = new List<Industry>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT industry_id, industry_name FROM Industries ORDER BY industry_name", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            industries.Add(new Industry
                            {
                                IndustryId = reader.GetInt32(0),
                                IndustryName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return industries;
        }

        /// <summary>
        /// Получение отрасли по наименованию отрасли
        /// </summary>
        /// <param name="industryName">Наименование отрасли</param>
        /// <returns>Код отрасли</returns>
        private int GetIndustryIdByName(string industryName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT industry_id FROM Industries WHERE industry_name = @industryName", connection))
                {
                    command.Parameters.AddWithValue("industryName", industryName);
                    var result = command.ExecuteScalar();
                    return result != null ? (int)result : -1;
                }
            }
        }

        /// <summary>
        /// Добавление несуществующей отрасли
        /// </summary>
        /// <param name="industryName">Наименование отрасли</param>
        private void AddIndustryIfNotExists(string industryName)
        {
            AddIndustry(industryName);
        }

        /// <summary>
        /// Обновление списка отраслей компании
        /// </summary>
        /// <param name="companyId">Код компании</param>
        /// <param name="industries">Список отраслей компании</param>
        private void UpdateCompanyIndustries(int companyId, List<string> industries)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "DELETE FROM CompanyIndustry WHERE company_id = @companyId", connection))
                {
                    command.Parameters.AddWithValue("companyId", companyId);
                    command.ExecuteNonQuery();
                }
                foreach (var industry in industries ?? new List<string>())
                {
                    if (string.IsNullOrWhiteSpace(industry)) continue;
                    AddIndustryIfNotExists(industry);
                    int industryId = GetIndustryIdByName(industry);
                    if (industryId == -1) continue;

                    using (var command = new NpgsqlCommand(
                        "INSERT INTO CompanyIndustry (company_id, industry_id) VALUES (@companyId, @industryId)", connection))
                    {
                        command.Parameters.AddWithValue("companyId", companyId);
                        command.Parameters.AddWithValue("industryId", industryId);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Класс для получения сводки по компаниям
    /// </summary>
    public class CompanySummary
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Industries { get; set; }
        public string ReportFiles { get; set; }
        public string ReportInfo { get; set; }
        public string NewsTitles { get; set; }
        public string NewsInfo { get; set; }
        public string WebsiteUrls { get; set; }
        public string WebsiteInfo { get; set; }
    }
}