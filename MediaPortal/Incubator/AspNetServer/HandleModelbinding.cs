using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.SwaggerGen.Generator;

namespace MediaPortal.Plugins.AspNetServer
{
    public class HandleModelbinding : IOperationFilter
  {
    public void Apply(Operation operation, OperationFilterContext context)
    {
      if (operation.Parameters == null) return;

      foreach (IParameter param in operation.Parameters)
      {
        if (param.In == "modelbinding")
          param.In = "query";
      }
    }
  }
}
