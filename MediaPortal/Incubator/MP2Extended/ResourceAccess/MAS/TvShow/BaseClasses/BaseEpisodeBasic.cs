﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.TvShow.BaseClasses
{
  class BaseEpisodeBasic
  {
    internal WebTVEpisodeBasic EpisodeBasic(MediaItem item, MediaItem showItem = null)
    {
      MediaItemAspect seriesAspects = item.Aspects[SeriesAspect.ASPECT_ID];

      if (showItem == null)
        showItem = GetMediaItems.GetMediaItemByName((string)seriesAspects[SeriesAspect.ATTR_SERIESNAME], null);

      WebTVEpisodeBasic webTvEpisodeBasic = new WebTVEpisodeBasic();
      var episodeNumber = ((HashSet<object>)item[SeriesAspect.ASPECT_ID][SeriesAspect.ATTR_EPISODE]).Cast<int>().ToList();
      webTvEpisodeBasic.EpisodeNumber = episodeNumber[0];
      var TvDbId = seriesAspects[SeriesAspect.ATTR_TVDB_ID];
      if (TvDbId != null)
      {
        webTvEpisodeBasic.ExternalId.Add(new WebExternalId
        {
          Site = "TVDB",
          Id = ((int)TvDbId).ToString()
        });
      }
      var ImdbId = seriesAspects[SeriesAspect.ATTR_TVDB_ID];
      if (ImdbId != null)
      {
        webTvEpisodeBasic.ExternalId.Add(new WebExternalId
        {
          Site = "IMDB",
          Id = (string)seriesAspects[SeriesAspect.ATTR_IMDB_ID]
        });
      }

      var firstAired = seriesAspects[SeriesAspect.ATTR_FIRSTAIRED];
      if (firstAired != null)
        webTvEpisodeBasic.FirstAired = (DateTime)seriesAspects[SeriesAspect.ATTR_FIRSTAIRED];
      webTvEpisodeBasic.IsProtected = false; //??
      webTvEpisodeBasic.Rating = seriesAspects[SeriesAspect.ATTR_TOTAL_RATING] == null ? 0 : Convert.ToSingle((double)seriesAspects[SeriesAspect.ATTR_TOTAL_RATING]);
      webTvEpisodeBasic.SeasonNumber = (int)seriesAspects[SeriesAspect.ATTR_SEASON];
      if (showItem != null)
      {
        webTvEpisodeBasic.ShowId = showItem.MediaItemId.ToString();
        webTvEpisodeBasic.SeasonId = string.Format("{0}:{1}", showItem.MediaItemId, (int)seriesAspects[SeriesAspect.ATTR_SEASON]);
      }
      webTvEpisodeBasic.Type = WebMediaType.TVEpisode;
      webTvEpisodeBasic.Watched = ((int)(item.Aspects[MediaAspect.ASPECT_ID][MediaAspect.ATTR_PLAYCOUNT] ?? 0) > 0);
      webTvEpisodeBasic.Path = new List<string> { item.MediaItemId.ToString() };
      //webTvEpisodeBasic.Artwork = ;
      webTvEpisodeBasic.DateAdded = (DateTime)item.Aspects[ImporterAspect.ASPECT_ID][ImporterAspect.ATTR_DATEADDED];
      webTvEpisodeBasic.Id = item.MediaItemId.ToString();
      webTvEpisodeBasic.PID = 0;
      webTvEpisodeBasic.Title = (string)seriesAspects[SeriesAspect.ATTR_EPISODENAME];

      return webTvEpisodeBasic;
    }
  }
}