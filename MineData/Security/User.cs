using ClussPro.ObjectBasedFramework;
using ClussPro.ObjectBasedFramework.Schema.Attributes;

namespace MineData.Security
{
    [Table("5C5EC1C6-6823-4F84-A78E-05B41726665E")]
    public class User : DataObject
    {
        private long? _userID = null;
        [Field("AE2E56A7-FB1C-4921-8720-EDF0A37A5F59")]
        public long? UserID
        {
            get { CheckGet(); return _userID; }
        }

        private string _username;
        [Field("73D9E9E3-82E2-48B0-B442-2975CC3E44CF", DataSize = 30)]
        public string Username
        {
            get { CheckGet(); return _username; }
            set { CheckSet(); _username = value; }
        }

        private byte[] _password;
        [Field("D6AD1A77-05B2-4B1A-8B43-DEC88F196B6B")]
        public byte[] Password
        {
            get { CheckGet(); return _password; }
            set { CheckSet(); _password = value; }
        }
    }
}
