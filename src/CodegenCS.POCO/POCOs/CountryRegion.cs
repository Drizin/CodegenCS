using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class CountryRegion
    {
        #region Members
        [Key]
        public string CountryRegionCode { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CountryRegionCode == null)
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [CountryRegion]
                (
                    [CountryRegionCode],
                    [ModifiedDate],
                    [Name]
                )
                VALUES
                (
                    @CountryRegionCode,
                    @ModifiedDate,
                    @Name
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [CountryRegion]
                    SET [CountryRegionCode] = @CountryRegionCode,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [Name] = @Name
                WHERE
                    [CountryRegionCode] = @CountryRegionCode";
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
            CountryRegion other = obj as CountryRegion;
            if (other == null) return false;

            if (CountryRegionCode != other.CountryRegionCode)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CountryRegionCode == null ? 0 : CountryRegionCode.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(CountryRegion left, CountryRegion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CountryRegion left, CountryRegion right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
