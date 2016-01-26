//using System;
//using System.Drawing;
//using DevExpress.Data.Mask;
//using DevExpress.XtraEditors;
//using DevExpress.XtraEditors.Mask;

//namespace DataDrain.ORM.Generator.Controles
//{
//    [ToolboxBitmap(typeof(TextEdit))]
//    class TextEditPlus : TextEdit
//    {

//        public TextEditPlus()
//        {
//            Properties.Mask.AutoComplete = AutoCompleteType.Optimistic;
//            Properties.Mask.MaskType = MaskType.Custom;
//        }

//        protected override MaskManager CreateMaskManager(MaskProperties mask)
//        {
//            try
//            {
//                if (!string.IsNullOrWhiteSpace(Name))
//                {
//                    switch (Name)
//                    {
//                        case "txtServidor":
//                            return mask.MaskType == MaskType.Custom
//                                    ? AutoCompleteSource
//                                    : base.CreateMaskManager(mask);
//                        default:
//                            return AutoCompleteSource;
//                    }
//                }
//                return base.CreateMaskManager(new NumericMaskProperties());
//            }
//            catch (Exception e)
//            {
//                return base.CreateMaskManager(new MaskProperties());
//            }
//        }

//        /// <summary>
//        /// Dados usados para o AutoComplete
//        /// </summary>
//        public MaskManagerCommon AutoCompleteSource { get; set; }
//    }


//}
