/// Copyright Rick Drizin (just kidding - this is MIT license - use however you like it!)
using System;
using System.IO;
using System.Collections.Generic;

namespace MyNamespace
{
    /// <summary>
    /// POCO for Users
    /// </summary>
    public class Users
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}