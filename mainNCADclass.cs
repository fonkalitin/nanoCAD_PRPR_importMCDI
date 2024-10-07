
    using App = HostMgd.ApplicationServices; 
    using Rtm = Teigha.Runtime;
    using Multicad.AplicationServices;
    using Multicad.DataServices;
    using IniFiles;
    using System.Reflection;
    using HostMgd.EditorInput;
    using env = System.Environment;

    using Db = Teigha.DatabaseServices;
    using Ed = HostMgd.EditorInput;
    using mcDBs = Multicad.DatabaseServices;
    using Multicad;
    using Multicad.Objects;
    using Multicad.DatabaseServices;

[assembly: Rtm.CommandClass(typeof(Tools.CadCommand))]

namespace Tools
    {
        /// <summary> 
        /// Комманды
        /// </summary>
        class CadCommand : Rtm.IExtensionApplication
        {
        
        public Editor ed = App.Application.DocumentManager.MdiActiveDocument.Editor;
        //App.DocumentCollection dm = App.Application.DocumentManager;

        // Константа с именем кофигурационного файла с настройками
        public const string configFile = "prPr_DBConfig.ini";

        #region INIT
        public void Initialize()    
        {
                // Получение фактического пути расположения данной сборки dll
                string dllFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // Сборка полного пуи к файлу конфигурации
                string iniFileFullPath = $"{dllFolder}\\{configFile}";
                // Чтение файла конфигурации
                IniFile INI = new IniFile(iniFileFullPath);
                bool autoUpdNeeded = bool.Parse(INI.ReadINI("options", "autoUpdNeeded"));

                string guestString = $"{env.NewLine}Доступные команды: PRPR_importMCDI - автоимпорт файла MCDI";
                ed.WriteMessage(guestString);
                if (autoUpdNeeded) // Если в настройках включено автообновление то запустить команлу обновления
                {
                    ed.WriteMessage($"{env.NewLine}Запуск команды автообновления локальной БД");
                    importMCDI(); // Вызов команды автообновления локальной базы данных (вызывается автоматически при инициализации модуля)
                }
        }

            public void Terminate()
            {
                // Пусто
            }

        #endregion

        #region Command

        /// <summary>
        /// Основная команда для вызова из командной строки
        /// </summary>
        
        [Rtm.CommandMethod("PRPR_importMCDI")]
        public static void importMCDI()
        {
            // Получение фактического пути расположения данной сборки dll
            string dllFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Сборка полного пуи к файлу конфигурации
            string iniFileFullPath = $"{dllFolder}\\{configFile}";
            // Чтение файла конфигурации
            IniFile INI = new IniFile(iniFileFullPath);

            string MCDIfolderPath = INI.ReadINI("MCDI", "MCDIfolderPath");
            string MCDIfilename = INI.ReadINI("MCDI", "MCDIfilename");

            string DBfolderName = INI.ReadINI("spdsDB", "DBfolderName");
            bool importToRoot = bool.Parse(INI.ReadINI("spdsDB", "importToRoot"));
            string DBname = INI.ReadINI("spdsDB", "DBname");

            bool silentMode = bool.Parse(INI.ReadINI("options", "silentMode"));

            // Сборка полного пути к MCDI файлу для импорта
            string MCDIfileFOrImport = MCDIfolderPath + MCDIfilename;
            // Получение времени изменения файла MCDI
            DateTime MCDIfileDateTime = File.GetLastWriteTime(MCDIfileFOrImport);
            string MCDIfileDate = MCDIfileDateTime.ToString();
            var howOldFile = DateTime.Now - MCDIfileDateTime;
            int howOldDaysFile = howOldFile.Days; // Разница в днях
            string fileDate = null;

            if (howOldDaysFile == 0)
            {
                fileDate = "Файл был обновлен сегодня";
            }
            else if (howOldDaysFile == 1)
            {
                fileDate = "Файл был обновлен вчера";
            }
            else if (howOldDaysFile == 2)
            {
                fileDate = "Файл был обновлен позавчера";
            }
            else
            {
                fileDate = $"Файл был обновлен {howOldDaysFile} дня/дней назад";
            }

            // Подключениек текущей БД
            Connection spdsDB = new Connection();



            void importMCDI()
            {  // -----  Функция импорта MCDI в БД -----

                bool DBsubFolderExist = false;
                Folder rootDBfolder = spdsDB.GetRoot(); // Получение кореня дерева БД

                // Выбор места в структуре БД для импорта (корень или подкаталог)
                if (importToRoot) // Импорт в корень БД (если включена настройка в конфиге)
                {
                    rootDBfolder.Import(MCDIfileFOrImport); // Непосредственно операция импорта MCDI в БД
                    MessageBox.Show($"Импорт {MCDIfilename} успешно выполнен в корневой каталог БД", "Импорт корень в локальной БД", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else // Ветка есои импортировать не в корень БД а в подкаталог
                {
                    bool importIsOk = false;
                    Folder DBsubFolder = null; // Подкаталог в корне БД

                    // Проверка на существование подкаталога в корне БД
                    if (rootDBfolder.GetSubFolder(DBfolderName) == null)
                    {
                        DBsubFolderExist = false;
                    }
                    else
                    {
                        DBsubFolder = rootDBfolder.GetSubFolder(DBfolderName); // Получение заданного подкаталога БД
                        DBsubFolderExist = true;
                    }

                    // Переход в заданный "DBfolderName" подкаталог дерева БД
                    // Получение целевого подкаталога в дереве БД
                    if (DBsubFolderExist) // Проверка на существование подкаталога в корне БД
                    {
                        DBsubFolder.Import(MCDIfileFOrImport); // Непосредственно операция импорта MCDI в подкаталог БД
                        importIsOk = true;
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show($"Импорт не удался, так как в БД отстутсвует необходимый каталог {DBfolderName} {env.NewLine} {env.NewLine}Хотите создать его сейчас и выполнить импорт в него?", "Импорт в БД", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            DBsubFolder = rootDBfolder.CreateSubFolder(DBfolderName); // Создание подкаталога в корне БД
                            DBsubFolder.Import(MCDIfileFOrImport); // Непосредственно операция импорта MCDI в подкаталог БД
                            importIsOk = true;
                        }
                    }
                    if (importIsOk && silentMode == false)
                    {
                        MessageBox.Show($"Импорт {MCDIfilename} успешно выполнен в следующий каталог БД: {DBsubFolder.ToString()}", "Импорт в локальную БД", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (importIsOk && silentMode) {
                        App.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"Импорт {MCDIfilename} успешно выполнен в следующий каталог БД: {DBsubFolder.ToString()}");
                    }

                    else
                    {
                        MessageBox.Show("Импорт не удалось выполнить!", "Ошибка импорта", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                    }
                }
            } ///  -----  Функция импорта MCDI в БД -----



            string curBD = McParamManager.GetStringParam(9); //получаем путь к  текущей базе СПДС (9 - это порядковый номер параметра в конфиг файле настроек СПДС .xml)
            // Определить какая БД в данный момент подключена (локальная = true / сетевая = false)
            bool isLocalDBname = new checkDBname().isLocalDB(curBD);

            bool checkFile() // Проверка существования файла MCDI
            {
                if (File.Exists(MCDIfileFOrImport))
                {
                    App.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"{env.NewLine}PRPR_importMCDI Файл обнаружен: {MCDIfileFOrImport}");
                    return true;
                }
                else
                {
                    if (silentMode) {
                        App.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"{env.NewLine}Невозможно выполнить импорт, так как файл {MCDIfilename} не обнаружен. {env.NewLine}Вероятно есть проблемы с сетевым подключением, или владелец файла сделал его недоступным для вас");
                    }
                    else
                    {
                        MessageBox.Show($"{env.NewLine}Невозможно выполнить импорт, так как файл {MCDIfilename} не обнаружен. {env.NewLine}Вероятно есть проблемы с сетевым подключением, или владелец файла сделал его недоступным для вас", "Ошибка доступа к файлу MCDI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return false;
                }

            }

            if (checkFile()) // Проверка существования файла MCDI
            {

                if (isLocalDBname) // Если сейчас подключение к локальной БД
                {
                    //string curUserRole = spdsDB.GetUserRole().ToString(); // Определить какие права у текущего пользователя БД

                    if (silentMode) // Если в настройках включен тихий режим то сразу выполнять обновление без вопросов
                    {
                        importMCDI();
                    }
                    else // Если тихий режим выключен то спросить пользователя
                    {
                        DialogResult result = MessageBox.Show($"Хотите импортировать в текущую базу данных файл? {env.NewLine}{MCDIfilename} {env.NewLine}{fileDate} [{MCDIfileDate}]", "Обновление локальной БД", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            importMCDI(); // Выполнить функцию импорта в БД если Ок
                        }
                    }
                }

                else
                {
                    DialogResult Result = MessageBox.Show($"Невозможно выполнить импорт и обновление, так как в данный момент вы подключены к сетевой базе. {env.NewLine}{env.NewLine}Хотите прямо сейчас переключиться на локальную базу и выполнить её обновление? {env.NewLine}{env.NewLine}БД: {DBname}", "Импорт в БД", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (Result == DialogResult.Yes)
                    {
                        McParamManager.SetParam(DBname, 9); // Установить подключение к заданной БД
                        importMCDI(); // Выполнить функцию импорта в БД
                    }
                }
            }

        }

        }
}

            #endregion