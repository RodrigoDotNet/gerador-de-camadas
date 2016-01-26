//using DataDrain.ORM.Generator.Apoio;
//using DevExpress.Data.Mask;
//using System.Collections.Generic;
//using System.Linq;

//namespace DataDrain.ORM.Generator.Controles.AutoComplete
//{
//    public class MnServidores : MaskManagerCommon
//    {

//        private List<string> Dados { get; set; }

//        public MnServidores()
//        {
//            Dados = Historico.RetornaListaNomeServidores();
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
//            else
//            {
//                return Apply(newText, (head + insertion).Length, (head + insertion).Length, changeType);
//            }
//            //const string sampleAutocompleteText = "_SampleAutocompleteText";

//            //if (changeType == StateChangeType.Insert && tail.Length == 0 && insertion.Length == 1)
//            //{
//            //    var autocompletedText = string.Format("{0}{1}", newText, sampleAutocompleteText);
//            //    return Apply(autocompletedText, newText.Length, autocompletedText.Length, changeType);
//            //}
//            //var cursorPosition = (head + insertion).Length;
//            //return Apply(newText, cursorPosition, cursorPosition, changeType);
//        }

//    }
//}
