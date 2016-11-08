using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace DataDrain.Library.Registry
{
    /// <summary>
    /// Classe responsavel por gravar e recuperar valores no registro do windows
    /// </summary>
    public sealed class RegistroWindows
    {

        private const string Key = @"HKEY_CURRENT_USER\Software\DataDrainORMGenerator";
        private const string KeyConexoes = @"HKEY_CURRENT_USER\Software\DataDrainORMGenerator\Conexoes";
        private const string KeyApp = @"Software\DataDrainORMGenerator\";

        public static string ChaveConexoes { get { return KeyConexoes; } }
        public static string ChaveAplicacao { get { return Key; } }


        /// <summary>
        /// Grava um par chave valor no registro do windows
        /// </summary>
        /// <param name="keyPath">Caminho Base</param>
        /// <param name="chave">chave usada para identificar o objeto</param>
        /// <param name="valor">valor a ser guardado</param>
        public static void GravaValor(string keyPath, string chave, string valor)
        {
            try
            {
                if (string.IsNullOrEmpty(keyPath))
                {
                    throw new ArgumentNullException("keyPath", "O parâmetro  chave não pode ser nulo");
                }

                if (string.IsNullOrEmpty(chave))
                {
                    throw new ArgumentNullException("chave", "O parâmetro  chave não pode ser nulo");
                }

                Microsoft.Win32.Registry.SetValue(keyPath, chave, valor ?? "", RegistryValueKind.String);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Grava um par chave valor no registro do windows
        /// </summary>
        /// <param name="chave">chave usada para identificar o objeto</param>
        /// <param name="valor">valor a ser guardado</param>
        public static void GravaValor(string chave, string valor)
        {
            GravaValor(Key, chave, valor);
        }

        /// <summary>
        /// Recupera um valor do registro baseado na chave
        /// </summary>
        /// <param name="keyPath">Caminho base</param>
        /// <param name="chave">chave usada para localizar o valor</param>
        /// <param name="valorPadrao">Valor que deve ser usado caso não obtenha resultado, caso nulo retorna Exception</param>
        /// <returns>Retorna o objeto referente a chave, caso não encontre volta uma string vazia</returns>
        public static string RecuperaValor(string keyPath, string chave, string valorPadrao)
        {
            try
            {
                if (string.IsNullOrEmpty(keyPath))
                {
                    throw new ArgumentNullException("keyPath", "O parâmetro  chave não pode ser nulo");
                }

                if (string.IsNullOrEmpty(chave))
                {
                    throw new ArgumentNullException("chave", "O parâmetro  chave não pode ser nulo");
                }

                return Microsoft.Win32.Registry.GetValue(keyPath, chave, "").ToString();
            }
            catch (Exception)
            {
                if (valorPadrao == null)
                {
                    throw;
                }
                return valorPadrao;
            }
        }

        /// <summary>
        /// Recupera um valor do registro baseado na chave
        /// </summary>
        /// <param name="chave">chave usada para localizar o valor</param>
        /// <param name="valorPadrao">Valor que deve ser usado caso não obtenha resultado, caso nulo retorna Exception</param>
        /// <returns>Retorna o objeto referente a chave, caso não encontre volta uma string vazia</returns>
        public static string RecuperaValor(string chave, string valorPadrao)
        {
            return RecuperaValor(Key, chave, valorPadrao);
        }

        /// <summary>
        /// Exclui uma chave e seu valor
        /// </summary>
        /// <param name="chave">chave a ser excluida</param>
        /// <returns></returns>
        public static bool ExcluiChave(string chave)
        {
            if (string.IsNullOrEmpty(chave))
            {
                throw new ArgumentNullException("chave", "O parâmetro  chave não pode ser nulo");
            }

            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Key, true))
            {
                if (key == null)
                {
                    return true;
                }

                try
                {
                    key.DeleteValue(chave);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Cria nova pasta (Chave ) no caminho alvo
        /// </summary>
        /// <param name="nomePasta">Nome da pasta</param>
        /// <returns>Retorna o caminho da chave criada</returns>
        public static string CriaChave(string nomePasta)
        {
            if (string.IsNullOrEmpty(nomePasta))
            {
                throw new ArgumentNullException("nomePasta", "O parâmetro  chave não pode ser nulo");
            }

            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(string.Format("{0}{1}", KeyApp, nomePasta));

            if (key == null)
            {
                var nKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(string.Format("{0}\\{1}", KeyApp, nomePasta));

                if (nKey == null)
                {
                    throw new Exception("Não foi possivel criar a chave");
                }

                nKey.Flush();

                return nKey.Name;
            }
            return key.Name;
        }

        /// <summary>
        /// Retorna todos os valores do caminho
        /// </summary>
        /// <param name="chave">caminho alvo</param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> RetornaTodosValoresChave(string chave)
        {
            if (string.IsNullOrEmpty(chave))
            {
                throw new ArgumentNullException("chave", "O parâmetro  chave não pode ser nulo");
            }

            var valorConteudo = new List<KeyValuePair<string, string>>();

            using (var settingsRegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(chave.Replace("HKEY_CURRENT_USER\\", "")))
            {
                if (settingsRegKey != null)
                {
                    var valueNames = settingsRegKey.GetValueNames().ToList();

                    valorConteudo.AddRange(valueNames.Select(valueName => new KeyValuePair<string, string>(valueName, RecuperaValor(chave, valueName, ""))));
                }
            }

            return valorConteudo;
        }

        /// <summary>
        /// Retorna todos os valores do caminho
        /// </summary>
        /// <param name="chave">caminho alvo</param>
        /// <returns></returns>
        public static List<string> RetornaTodasSubChaves(string chave)
        {
            if (string.IsNullOrEmpty(chave))
            {
                throw new ArgumentNullException("chave", "O parâmetro  chave não pode ser nulo");
            }

            var valorConteudo = new List<string>();

            using (var settingsRegKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(chave.Replace("HKEY_CURRENT_USER\\", "")))
            {
                if (settingsRegKey != null)
                {
                    return settingsRegKey.GetSubKeyNames().ToList();
                }
            }

            return valorConteudo;
        }
    }
}

