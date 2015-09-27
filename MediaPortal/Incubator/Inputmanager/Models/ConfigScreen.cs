using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;
using MediaPortal.Common.PluginManager;
using MediaPortal.Common.Settings;
using MediaPortal.UiComponents.SkinBase.General;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;
using MediaPortal.Utilities;
using RawInput_dll;

namespace MediaPortal.Plugins.Inputmanager.Models
{
  public class ConfigScreen : IWorkflowModel
  {
    public const string INPUTDEVICES_ID_STR = "CC11183C-01A9-4F96-AF90-FAA046981006";

    protected AbstractProperty _inputDevicesProperty;
    protected AbstractProperty _addKeyLabelProperty;
    protected AbstractProperty _addKeyCountdownLabelProperty;
    protected ItemsList _items;
    protected ItemsList _actionItems;
    protected Inputmanager _inputmanagerInstance;

    private RawInput _rawinput;
    private const bool CaptureOnlyInForeground = true;
    private static IScreenControl screenControl;
    private static string _currentInputDevice;
    private static bool _inWorkflowKeyMapping = false;
    private static bool _inWorkflowAddKey = false;
    private static ConcurrentDictionary<string, int> _pressedKeys = new ConcurrentDictionary<string, int>();
    private static Dictionary<string, int> _pressedAddKeyCombo = new Dictionary<string, int>();
    private static int _maxPressedKeys = 0;
    private static Timer _timer = new Timer(500);
    private DateTime _endTime;
    private Key _choosenAction;

    public string AddKeyLabel
    {
      get { return (string)_addKeyLabelProperty.GetValue(); }
      set { _addKeyLabelProperty.SetValue(value); }
    }

    public AbstractProperty AddKeyLabelProperty
    {
      get { return _addKeyLabelProperty; }
    }

    public string AddKeyCountdownLabel
    {
      get { return (string)_addKeyCountdownLabelProperty.GetValue(); }
      set { _addKeyCountdownLabelProperty.SetValue(value); }
    }

    public AbstractProperty AddKeyCountdownLabelProperty
    {
      get { return _addKeyCountdownLabelProperty; }
    }

    public string InputDevices
    {
      get { return (string)_inputDevicesProperty.GetValue(); }
      set { _inputDevicesProperty.SetValue(value); }
    }

    public ItemsList Items
    {
      get { return _items; }
    }

    public ItemsList ActionItems
    {
      get { return _actionItems; }
    }

    private void InitModel()
    {
      _inputDevicesProperty = new WProperty(typeof(string), "TEST");
      _addKeyLabelProperty = new WProperty(typeof(string), "No Keys");
      _addKeyCountdownLabelProperty = new WProperty(typeof(string), "5");
      _items = new ItemsList();
      _actionItems = new ItemsList();

      foreach (var property in typeof(Key).GetFields())
      {
        var property1 = property;
        var listItem = new ListItem(Consts.KEY_NAME, "Key:" + property.Name) { Command = new MethodDelegateCommand(() => ChooseKeyAction("Key:" + property1.Name)) };
        _actionItems.Add(listItem);
      }

      // TODO: Disable this for now
      /*IWorkflowManager workflowManager = ServiceRegistration.Get<IWorkflowManager>();
      foreach (NavigationContext context in workflowManager.NavigationContextStack)
      {
        var itemsList = UpdateMenu(context);
        if (itemsList != null) CollectionUtils.AddAll(_actionItems, itemsList);
      }*/

      IPluginManager pluginManager = ServiceRegistration.Get<IPluginManager>();
      foreach (var plugin in pluginManager.AvailablePlugins)
      {
        if (plugin.Key == Guid.Parse("359A4AA5-B25F-4961-896B-C1323AF4A4A4"))
        {
          ServiceRegistration.Get<ILogger>().Info("Plugin found!!!!!!!");
          foreach (var data in plugin.Value.Metadata.PluginItemsMetadata)
            ServiceRegistration.Get<ILogger>().Info(data.RegistrationLocation);
          foreach (var build in plugin.Value.Metadata.Builders)
            ServiceRegistration.Get<ILogger>().Info("Builders {0}:{1}: ", build.Key, build.Value);
          foreach (var reg in plugin.Value.ItemRegistrations)
          {
            ServiceRegistration.Get<ILogger>().Info("reg loc: {0}, Name: {1}", reg.Key.RegistrationLocation, reg.Key.BuilderName);
            ServiceRegistration.Get<ILogger>().Info("reg loc: Metadata: {0}, Name {1}", reg.Value.Metadata.RegistrationLocation, reg.Value.Metadata.BuilderName);
          }
        }
      }

      screenControl = ServiceRegistration.Get<IScreenControl>();

      _inputmanagerInstance = Inputmanager.Instance;
      Inputmanager._rawinput.KeyPressed += OnKeyPressed;
    }

