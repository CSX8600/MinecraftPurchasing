using ClussPro.ObjectBasedFramework;
using ClussPro.ObjectBasedFramework.Schema.Attributes;

namespace MineData.World
{
    [Table("27FC741C-81A8-433C-ABD7-FE86CCA8082B")]
    public class City : DataObject
    {
        private long? _cityID = null;
        [Field("5B33F2AA-D893-4ADE-9BE0-D9F5168990C4")]
        public long? CityID
        {
            get { CheckGet(); return _cityID; }
        }

        private long? _countryID;
        [Field("604CB70B-3536-4207-BF5E-7F3A885BD8AB")]
        public long? CountryID
        {
            get { CheckGet(); return _countryID; }
            set { CheckSet(); _countryID = value; }
        }

        private Country _country = null;
        [Relationship("72AE7058-887B-4F15-9575-D204769B33B9")]
        public Country Country
        {
            get { CheckGet(); return _country; }
        }

        private string _name;
        [Field("17E0CCBB-AB88-49B7-80D0-8A577E99FDB0", DataSize = 30)]
        public string Name
        {
            get { CheckGet(); return _name; }
            set { CheckSet(); _name = value; }
        }
    }
}
