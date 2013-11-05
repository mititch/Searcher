namespace FileSearch.Visual
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Lib;

    public class VisualBinding
    {
        public TextBox TextBox { get; set; }
        public Button CancelBtn { get; set; }
        public Button SearchBtn { get; set; }
        public Label Label { get; set; }
        public Result Result { get; set; }

    }
}
