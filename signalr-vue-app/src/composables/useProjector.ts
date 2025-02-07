import { reactive, computed } from "vue";
import { SignalRInstance } from "@/SignalRServiceManager";
import * as projectorConstants from "@/Constants/ProjectorConstants";
import * as hdmiSwitchConstants from "@/Constants/HdmiSwitchConstants";
import * as adbConstants from "@/Constants/AdbConstants";
import * as tvConstants from "@/Constants/TVConstants";

export function useProjector() {
  const state = reactive({
    selectedInput: -1,
    targetVolume: 0,
    GUIConnected: false,
    ProjectorConnected: false,
    AndroidTVConnected: false,
    ProjectorPoweredOn: projectorConstants.PowerStatusGui.Pending,
  });

  const buttonDisabledWhenPoweredOff = computed(() => {
    return !state.GUIConnected ||
        !state.ProjectorConnected ||
        state.ProjectorPoweredOn !== projectorConstants.PowerStatusGui.On;
  })
  
  const buttonDisabledWhenPoweredOffOrNotConnectedToAndroidTV = computed(() => {
    return buttonDisabledWhenPoweredOff.value ||
      !state.AndroidTVConnected ||
      state.selectedInput !== hdmiSwitchConstants.Inputs.SmartTV;
  });
  
  const handlePowerToggle = (isPoweredOn: boolean) => {
    if (isPoweredOn) {
      handleClickProjectorCommands(projectorConstants.ProjectorCommands.SystemControlPowerOn);
    } else {
      handleClickProjectorCommands(projectorConstants.ProjectorCommands.SystemControlPowerOff);
    }
  };

  const handleProjectorConnectionStateChange = async (isConnected: boolean) => {
    state.ProjectorConnected = isConnected;
    if (state.ProjectorConnected) {
      SignalRInstance.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery)
    }
    else
    {
      state.selectedInput = -1;
      state.ProjectorPoweredOn = projectorConstants.PowerStatusGui.Pending;
    }
  }

  const handleAndroidTVConnectionStateChange = async (isConnected: boolean) => {
    if (state.AndroidTVConnected != isConnected)
    {
      state.AndroidTVConnected = isConnected;
      if (state.AndroidTVConnected) {
        SignalRInstance.getIsConnectedToAndroidTV();
      }
    }
  }
  
  const handleHdmiInputQuery = (currentStatus:Number) => {
    console.log(`Hdmi switch input query response: ${currentStatus as number}`);
    state.selectedInput = currentStatus as number;
  }

  const handleProjectorQueryResponse = (queryType:Number, currentStatus:Number) => {
    switch (queryType) {
      case projectorConstants.ProjectorCommands.SystemControlVolumeQuery:
        console.log(`Projector Volume query response: ${currentStatus}`);
        state.targetVolume = currentStatus as number;
        break;
      case projectorConstants.ProjectorCommands.SystemControlSourceQuery:
        break;
      case projectorConstants.ProjectorCommands.SystemControlPowerQuery:
        console.log(`Projector Power query response: ${currentStatus}`);
        state.ProjectorPoweredOn = getPowerStatusGui(currentStatus as projectorConstants.PowerStatusProjector);
        if (state.ProjectorPoweredOn){
          SignalRInstance.queryForInitialProjectorStatuses();
        }
        break;
      default:
        console.error("Invalid input selected");
        return;
    }
  }

  const getPowerStatusGui = (status:projectorConstants.PowerStatusProjector) => {
    switch (status) {
      case projectorConstants.PowerStatusProjector.StandbyNetworkOn:
        return projectorConstants.PowerStatusGui.Off;
      case projectorConstants.PowerStatusProjector.LampOn:
      case projectorConstants.PowerStatusProjector.Warmup:
        return projectorConstants.PowerStatusGui.On;
      default:
        return projectorConstants.PowerStatusGui.Pending;
    }
  }

  const handleGUIConnectionStateChange = (isConnected: boolean) => {
    state.GUIConnected = isConnected;
    if (!state.GUIConnected) {
      state.selectedInput = -1;
      state.ProjectorPoweredOn = projectorConstants.PowerStatusGui.Pending;
      state.ProjectorConnected = false;
    }
  }

  const handleClickTVCommand = async (command: tvConstants.IRCommands) => {
    SignalRInstance.sendTVCommand(command);
    console.log(`TV Command sent: ${command}`);
  }

  const handleClickAndroidCommand = async (command: adbConstants.KeyCodes, isLongPress: boolean) => {
    SignalRInstance.sendAndroidCommand(command, isLongPress);
    console.log(`Android Command sent: ${command}, IsLongPress: ${isLongPress}`);
  }

  const handleClickAndroidOpenAppCommand = async (command: adbConstants.KeyCodes) => {
    SignalRInstance.sendAndroidOpenAppCommand(command);
    console.log(`Android App Command sent: ${command}`);
  }

  const handleClickProjectorCommands = async (command: projectorConstants.ProjectorCommands) => {
    SignalRInstance.sendProjectorCommand(command);
    console.log(`Projector Command sent: ${command}`);

    switch (command) {
      case projectorConstants.ProjectorCommands.SystemControlPowerOff:
        while (state.ProjectorPoweredOn != projectorConstants.PowerStatusGui.Off) {
          console.log("Waiting for power off...");
          SignalRInstance.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery)
          await new Promise(resolve => setTimeout(resolve, 4000));
        }
        console.log("Power off complete");
        break;
      case projectorConstants.ProjectorCommands.SystemControlPowerOn:
        while (state.ProjectorPoweredOn != projectorConstants.PowerStatusGui.On) {
          console.log("Waiting for power on...");
          SignalRInstance.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery)
          await new Promise(resolve => setTimeout(resolve, 4000));
        }
        console.log("Power on complete");
        break;
    }
  };

  const handleDropdownChange = () => {
    console.log(`Selected Input: ${state.selectedInput}`);
    SignalRInstance.sendHdmiCommand(state.selectedInput);
    console.log(`Command sent: ${state.selectedInput}`);
  };

  return {
    state,
    buttonDisabledWhenPoweredOff,
    buttonDisabledWhenPoweredOffOrNotConnectedToAndroidTV,
    handleDropdownChange,
    handleClickProjectorCommands,
    handleClickTVCommand,
    handleClickAndroidCommand,
    handleClickAndroidOpenAppCommand,
    handleProjectorConnectionStateChange,
    handleAndroidTVConnectionStateChange,
    handleHdmiInputQuery,
    handleProjectorQueryResponse,
    handleGUIConnectionStateChange,
    handlePowerToggle
  };
}