    protected List<ListItem> UpdateMenu(NavigationContext context)
    {
      List<ListItem> result = new List<ListItem>();
      var temp3 = context.MenuActions.Values;
      foreach (var item in temp3.ToList())
      {
        ServiceRegistration.Get<ILogger>().Info("MenuActions - Name: {0}, DisplayTitle: {1}, ActionId: {2}", item.Name, item.DisplayTitle, item.ActionId);
        var item1 = item;
        ListItem listItem = new ListItem(Consts.KEY_NAME, "Menu:" + item.DisplayTitle) { Command = new MethodDelegateCommand(() => ChooseKeyAction("Menu:" + item1.ActionId)) };
        result.Add(listItem);
      }

      var temp = (ItemsList)context.GetContextVariable(Consts.KEY_MENU_ITEMS, false);
      if (temp != null)
        foreach (var item in temp.ToList())
        {
          ServiceRegistration.Get<ILogger>().Info(String.Join(" + ", string.Join(" + ", item.Labels.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToString()).ToArray())));
          ServiceRegistration.Get<ILogger>().Info(item.Command.ToString());
          IResourceString label;
          //if (item.Labels.TryGetValue(Consts.KEY_NAME, out label))
            //result.Add(new ListItem(Consts.KEY_NAME, label));
        }

      var temp4 = (ICollection<WorkflowAction>)context.GetContextVariable(Consts.KEY_ITEM_ACTION, false);
      if (temp4 != null)
        foreach (var item in temp4.ToList())
        {
          ServiceRegistration.Get<ILogger>().Info("Temp4 - Name: {0}, ActionId: {1}, DisplayTitle: {2}", item.Name, item.ActionId.ToString(), item.DisplayTitle);
        }

      var temp2 = (ICollection<WorkflowAction>)context.GetContextVariable(Consts.KEY_REGISTERED_ACTIONS, false);
      if (temp2 != null)
        foreach (var item in temp2)
        {
          ServiceRegistration.Get<ILogger>().Info("Name: {0}, ActionId: {1}, DisplayTitle: {2}", item.Name, item.ActionId.ToString(), item.DisplayTitle);
        }
      return result;
    }

    private void OnKeyPressed(object sender, RawInputEventArg e)
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


      if (_inWorkflowKeyMapping)
      {
        if (_inWorkflowAddKey)
        {
          if (_pressedKeys.Count > _maxPressedKeys)
          {
            _pressedAddKeyCombo = _pressedKeys.ToDictionary(pair => pair.Key, pair => pair.Value);
            _maxPressedKeys = _pressedKeys.Count;
            ServiceRegistration.Get<ILogger>().Info("pressedKeys: {0}, maxPressedKEys: {1}, _pressedAddKeyCombo: {2}", _pressedKeys.Count, _maxPressedKeys, _pressedAddKeyCombo.Count);
            _timer.Elapsed += timer_Tick;
            _timer.Interval = 500;
            _timer.AutoReset = true;
            _endTime = DateTime.Now.AddSeconds(5);
            if (!_timer.Enabled)
              _timer.Start();
          }
          AddKeyLabel = String.Join(" + ", string.Join(" + ", _pressedAddKeyCombo.Select(kv => kv.Key.ToString())));
        }
      }
      else
      {
        _currentInputDevice = e.KeyPressEvent.Source;
        UpdateKeymapping();
      }


