using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("Illustration", Schema = "Production")]
    public partial class Illustration
    {
        #region Members
        [Key]
        public int IllustrationId { get; set; }
        public string Diagram { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (IllustrationId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [Production].[Illustration]
                (
                    [Diagram],
                    [ModifiedDate]
                )
                VALUES
                (
                    @Diagram,
                    @ModifiedDate
                )";

                this.IllustrationId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [Production].[Illustration] SET
                    [Diagram] = @Diagram,
                    [ModifiedDate] = @ModifiedDate
                WHERE
                    [IllustrationID] = @IllustrationId";
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
            Illustration other = obj as Illustration;
            if (other == null) return false;

            if (Diagram != other.Diagram)
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
                hash = hash * 23 + (Diagram == null ? 0 : Diagram.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Illustration left, Illustration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Illustration left, Illustration right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
