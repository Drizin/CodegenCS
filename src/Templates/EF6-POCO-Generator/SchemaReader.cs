using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EF6POCOGenerator
{
    public abstract class SchemaReader
    {
        protected readonly DbCommand Cmd;
        protected readonly System.IO.TextWriter Writer;

        protected SchemaReader(DbConnection connection, System.IO.TextWriter writer)
        {
            Cmd = connection.CreateCommand();
            if (Cmd != null)
                Cmd.Connection = connection;
            this.Writer = writer;
        }

        public abstract Tables ReadSchema();
        public abstract List<StoredProcedure> ReadStoredProcs();
        public abstract List<ForeignKey> ReadForeignKeys();
        public abstract void ProcessForeignKeys(List<ForeignKey> fkList, Tables tables, bool checkForFkNameClashes);
        public abstract void IdentifyForeignKeys(List<ForeignKey> fkList, Tables tables);
        public abstract void ReadIndexes(Tables tables);
        public abstract void ReadExtendedProperties(Tables tables, bool commentsInSummaryBlock);

        protected void WriteLine(string o)
        {
            this.Writer?.WriteLine(o);
        }

        protected bool IsFilterExcluded(Regex filterExclude, Regex filterInclude, string name)
        {
            if (filterExclude != null && filterExclude.IsMatch(name))
                return true;
            if (filterInclude != null && !filterInclude.IsMatch(name))
                return true;
            if (name.Contains('.'))    // EF does not allow tables to contain a period character
                return true;
            return false;
        }
    }
}
