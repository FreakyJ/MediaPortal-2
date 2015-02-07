using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using Microsoft.Win32;

namespace UPnPRenderer.UPnP
{
  public enum ContentType
  {
    Image,
    Video,
    Audio,
    Unknown
  }

  public class utils
  {
    [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
    private static extern UInt32 FindMimeFromData(
      UInt32 pBC,
      [MarshalAs(UnmanagedType.LPStr)] String pwzUrl,
      [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
      UInt32 cbSize,
      [MarshalAs(UnmanagedType.LPStr)] String pwzMimeProposed,
      UInt32 dwMimeFlags,
      out UInt32 ppwzMimeOut,
      UInt32 dwReserverd
      );

    public static string GetMimeFromUrl(string url)
    {
      WebRequest request = WebRequest.Create(url) as HttpWebRequest;

      byte[] buffer = new byte[256];
      string mime = "application/octet-stream";

      // Try to get the mime type from the registry, works only if the server sends a file extension
      Uri uri = new Uri(url);
      string fileName = uri.Segments.Last();
      mime = GetMimeFromRegistry(fileName);
      Console.WriteLine("Mime from registry: " + GetMimeFromRegistry(fileName));

      if (mime == "application/octet-stream")
      {
        using (WebResponse response = request.GetResponse())
        {
          using (Stream stream = response.GetResponseStream())
          {
            int count = stream.Read(buffer, 0, 256);

            Console.WriteLine("Bufer: " + BitConverter.ToString(buffer));
            Console.WriteLine(response.ContentType);
            Console.WriteLine("Sytem Mimemapping" + MimeMapping.GetMimeMapping(url));

            try
            {
              UInt32 mimetype;
              FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
              IntPtr mimeTypePtr = new IntPtr(mimetype);
              mime = Marshal.PtrToStringUni(mimeTypePtr);
              Marshal.FreeCoTaskMem(mimeTypePtr);

              Console.WriteLine("MimeType from urlmon.dll: " + mime);

              // if we get application/octet-stream => unknown mime type
              if (mime == "application/octet-stream")
              {
                Console.WriteLine("urlmon.dll couldn't find mime type");
                mime = response.ContentType;
                Console.WriteLine("MimeType from response.ContentType: " + mime);
                if (mime == "application/octet-stream")
                {
                  Console.WriteLine("response.ContentType couldn't find mime type");
                  mime = MimeMapping.GetMimeMapping(url);
                  Console.WriteLine("MimeType from GetMimeMapping: " + mime);

                  if (mime == "application/octet-stream")
                  {
                    throw new Exception("no mime type found");
                  }
                }
              }

              return mime;
            }
            catch (Exception e)
            {
              return "unknown/unknown";
            }
          }
        }
      }
      else
      {
        return mime;
      }
    }

    public static ContentType GetContentTypeFromUrl(string url)
    {
      string mimeType = GetMimeFromUrl(url);
      if (mimeType.Contains("video"))
      {
        return ContentType.Video;
      }

      if (mimeType.Contains("image"))
      {
        return ContentType.Image;
      }

      if (mimeType.Contains("audio"))
      {
        return ContentType.Audio;
      }

      return ContentType.Unknown;
    }

    #region helpers

    private static string GetMimeFromRegistry(string Filename)
    {
      string mime = "application/octetstream";
      string ext = Path.GetExtension(Filename).ToLower();
      RegistryKey rk = Registry.ClassesRoot.OpenSubKey(ext);
      if (rk != null && rk.GetValue("Content Type") != null)
        mime = rk.GetValue("Content Type").ToString();
      return mime;
    }

    private static string getFileExtention(string fileName, bool includeDot)
    {
      string fileExtension = "";
      int flength = fileName.Length;
      int fdot = fileName.LastIndexOf('.');
      if (fdot != flength && fdot > 0 && flength > 0)
      {
        if (includeDot)
        {
          fileExtension = fileName.Substring(fdot, flength - fdot);
        }
        else
        {
          fileExtension = fileName.Substring(fdot + 1, flength - fdot - 1);
        }
      }
      return fileExtension;
    }

    #endregion helpers
  }
}