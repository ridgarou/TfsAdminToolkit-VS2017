// Guids.cs
// MUST match guids.h

namespace mskold.TfsAdminToolKit
{ 
    using System;

    internal static class GuidList
    {
        public const string guidTfsAdminToolsPkgstring = "0d03c503-4723-4554-a216-7ac09af4c62e";

        public const string guidTfsAdminToolsCmdSetstring = "1dee15dd-438e-4231-b77f-d714b95138b7";

        public static readonly Guid guidTfsAdminToolsCmdSet = new Guid(guidTfsAdminToolsCmdSetstring);
    };
}