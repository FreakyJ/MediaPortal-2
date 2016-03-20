using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MediaPortal.Common;
using MediaPortal.Common.Commands;
using MediaPortal.Common.General;
using MediaPortal.Common.Localization;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.PluginManager;
using MediaPortal.Common.Settings;
using MediaPortal.UiComponents.SkinBase.General;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UI.Presentation.Models;
using MediaPortal.UI.Presentation.Screens;
using MediaPortal.UI.Presentation.Workflow;
using RawInput_dll;

namespace MediaPortal.Plugins.InputdeviceManager.Models
{
  public class ConfigScreen : IWorkflowModel
  {
    public const string INPUTDEVICES_ID_STR = "CC11183C-01A9-4F96-AF90-FAA046981006";
    public const string RES_REMOVE_MAPPING_TEXT = "[InputDeviceManager.KeyMapping.Dialog.RemoveMapping]";
    public const string KEY_KEYMAP = "KeyMapData";

    protected AbstractProperty _inputDevicesProperty;
    protected AbstractProperty _addKeyLabelProperty;
    protected AbstractProperty _addKeyCountdownLabelProperty;
    protected AbstractProperty _showInputDeviceSelectionProperty;
    protected AbstractProperty _showKeyMappingProperty;
    protected AbstractProperty _showAddKeyProperty;
    protected AbstractProperty _showAddActionProperty;
    protected AbstractProperty _selectedItemProperty;
    protected ItemsList _items;
    protected ItemsList _actionItems;
    protected InputdeviceManager _inputmanagerInstance;

    //private RawInput _rawinput;
    private static string _currentInputDevice;
    private static bool _inWorkflowKeyMapping = false;
    private static bool _inWorkflowAddKey = false;
    private static ConcurrentDictionary<string, int> _pressedKeys = new ConcurrentDictionary<string, int>();
    private static Dictionary<string, int> _pressedAddKeyCombo = new Dictionary<string, int>();
    private static int _maxPressedKeys = 0;
    private static readonly Timer _timer = new Timer(500);
    private DateTime _endTime;
    private Key _choosenAction;

    protected AsynchronousMessageQueue _messageQueue;

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

    public bool ShowInputDeviceSelection
    {
      get { return (bool)_showInputDeviceSelectionProperty.GetValue(); }
      set { _showInputDeviceSelectionProperty.SetValue(value); }
    }

    public AbstractProperty ShowInputDeviceSelectionProperty
    {
      get { return _showInputDeviceSelectionProperty; }
    }

    public bool ShowKeyMapping
    {
      get { return (bool)_showKeyMappingProperty.GetValue(); }
      set { _showKeyMappingProperty.SetValue(value); }
    }

    public AbstractProperty ShowKeyMappingProperty
    {
      get { return _showKeyMappingProperty; }
    }

    public bool ShowAddKey
    {
      get { return (bool)_showAddKeyProperty.GetValue(); }
      set { _showAddKeyProperty.SetValue(value); }
    }

    public AbstractProperty ShowAddKeyProperty
    {
      get { return _showAddKeyProperty; }
    }

    public bool ShowAddAction
    {
      get { return (bool)_showAddActionProperty.GetValue(); }
      set { _showAddActionProperty.SetValue(value); }
    }

    public AbstractProperty ShowAddActionProperty
    {
      get { return _showAddActionProperty; }
    }

    public ListItem SelectedItem
    {
      get { return (ListItem)_selectedItemProperty.GetValue(); }
      set { _selectedItemProperty.SetValue(value); }
    }

    public AbstractProperty SelectedItemProperty
    {
      get { return _selectedItemProperty; }
    }

