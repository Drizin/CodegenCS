using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class Address
    {
        #region Members
        [Key]
        public int AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string PostalCode { get; set; }
        public Guid Rowguid { get; set; }
        public int StateProvinceId { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (AddressId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Address]
                (
                    [AddressLine1],
                    [AddressLine2],
                    [City],
                    [ModifiedDate],
                    [PostalCode],
                    [StateProvinceID]
                )
                VALUES
                (
                    @AddressLine1,
                    @AddressLine2,
                    @City,
                    @ModifiedDate,
                    @PostalCode,
                    @StateProvinceId
                )";

                this.AddressId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Address]
                    SET [AddressLine1] = @AddressLine1,
                    SET [AddressLine2] = @AddressLine2,
                    SET [City] = @City,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [PostalCode] = @PostalCode,
                    SET [StateProvinceID] = @StateProvinceId
                WHERE
                    [AddressID] = @AddressId";
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
            Address other = obj as Address;
            if (other == null) return false;

            if (AddressLine1 != other.AddressLine1)
                return false;
            if (AddressLine2 != other.AddressLine2)
                return false;
            if (City != other.City)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (PostalCode != other.PostalCode)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StateProvinceId != other.StateProvinceId)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AddressLine1 == null ? 0 : AddressLine1.GetHashCode());
                hash = hash * 23 + (AddressLine2 == null ? 0 : AddressLine2.GetHashCode());
                hash = hash * 23 + (City == null ? 0 : City.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (PostalCode == null ? 0 : PostalCode.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StateProvinceId == default(int) ? 0 : StateProvinceId.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
