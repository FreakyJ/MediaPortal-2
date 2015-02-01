﻿//using HttpServer;
using System;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.PluginManager;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.Runtime;
using MediaPortal.UI;
using MediaPortal.UI.Services.FrontendServer;
using UPnP_Renderer;

namespace MediaPortal.Extensions.UPnPRenderer
{
  internal class UPnP_RendererMain : IPluginStateTracker, IMessageReceiver
  {
    private readonly upnpDevice _device;
    public const string DEVICE_UUID = "D6185023-12EC-4E27-A4AA-C8D9E993974D";

    //static HttpServer.HttpServer _httpServer = new HttpServer.HttpServer();
    private static UPnPLightServer _upnpServer = null;

    public UPnP_RendererMain()
    {
      _device = new upnpDevice(DEVICE_UUID.ToLower());

      //_httpServer.Start(IPAddress.Any, 80);
      _upnpServer = new UPnPLightServer(DEVICE_UUID);
      _upnpServer.Start();
      Player tmp = new Player();
      Console.ReadLine();
    }

    public void Activated(PluginRuntime pluginRuntime)
    {
      var meta = pluginRuntime.Metadata;
      Logger.Info(string.Format("{0} v{1} [{2}] by {3}", meta.Name, meta.PluginVersion, meta.Description, meta.Author));
      ServiceRegistration.Get<IMessageBroker>().RegisterMessageReceiver(SystemMessaging.CHANNEL, this);
      Logger.Debug("UPnPRenderPlugin: Adding UPNP device as a root device");
      ServiceRegistration.Get<FrontendServer>().UPnPFrontendServer.AddRootDevice(_device);
    }

    public bool RequestEnd()
    {
      return true;
    }

    public void Stop()
    {
    }

    public void Continue()
    {
    }

    public void Shutdown()
    {
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }

    public void Receive(SystemMessage message)
    {
      if (message.MessageType is SystemMessaging.MessageType)
      {
        if (((SystemMessaging.MessageType)message.MessageType) == SystemMessaging.MessageType.SystemStateChanged)
        {
          SystemState newState = (SystemState)message.MessageData[SystemMessaging.NEW_STATE];
          if (newState == SystemState.Running)
          {
            RegisterWithServices();
          }
        }
      }
    }

    protected void RegisterWithServices()
    {
// All non-default media item aspects must be registered
      
    }
  }
}