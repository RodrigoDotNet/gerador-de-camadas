//using System.Collections.Generic;
//using System.Linq;
//using DataDrain.ORM.Generator.Apoio;
//using DevExpress.Data.Mask;

//namespace DataDrain.ORM.Generator.Controles.AutoComplete
//{
//    class MnLogins : MaskManagerCommon
//    {

//        private List<string> Dados { get; set; }

//        public MnLogins(string servidor)
//        {
//            Dados = !string.IsNullOrWhiteSpace(servidor) ? Historico.RetornaListaNomeLogins(servidor) : new List<string>();
//        }

//        public override bool Insert(string insertion)
//        {

//            var changeType = (insertion.Length == 0 && IsSelection) ? StateChangeType.Delete : StateChangeType.Insert;
//            var head = GetCurrentEditText().Substring(0, DisplaySelectionStart);
//            var tail = GetCurrentEditText().Substring(DisplaySelectionEnd);
//            var newText = string.Format("{0}{1}{2}", head, insertion, tail);

//            if (!string.IsNullOrWhiteSpace(insertion))
//            {
//                var retorno = Dados.FirstOrDefault(d => d.StartsWith(newText));

//                return !string.IsNullOrWhiteSpace(retorno)
//                           ? Apply(retorno, newText.Length, retorno.Length, changeType)
//                           : Apply(newText, (head + insertion).Length, (head + insertion).Length, changeType);
//            }

//            return Apply(newText, (head + insertion).Length, (head + insertion).Length, changeType);

//        }
//    }
//}
