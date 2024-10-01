
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

                string sCom = "disconobj" + "\tдисконнектор объектов";
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
        [Rtm.CommandMethod("disconobj")]


        public static void backupDwg_mode() // Данный метод только вызывает основную форму
        {
            //MainForm mf = new MainForm();
            //mf.ShowDialog();
            getParamObj();   
        
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
        public static void getParamObj() {

            McObjectId curObjID = mcDBs.McObjectManager.SelectObject("Выберите объект на чертеже"); // Получить ID обьекта 
            McObject curObj = curObjID.GetObject(); // Получить обьект
                    


            // Проверить тип и если соответствует то явно преобразовать для доступа к его методам и свойствам
            if (curObj is McParametricObject curParObj)
            {

                 

                string parObjName = curParObj.Name.ToString();

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



//string parObjDepts = curParObj.GetDependent().ToString();
                //string parObjDeptFrom = curParObj.GetDependsOn(true, true).ToString();

                
}



        }


            #endregion

