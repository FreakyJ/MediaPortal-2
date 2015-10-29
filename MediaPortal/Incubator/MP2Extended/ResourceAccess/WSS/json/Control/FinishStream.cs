﻿using System;
using System.Security.Policy;
using HttpServer;
using HttpServer.Exceptions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.General;
using MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.Profiles;
using MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.stream;
using MediaPortal.Plugins.MP2Extended.WSS.General;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.json.General
{
  internal class FinishStream : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      HttpParam httpParam = request.Param;
      
      string identifier = httpParam["identifier"].Value;
      bool result = true;

      if (identifier == null)
      {
        Logger.Debug("FinishStream: identifier is null");
        result = false;
      }

      if (!StreamControl.ValidateIdentifie(identifier))
      {
        Logger.Debug("FinishStream: unknown identifier: {0}", identifier);
        result = false;
      }

      // Remove the stream from the stream controler
      StreamControl.DeleteStreamItem(identifier);

     return new WebBoolResult { Result = result };
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}