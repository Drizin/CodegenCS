using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using System.ComponentModel;

namespace CodegenCS.AdventureWorksPOCOSample
{
    [Table("JobCandidate", Schema = "HumanResources")]
    public partial class JobCandidate : INotifyPropertyChanged
    {
        #region Members
        private int _jobCandidateId;
        [Key]
        public int JobCandidateId 
        { 
            get { return _jobCandidateId; } 
            set { SetField(ref _jobCandidateId, value, nameof(JobCandidateId)); } 
        }
        private int? _businessEntityId;
        public int? BusinessEntityId 
        { 
            get { return _businessEntityId; } 
            set { SetField(ref _businessEntityId, value, nameof(BusinessEntityId)); } 
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate 
        { 
            get { return _modifiedDate; } 
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); } 
        }
        private string _resume;
        public string Resume 
        { 
            get { return _resume; } 
            set { SetField(ref _resume, value, nameof(Resume)); } 
        }
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
                INSERT INTO [HumanResources].[JobCandidate]
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
                UPDATE [HumanResources].[JobCandidate] SET
                    [BusinessEntityID] = @BusinessEntityId,
                    [ModifiedDate] = @ModifiedDate,
                    [Resume] = @Resume
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

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}
