using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class Store
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        public string Demographics { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public Guid Rowguid { get; set; }
        public int? SalesPersonId { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Store]
                (
                    [BusinessEntityID],
                    [Demographics],
                    [ModifiedDate],
                    [Name],
                    [SalesPersonID]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @Demographics,
                    @ModifiedDate,
                    @Name,
                    @SalesPersonId
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Store] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [Demographics] = @Demographics,
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [SalesPersonID] = @SalesPersonId
                WHERE
                    [BusinessEntityID] = @BusinessEntityId";
                conn.Execute(cmd, this);
            }
        }
        #endregion ActiveRecord

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            Store other = obj as Store;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (Demographics != other.Demographics)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SalesPersonId != other.SalesPersonId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (Demographics == null ? 0 : Demographics.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SalesPersonId == null ? 0 : SalesPersonId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Store left, Store right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Store left, Store right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
