﻿#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Text.RegularExpressions;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using MediaPortal.Utilities.Network;
using System.Net.Sockets;
using MediaPortal.Common.Services.ResourceAccess.Settings;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.Settings;
using MediaPortal.Plugins.Transcoding.Service;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.Extensions.MediaServer.Profiles;
using MediaPortal.Common.MediaManagement;
using System.IO;

namespace MediaPortal.Extensions.MediaServer.ResourceAccess
{
  public static class DlnaResourceAccessUtils
  {
    /// <summary>
    /// Base HTTP path for resource access, e.g. "/GetDlnaResource".
    /// </summary>
    public const string RESOURCE_ACCESS_PATH = "/GetDlnaResource";

    /// <summary>
    /// Argument name for the resource path argument, e.g. "MediaItem".
    /// </summary>
    public const string RESOURCE_PATH_ARGUMENT_NAME = "ResourcePath";


    public const string SYNTAX = RESOURCE_ACCESS_PATH + "/[media item guid]";

    public static bool useIPv4 = true;
    public static bool useIPv6 = false;

    public static string GetResourceUrl(Guid mediaItem)
    {
      return RESOURCE_ACCESS_PATH + "/" + mediaItem;
    }

    public static bool ParseMediaItem(Uri resourceUri, out Guid mediaItemGuid)
    {
      try
      {
        var r = Regex.Match(resourceUri.PathAndQuery, RESOURCE_ACCESS_PATH + @"\/([\w-]*)\/?");
        var mediaItem = r.Groups[1].Value;
        if (mediaItem.Contains("."))
        {
          mediaItem = mediaItem.Substring(0, mediaItem.IndexOf("."));
        }
        mediaItemGuid = new Guid(mediaItem);
      }
      catch (Exception e)
      {
        ServiceRegistration.Get<ILogger>().Warn("ParseMediaItem: Failed with input url {0}", e, resourceUri.OriginalString);
        mediaItemGuid = Guid.Empty;
        return false;
      }

      return true;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath, uint cchBuffer);

    public static string GetFileShortName(string longName)
    {
      StringBuilder shortNameBuffer = new StringBuilder(256);
      uint result = GetShortPathName(longName, shortNameBuffer, 256);
      return shortNameBuffer.ToString();
    }

    private static string GetSubtitleMime(SubtitleCodec codec)
    {
      switch (codec)
      {
        case SubtitleCodec.Srt:
          return "text/srt";
        case SubtitleCodec.MicroDvd:
          return "text/microdvd";
        case SubtitleCodec.SubView:
          return "text/plain";
        case SubtitleCodec.Ass:
          return "text/x-ass";
        case SubtitleCodec.Ssa:
          return "text/x-ssa";
        case SubtitleCodec.Smi:
          return "smi/caption";
      }
      return "text/plain";
    }

    public static bool FindSubtitle(EndPointSettings client, out SubtitleCodec targetCodec, out string targetMime)
    {
      targetCodec = SubtitleCodec.Unknown;
      targetMime = "text/plain";
      if (client.Profile.Settings.Subtitles.SubtitleMode == SubtitleSupport.SoftCoded)
      {
        targetCodec = client.Profile.Settings.Subtitles.SubtitlesSupported[0].Format;
        if (string.IsNullOrEmpty(client.Profile.Settings.Subtitles.SubtitlesSupported[0].Mime) == false)
          targetMime = client.Profile.Settings.Subtitles.SubtitlesSupported[0].Mime;
        else
          targetMime = GetSubtitleMime(targetCodec);
        return true;
      }
      return false;
    }
    
    private static string GetLocalIp()
    {
      var localIp = Dns.GetHostName();
      var host = Dns.GetHostEntry(localIp);
      string ip6 = null;
      foreach (var ip in host.AddressList)
      {
        if (IPAddress.IsLoopback(ip) == true)
        {
          continue;
        }
        if (useIPv4)
        {
          if (ip.AddressFamily == AddressFamily.InterNetwork)
          {
            return NetworkUtils.IPAddrToString(ip);
          }
        }
        else
        {
          if (ip.AddressFamily == AddressFamily.InterNetworkV6)
          {
            ip6 = NetworkUtils.IPAddrToString(ip);
          }
        }
      }
      if (ip6 != null)
      {
        return ip6;
      }
      return localIp;
    }

    public static string GetSubtitleBaseURL(MediaItem item, EndPointSettings client, out string subMime, out string subExtension)
    {
      SubtitleCodec codec = SubtitleCodec.Unknown;
      subMime = null;
      subExtension = null;

      if (FindSubtitle(client, out codec, out subMime) == false)
      {
        subExtension = "srt";
        string subType = codec.ToString();
        switch (codec)
        {
          case SubtitleCodec.Ass:
            subExtension = "ass";
            break;
          case SubtitleCodec.Ssa:
            subExtension = "ssa";
            break;
          case SubtitleCodec.Smi:
            subExtension = "smi";
            break;
          case SubtitleCodec.Srt:
            subExtension = "srt";
            break;
          case SubtitleCodec.MicroDvd:
            subExtension = "sub";
            break;
          case SubtitleCodec.SubView:
            subExtension = "sub";
            break;
        }

        return string.Format(GetBaseResourceURL()
                    + GetResourceUrl(item.MediaItemId)
                    + "?aspect=SUBTITLE&type={0}&file=subtitle.{1}", subType, subExtension);
      }
      return null;
    }

    public static string GetBaseResourceURL()
    {
      if (useIPv4 == false && useIPv6 == false)
      {
        ServerSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<ServerSettings>();
        if (settings.UseIPv4)
          useIPv4 = true;
        if (settings.UseIPv6)
          useIPv6 = true;
      }
      var rs = ServiceRegistration.Get<IResourceServer>();
      if (useIPv4)
        return "http://" + GetLocalIp() + ":" + rs.GetPortForIP();
      else
        return "http://" + GetLocalIp() + ":" + rs.GetPortForIP();
    }
  }
}
