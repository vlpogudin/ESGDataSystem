using System;
using System.Globalization;
using System.Windows.Data;

namespace ESG
{
    public class RoleIdToRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int roleId)
            {
                return roleId switch
                {
                    1 => "Администратор",
                    2 => "Модератор данных",
                    3 => "Аналитик",
                    _ => "Неизвестная роль"
                };
            }
            return "Неизвестная роль";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Конверсия обратно не поддерживается.");
        }
    }
}