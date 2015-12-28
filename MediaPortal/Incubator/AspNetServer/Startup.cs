#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using MediaPortal.Plugins.AspNetServer.Logger;
using MediaPortal.Plugins.AspNetServer.PlatformServices;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.Application;

namespace MediaPortal.Plugins.AspNetServer
{
  /// <summary>
  /// was that missing?!
  /// </summary>
  public class Item
  {
    /// <summary>
    /// or that
    /// </summary>
    public int Id;
    /// <summary>
    /// or this
    /// </summary>
    public string Name;
  }

  /// <summary>
  /// Some summary
  /// </summary>
  [Route("api/[Controller]")]
  [Produces("application/json", "text/json")]
  public class ItemsController : Controller
  {
    /// <summary>
    /// Return all items
    /// </summary>
    public static List<Item> Items = new List<Item>
    {
      new Item { Id = 1, Name = "First Test Item" },
      new Item { Id = 2, Name = "Second Test Item" },
    };

    /// <summary>
    /// Get all items
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<Item> Get()
    {
      return Items;
    }

    /// <summary>
    /// Get Item by Id
    /// </summary>
    /// <param name="id">A var</param>
    /// <returns></returns>
    [HttpGet("itemId")]
    public Item Get([FromQuery]int id, int dummy)
    {
      return Items.FirstOrDefault(item => item.Id == id);
    }
  }

  public class Startup
  {
    private static readonly Assembly ASS = Assembly.GetExecutingAssembly();
    private static readonly string ASSEMBLY_PATH = Path.GetDirectoryName(ASS.Location);

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IApplicationEnvironment>(new MP2ApplicationEnvironment());
      services.AddTransient(typeof(ILibraryManager), typeof(MP2LibraryManager));
      services.AddMvc();
      services.AddSwaggerGen(c =>
      {
        c.DescribeAllEnumsAsStrings();
        c.OperationFilter<HandleModelbinding>();
        //c.IncludeXmlComments(Path.Combine(ASSEMBLY_PATH, ASS.GetName().Name+".xml"));
        c.SingleApiVersion(new Info
        {
          Title = "MP2Extended API",
          Description = "MP2Extended brings the well known MPExtended from MP1 to MP2",
          Contact = new Contact
          {
            Name = "FreakyJ"
          },
          Version = "v1"
        });
      });
    }

    public void Configure(IApplicationBuilder app)
    {
      app.UseSwaggerUi(swaggerUrl: "/swagger/v1/swagger.json");

      string resourcePath = Path.Combine(ASSEMBLY_PATH, "www").TrimEnd(Path.DirectorySeparatorChar);
      app.UseFileServer(new FileServerOptions
      {
        FileProvider = new PhysicalFileProvider(resourcePath),
        RequestPath = new PathString("/swagger/ui"),
        EnableDirectoryBrowsing = true,
      });
      // Configure the HTTP request pipeline.
      app.UseStaticFiles();
      app.UseMvc();
      app.UseSwaggerGen();
      app.Run(context => context.Response.WriteAsync("Hello World"));
    }
  }

}
