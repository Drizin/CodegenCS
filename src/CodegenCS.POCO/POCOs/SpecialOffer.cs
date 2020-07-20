using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class SpecialOffer
    {
        #region Members
        [Key]
        public int SpecialOfferId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal DiscountPct { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxQty { get; set; }
        public int MinQty { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime StartDate { get; set; }
        public string Type { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (SpecialOfferId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [SpecialOffer]
                (
                    [Category],
                    [Description],
                    [DiscountPct],
                    [EndDate],
                    [MaxQty],
                    [MinQty],
                    [ModifiedDate],
                    [StartDate],
                    [Type]
                )
                VALUES
                (
                    @Category,
                    @Description,
                    @DiscountPct,
                    @EndDate,
                    @MaxQty,
                    @MinQty,
                    @ModifiedDate,
                    @StartDate,
                    @Type
                )";

                this.SpecialOfferId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [SpecialOffer] SET
                    [Category] = @Category,
                    [Description] = @Description,
                    [DiscountPct] = @DiscountPct,
                    [EndDate] = @EndDate,
                    [MaxQty] = @MaxQty,
                    [MinQty] = @MinQty,
                    [ModifiedDate] = @ModifiedDate,
                    [StartDate] = @StartDate,
                    [Type] = @Type
                WHERE
                    [SpecialOfferID] = @SpecialOfferId";
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
            SpecialOffer other = obj as SpecialOffer;
            if (other == null) return false;

            if (Category != other.Category)
                return false;
            if (Description != other.Description)
                return false;
            if (DiscountPct != other.DiscountPct)
                return false;
            if (EndDate != other.EndDate)
                return false;
            if (MaxQty != other.MaxQty)
                return false;
            if (MinQty != other.MinQty)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (StartDate != other.StartDate)
                return false;
            if (Type != other.Type)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Category == null ? 0 : Category.GetHashCode());
                hash = hash * 23 + (Description == null ? 0 : Description.GetHashCode());
                hash = hash * 23 + (DiscountPct == default(decimal) ? 0 : DiscountPct.GetHashCode());
                hash = hash * 23 + (EndDate == default(DateTime) ? 0 : EndDate.GetHashCode());
                hash = hash * 23 + (MaxQty == null ? 0 : MaxQty.GetHashCode());
                hash = hash * 23 + (MinQty == default(int) ? 0 : MinQty.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (StartDate == default(DateTime) ? 0 : StartDate.GetHashCode());
                hash = hash * 23 + (Type == null ? 0 : Type.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(SpecialOffer left, SpecialOffer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SpecialOffer left, SpecialOffer right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