      /*ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.DeviceHandle.ToString());
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.DeviceType);
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.DeviceName);
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.Name);
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.VKey.ToString(CultureInfo.InvariantCulture));
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", _rawinput.NumberOfKeyboards.ToString(CultureInfo.InvariantCulture));
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.VKeyName);
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.Source);
      ServiceRegistration.Get<ILogger>().Info("Confscren: {0}", e.KeyPressEvent.KeyPressState);
      ServiceRegistration.Get<ILogger>().Info("0x{0:X4} ({0})", e.KeyPressEvent.Message);*/
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      TimeSpan leftTime = _endTime.Subtract(DateTime.Now);
      if (leftTime.TotalSeconds < 0)
      {
        AddKeyCountdownLabel = "0";
        _timer.Stop();
        _inWorkflowAddKey = false;
        ServiceRegistration.Get<ILogger>().Info("Next Screen!!!");
        ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse("FD7FEDE0-9268-41AE-AD0A-CC8066A41ED9"));
      }
      else
      {
        AddKeyCountdownLabel = leftTime.Seconds.ToString("0");
      }
    }

    /// <summary>
    /// This updates the screen where the user can select which Keys he wants to add to the current input device.
    /// </summary>
    private void UpdateKeymapping()
    {
      _items.Clear();
      InputDevice device;
      if (Inputmanager._inputDevices.TryGetValue(_currentInputDevice, out device))
      {
        foreach (var keyMapping in device.KeyMap)
        {
          _items.Add(new ListItem(Consts.KEY_NAME, String.Format("{0}: {1}", keyMapping.Key, String.Join(" + ", string.Join(" + ", keyMapping.Code.Select(KeyMapper.GetKeyName))))));
        }
      }

      _items.FireChange();

      ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse("6ABF367E-346B-459F-B5A6-B61A1E285A64"));
      _inWorkflowKeyMapping = true;
    }

    /// <summary>
    /// This function makes us ready to accept new key mappings
    /// </summary>
    private void ResetAddKey()
    {
      _maxPressedKeys = 0;
      _pressedAddKeyCombo.Clear();
      AddKeyLabel = "";
      AddKeyCountdownLabel = "5";
      _inWorkflowAddKey = false;
    }

    private void ResetCompleteModel()
    {
      ResetAddKey();
      _inWorkflowKeyMapping = false;
      Inputmanager._rawinput.KeyPressed -= OnKeyPressed;
    }

    #region buttonActions

    public void AddKeyMapping()
    {
      ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse("9907E2BF-CCE9-4CF7-9F4D-807F14A5DF47"));
      _inWorkflowAddKey = true;
    }

    public void CancelMapping()
    {
      ResetCompleteModel();
      ServiceRegistration.Get<IScreenManager>().ShowScreen("configuration-section");
      //ServiceRegistration.Get<IWorkflowManager>().NavigatePopModel(Guid.Parse("CB09DF01-65FA-4550-977C-B685C237ED3D"));
    }

    #endregion buttonActions

    #region ListViewActions

    public void ChooseKeyAction(string choosenAction)
    {
      //ServiceRegistration.Get<ILogger>().Info("choosen Action: {0}", typeof(Key).GetField(choosenAction).GetValue(null));
      ServiceRegistration.Get<ILogger>().Info("choosen Action: {0}:{1}", choosenAction, Key.GetSpecialKeyByName(choosenAction));
      _choosenAction = Key.GetSpecialKeyByName(choosenAction);

      List<int> keys = _pressedAddKeyCombo.Select(key => key.Value).ToList();

      

      ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
      InputmanagerSettings settings = settingsManager.Load<InputmanagerSettings>();

      List<InputDevice> inputDevices = new List<InputDevice>();

      if (settings != null)
        try
        {
          inputDevices = settings.InputDevices.ToList();
        }catch{}

      bool foundDevice = false;
      for (int i = 0; i < inputDevices.Count; i++)
      {
        if (inputDevices[i].DeviceID == _currentInputDevice)
        {
          inputDevices[i].KeyMap.Add(new MappedKeyCode(choosenAction, keys));
          foundDevice = true;
        }
      }

      if (inputDevices.Count == 0 || !foundDevice)
      {
        var inputDevice = new InputDevice
        {
          DeviceID = _currentInputDevice,
          Name = _currentInputDevice,
          KeyMap = new List<MappedKeyCode>{new MappedKeyCode(choosenAction, keys)}
        };
        inputDevices.Add(inputDevice);
      }
      if (settings != null)
      {
        settings.InputDevices = inputDevices;
      }
      else
      {
        settings = new InputmanagerSettings { InputDevices = inputDevices };
      }
      settingsManager.Save(settings);

      // update settings in the main plugin
      Inputmanager.Instance.UpdateLoadedSettings(settings);

      ResetAddKey();

      // this brings us back to the add key menu
      UpdateKeymapping();
    }

    #endregion ListViewActions

    #region IWorkflowModel implementation

    public Guid ModelId
    {
      get { return new Guid(INPUTDEVICES_ID_STR); }
    }

    public bool CanEnterState(NavigationContext oldContext, NavigationContext newContext)
    {
      return true;
    }

    public void EnterModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      InitModel();
    }

    public void ExitModelContext(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
      ServiceRegistration.Get<ILogger>().Info("Exit Modelcontext");
      ResetCompleteModel();
    }

    public void ChangeModelContext(NavigationContext oldContext, NavigationContext newContext, bool push)
    {
      // Nothing to do here
      ServiceRegistration.Get<ILogger>().Info("Change Modelcontext");
    }

    public void Deactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
      ServiceRegistration.Get<ILogger>().Info("Deactivate Modelcontext");
      //ResetCompleteModel();
    }

    public void Reactivate(NavigationContext oldContext, NavigationContext newContext)
    {
      // Nothing to do here
      ServiceRegistration.Get<ILogger>().Info("Reactivate Modelcontext");
      //InitModel();
    }

    public void UpdateMenuActions(NavigationContext context, IDictionary<Guid, WorkflowAction> actions)
    {
      // Nothing to do here
    }

    public ScreenUpdateMode UpdateScreen(NavigationContext context, ref string screen)
    {
      return ScreenUpdateMode.AutoWorkflowManager;
    }

    #endregion
  }
}