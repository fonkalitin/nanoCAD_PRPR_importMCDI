
    using App = HostMgd.ApplicationServices; 
    using Db = Teigha.DatabaseServices;
    using Ed = HostMgd.EditorInput;
    using Rtm = Teigha.Runtime;
    using System.Windows.Forms;
    using prPr_FileBackupper;
    using System.IO;
    using Multicad;
    using mcDBs = Multicad.DatabaseServices;
    using Multicad.Objects;
    using Multicad.DatabaseServices;
    using Multicad.AplicationServices;
    using env = System.Environment;
    using DBserv = Multicad.DatabaseServices;
    using Multicad.DataServices;
    using IniFiles;
    using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Multicad.Runtime;


[assembly: Rtm.CommandClass(typeof(Tools.CadCommand))]

namespace Tools
    {
        /// <summary> 
        /// Комманды
        /// </summary>
        class CadCommand : Rtm.IExtensionApplication
        {

        // Константа с именем кофигурационного файла с настройками
        public const string configFile = "prPr_DBConfig.ini";

        #region INIT
        public void Initialize()    
            {
  
                App.DocumentCollection dm = App.Application.DocumentManager;

                Ed.Editor ed = dm.MdiActiveDocument.Editor;

                string sCom = $"Доступные команды:{env.NewLine} PRPR_disconobj - дисконнектор объектов; {env.NewLine} PRPR_importMCDI - импорт файла MCDI; {env.NewLine} PRPR_backupdwg - резервная копия DWG-файла;";
                ed.WriteMessage(sCom);

#if DEBUG
                //для отладки список команд
#endif
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
        [Rtm.CommandMethod("PRPR_backupdwg")]


        public static void backupDwg_mode() // Данный метод только вызывает основную форму
        {
            MainForm mf = new MainForm();
            mf.ShowDialog();
            
        
        }


        /// <summary>
        /// Это основной метод для обработки, он принимает значение номера ревизии (RevisionValue) из формы
        /// </summary>
        public static void backupDwg_mode_1(string RevisionValue)
            {
                      
                Db.Database db = Db.HostApplicationServices.WorkingDatabase;
                App.Document doc = App.Application.DocumentManager.MdiActiveDocument;
                Ed.Editor ed = doc.Editor;
                

            //string fName = db.Filename; // Альтернативный метод получения полного пути и имени текущего dwg-файла
            string dwgName = doc.Name; // метод получения полного пути и имени текущего dwg-файла
            string[] dwgSplitName = dwgName.Split(new char[] { '.' }); // Отделение расширения *.dwg от полного имени файла
            string timeNow = DateTime.Now.ToString().Replace(':', '-'); // Замена двоеточий в формате времени на дефисы
            string dwgNewName = dwgSplitName[0] + "_back_" + RevisionValue + "_" + timeNow + ".dwg"; // Сборка итоговой полной строки с путем к файлу с новым именем
            
            //DialogResult res = MessageBox.Show("Вы действительно хотите сделать резервную копию текущего dwg-файла? " + Environment.NewLine + dwgNewName, "Подтверждение резервирования", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            //if (res == DialogResult.OK){
                db.SaveAs(dwgNewName, Db.DwgVersion.Current, false);

            if (File.Exists(dwgNewName))
            {
                MessageBox.Show("Резервная копия успешно создана!");
            }
            else { 
                MessageBox.Show("Ошибка создания файла!", "Ошибка!"); 
            }
                
}

        [Rtm.CommandMethod("PRPR_disconobj")]
        public static void disconobj() {

            McObjectId curObjID = mcDBs.McObjectManager.SelectObject("Выберите объект на чертеже"); // Получить ID обьекта 
            McObject curObj = curObjID.GetObject(); // Получить обьект
                    


            // Проверить тип и если соответствует то явно преобразовать для доступа к его методам и свойствам
            if (curObj is McParametricObject curParObj)
            {

                string parObjName = curParObj.Name.ToString();
                McDbEntity entObj = curParObj.DbEntity;

                entObj.Highlight(true);

                DialogResult result = MessageBox.Show($"Это реально параметрический объект: {parObjName} {env.NewLine} Хотите разорвать все связи этого объекта?", "Про связи обьекта", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    curParObj.Disconnect();
                }
            }

            else {
                MessageBox.Show($"Не суй мне фуфло! {env.NewLine} Это не параметрический обжект вовсе! {env.NewLine} Я такое не ем!");
            }
        }

        [Rtm.CommandMethod("PRPR_importMCDI")]
        public static void importMCDI() {

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


            // Подключениек текущей БД
            Connection spdsDB = new Connection(); 

            // Сборка полного пути к MCDI файлу для импорта
            string MCDIfileFOrImport = MCDIfolderPath + MCDIfilename;

            // Получение времени изменения файла MCDI
            DateTime MCDIfileDate = File.GetLastWriteTime(MCDIfileFOrImport);

            string curBD = McParamManager.GetStringParam(9); //получаем путь к  текущей базе СПДС (9 - это порядковый номер параметра в конфиг файле настроек СПДС .xml)

            bool isLocalhostDB = curBD.Contains(":localhost");
            bool isLocalDB = curBD.Contains("");
            int countOf = curBD.Split(":").Length - 1;

            // Определить какие права у текущего пользователя БД
            string curUserRole = spdsDB.GetUserRole().ToString();

            DialogResult result = MessageBox.Show($"Хотите импортировать в текущую базу данных файл? {env.NewLine}{MCDIfilename} {env.NewLine}обновлен: {MCDIfileDate.ToString()}", "Импорт в локальную БД", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes) {

                // Выбор места в структуре БД для импорта (корень или подкаталог)
                if (importToRoot) // Импорт в корень БД (если включена настройка в конфиге)
            {
                // Переход в корень дерева БД
                Folder rootDBfolder = spdsDB.GetRoot();
                rootDBfolder.Import(MCDIfileFOrImport); // Непосредственно операция импорта MCDI в БД
                
                    MessageBox.Show($"Импорт {MCDIfilename} успешно выполнен в корневой каталог БД", "Импорт корень в локальной БД", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            else {
                // Переход в заданный "DBfolderName" подкаталог дерева БД
                Folder DBsubFolder = spdsDB.GetRoot().GetSubFolder(DBfolderName); // Получение целевого подкаталога в дереве БД
                DBsubFolder.Import(MCDIfileFOrImport); // Непосредственно операция импорта MCDI в подкаталог БД
                
                    MessageBox.Show($"Импорт {MCDIfilename} успешно выполнен в следующий каталог БД: {DBsubFolder.ToString()}", "Импорт в локальную БД", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
                

             }

            // McParamManager.SetParam("полный путь БД", 9); // Установить подключение к заданной БД


        }


                
}

        }


            #endregion