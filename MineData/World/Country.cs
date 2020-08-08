using ClussPro.ObjectBasedFramework;
using ClussPro.ObjectBasedFramework.Schema.Attributes;
using System.Collections.Generic;

namespace MineData.World
{
    [Table("AAAA9683-1D61-4436-A1A3-C656FDD7711E")]
    public class Country : DataObject
    {
        private long? _countryID = null;
        [Field("854A00D6-0066-4487-8893-FB0A74609219")]
        public long? CountryID
        {
            get { CheckGet(); return _countryID; }
        }

        private string _name;
        [Field("F75F3185-DB78-4890-BEE2-BAE9D348515D")]
        public string Name
        {
            get { CheckGet(); return _name; }
            set { CheckSet(); _name = value; }
        }

        #region Relationships
        #region World
        private List<City> _cities = new List<City>();
        [RelationshipList("E2CBFCCF-5801-4305-9C43-E7984506EA38", "CountryID")]
        public IReadOnlyCollection<City> Cities
        {
            get { CheckGet(); return _cities; }
        }
        #endregion
        #endregion
    }
}
