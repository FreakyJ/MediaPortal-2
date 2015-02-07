using MediaPortal.UI.Players.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.UI.Presentation.Players;

namespace MediaPortal.Extensions.UPnPRenderer.Players
{
    public class UPnPRendererVideoPlayer : VideoPlayer
    {
        public const string MIMETYPE = "video/airplayer";

      public UPnPRendererVideoPlayer()
      {
        Console.WriteLine("Initialize Player Events");
        InitializePlayerEvents(PlaybackStarted, null, null, PlaybackStarted, null, null);
      }

      public void PlaybackStarted(IPlayer player)
      {
        Console.WriteLine("PlaybackStarted!!");
      }
    }
}
