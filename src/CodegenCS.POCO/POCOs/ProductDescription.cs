using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class ProductDescription
    {
        #region Members
        [Key]
        public int ProductDescriptionId { get; set; }
        public string Description { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid Rowguid { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (ProductDescriptionId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [ProductDescription]
                (
                    [Description],
                    [ModifiedDate]
                )
                VALUES
                (
                    @Description,
                    @ModifiedDate
                )";

                this.ProductDescriptionId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [ProductDescription]
                    SET [Description] = @Description,
                    SET [ModifiedDate] = @ModifiedDate
                WHERE
                    [ProductDescriptionID] = @ProductDescriptionId";
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
            ProductDescription other = obj as ProductDescription;
            if (other == null) return false;

            if (Description != other.Description)
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
                hash = hash * 23 + (Description == null ? 0 : Description.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(ProductDescription left, ProductDescription right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductDescription left, ProductDescription right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
