using System;
using System.Collections.Generic;
using System.Text;

namespace RepBot.lib.Exceptions
{
    class ServerNotConfiguredException : Exception
    {
        public override string Message => "Server not configured";
        public ServerNotConfiguredException()
        {
        }
    }
}
