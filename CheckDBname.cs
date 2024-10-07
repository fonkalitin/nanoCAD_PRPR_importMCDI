

public class checkDBname
{
    public bool isLocalDB(string DBname)
    { // Метод для проверки имени БД - локальная или сетевая
        bool result = false;
        bool isLocalhostDB = DBname.Contains(":localhost");
        bool isPGsqlDB = DBname.Contains("pgsql:");

        if ((DBname.Split(":").Length - 1) > 1 && DBname.Contains(":localhost") == false)
        {
            result = false; // Это сетевая БД если есть два двоеточия : и нет localhost
        }

        else if ((DBname.Split(":").Length - 1) < 1 || DBname.Contains(":localhost") || DBname.Contains("\\"))
        {
            result = true; // Имя БД локальное, т.к. в  имени меньше двух : или есть слово Локалхост, или есть слэш пути
        }

        else
        {
            result = false; // Если ни одно из прыдыдущих не подошло то на всякий случай скажем что сетевая БД
        }

        return result;
    }

}