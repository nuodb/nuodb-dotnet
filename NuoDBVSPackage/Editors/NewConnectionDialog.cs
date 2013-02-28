using System.Windows.Forms;

namespace NuoDb.VisualStudio.DataTools.Editors
{
    public partial class NewConnectionDialog : Form
    {
        public NewConnectionDialog()
        {
            InitializeComponent();
        }

        public string Server
        {
            get
            {
                object value;
                if (nuoDbDataConnectionUIControl1.ConnectionStringBuilder.TryGetValue("Server", out value))
                    return value.ToString();
                return "";
            }
        }

        public string User
        {
            get
            {
                object value;
                if (nuoDbDataConnectionUIControl1.ConnectionStringBuilder.TryGetValue("User", out value))
                    return value.ToString();
                return "";
            }
        }

        public string ConnectionString
        {
            get { return nuoDbDataConnectionUIControl1.ConnectionStringBuilder.ConnectionString; }
            set { }
        }
    }
}
