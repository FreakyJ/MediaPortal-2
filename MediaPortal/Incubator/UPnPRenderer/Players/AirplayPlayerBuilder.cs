using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.PluginManager;
using MediaPortal.UI.Presentation.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.UiComponents.Media.Models.Navigation;
using ImageItem = AirPlayer.MediaPortal2.MediaItems.ImageItem;

namespace MediaPortal.Extensions.UPnPRenderer.Players
{
    /// <summary>
    /// Player builder for Airplay audio streams.
    /// </summary>
    public class UPnPRendererPlayerBuilder : IPlayerBuilder
    {
        #region IPlayerBuilder implementation

        public IPlayer GetPlayer(MediaItem mediaItem)
        {
            /*AudioItem audioItem = mediaItem as AudioItem;
            if (audioItem != null)
                return getAudioPlayer(audioItem);*/

            ImageItem imageItem = mediaItem as ImageItem;
            if (imageItem != null)
                return getImagePlayer(imageItem);

            return null;
        }

        #endregion

        /*IPlayer getAudioPlayer(AudioItem mediaItem)
        {
            UPnPRendererAudioPlayer player = new UPnPRendererAudioPlayer(mediaItem.PlayerSettings);
            try
            {
                player.SetMediaItem(mediaItem.GetResourceLocator(), null);
            }
            catch (Exception e)
            {
                ServiceRegistration.Get<ILogger>().Warn("UPnPRendererAudioPlayer: Unable to play audio stream", e);
                IDisposable disposablePlayer = player as IDisposable;
                if (disposablePlayer != null)
                    disposablePlayer.Dispose();
                throw;
            }
            return (IPlayer)player;
        }*/

        IPlayer getImagePlayer(ImageItem mediaItem)
        {
            UPnPRendererImagePlayer player = new UPnPRendererImagePlayer();
            player.NextItem(mediaItem, StartTime.AtOnce);
            return (IPlayer)player;
        }
    }
}
