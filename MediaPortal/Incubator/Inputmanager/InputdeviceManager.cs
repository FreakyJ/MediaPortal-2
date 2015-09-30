using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.PluginManager;
using MediaPortal.Common.Settings;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.UI.SkinEngine.InputManagement;
using RawInput_dll;

namespace MediaPortal.Plugins.InputdeviceManager
{
  public class InputdeviceManager : IPluginStateTracker
  {
    protected static InputdeviceManager _instance = null;
    public static RawInput _rawinput;
    private const bool CaptureOnlyInForeground = false;
    private ICollection<MappedKeyCode> _keyMap = new List<MappedKeyCode>();
    public static Dictionary<string, InputDevice> _inputDevices = new Dictionary<string, InputDevice>(); 
    private Thread t;
    private static IScreenControl screenControl;
    private static ConcurrentDictionary<string, int> _pressedKeys = new ConcurrentDictionary<string, int>();

    public InputdeviceManager()
    {
      _instance = this;
    }

    public static InputdeviceManager Instance
    {
      get
      {
        return _instance;
      }
    }

    private static void OnKeyPressed(object sender, RawInputEventArg e)
    {
      ServiceRegistration.Get<ILogger>().Info("------");
      try
      {
        switch (e.KeyPressEvent.Message)
        {
          case Win32.WM_KEYDOWN:
            _pressedKeys.GetOrAdd(e.KeyPressEvent.VKeyName, e.KeyPressEvent.VKey);
            break;
          case Win32.WM_KEYUP:
            int tmp;
            _pressedKeys.TryRemove(e.KeyPressEvent.VKeyName, out tmp);
            break;
        }
      }
      catch
      {
        ServiceRegistration.Get<ILogger>().Info("stateNr {0}", e.KeyPressEvent.Message);
      }
      
      InputDevice device;
      if (_inputDevices.TryGetValue(e.KeyPressEvent.Source, out device))
      {
        ServiceRegistration.Get<ILogger>().Info("Found input device!");
        foreach (var keyMapping in device.KeyMap)
        {
          bool executeAction = true;
          foreach (var pressedKey in _pressedKeys)
            if (!keyMapping.Code.Contains(pressedKey.Value))
              executeAction = false;
          if (executeAction && _pressedKeys.Count != 0)
          {
            string[] actionArray = keyMapping.Key.Split(':');
            ServiceRegistration.Get<ILogger>().Info("Execute action! Type: {0}, Parameter: {1}", actionArray[0], actionArray[1]);
            if (actionArray[0] == "Key")
              ServiceRegistration.Get<IInputManager>().KeyPress(Key.GetSpecialKeyByName(actionArray[1]));
            else if (actionArray[0] == "Menu")
            {
              //ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse(actionArray[1]));
              WorkflowAction action;
              if (ServiceRegistration.Get<IWorkflowManager>().CurrentNavigationContext.MenuActions.TryGetValue(Guid.Parse(actionArray[1]), out action))
              {
                ServiceRegistration.Get<ILogger>().Info("Found Action!");
                action.Execute();
              }

            }

          }
        }
      }
      ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.DeviceHandle.ToString());
      //ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.DeviceType);
      //ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.DeviceName);
      ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.Name);
      ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.VKey.ToString(CultureInfo.InvariantCulture));
      //ServiceRegistration.Get<ILogger>().Info(_rawinput.NumberOfKeyboards.ToString(CultureInfo.InvariantCulture));
      ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.VKeyName);
      ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.Source);
      ServiceRegistration.Get<ILogger>().Info(e.KeyPressEvent.KeyPressState);
      //ServiceRegistration.Get<ILogger>().Info("0x{0:X4} ({0})", e.KeyPressEvent.Message);


      ServiceRegistration.Get<ILogger>().Info("All presses Keys: {0}", String.Join(" + ", _pressedKeys.ToArray()));
      ServiceRegistration.Get<ILogger>().Info("------");

      /*ICollection<MappedKeyCode> keyCodes;
      if (_inputDevices.TryGetValue(e.KeyPressEvent.DeviceHandle.ToString(), out keyCodes))
      {
        foreach (var key in keyCodes)
        {
          if (e.KeyPressEvent.VKey == key.Code)
            ServiceRegistration.Get<ILogger>().Info("Found Key!!");
        }
      }*/

      //switch (e.KeyPressEvent.Message)
      //{
      //    case Win32.WM_KEYDOWN:
      //        Debug.WriteLine(e.KeyPressEvent.KeyPressState);
      //        break;
      //     case Win32.WM_KEYUP:
      //        Debug.WriteLine(e.KeyPressEvent.KeyPressState);
      //        break;
      //}
    }


    public static void ThreadProc()
    {
      while (screenControl == null)
      {
        try
        {
          screenControl = ServiceRegistration.Get<IScreenControl>();

          _rawinput = new RawInput(screenControl.MainWindowHandle, CaptureOnlyInForeground);
          _rawinput.AddMessageFilter(); // Adding a message filter will cause keypresses to be handled
          _rawinput.KeyPressed += OnKeyPressed;
        }
        catch
        {
        }
        Thread.Sleep(500);
      }
      ServiceRegistration.Get<ILogger>().Info("Leave Thread!");
    }

    public void LoadSettings()
    {
      ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
      InputmanagerSettings settings = settingsManager.Load<InputmanagerSettings>();

      UpdateLoadedSettings(settings);
    }

    /// <summary>
    /// This function updates the local variable "_inputDevices"
    /// </summary>
    /// <param name="settings"></param>
    public void UpdateLoadedSettings(InputmanagerSettings settings)
    {
      _inputDevices.Clear();
      if (settings != null)
        try
        {
          foreach (InputDevice device in settings.InputDevices)
            _inputDevices.Add(device.DeviceID, device);
        }
        catch { }
    }

    #region Implementation of IPluginStateTracker

    /// <summary>
    /// Will be called when the plugin is started. This will happen as a result of a plugin auto-start
    /// or an item access which makes the plugin active.
    /// This method is called after the plugin's state was set to <see cref="PluginState.Active"/>.
    /// </summary>
    public void Activated(PluginRuntime pluginRuntime)
    {
      LoadSettings();

      //InputManager inputManager = InputManager.Instance;
      //inputManager.KeyPressed += HandleKeyPress;

      

      t = new Thread(ThreadProc);
      t.Start();

    }

    /// <summary>
    /// Schedules the stopping of this plugin. This method returns the information
    /// if this plugin can be stopped. Before this method is called, the plugin's state
    /// will be changed to <see cref="PluginState.EndRequest"/>.
    /// </summary>
    /// <remarks>
    /// This method is part of the first phase in the two-phase stop procedure.
    /// After this method returns <c>true</c> and all item's clients also return <c>true</c>
    /// as a result of their stop request, the plugin's state will change to
    /// <see cref="PluginState.Stopping"/>, then all uses of items by clients will be canceled,
    /// then this plugin will be stopped by a call to method <see cref="IPluginStateTracker.Stop"/>.
    /// If either this method returns <c>false</c> or one of the items clients prevent
    /// the stopping, the plugin will continue to be active and the method <see cref="IPluginStateTracker.Continue"/>
    /// will be called.
    /// </remarks>
    /// <returns><c>true</c>, if this plugin can be stopped at this time, else <c>false</c>.
    /// </returns>
    public bool RequestEnd()
    {
      return true;
    }

    /// <summary>
    /// Second step of the two-phase stopping procedure. This method stops this plugin,
    /// i.e. removes the integration of this plugin into the system, which was triggered
    /// by the <see cref="IPluginStateTracker.Activated"/> method.
    /// </summary>
    public void Stop()
    {
    }

    /// <summary>
    /// Revokes the end request which was triggered by a former call to the
    /// <see cref="IPluginStateTracker.RequestEnd"/> method and restores the active state. After this call, the plugin remains active as
    /// it was before the call of <see cref="IPluginStateTracker.RequestEnd"/> method.
    /// </summary>
    public void Continue()
    {
    }

    /// <summary>
    /// Will be called before the plugin manager shuts down. The plugin can perform finalization
    /// tasks here. This method will called independently from the plugin state, i.e. it will also be called when the plugin
    /// was disabled or not started at all.
    /// </summary>
    public void Shutdown()
    {
    }

    #endregion
  }
}