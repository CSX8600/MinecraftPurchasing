namespace ClussPro.Base.Data
{
    public class FieldSpecification
    {
        public enum FieldTypes
        {
            NVarChar,
            BigInt,
            Binary
        }

        public FieldTypes FieldType { get; set; }
        public int DataSize { get; set; }
        public int DataScale { get; set; }
        public object DefaultValue { get; set; }
        public bool IsPrimary { get; set; }

        public FieldSpecification(FieldTypes fieldType, int dataSize, int dataScale)
        {
            this.FieldType = fieldType;
            this.DataSize = dataSize;
            this.DataScale = dataScale;
        }

        public FieldSpecification(FieldTypes fieldType, int dataSize) : this(fieldType, dataSize, -1) { }
        public FieldSpecification(FieldTypes fieldType) : this(fieldType, -1, -1) { }
    }
}
