using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using UPnPRenderer.UPnP;

namespace MediaPortal.Extensions.UPnPRenderer
{
  class Player
  {
    private ContentType playerType = ContentType.Unknown;
    
    // TODO remove
    private static Timer _timer;
    
    public Player()
    {
      // subscribe to Events
      UPnPAVTransportServiceImpl.Play += new PlayEventHandler(Play);
      UPnPAVTransportServiceImpl.Stop += new StopEventHandler(Stop);
      UPnPAVTransportServiceImpl.SetAVTransportURI += new SetAVTransportURIEventHandler(SetAVTransportURI);

      _timer = new Timer(500);
    }

    private void Play(DvAction action)
    {
      Console.WriteLine("Event Fired! - Play -- " + action.Name);

      switch (playerType)
      {
        case ContentType.Audio:
          break;
        case ContentType.Image:
          break;
        case ContentType.Video:
          //getPlayer<UPnPRendererVideoPlayer>().InitializePlayerEvents(PlaybackStarted, null, null, null, null, null);
          stopPlayer<UPnPRendererVideoPlayer>();
          PlayItemsModel.CheckQueryPlayAction(new VideoItem(action.ParentService.StateVariables["AVTransportURI"].Value.ToString()));
          //Console.WriteLine("Duration: " + getPlayer<UPnPRendererVideoPlayer>().Duration.ToString());
          //UPnPAVTransportServiceImpl.ChangeStateVariable(action, "CurrentTrackDuration", getPlayer<UPnPRendererVideoPlayer>().Duration.ToString());
          break;
        case ContentType.Unknown:
          break;
      }
      

      // TODO somehow I can't subscribe to events => use a timer as workaround
      _timer = new Timer(500);
      _timer.Elapsed += (sender, e) => _timer_Elapsed(action);
      _timer.Enabled = true;
      _timer.AutoReset = true;
    }

    public static void PlaybackStarted(IPlayer player)
    {
      Console.WriteLine("PlaybackStarted!!");
    }

    private void Stop(DvAction action)
    {
      Console.WriteLine("Event Fired! - Stop -- " + action.Name);

      // TODO Dummy Impl
      _timer.Enabled = false;
      string elapsedTime = TimeSpan.FromSeconds(0).ToString();
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "TransportState", "STOPPED");
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "AbsoluteTimePosition", elapsedTime);
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "RelativeTimePosition", elapsedTime);
    }

    private void SetAVTransportURI(DvAction action, OnEventSetAVTransportURIEventArgs e)
    {
      Console.WriteLine("Set Uri Event fired");
      Console.WriteLine("CurrentURI " + e.CurrentURI);
      Console.WriteLine("CurrentURIMetaData" + e.CurrentURIMetaData);

      Console.WriteLine("MimeType: " + utils.GetMimeFromUrl(action.ParentService.StateVariables["AVTransportURI"].Value.ToString()));
      playerType = utils.GetContentTypeFromUrl(action.ParentService.StateVariables["AVTransportURI"].Value.ToString());
      //PlayItemsModel.PlayOrEnqueueItem(new VideoItem(action.ParentService.StateVariables["AVTransportURI"].Value.ToString()), false, PlayerContextConcurrencyMode.ConcurrentVideo);
    }

    private void SetDuration(DvAction action, string duration)
    {
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "CurrentTrackDuration", duration);
    }

    // TODO Dummy Implementation

    void _timer_Elapsed(DvAction action)
    {
      string elapsedTime = "00:00:00";
      string duration = "00:00:00";

      switch (playerType)
      {
        case ContentType.Audio:
          break;
        case ContentType.Image:
          break;
        case ContentType.Video:
          var videoContexts = ServiceRegistration.Get<IPlayerContextManager>().GetPlayerContextsByAVType(AVType.Video);
          var UPnPPlayerCtx = videoContexts.FirstOrDefault(vc => vc.CurrentPlayer is UPnPRendererVideoPlayer);
          if (UPnPPlayerCtx != null)
          {
            if (getPlayer<UPnPRendererVideoPlayer>().State == PlayerState.Ended)
            {
              Console.WriteLine("Playback ended");
              Stop(action);
              return;
            }
            elapsedTime = getPlayer<UPnPRendererVideoPlayer>().CurrentTime.ToString(@"hh\:mm\:ss");
            duration = getPlayer<UPnPRendererVideoPlayer>().Duration.ToString(@"hh\:mm\:ss");
          }
          else
          {
            Console.WriteLine("PlayerContext null");
          }
          ;
          break;
        case ContentType.Unknown:
          break;
      }
      
      // TODO build a function which takes a list => reduces events
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "AbsoluteTimePosition", elapsedTime);
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "RelativeTimePosition", elapsedTime);
      UPnPAVTransportServiceImpl.ChangeStateVariable(action, "CurrentTrackDuration", duration);
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
