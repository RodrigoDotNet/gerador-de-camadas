using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using DataDrain.Library.Helpers;

namespace DataDrain.IO
{
    public class OrmDirectory
    {
        public static KeyValuePair<bool, string> Create(string caminho)
        {
            if (!IoHelper.VerifyVermission(FileIOPermissionAccess.Write, caminho))
            {
                return new KeyValuePair<bool, string>(false, "No write permission in the directory");
            }

            try
            {
                Directory.CreateDirectory(string.Format("{0}\\BLL", caminho));
                Directory.CreateDirectory(string.Format("{0}\\BLL\\Validacao", caminho));
                Directory.CreateDirectory(string.Format("{0}\\BLL\\Base", caminho));
                Directory.CreateDirectory(string.Format("{0}\\BLL\\Properties", caminho));

                Directory.CreateDirectory(string.Format("{0}\\DAL", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\Apoio", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\Apoio\\Base", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\Apoio\\CommandMap", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\Apoio\\Conexao", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\Apoio\\Enumeradores", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\DataDrain", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\DataDrain\\Factories", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\DataDrain\\TSqlORM", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\ORM", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\ORM\\Caching", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\ORM\\Caching\\Enuns", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\ORM\\Caching\\Events", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\ORM\\Caching\\Interfaces", caminho));
                Directory.CreateDirectory(string.Format("{0}\\DAL\\Properties", caminho));

                Directory.CreateDirectory(string.Format("{0}\\TO", caminho));
                Directory.CreateDirectory(string.Format("{0}\\TO\\AttributeValidators", caminho));
                Directory.CreateDirectory(string.Format("{0}\\TO\\Properties", caminho));

                Directory.CreateDirectory(string.Format("{0}\\Interfaces", caminho));
                Directory.CreateDirectory(string.Format("{0}\\Interfaces\\Properties", caminho));

                return new KeyValuePair<bool, string>(true, "directories created");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.ToString());
            }

        }
    }
}
