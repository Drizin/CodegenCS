using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Vendor", Schema = "Purchasing")]
    public partial class Vendor
    {
        #region Members
        [Key]
        public int BusinessEntityId { get; set; }
        public string AccountNumber { get; set; }
        public bool ActiveFlag { get; set; }
        public byte CreditRating { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public bool PreferredVendorStatus { get; set; }
        public string PurchasingWebServiceUrl { get; set; }
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
                INSERT INTO [Purchasing].[Vendor]
                (
                    [AccountNumber],
                    [ActiveFlag],
                    [BusinessEntityID],
                    [CreditRating],
                    [ModifiedDate],
                    [Name],
                    [PreferredVendorStatus],
                    [PurchasingWebServiceURL]
                )
                VALUES
                (
                    @AccountNumber,
                    @ActiveFlag,
                    @BusinessEntityId,
                    @CreditRating,
                    @ModifiedDate,
                    @Name,
                    @PreferredVendorStatus,
                    @PurchasingWebServiceUrl
                )";

                conn.Execute(cmd, this);
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Purchasing].[Vendor] SET
                    [AccountNumber] = @AccountNumber,
                    [ActiveFlag] = @ActiveFlag,
                    [BusinessEntityID] = @BusinessEntityId,
                    [CreditRating] = @CreditRating,
                    [ModifiedDate] = @ModifiedDate,
                    [Name] = @Name,
                    [PreferredVendorStatus] = @PreferredVendorStatus,
                    [PurchasingWebServiceURL] = @PurchasingWebServiceUrl
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
            Vendor other = obj as Vendor;
            if (other == null) return false;

            if (AccountNumber != other.AccountNumber)
                return false;
            if (ActiveFlag != other.ActiveFlag)
                return false;
            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (CreditRating != other.CreditRating)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (PreferredVendorStatus != other.PreferredVendorStatus)
                return false;
            if (PurchasingWebServiceUrl != other.PurchasingWebServiceUrl)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (AccountNumber == null ? 0 : AccountNumber.GetHashCode());
                hash = hash * 23 + (ActiveFlag == default(bool) ? 0 : ActiveFlag.GetHashCode());
                hash = hash * 23 + (BusinessEntityId == default(int) ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (CreditRating == default(byte) ? 0 : CreditRating.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (PreferredVendorStatus == default(bool) ? 0 : PreferredVendorStatus.GetHashCode());
                hash = hash * 23 + (PurchasingWebServiceUrl == null ? 0 : PurchasingWebServiceUrl.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Vendor left, Vendor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Vendor left, Vendor right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
