using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("BusinessEntityAddress", Schema = "Person")]
    public partial class BusinessEntityAddress
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        [Key]
        public int AddressId { get; set; }
        [Key]
        public int AddressTypeId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (BusinessEntityId == default(int) && AddressId == default(int) && AddressTypeId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Person].[BusinessEntityAddress]
                (
                    [AddressID],
                    [AddressTypeID],
                    [BusinessEntityID],
                    [ModifiedDate]
                )
                VALUES
                (
                    @AddressId,
                    @AddressTypeId,
                    @BusinessEntityId,
                    @ModifiedDate
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Person].[BusinessEntityAddress] SET
                    [AddressID] = @AddressId,
                    [AddressTypeID] = @AddressTypeId,
                    [BusinessEntityID] = @BusinessEntityId,
                    [ModifiedDate] = @ModifiedDate
                WHERE
                    [BusinessEntityID] = @BusinessEntityId AND 
                    [AddressID] = @AddressId AND 
                    [AddressTypeID] = @AddressTypeId";
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
            BusinessEntityAddress other = obj as BusinessEntityAddress;
            if (other == null) return false;

            if (AddressId != other.AddressId)
                return false;
            if (AddressTypeId != other.AddressTypeId)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AddressId == default(int) ? 0 : AddressId.GetHashCode());
                hash = hash * 23 + (AddressTypeId == default(int) ? 0 : AddressTypeId.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(BusinessEntityAddress left, BusinessEntityAddress right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BusinessEntityAddress left, BusinessEntityAddress right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
