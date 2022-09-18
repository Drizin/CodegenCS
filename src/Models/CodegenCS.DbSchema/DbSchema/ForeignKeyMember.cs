namespace CodegenCS.DbSchema
{
    public class ForeignKeyMember
    {
        public int PKColumnOrdinalPosition { get; set; }
        public string PKColumnName { get; set; }

        public string FKColumnName { get; set; }
    }
}
