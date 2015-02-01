#region Copyright (C) 2007-2012 Team MediaPortal

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;


namespace MediaPortal.Extensions.UPnPRenderer
{
  public class UPnPRenderingControlServiceImpl : DvService
  {
    
    public UPnPRenderingControlServiceImpl()
      : base(
        upnpDevice.RENDERING_CONTROL_SERVICE_TYPE,
        upnpDevice.RENDERING_CONTROL_SERVICE_TYPE_VERSION,
        upnpDevice.RENDERING_CONTROL_SERVICE_ID)
    {

      #region DvStateVariables

      // Used for a boolean value
      DvStateVariable BlueVideoBlackLevel = new DvStateVariable("BlueVideoBlackLevel",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add value range
      };
      AddStateVariable(BlueVideoBlackLevel);

      // Used for a boolean value
      DvStateVariable BlueVideoGain = new DvStateVariable("BlueVideoGain",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add vlue range
      };
      AddStateVariable(BlueVideoGain);

      // Used for a boolean value
      DvStateVariable Brightness = new DvStateVariable("Brightness",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add value range
      };
      AddStateVariable(Brightness);

      // Used for a boolean value
      DvStateVariable ColorTemperature = new DvStateVariable("ColorTemperature",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add value range
      };
      AddStateVariable(ColorTemperature);

      // Used for a boolean value
      DvStateVariable Contrast = new DvStateVariable("Contrast",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add value range
      };
      AddStateVariable(Contrast);

      // Used for a boolean value
      DvStateVariable GreenVideoBlackLevel = new DvStateVariable("GreenVideoBlackLevel",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add range
      };
      AddStateVariable(GreenVideoBlackLevel);

      // Used for a boolean value
      DvStateVariable GreenVideoGain = new DvStateVariable("GreenVideoGain",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add range
      };
      AddStateVariable(GreenVideoGain);

      // Used for a boolean value
      DvStateVariable HorizontalKeystone = new DvStateVariable("HorizontalKeystone",
        new DvStandardDataType(UPnPStandardDataType.I2))
      {
        SendEvents = false,
        // TODO Add allowed Range
        // AllowedValueRange = new DvAllowedValueRange()
      };
      AddStateVariable(HorizontalKeystone);

      // Used for a boolean value
      DvStateVariable LastChange = new DvStateVariable("LastChange",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = true,
      };
      AddStateVariable(LastChange);

      // Used for a boolean value
      DvStateVariable Loudness = new DvStateVariable("Loudness",
        new DvStandardDataType(UPnPStandardDataType.Boolean))
      {
        SendEvents = false,
      };
      AddStateVariable(Loudness);

      // Used for a boolean value
      DvStateVariable Mute = new DvStateVariable("Mute",
        new DvStandardDataType(UPnPStandardDataType.Boolean))
      {
        SendEvents = false,
      };
      AddStateVariable(Mute);

      // Used for a boolean value
      DvStateVariable PresetNameList = new DvStateVariable("PresetNameList",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "FactoryDefaults," +
                "InstallationDefaults",
      };
      AddStateVariable(PresetNameList);

      // Used for a boolean value
      DvStateVariable RedVideoBlackLevel = new DvStateVariable("RedVideoBlackLevel",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = true,
        // TODO add range
      };
      AddStateVariable(RedVideoBlackLevel);

      // Used for a boolean value
      DvStateVariable RedVideoGain = new DvStateVariable("RedVideoGain",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add range
      };
      AddStateVariable(RedVideoGain);

      // Used for a boolean value
      DvStateVariable Sharpness = new DvStateVariable("Sharpness",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add range 
      };
      AddStateVariable(Sharpness);

      // Used for a boolean value
      DvStateVariable VerticalKeystone = new DvStateVariable("VerticalKeystone",
        new DvStandardDataType(UPnPStandardDataType.I2))
      {
        SendEvents = false,
        // TODO Add valueRange
      };
      AddStateVariable(VerticalKeystone);

      // Used for a boolean value
      DvStateVariable Volume = new DvStateVariable("Volume",
        new DvStandardDataType(UPnPStandardDataType.Ui2))
      {
        SendEvents = false,
        // TODO add range
      };
      AddStateVariable(Volume);

      // Used for a boolean value
      DvStateVariable VolumeDB = new DvStateVariable("VolumeDB",
        new DvStandardDataType(UPnPStandardDataType.I2))
      {
        SendEvents = false,
        // TODO add range
      };
      AddStateVariable(VolumeDB);

      #endregion DvStateVariables

      #region A_ARG_TYPE

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_Channel = new DvStateVariable("A_ARG_TYPE_Channel",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        AllowedValueList = new List<string>
        {
          "Master",
          "LF",
          "RF"
        }
      };
      AddStateVariable(A_ARG_TYPE_Channel);

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_InstanceID = new DvStateVariable("A_ARG_TYPE_InstanceID",
        new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = false,
      };
      AddStateVariable(A_ARG_TYPE_InstanceID);

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_PresetName = new DvStateVariable("A_ARG_TYPE_PresetName",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        AllowedValueList = new List<string>
        {
          "FactoryDefaults",
          "InstallationDefaults",
          "Vendor defined"
        }
      };
      AddStateVariable(A_ARG_TYPE_PresetName);

      #endregion A_ARG_TYPE;

      #region Actions

      DvAction getBlueVideoBlackLevelAction = new DvAction("GetBlueVideoBlackLevel", OnGetBlueVideoBlackLevel,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentBlueVideoBlackLevel",
            BlueVideoBlackLevel,
            ArgumentDirection.Out),
        });
      AddAction(getBlueVideoBlackLevelAction);

