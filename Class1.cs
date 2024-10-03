
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
    using env = System.Environment;
    using DBserv = Multicad.DatabaseServices;
    using Multicad.DataServices;
    using IniFiles;

[assembly: Rtm.CommandClass(typeof(Tools.CadCommand))]

namespace Tools
    {
        /// <summary> 
        /// Комманды
        /// </summary>
        class CadCommand : Rtm.IExtensionApplication
        {

        

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

            // Чтение файла конфигурации
            IniFile INI = new IniFile("D:\\Soft\\nCAD\\MCDI\\prPr_DBConfig.ini");

            string MCDIfolderPath = INI.ReadINI("MCDI", "MCDIfolderPath");
            string MCDIfilename = INI.ReadINI("MCDI", "MCDIfilename");

            string DBfolderName = INI.ReadINI("spdsDB", "DBfolderName");
            string DBname = INI.ReadINI("spdsDB", "DBname");

            // Подключениек текущей БД
            Connection spdsDB = new Connection(); 

            // Сборка полного пути к MCDI файлу для импорта
            string MCDIfileFOrImport = MCDIfolderPath + MCDIfilename;

            // Проверка места в структуре БД для импорта
            if (DBfolderName == "DBroot") // Импорт в корень БД
            {
                // Переход в корень дерева БД
                Folder rootDBfolder = spdsDB.GetRoot();
                rootDBfolder.Import(MCDIfileFOrImport); // Непосредственно операция импорта MCDI в БД
            }

            else {
                // Переход в заданный "DBfolderName" подкаталог дерева БД
                Folder DBfolder = spdsDB.GetRoot("PRPR_DBfolderName", true); // true - создать подкаталог если его не существует в дереве БД
                DBfolder.GetSubFolder(MCDIfileFOrImport);
            }
            

            


        }


                
}



        }


            #endregion

