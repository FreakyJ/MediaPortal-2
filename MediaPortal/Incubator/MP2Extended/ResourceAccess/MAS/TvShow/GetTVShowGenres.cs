﻿using System;
using System.Collections.Generic;
using System.Linq;
using HttpServer;
using HttpServer.Exceptions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Plugins.MP2Extended.Attributes;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS;
using MediaPortal.Plugins.MP2Extended.MAS.TvShow;
using Newtonsoft.Json;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.TvShow
{
  // TODO: Rework after MIA Rework
  [ApiFunctionDescription(Type = ApiFunctionDescription.FunctionType.Json, Summary = "")]
  [ApiFunctionParam(Name = "sort", Type = typeof(WebSortField), Nullable = true)]
  [ApiFunctionParam(Name = "order", Type = typeof(WebSortOrder), Nullable = true)]
  internal class GetTVShowGenres : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      // TODO: Series don't have VideoAspect. Where will ATTR_GENRES come from?
      ISet<Guid> necessaryMIATypes = new HashSet<Guid>();
      necessaryMIATypes.Add(MediaAspect.ASPECT_ID);
      necessaryMIATypes.Add(ProviderResourceAspect.ASPECT_ID);
      necessaryMIATypes.Add(ImporterAspect.ASPECT_ID);
      necessaryMIATypes.Add(VideoAspect.ASPECT_ID);
      necessaryMIATypes.Add(SeriesAspect.ASPECT_ID);

      IList<MediaItem> items = GetMediaItems.GetMediaItemsByAspect(necessaryMIATypes);

      if (items.Count == 0)
        throw new BadRequestException("No Tv Episodes found");

      var output = new List<WebGenre>();

      foreach (var item in items)
      {
        var videoGenres = (HashSet<object>)item[VideoAspect.Metadata][VideoAspect.ATTR_GENRES];
        List<string> videoGenresList = new List<string>();
        if (videoGenres != null)
          videoGenresList = videoGenres.Cast<string>().ToList();
        foreach (var genre in videoGenresList)
        {
          int index = output.FindIndex(x => x.Title == genre);
          if (index == -1)
          {
            WebGenre webGenre = new WebGenre { Title = genre };

            output.Add(webGenre);
          }
        }
      }

      // sort
      HttpParam httpParam = request.Param;
      string sort = httpParam["sort"].Value;
      string order = httpParam["order"].Value;
      if (sort != null && order != null)
      {
        WebSortField webSortField = (WebSortField)JsonConvert.DeserializeObject(sort, typeof(WebSortField));
        WebSortOrder webSortOrder = (WebSortOrder)JsonConvert.DeserializeObject(order, typeof(WebSortOrder));

        output = output.SortWebGenre(webSortField, webSortOrder).ToList();
      }

      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}