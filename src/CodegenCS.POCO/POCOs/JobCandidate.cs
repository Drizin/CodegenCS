using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public partial class JobCandidate
    {
        #region Members
        [Key]
        public int JobCandidateId { get; set; }
        public int? BusinessEntityId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Resume { get; set; }
        #endregion Members

        #region ActiveRecord
        public void Save()
        {
            if (JobCandidateId == default(int))
                Insert();
            else
                Update();
        }
        public void Insert()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                INSERT INTO [JobCandidate]
                (
                    [BusinessEntityID],
                    [ModifiedDate],
                    [Resume]
                )
                VALUES
                (
                    @BusinessEntityId,
                    @ModifiedDate,
                    @Resume
                )";

                this.JobCandidateId = conn.Query<int>(cmd + "SELECT SCOPE_IDENTITY();", this).Single();
            }
        }
        public void Update()
        {
            using (var conn = IDbConnectionFactory.CreateConnection())
            {
                string cmd = @"
                UPDATE [JobCandidate]
                    SET [BusinessEntityID] = @BusinessEntityId,
                    SET [ModifiedDate] = @ModifiedDate,
                    SET [Resume] = @Resume
                WHERE
                    [JobCandidateID] = @JobCandidateId";
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
            JobCandidate other = obj as JobCandidate;
            if (other == null) return false;

            if (BusinessEntityId != other.BusinessEntityId)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Resume != other.Resume)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (BusinessEntityId == null ? 0 : BusinessEntityId.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Resume == null ? 0 : Resume.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(JobCandidate left, JobCandidate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(JobCandidate left, JobCandidate right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode
    }
}
