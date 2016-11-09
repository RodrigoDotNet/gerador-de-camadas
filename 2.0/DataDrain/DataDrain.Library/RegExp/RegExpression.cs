using System.Collections.Generic;

namespace DataDrain.Library.RegExp
{
    public static class RegExpression
    {
        /// <summary>
        /// REtorna uma lista com as Regular Expression padrão
        /// </summary>
        /// <returns></returns>
        public static List<DadosRegExpression> RetornaRegularExpressions()
        {
            return new List<DadosRegExpression>
            {
                new DadosRegExpression("Apenas números [0 á 9]","^[0-9]+$"),
                new DadosRegExpression("Apenas letras [A-Z]","^[a-zA-Z]+$"),
                new DadosRegExpression("Alfanumérico [A-Z e 0 á 9]","^[a-zA-Z0-9]+$"),
                new DadosRegExpression("Numérico [0 á 9]","^[0-9]+$"),
                new DadosRegExpression("Telefone - [(99)9999-9999]",@"^\(\d{2}\)\d{4}-\d{4}$"),
                new DadosRegExpression("CEP - [99.999-999]",@"^\d{2}.\d{3}-\d{3}$"),
                new DadosRegExpression("E-mail",@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"),
                new DadosRegExpression("Url",@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"),
                new DadosRegExpression("IP",@"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$"),
                new DadosRegExpression("Caminho Fisico",@"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$"),
                new DadosRegExpression("Senha segura",@"^.*(?=.{6,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).*$"),
                new DadosRegExpression("GUID",@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$"),
                new DadosRegExpression("String sem espaço",@"^[\S]+$"),
                new DadosRegExpression("Valida Hexadecimal","^([A-Fa-f0-9]{2}){8,9}$"),
                new DadosRegExpression("Valida Hash MD5","^[0-9a-f]{32}$"),
                new DadosRegExpression("Valida Hash SHA1","^[0-9a-f]{40}$")
            };
        }
    }
}
