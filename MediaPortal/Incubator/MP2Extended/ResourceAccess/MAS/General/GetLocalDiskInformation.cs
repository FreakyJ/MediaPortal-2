﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HttpServer;
using HttpServer.Exceptions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.TAS.Misc.BaseClasses;
using MediaPortal.Plugins.MP2Extended.Utils;
using MediaPortal.Plugins.SlimTv.Interfaces;
using MediaPortal.Plugins.SlimTv.Interfaces.Items;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.General
{
  internal class GetLocalDiskInformation : BaseCard, IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      return DriveInfo.GetDrives().Select(x => DiskSpaceInformation.GetSpaceInformation(x.Name)).ToList();
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}