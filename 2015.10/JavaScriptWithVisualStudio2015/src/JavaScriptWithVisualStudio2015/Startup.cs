using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.StaticFiles;

namespace JavaScriptWithVisualStudio2015
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDirectoryBrowser();
    }

    public void Configure(IApplicationBuilder app)
    {


      app.UseFileServer(new FileServerOptions()
      {
        EnableDirectoryBrowsing = true,
      });
    }
  }
}
