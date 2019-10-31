using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6POCOGenerator
{
    [Flags]
    public enum CommentsStyle
    {
        None,
        InSummaryBlock,
        AtEndOfField
    };

    // Settings to allow selective code generation
    [Flags]
    public enum Elements
    {
        None = 0,
        Poco = 1,
        Context = 2,
        UnitOfWork = 4,
        PocoConfiguration = 8
    };

    public class EnumDefinition
    {
        public string Schema;
        public string Table;
        public string Column;
        public string EnumType;
    }

}
