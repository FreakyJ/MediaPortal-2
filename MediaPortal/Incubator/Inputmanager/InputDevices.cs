using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaPortal.Plugins.InputdeviceManager
{
  public class InputDevice
  {
    [XmlAttribute("DeviceID")]
    public string DeviceID { get; set; }
    [XmlAttribute("Name")]
    public string Name { get; set; }
    [XmlElement("KeyMap")]
    public List<MappedKeyCode> KeyMap { get; set; }
  }
}