      DvAction getBlueVideoGainAction = new DvAction("GetBlueVideoGain", OnGetBlueVideoGain,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentBlueVideoGain",
            BlueVideoGain,
            ArgumentDirection.Out),
        });
      AddAction(getBlueVideoGainAction);

      DvAction getBrightnessAction = new DvAction("GetBrightness", OnGetBrightness,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentBrightness",
            Brightness,
            ArgumentDirection.Out),
        });
      AddAction(getBrightnessAction);

      DvAction getColorTemperatureAction = new DvAction("GetColorTemperature", OnGetColorTemperature,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentColorTemperature",
            ColorTemperature,
            ArgumentDirection.Out),
        });
      AddAction(getColorTemperatureAction);

      DvAction GetContrastAction = new DvAction("GetContrast", OnGetContrast,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentContrast",
            Contrast,
            ArgumentDirection.Out),
        });
      AddAction(GetContrastAction);

      DvAction getGreenVideoBlackLevelAction = new DvAction("GetGreenVideoBlackLevel", OnGetGreenVideoBlackLevel,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentGreenVideoBlackLevel",
            GreenVideoBlackLevel,
            ArgumentDirection.Out),
        });
      AddAction(getGreenVideoBlackLevelAction);

      DvAction getGreenVideoGainAction = new DvAction("GetGreenVideoGain", OnGetGreenVideoGain,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentGreenVideoGain",
            GreenVideoGain,
            ArgumentDirection.Out),
        });
      AddAction(getGreenVideoGainAction);

      // stopped implementing optional stuff...

      DvAction ListPresetsAction = new DvAction("ListPresets", OnListPresets,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentPresetNameList",
            PresetNameList,
            ArgumentDirection.Out),
        });
      AddAction(ListPresetsAction);

      DvAction SelectPresetAction = new DvAction("SelectPreset", OnSelectPreset,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
          new DvArgument("PresetName",
            A_ARG_TYPE_PresetName,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(SelectPresetAction);

      #endregion Actions

    }

    #region OnAction

    private static UPnPError OnGetBlueVideoBlackLevel(DvAction action, IList<object> inParams, out IList<object> outParams,
      CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      
      foreach (var inArgument in action.InArguments)
      {
        Console.WriteLine("In Argument: " + inArgument.Name);
        switch (inArgument.Name)
        {
          case "InstanceID":
            inArgument.RelatedStateVar.Value = inParams[0];
            break;
        }
      }

      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();

      return null;
    }

    private static UPnPError OnGetBlueVideoGain(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      Console.WriteLine(action.OutArguments[0].RelatedStateVar.DefaultValue);
      return null;
    }

    private static UPnPError OnGetBrightness(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnGetColorTemperature(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnGetContrast(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnGetGreenVideoBlackLevel(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnGetGreenVideoGain(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnListPresets(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnSelectPreset(DvAction action, IList<object> inParams,
      out IList<object> outParams, CallContext context)
    {
      Console.WriteLine("*************");
      Console.WriteLine("Current method: " + GetCurrentMethod());
      Console.WriteLine("In Params");
      foreach (var inParam in inParams)
      {
        Console.WriteLine(inParam);
      }
      Console.WriteLine("*************");
      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetCurrentMethod()
    {
      StackTrace st = new StackTrace();
      StackFrame sf = st.GetFrame(1);

      return sf.GetMethod().Name;
    }

    #endregion OnAction

    /*internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }*/
  }
}