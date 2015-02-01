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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using MediaPortal.Utilities.DB;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;


namespace MediaPortal.Extensions.UPnPRenderer
{

  public delegate void PlayEventHandler(DvAction action);
  public delegate void StopEventHandler(DvAction action);
  public delegate void SetAVTransportURIEventHandler(DvAction action, OnEventSetAVTransportURIEventArgs e);
  
  public class UPnPAVTransportServiceImpl : DvService
  {
    
    public UPnPAVTransportServiceImpl()
      : base(
        upnpDevice.AV_TRANSPORT_SERVICE_TYPE,
        upnpDevice.AV_TRANSPORT_SERVICE_TYPE_VERSION,
        upnpDevice.AV_TRANSPORT_SERVICE_ID)
    {

      #region DvStateVariables

      // Used for a boolean value
      DvStateVariable AbsoluteCounterPosition = new DvStateVariable("AbsoluteCounterPosition",
        new DvStandardDataType(UPnPStandardDataType.I4))
      {
        SendEvents = false,
        Value = 2147483647,
      };
      AddStateVariable(AbsoluteCounterPosition);

      // Used for a boolean value
      DvStateVariable AbsoluteTimePosition = new DvStateVariable("AbsoluteTimePosition",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NOT_IMPLEMENTED",
      };
      AddStateVariable(AbsoluteTimePosition);

      // Used for a boolean value
      DvStateVariable AVTransportURI = new DvStateVariable("AVTransportURI",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(AVTransportURI);

      // Used for a boolean value
      DvStateVariable AVTransportURIMetaData = new DvStateVariable("AVTransportURIMetaData",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(AVTransportURIMetaData);

      // Used for a boolean value
      DvStateVariable CurrentMediaDurtaion = new DvStateVariable("CurrentMediaDuration",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "00:00:00",
      };
      AddStateVariable(CurrentMediaDurtaion);

      // Used for a boolean value
      DvStateVariable CurrentPlayMode = new DvStateVariable("CurrentPlayMode",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NORMAL",
        AllowedValueList = new List<string>
        {
          "NORMAL",
          "REPEAT_ALL",
          "INTRO"
        }
      };
      AddStateVariable(CurrentPlayMode);

      // Used for a boolean value
      DvStateVariable CurrentRecordQualityMode = new DvStateVariable("CurrentRecordQualityMode",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NOT_IMPLEMENT",
        AllowedValueList = new List<string>
        {
          "0:EP",
          "1:LP",
          "2:SP",
          "0:BASIC",
          "1:MEDIUM",
          "2:HIGH",
          "NOT_IMPLEMENTED",
          "vendor-defined"
        }
      };
      AddStateVariable(CurrentRecordQualityMode);

      // Used for a boolean value
      DvStateVariable CurrentTrack = new DvStateVariable("CurrentTrack",
        new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = false,
        Value = (UInt32)0,
        // TODO Add allowed Range
        // AllowedValueRange = new DvAllowedValueRange()
      };
      AddStateVariable(CurrentTrack);

      // Used for a boolean value
      DvStateVariable CurrentTrackDurtion = new DvStateVariable("CurrentTrackDuration",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "00:00:00",
      };
      AddStateVariable(CurrentTrackDurtion);

      // Used for a boolean value
      DvStateVariable CurrentTrackMetaData = new DvStateVariable("CurrentTrackMetaData",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(CurrentTrackMetaData);

      // Used for a boolean value
      DvStateVariable CurrentTrackURI = new DvStateVariable("CurrentTrackURI",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(CurrentTrackURI);

      // Used for a boolean value
      DvStateVariable CurrentTransportActions = new DvStateVariable("CurrentTransportActions",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(CurrentTransportActions);

      // Used for a boolean value
      DvStateVariable LastChange = new DvStateVariable("LastChange",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = true,
      };
      AddStateVariable(LastChange);

      // Used for a boolean value
      DvStateVariable NextAVTransportURI = new DvStateVariable("NextAVTransportURI",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(NextAVTransportURI);

      // Used for a boolean value
      DvStateVariable NextAVTransportURIMetaData = new DvStateVariable("NextAVTransportURIMetaData",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(NextAVTransportURIMetaData);

      // Used for a boolean value
      DvStateVariable NumberOfTracks = new DvStateVariable("NumberOfTracks",
        new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = false,
        Value = (UInt32)0,
        // TODO Add valueRange
      };
      AddStateVariable(NumberOfTracks);

      // Used for a boolean value
      DvStateVariable PlaybackStorageMedium = new DvStateVariable("PlaybackStorageMedium",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NONE",
        AllowedValueList = new List<string>
        {
          "UNKNOWN",
          "DV",
          "MINI-DV",
          "VHS",
          "W-VHS",
          "S-VHS",
          "D-VHS",
          "VHSC",
          "VIDEO8",
          "HI8",
          "CD-ROM",
          "CD-DA",
          "CD-R",
          "CD-RW",
          "VIDEO-CD",
          "SACD",
          "MD-AUDIO",
          "MD-PICTURE",
          "DVD-ROM",
          "DVD-VIDEO",
          "DVD-R",
          "DVD+RW",
          "DVD-RW",
          "DVD-RAM",
          "DVD-AUDIO",
          "DAT",
          "LD",
          "HDD",
          "MICRO-MV",
          "NETWORK",
          "NONE",
          "NOT_IMPLEMENTED",
          "vendor-defined"
        }
      };
      AddStateVariable(PlaybackStorageMedium);

      // Used for a boolean value
      DvStateVariable PossiblePlaybackStorageMedia = new DvStateVariable("PossiblePlaybackStorgrageMedia",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = String.Join(",", PlaybackStorageMedium.AllowedValueList),
      };
      AddStateVariable(PossiblePlaybackStorageMedia);

      // Used for a boolean value
      DvStateVariable PossibleRecordQualityModes = new DvStateVariable("PossibleRecordQualityModes",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NOT_IMPLEMENT",
      };
      AddStateVariable(PossibleRecordQualityModes);


      // Used for a boolean value
      DvStateVariable RecordStorageMedium = new DvStateVariable("RecordStorageMedium",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NONE",
        AllowedValueList = new List<string>
        {
          "UNKNOWN",
          "DV",
          "MINI-DV",
          "VHS",
          "W-VHS",
          "S-VHS",
          "D-VHS",
          "VHSC",
          "VIDEO8",
          "HI8",
          "CD-ROM",
          "CD-DA",
          "CD-R",
          "CD-RW",
          "VIDEO-CD",
          "SACD",
          "MD-AUDIO",
          "MD-PICTURE",
          "DVD-ROM",
          "DVD-VIDEO",
          "DVD-R",
          "DVD+RW",
          "DVD-RW",
          "DVD-RAM",
          "DVD-AUDIO",
          "DAT",
          "LD",
          "HDD",
          "MICRO-MV",
          "NETWORK",
          "NONE",
          "NOT_IMPLEMENTED",
          "vendor-defined"
        }
      };
      AddStateVariable(RecordStorageMedium);

      // Used for a boolean value
      DvStateVariable PossibleRecordStorageMedia = new DvStateVariable("PossibleRecordStorageMedia",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = String.Join(",", RecordStorageMedium.AllowedValueList),
      };
      AddStateVariable(PossibleRecordStorageMedia);

      // Used for a boolean value
      DvStateVariable RecordMediumWriteStatus = new DvStateVariable("RecordMediumWriteStatus",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NOT_IMPLEMENT",
        AllowedValueList = new List<string>
        {
          "WRITABLE",
          "PROTECTED",
          "NOT_WRITABLE",
          "UNKNOWN",
          "NOT_IMPLEMENTED"
        }
      };
      AddStateVariable(RecordMediumWriteStatus);

      // Used for a boolean value
      DvStateVariable RelativeCounterPosition = new DvStateVariable("RelativeCounterPosition",
        new DvStandardDataType(UPnPStandardDataType.I4))
      {
        SendEvents = false,
        Value = 2147483647,
      };
      AddStateVariable(RelativeCounterPosition);

      // Used for a boolean value
      DvStateVariable RelativeTimePosition = new DvStateVariable("RelativeTimePosition",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NOT_IMPLEMENTED",
      };
      AddStateVariable(RelativeTimePosition);

      // Used for a boolean value
      DvStateVariable TransportPlaySpeed = new DvStateVariable("TransportPlaySpeed",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "1",
        AllowedValueList = new List<string>
        {
          "1",
          "vendor-defined"
        }
      };
      AddStateVariable(TransportPlaySpeed);

      // Used for a boolean value
      DvStateVariable TransportState = new DvStateVariable("TransportState",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "NO_MEDIA_PRESENT",
        AllowedValueList = new List<string>
        {
          "STOPPED",
          "PAUSED_PLAYBACK",
          "PAUSED_RECORDING",
          "PLAYING",
          "RECORDING",
          "TRANSITIONING",
          "NO_MEDIA_PRESENT"
        }
      };
      AddStateVariable(TransportState);

      // Used for a boolean value
      DvStateVariable TransportStatus = new DvStateVariable("TransportStatus",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        Value = "OK",
        AllowedValueList = new List<string>
        {
          "OK",
          "ERROR_OCCURRED",
          "vendor-defined"
        }
      };
      AddStateVariable(TransportStatus);

      #endregion DvStateVariables

      #region A_ARG_TYPE

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_InstanceID = new DvStateVariable("A_ARG_TYPE_InstanceID",
        new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = false,
      };
      AddStateVariable(A_ARG_TYPE_InstanceID);

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_SeekMode = new DvStateVariable("A_ARG_TYPE_SeekMode",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
        AllowedValueList = new List<string>
        {
          "ABS_TIME",
          "REL_TIME",
          "ABS_COUNT",
          "REL_COUNT",
          "TRACK_NR",
          "CHANNEL_FREQ",
          "TAPE-INDEX",
          "FRAME"
        }
      };
      AddStateVariable(A_ARG_TYPE_SeekMode);

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_SeekTarget = new DvStateVariable("A_ARG_TYPE_SeekTarget",
        new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false,
      };
      AddStateVariable(A_ARG_TYPE_SeekTarget);

      #endregion A_ARG_TYPE;

      #region Actions

      DvAction getCurrentTransportActionsction = new DvAction("GetCurrentTransportActions", OnGetCurrentTransportActions,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("Sink",
            CurrentTransportActions,
            ArgumentDirection.Out),
        });
      AddAction(getCurrentTransportActionsction);

      DvAction getDeviceCapabilitiesAction = new DvAction("GetDeviceCapabilities", OnGetDeviceCapabilities,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("PlayMedia",
            PossiblePlaybackStorageMedia,
            ArgumentDirection.Out),
          new DvArgument("RecMedia",
            PossibleRecordStorageMedia,
            ArgumentDirection.Out),
          new DvArgument("RecQualityModes",
            PossibleRecordQualityModes,
            ArgumentDirection.Out),
        });
      AddAction(getDeviceCapabilitiesAction);

      DvAction getMediaInfoAction = new DvAction("GetMediaInfo", OnGetMediaInfo,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("NrTracks",
            NumberOfTracks,
            ArgumentDirection.Out),
          new DvArgument("MediaDuration",
            CurrentMediaDurtaion,
            ArgumentDirection.Out),
          new DvArgument("CurrentURI",
            AVTransportURI,
            ArgumentDirection.Out),
          new DvArgument("CurrentURIMetaData",
            AVTransportURIMetaData,
            ArgumentDirection.Out),
          new DvArgument("NextURI",
            NextAVTransportURI,
            ArgumentDirection.Out),
          new DvArgument("NextURIMetaData",
            NextAVTransportURIMetaData,
            ArgumentDirection.Out),
          new DvArgument("PlayMedium",
            PlaybackStorageMedium,
            ArgumentDirection.Out),
          new DvArgument("RecordMedium",
            RecordStorageMedium,
            ArgumentDirection.Out),
          new DvArgument("WriteStatus",
            RecordMediumWriteStatus,
            ArgumentDirection.Out),
        });
      AddAction(getMediaInfoAction);

      DvAction getPositionInfoAction = new DvAction("GetPositionInfo", OnGetPositionInfo,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("Track",
            CurrentTrack,
            ArgumentDirection.Out),
          new DvArgument("TrackDuration",
            CurrentTrackDurtion,
            ArgumentDirection.Out),
          new DvArgument("TrackMetaData",
            CurrentTrackMetaData,
            ArgumentDirection.Out),
          new DvArgument("TrackURI",
            CurrentTrackURI,
            ArgumentDirection.Out),
          new DvArgument("RelTime",
            RelativeTimePosition,
            ArgumentDirection.Out),
          new DvArgument("AbsTime",
            AbsoluteTimePosition,
            ArgumentDirection.Out),
          new DvArgument("RelCount",
            RelativeCounterPosition,
            ArgumentDirection.Out),
          new DvArgument("AbsCount",
            AbsoluteCounterPosition,
            ArgumentDirection.Out),
        });
      AddAction(getPositionInfoAction);

      DvAction getTransportInfoAction = new DvAction("GetTransportInfo", OnGetTransportInfo,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("CurrentTransportState",
            TransportState,
            ArgumentDirection.Out),
          new DvArgument("CurrentTransportStatus",
            TransportStatus,
            ArgumentDirection.Out),
          new DvArgument("CurrentSpeed",
            TransportPlaySpeed,
            ArgumentDirection.Out),
        });
      AddAction(getTransportInfoAction);

      DvAction getTransportSettingsAction = new DvAction("GetTransportSettings", OnGetTransportSettings,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          new DvArgument("PlayMode",
            CurrentPlayMode,
            ArgumentDirection.Out),
          new DvArgument("RecQualityMode",
            CurrentRecordQualityMode,
            ArgumentDirection.Out),
        });
      AddAction(getTransportSettingsAction);

      DvAction nextAction = new DvAction("Next", OnNext,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(nextAction);

      DvAction pauseAction = new DvAction("Pause", OnPause,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(pauseAction);

      DvAction playAction = new DvAction("Play", OnPlay,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
          new DvArgument("Speed",
            TransportPlaySpeed,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
          
        });
      AddAction(playAction);

      DvAction previousAction = new DvAction("Previous", OnPrevious,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(previousAction);

      DvAction seekAction = new DvAction("Seek", OnSeek,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
          new DvArgument("Unit",
            A_ARG_TYPE_SeekMode,
            ArgumentDirection.In),
          new DvArgument("Target",
            A_ARG_TYPE_SeekTarget,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(seekAction);

      DvAction setAVTransportURIAction = new DvAction("SetAVTransportURI", OnSetAVTransportURI,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
          new DvArgument("CurrentURI",
            AVTransportURI,
            ArgumentDirection.In),
          new DvArgument("CurrentURIMetaData",
            AVTransportURIMetaData,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(setAVTransportURIAction);

      DvAction setNextAVTransportURIAction = new DvAction("SetNextAVTransportURI", OnSetNextAVTransportURI,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
          new DvArgument("NextURI",
            NextAVTransportURI,
            ArgumentDirection.In),
          new DvArgument("NextURIMetaData",
            NextAVTransportURIMetaData,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(setNextAVTransportURIAction);

      DvAction setPlayModeAction = new DvAction("SetPlayMode", OnSetPlayMode,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
          new DvArgument("NewPlayMode",
            CurrentPlayMode,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(setPlayModeAction);

      DvAction stopction = new DvAction("Stop", OnStop,
        new DvArgument[]
        {
          new DvArgument("InstanceID",
            A_ARG_TYPE_InstanceID,
            ArgumentDirection.In),
        },
        new DvArgument[]
        {
        });
      AddAction(stopction);
    }

      #endregion Actions

    #region OnAction

    private static UPnPError OnGetCurrentTransportActions(DvAction action, IList<object> inParams, out IList<object> outParams,
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

    private static UPnPError OnGetDeviceCapabilities(DvAction action, IList<object> inParams,
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

    private static UPnPError OnGetMediaInfo(DvAction action, IList<object> inParams,
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
      foreach (var outArgument in action.OutArguments)
      {
        Console.WriteLine("- " + outArgument.Name + " - " + outArgument.RelatedStateVar.Value);
      }
      return null;
    }

    private static UPnPError OnGetPositionInfo(DvAction action, IList<object> inParams,
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
      foreach (var outArgument in action.OutArguments)
      {
        Console.WriteLine("- " + outArgument.Name + " - " + outArgument.RelatedStateVar.Value);
      }
      return null;
    }

    private static UPnPError OnGetTransportInfo(DvAction action, IList<object> inParams,
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

    private static UPnPError OnGetTransportSettings(DvAction action, IList<object> inParams,
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

    private static UPnPError OnNext(DvAction action, IList<object> inParams,
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

    private static UPnPError OnPause(DvAction action, IList<object> inParams,
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

    private static UPnPError OnPlay(DvAction action, IList<object> inParams,
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

      changeStateVariables(action, new List<string>
      {
        "TransportState",
        "TransportPlaySpeed"

      },  new List<object>
      {
        "PLAYING",
        inParams[1]
      });

      OnEventPlay(action); // FireEfent

      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();
      return null;
    }

    private static UPnPError OnPrevious(DvAction action, IList<object> inParams,
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

    private static UPnPError OnSeek(DvAction action, IList<object> inParams,
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

    private static UPnPError OnSetAVTransportURI(DvAction action, IList<object> inParams,
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

      changeStateVariables(action, new List<string>
      {
        "AVTransportURI",
        "AVTransportURIMetaData"

      }, new List<object>
      {
        inParams[1],
        inParams[2]
      });


      outParams = action.OutArguments.Select(outArgument => outArgument.RelatedStateVar.Value).ToList();

      OnEventSetAVTransportURIEventArgs eventArgs = new OnEventSetAVTransportURIEventArgs();
      eventArgs.CurrentURI = inParams[1];
      eventArgs.CurrentURIMetaData = inParams[2];

      OnEventSetAVTransportURI(action, eventArgs);

      return null;
    }

    private static UPnPError OnSetNextAVTransportURI(DvAction action, IList<object> inParams,
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

    private static UPnPError OnSetPlayMode(DvAction action, IList<object> inParams,
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

    private static UPnPError OnStop(DvAction action, IList<object> inParams,
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

      changeStateVariables(action, new List<string>
      {
        "TransportState"
      }, new List<object>
      {
        "STOPPED"
      });

      OnEventStop(action); // FireEfent

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

    private static void changeStateVariables(DvAction action, List<string> varNames, IList<object> inParams)
    {
      List<string> changedValues = new List<string>();
      
      for (int i = 0; i < varNames.Count; i++)
      {
        Console.WriteLine("Name: " + varNames[i] + " Value: " + inParams[i]);
        action.ParentService.StateVariables[varNames[i]].Value = inParams[i];
        changedValues.Add(inParams[i].ToString());
      }

      LastChangeXML(action, varNames, changedValues);
    }

    #endregion OnAction

    private static string LastChangeXML(DvAction action, List<string> StateVariables, List<string> StateValues)
    {
      XNamespace aw = "urn:schemas-upnp-org:metadata-1-0/AVT_RCS";
      XElement Event = new XElement(aw + "Event");
      
      XElement InstanceID = new XElement(aw + "InstanceID");
      InstanceID.SetAttributeValue("val", "0");

      for (int i = 0; i < StateVariables.Count; i++)
      {
        var StateVariableElement = new XElement(aw + StateVariables[i]);
        StateVariableElement.SetAttributeValue("val", StateValues[i]);
        InstanceID.Add(StateVariableElement);
      }

      
      Event.Add(InstanceID);

      XDocument srcTree = new XDocument(
        Event
        );
      Console.WriteLine(srcTree.ToString());
      action.ParentService.StateVariables["LastChange"].Value = srcTree.ToString();

      return srcTree.ToString();
    }

    #region Events

    public static event PlayEventHandler Play;
    public static event StopEventHandler Stop;
    public static event SetAVTransportURIEventHandler SetAVTransportURI;

    // Invoke the Play event; called whenever list changes:
    protected static void OnEventPlay(DvAction action)
    {
      if (Play != null)
        Play(action);
    }

    protected static void OnEventStop(DvAction action)
    {
      if (Stop != null)
        Stop(action);
    }

    protected static void OnEventSetAVTransportURI(DvAction action, OnEventSetAVTransportURIEventArgs e)
    {
      if (SetAVTransportURI != null)
        SetAVTransportURI(action, e);
    }

    #endregion Events

    #region ChangeStateVariablesFromOutside

    public static void ChangeStateVariable(DvAction action, string name, string value)
    {
      action.ParentService.StateVariables[name].Value = value;
      LastChangeXML(action, new List<string>
      {
        name
      },
      new List<string>
      {
        value
      });
    }

    #endregion ChangeStateVariablesFromOutside

    /*internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }*/
  }

  public class OnEventSetAVTransportURIEventArgs : EventArgs
  {
    public object CurrentURI { get; set; }
    public object CurrentURIMetaData { get; set; }
  }
}