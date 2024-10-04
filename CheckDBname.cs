using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;

namespace prPr_FileBackupper
{
    internal class isLocalDB
    {
        public isLocalDB(string DBname) {
            bool result = false;
            bool isLocalhostDB = DBname.Contains(":localhost");
            bool isPGsqlDB = DBname.Contains("pgsql:");
            //int countOf = DBname.Split(":").Length - 1;

            if ((DBname.Split(":").Length - 1) > 1 && DBname.Contains(":localhost") == false)
            {
                result = true; // Это сетевая БД если есть два двоеточия : и нет localhost
            }

            else { 
            
            }

            //return result;
        }
    }
}
