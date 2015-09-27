using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Common.Settings;

namespace MediaPortal.Plugins.Inputmanager
{
  public class InputmanagerSettings
  {
    #region Variables

    private List<InputDevice> _inputDevices;

    #endregion Variables

    #region Properties

    /// <summary>
    /// Gets the transceiver list.
    /// </summary>
    [Setting(SettingScope.User)]
    public List<InputDevice> InputDevices
    {
      get { return _inputDevices; }
      set { _inputDevices = value; }
    }

    #endregion Properties

    #region Additional members for the XML serialization

    public List<InputDevice> XML_InputDevices
    {
      get { return _inputDevices; }
      set { _inputDevices = value; }
    }

    #endregion
  }
}
