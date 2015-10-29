﻿#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common;
using System.IO;
using MediaPortal.Utilities.SystemAPI;

namespace MediaPortal.Plugins.Transcoding.Service.Transcoders.Base
{
  public class MimeDetector
  {
    public static string GetFileMime(ILocalFsResourceAccessor lfsra)
    {
      // Impersonation
      using (ServiceRegistration.Get<IImpersonationService>().CheckImpersonationFor(lfsra.CanonicalLocalResourcePath))
      {
        FileStream raf = null;
        try
        {
          raf = File.OpenRead(lfsra.LocalFileSystemPath);
          return MimeTypeDetector.GetMimeType(raf);
        }
        catch (FileNotFoundException)
        {
          return MimeTypeDetector.GetMimeTypeFromRegistry(lfsra.LocalFileSystemPath);
        }
        finally
        {
          if (raf != null) raf.Close();
        }
      }
    }

    public static string GetUrlMime(string url)
    {
      if(url.StartsWith("RTSP:", StringComparison.InvariantCultureIgnoreCase) == true ||
        url.StartsWith("MMS:", StringComparison.InvariantCultureIgnoreCase) == true)
      {
        return "RTSP";
      }
      if (url.StartsWith("RTP:", StringComparison.InvariantCultureIgnoreCase) == true)
      {
        return "RTP";
      }
      if (url.StartsWith("HTTP:", StringComparison.InvariantCultureIgnoreCase) == true)
      {
        return "HTTP";
      }
      if (url.StartsWith("UDP:", StringComparison.InvariantCultureIgnoreCase) == true)
      {
        return "UDP";
      }
      return null;
    }
  }
}