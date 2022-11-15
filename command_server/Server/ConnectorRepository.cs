using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ConnectorRepository
    {
        List<Connector> connectors = new List<Connector>();
        internal void Create(ConnectorHandler handler)
        {
            var connector = new Connector(handler);
            connectors.Add(connector);

        }
    }
}
