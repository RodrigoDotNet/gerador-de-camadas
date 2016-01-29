using System.Collections.Generic;
using System.IO;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public class ArquivosObjetos
    {
        public static bool Log { get; set; }

        /// <summary>
        /// Gera os arquivos ORM (TO/DAL/BLL)
        /// </summary>
        public static void Gerar(ParametrosCriarProjetos parametros)
        {
            foreach (var objetoOrm in parametros.ObjetosMapeaveis)
            {
                GeraClassesORM(objetoOrm.Key, objetoOrm.Value, parametros);
            }
        }

        private static void GeraClassesORM(TipoObjetoBanco objOrm, List<DadosColunas> colunasObjeto, ParametrosCriarProjetos parametros)
        {
            var gerador = new Gerador { Log = Log };
            var nomeArquivo = Gerador.RetornaNomeClasseAjustado(objOrm.NomeObjeto);

            var arquivo = gerador.GerarClasseTO(colunasObjeto, objOrm.NomeObjeto, parametros.NameSpace, objOrm.Associacoes, objOrm.Enumns, parametros);
            var arquivoBLL = gerador.GerarClasseBLL(nomeArquivo, parametros.NameSpace, objOrm.TipoObjeto, objOrm.Parametros);
            var arquivoDAL = gerador.GerarClasseDAL(nomeArquivo, parametros.NameSpace, objOrm.TipoObjeto, objOrm.Parametros);

            File.WriteAllText(string.Format("{0}\\TO\\{1}TO.cs", parametros.CaminhoDestino, nomeArquivo), arquivo);
            File.WriteAllText(string.Format("{0}\\BLL\\{1}BLL.cs", parametros.CaminhoDestino, nomeArquivo), arquivoBLL);
            File.WriteAllText(string.Format("{0}\\DAL\\{1}DAL.cs", parametros.CaminhoDestino, nomeArquivo), arquivoDAL);
        }
    }
}