    private void InitModel()
    {
      _inputDevicesProperty = new WProperty(typeof(string), "TEST");
      _addKeyLabelProperty = new WProperty(typeof(string), "No Keys");
      _addKeyCountdownLabelProperty = new WProperty(typeof(string), "5");
      _showInputDeviceSelectionProperty = new WProperty(typeof(bool), true);
      _showKeyMappingProperty = new WProperty(typeof(bool), false);
      _showAddKeyProperty = new WProperty(typeof(bool), false);
      _showAddActionProperty = new WProperty(typeof(bool), false);
      _selectedItemProperty = new WProperty(typeof(ListItem), null);
      _items = new ItemsList();
      _actionItems = new ItemsList();

      foreach (var key in Key.NAME2SPECIALKEY)
      {
        var key1 = key;
        var listItem = new ListItem(Consts.KEY_NAME, "Key:" + key.Key) { Command = new MethodDelegateCommand(() => ChooseKeyAction("Key:" + key1.Key)) };
        _actionItems.Add(listItem);
      }

      /*foreach (var property in typeof(Key).GetFields())
      {
        var property1 = property;
        var listItem = new ListItem(Consts.KEY_NAME, "Key:" + property.Name) { Command = new MethodDelegateCommand(() => ChooseKeyAction("Key:" + property1.Name)) };
        _actionItems.Add(listItem);
      }*/

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

      _inputmanagerInstance = InputdeviceManager.Instance;
      InputdeviceManager.RawInput.KeyPressed += OnKeyPressed;

      _messageQueue = new AsynchronousMessageQueue(this, new[] { DialogManagerMessaging.CHANNEL });
      _messageQueue.MessageReceived += OnMessageReceived;
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
          //IResourceString label;
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
        case Win32.WM_SYSKEYDOWN:
          _pressedKeys.GetOrAdd(e.KeyPressEvent.VKeyName, e.KeyPressEvent.VKey);
          break;
        case Win32.WM_KEYUP:
          int tmp;
          _pressedKeys.TryRemove(e.KeyPressEvent.VKeyName, out tmp);
          break;
      }


      if (ShowKeyMapping || _inWorkflowAddKey)
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
      try
      {
        TimeSpan leftTime = _endTime.Subtract(DateTime.Now);
        if (leftTime.TotalSeconds < 0)
        {
          AddKeyCountdownLabel = "0";
          _timer.Stop();
          _inWorkflowAddKey = false;
          var logger = ServiceRegistration.Get<ILogger>(false);
          if (logger != null)
            logger.Debug("Next Screen!!!");
          //ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse("FD7FEDE0-9268-41AE-AD0A-CC8066A41ED9"));
          ShowAddActionScreen();
        }
        else
        {
          AddKeyCountdownLabel = leftTime.Seconds.ToString("0");
        }
      }
      catch
      {
        // ignored
      }
    }

    /// <summary>
    /// This updates the screen where the user can select which Keys he wants to add to the current input device.
    /// </summary>
    private void UpdateKeymapping()
    {
      _items.Clear();
      InputDevice device;
      if (InputdeviceManager.InputDevices.TryGetValue(_currentInputDevice, out device))
      {
        foreach (var keyMapping in device.KeyMap)
        {
          var item = new ListItem(Consts.KEY_NAME, String.Format("{0}: {1}", keyMapping.Key, String.Join(" + ", string.Join(" + ", keyMapping.Code.Select(KeyMapper.GetKeyName)))))
          {
            Command = new MethodDelegateCommand(MappingCommand)
          };
          item.AdditionalProperties.Add(KEY_KEYMAP, keyMapping);
          _items.Add(item);
        }
      }

      _items.FireChange();

      //ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse("6ABF367E-346B-459F-B5A6-B61A1E285A64"));
      ShowKeyMappingScreen();
      //_inWorkflowKeyMapping = true;
    }

    private void MappingCommand()
    {
      if (SelectedItem != null)
      {
        _deleteMappingItem = SelectedItem;
        var dialogManager = ServiceRegistration.Get<IDialogManager>();

        _messageQueue.Start();

        _deleteMappingDialogHandle = dialogManager.ShowDialog(SelectedItem.Label(Consts.KEY_NAME, SelectedItem.ToString()).ToString(),
          LocalizationHelper.Translate(RES_REMOVE_MAPPING_TEXT), 
          DialogType.YesNoDialog, false, DialogButtonType.No);
      }
    }

    private Guid _deleteMappingDialogHandle = Guid.Empty;
    private ListItem _deleteMappingItem;

