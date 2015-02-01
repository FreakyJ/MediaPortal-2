using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPnP.Infrastructure.Dv.DeviceTree;
using UPnP_Renderer;

namespace MediaPortal.Extensions.UPnPRenderer
{
  class upnpDevice : DvDevice
  {
    public const string MEDIASERVER_DEVICE_TYPE = "schemas-upnp-org:device:MediaServer";
    public const int MEDIASERVER_DEVICE_VERSION = 1;
    public const string CONTENT_DIRECTORY_SERVICE_TYPE = "schemas-upnp-org:service:ContentDirectory";
    public const int CONTENT_DIRECTORY_SERVICE_TYPE_VERSION = 1;
    public const string CONTENT_DIRECTORY_SERVICE_ID = "urn:upnp-org:serviceId:ContentDirectory";

    public const string CONNECTION_MANAGER_SERVICE_TYPE = "schemas-upnp-org:service:ConnectionManager";
    public const int CONNECTION_MANAGER_SERVICE_TYPE_VERSION = 1;
    public const string CONNECTION_MANAGER_SERVICE_ID = "urn:upnp-org:serviceId:ConnectionManager";

    public const string AV_TRANSPORT_SERVICE_TYPE = "schemas-upnp-org:service:AVTransport";
    public const int AV_TRANSPORT_SERVICE_TYPE_VERSION = 1;
    public const string AV_TRANSPORT_SERVICE_ID = "urn:schemas-upnp-org:service:AVTransport";

    public const string RENDERING_CONTROL_SERVICE_TYPE = "schemas-upnp-org:service:RenderingControl";
    public const int RENDERING_CONTROL_SERVICE_TYPE_VERSION = 1;
    public const string RENDERING_CONTROL_SERVICE_ID = "urn:schemas-upnp-org:service:RenderingControl";
    
    public upnpDevice(string deviceUuid)
    : base(MEDIASERVER_DEVICE_TYPE, MEDIASERVER_DEVICE_VERSION, deviceUuid, new MediaServerUpnPDeviceInformation())
    {
        AddService(new UPnPConnectionManagerServiceImpl());
        AddService(new UPnPRenderingControlServiceImpl());
        AddService(new UPnPAVTransportServiceImpl());
    }
  }
}
