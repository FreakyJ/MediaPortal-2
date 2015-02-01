using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Extensions.UPnPRenderer.MediaItems;
using MediaPortal.Common;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.Services.ResourceAccess;
using MediaPortal.Extensions.UPnPRenderer.Players;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Services.Players.Builders;
//using MediaPortal.UiComponents.Media.Models.Navigation;
using MediaPortal.UiComponents.Media.Models;
using OnlineVideos;
using OnlineVideos.MediaPortal2;
using UPnP.Infrastructure.Dv.DeviceTree;

namespace MediaPortal.Extensions.UPnPRenderer
{
  class Player
  {
    // TODO remove
    private static Timer time;
    private static TimeSpan elapsedTime;
    
    public Player()
    {
      // subscribe to Events
      UPnPAVTransportServiceImpl.Play += new PlayEventHandler(Play);
      UPnPAVTransportServiceImpl.Stop += new StopEventHandler(Stop);
      UPnPAVTransportServiceImpl.SetAVTransportURI += new SetAVTransportURIEventHandler(SetAVTransportURI);

      time = new Timer(1000);
    }

    private void Play(DvAction action)
    {
      Console.WriteLine("Event Fired! - Play -- " + action.Name);
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "CurrentTrackDuration", "00:05:00");

      stopPlayer<UPnPRendererVideoPlayer>();
      PlayItemsModel.CheckQueryPlayAction(new VideoItem(action.ParentService.StateVariables["AVTransportURI"].Value.ToString()));
      //startPlayback();

      //UPnPRendererVideoPlayer player = getPlayer<UPnPRendererVideoPlayer>();
      
      //Utils.RemoveInvalidUrls(urls);
      //if (urls != null && urls.Count > 0)
      //{
        // if there is already an OnlineVideo playing stop it first, 2 streams at the same time might saturate the connection or not be allowed by the server
        /*var videoContexts = ServiceRegistration.Get<IPlayerContextManager>().GetPlayerContextsByAVType(AVType.Video);
        var ovPlayerCtx = videoContexts.FirstOrDefault(vc => vc.CurrentPlayer is OnlineVideosPlayer);
        if (ovPlayerCtx != null)
          ovPlayerCtx.Stop();
        VideoViewModel vmodel = new VideoViewModel("test", string.Empty);
      vmodel.Description = "lol";
      vmodel.Length = "00:05:00";*/
        //MediaPortal.UiComponents.Media.Models.PlayItemsModel.CheckQueryPlayAction(new PlaylistItem(vmodel, action.ParentService.StateVariables["AVTransportURI"].Value.ToString()));
      /*}
      else
      {
        // todo : show dialog that no valid urls were found
      }*/

      // TODO Dummy Impl
      time = new Timer(1000);
      time.Elapsed += (sender, e) => { _timer_Elapsed(action); };
      time.Enabled = true;
      time.AutoReset = true;
    }

    private void Stop(DvAction action)
    {
      Console.WriteLine("Event Fired! - Stop -- " + action.Name);

      // TODO Dummy Impl
      time.Enabled = false;
      elapsedTime = TimeSpan.FromSeconds(0);
      Console.WriteLine(elapsedTime.ToString());
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "AbsoluteTimePosition", elapsedTime.ToString());
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "RelativeTimePosition", elapsedTime.ToString());
    }

    private void SetAVTransportURI(DvAction action, OnEventSetAVTransportURIEventArgs e)
    {
      Console.WriteLine("Set Uri Event fired");
      Console.WriteLine("CurrentURI " + e.CurrentURI);
      Console.WriteLine("CurrentURIMetaData" + e.CurrentURIMetaData);
    }

    // TODO Dummy Implementation

    static void _timer_Elapsed(DvAction action)
    {
      elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
      Console.WriteLine(elapsedTime.ToString());
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "AbsoluteTimePosition", elapsedTime.ToString());
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "RelativeTimePosition", elapsedTime.ToString());
    }

    #region Utils

    T getPlayer<T>()
    {
      var context = getPlayerContext<T>();
      if (context != null)
        return (T)context.CurrentPlayer;
      return default(T);
    }

    IPlayerContext getPlayerContext<T>()
    {
      var contexts = ServiceRegistration.Get<IPlayerContextManager>().PlayerContexts;
      return contexts.FirstOrDefault(vc => vc.CurrentPlayer is T);
    }

    void stopPlayer<T>()
    {
      IPlayerContext context = getPlayerContext<T>();
      if (context != null)
        context.Stop();
    }

    /*void cleanupAudioPlayback()
    {
      if (isAudioBuffering)
      {
        ServiceRegistration.Get<ISuperLayerManager>().HideBusyScreen();
        isAudioBuffering = false;
      }
      restoreVolume();
      isAudioPlaying = false;
    }

    void cleanupVideoPlayback()
    {
      if (hlsParser != null)
      {
        ServiceRegistration.Get<ISuperLayerManager>().HideBusyScreen();
        hlsParser = null;
      }
      if (proxy != null)
      {
        proxy.Stop();
        proxy = null;
      }

      restoreVolume();
      currentVideoSessionId = null;
      currentVideoUrl = null;
    }

    void restoreVolume()
    {
      lock (volumeSync)
      {
        if (savedVolume != null)
        {
          ServiceRegistration.Get<IPlayerManager>().Volume = (int)savedVolume;
          savedVolume = null;
        }
      }
    }

    static bool isSecureUrl(string url)
    {
      return !string.IsNullOrEmpty(url) && url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);
    }

    static bool isKnownExtension(string url)
    {
      return url.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase) || url.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase);
    }*/

    #endregion
  }
}