    private void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    {
      if (message.ChannelName == DialogManagerMessaging.CHANNEL)
      {
        DialogManagerMessaging.MessageType messageType = (DialogManagerMessaging.MessageType)message.MessageType;
        if (messageType == DialogManagerMessaging.MessageType.DialogClosed)
        {
          Guid dialogHandle = (Guid)message.MessageData[DialogManagerMessaging.DIALOG_HANDLE];
          if (_deleteMappingDialogHandle == dialogHandle)
          {
            DialogResult dialogResult = (DialogResult)message.MessageData[DialogManagerMessaging.DIALOG_RESULT];

            if (dialogResult == DialogResult.Yes && _deleteMappingItem != null)
            {
              InputDevice device;
              if (InputdeviceManager.InputDevices.TryGetValue(_currentInputDevice, out device))
              {
                MappedKeyCode mappedKeyCode = null;
                foreach (var keyMapping in device.KeyMap)
                {
                  if (ReferenceEquals(keyMapping, _deleteMappingItem.AdditionalProperties[KEY_KEYMAP]))
                  {
                    mappedKeyCode = keyMapping;
                  }
                }
                if (mappedKeyCode != null)
                {
                  device.KeyMap.Remove(mappedKeyCode);

                  ISettingsManager settingsManager = ServiceRegistration.Get<ISettingsManager>();
                  var settings = settingsManager.Load<InputmanagerSettings>();
                  if (settings != null)
                  {
                    foreach (var inputDevice in settings.InputDevices)
                    {
                      if (inputDevice.DeviceID == _currentInputDevice)
                      {
                        inputDevice.KeyMap = device.KeyMap;

                        settingsManager.Save(settings);

                        // update settings in the main plugin
                        InputdeviceManager.Instance.UpdateLoadedSettings(settings);

                        // this brings us back to the add key menu
                        UpdateKeymapping();

                        break;
                      }
                    }
                  }
                }
              }
            }

            _deleteMappingItem = null;
            _deleteMappingDialogHandle = Guid.Empty;
            _messageQueue.Shutdown();
          }
        }
      }
    }

    /// <summary>
    /// This function makes us ready to accept new key mappings
    /// </summary>
    private void ResetAddKey()
    {
      _maxPressedKeys = 0;
      _pressedKeys.Clear();
      _pressedAddKeyCombo.Clear();
      AddKeyLabel = "";
      AddKeyCountdownLabel = "5";
      _inWorkflowAddKey = false;
    }

    private void ResetCompleteModel(bool removeOnKeyPressed = true)
    {
      ResetAddKey();
      //_inWorkflowKeyMapping = false;

      // Reset screens
      ShowInputDeviceSelection = true;
      ShowAddKey = false;
      ShowAddAction = false;
      ShowKeyMapping = false;
      if (removeOnKeyPressed)
        InputdeviceManager.RawInput.KeyPressed -= OnKeyPressed;
    }

    #region Screen switching functions

    private void ShowKeyMappingScreen()
    {
      ShowInputDeviceSelection = false;
      ShowAddKey = false;
      ShowAddAction = false;
      ShowKeyMapping = true;
    }

    private void ShowAddKeyScreen()
    {
      ShowInputDeviceSelection = false;
      ShowKeyMapping = false;
      ShowAddAction = false;
      ShowAddKey = true;
    }

    private void ShowAddActionScreen()
    {
      ShowInputDeviceSelection = false;
      ShowKeyMapping = false;
      ShowAddKey = false;
      ShowAddAction = true;
    }

    #endregion Screen switching functions

    #region buttonActions

    public void AddKeyMapping()
    {
      //ServiceRegistration.Get<IWorkflowManager>().NavigatePush(Guid.Parse("9907E2BF-CCE9-4CF7-9F4D-807F14A5DF47"));
      ResetAddKey();
      ShowAddKeyScreen();
      _inWorkflowAddKey = true;
    }

    public void CancelMapping()
    {
      ResetCompleteModel(false);
      //ServiceRegistration.Get<IScreenManager>().ShowScreen("configuration-section");
      //ServiceRegistration.Get<IWorkflowManager>().NavigatePopModel(Guid.Parse("de84ff86-5ced-4416-8e69-7e8a604ad32c"));
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
        }
        catch { }

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
          KeyMap = new List<MappedKeyCode> { new MappedKeyCode(choosenAction, keys) }
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
      InputdeviceManager.Instance.UpdateLoadedSettings(settings);

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