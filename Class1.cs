
    using App = HostMgd.ApplicationServices;
    using Db = Teigha.DatabaseServices;
    using Ed = HostMgd.EditorInput;
    using Rtm = Teigha.Runtime;
    using System.Windows.Forms;



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

                string sCom = "backupdwg" + "\tСоздание резервных копий dwg";
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
            [Rtm.CommandMethod("backupdwg")]
            public void backupDwg_mode_1()
            {
                Db.Database db = Db.HostApplicationServices.WorkingDatabase;
                App.Document doc = App.Application.DocumentManager.MdiActiveDocument;
                Ed.Editor ed = doc.Editor;


            //string fName = db.Filename; // Альтернативный метод получения полного пути и имени текущего dwg-файла
            string dwgName = doc.Name; // метод получения полного пути и имени текущего dwg-файла
            string[] dwgSplitName = dwgName.Split(new char[] { '.' }); // Отделение расширения *.dwg от полного имени файла
            string timeNow = DateTime.Now.ToString().Replace(':', '-'); // Замена двоеточий в формате времени на дефисы
            string dwgNewName = dwgSplitName[0] + "_back_" + timeNow + ".dwg"; // Сборка итоговой полной строки с путем к файлу с новым именем

            DialogResult res = MessageBox.Show("Вы действительно хотите сделать резервную копию текущего dwg-файла? " + Environment.NewLine + dwgNewName, "Подтверждение резервирования", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (res == DialogResult.OK)
            {

                db.SaveAs(dwgNewName, Db.DwgVersion.Current, false);
                
                MessageBox.Show("Резервная копия успешно создана!");
}
            if (res == DialogResult.Cancel)

            {
                
}


        }


            #endregion
        }
    }


