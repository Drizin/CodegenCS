using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class CountryRegionCurrency
    {
        #region Members
        [Key]
        public string CountryRegionCode { get; set; }
        [Key]
        public string CurrencyCode { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (CountryRegionCode == null && CurrencyCode == null)
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [CountryRegionCurrency]
                (
                    [CountryRegionCode],
                    [CurrencyCode],
                    [ModifiedDate]
                )
                VALUES
                (
                    @CountryRegionCode,
                    @CurrencyCode,
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
                UPDATE [CountryRegionCurrency] SET
                    [CountryRegionCode] = @CountryRegionCode,
                    [CurrencyCode] = @CurrencyCode,
                    [ModifiedDate] = @ModifiedDate
                WHERE
                    [CountryRegionCode] = @CountryRegionCode AND 
                    [CurrencyCode] = @CurrencyCode";
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
            CountryRegionCurrency other = obj as CountryRegionCurrency;
            if (other == null) return false;

            if (CountryRegionCode != other.CountryRegionCode)
                return false;
            if (CurrencyCode != other.CurrencyCode)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (CountryRegionCode == null ? 0 : CountryRegionCode.GetHashCode());
                hash = hash * 23 + (CurrencyCode == null ? 0 : CurrencyCode.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(CountryRegionCurrency left, CountryRegionCurrency right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CountryRegionCurrency left, CountryRegionCurrency right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
