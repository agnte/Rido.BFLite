using Rido.BFLite.Teams.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rido.BFLite.Teams.Handlers
{
    public delegate Task MessageHandler(TeamsActivity activity, CancellationToken cancellationToken = default);
}
