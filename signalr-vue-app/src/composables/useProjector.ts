import { reactive, computed } from "vue";
import { SignalRInstance } from "@/SignalRServiceManager";
import * as projectorConstants from "@/Constants/ProjectorConstants";
import * as hdmiSwitchConstants from "@/Constants/HdmiSwitchConstants";
import * as adbConstants from "@/Constants/AdbConstants";
import * as tvConstants from "@/Constants/TVConstants";

export function useProjector() {
  const state = reactive({
    selectedTab: "adb",
    selectedInput: -1,
    selectedImageMode: -1,
    isDarkMode: false,
    isTouchPadInverted: false,
    targetVolume: 0,
    GUIConnected: false,
    ProjectorConnected: false,
    AndroidTVConnected: false,
    VPNConnected: projectorConstants.ToggleStatusGui.Pending,
    ProjectorPoweredOn: projectorConstants.ToggleStatusGui.Pending,
  });

  const buttonDisabledWhenPoweredOff = computed(() => {
    return !state.GUIConnected ||
        !state.ProjectorConnected ||
        state.ProjectorPoweredOn !== projectorConstants.ToggleStatusGui.On;
  })
  
  const buttonDisabledWhenPoweredOffOrNotConnectedToAndroidTV = computed(() => {
    return buttonDisabledWhenPoweredOff.value ||
      !state.AndroidTVConnected ||
      state.selectedInput !== hdmiSwitchConstants.Inputs.SmartTV;
  });

  const handleVPNToggle = (isVPNOn: boolean) => {
    if (isVPNOn) {
      handleClickAndroidCommand(adbConstants.KeyCodes.VpnOn, "");
    } else {
      handleClickAndroidCommand(adbConstants.KeyCodes.VpnOff, "");
    }
  };
  
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
      state.ProjectorPoweredOn = projectorConstants.ToggleStatusGui.Pending;
    }
  }

  const handleAndroidTVConnectionStateChange = async (isConnected: boolean) => {
    state.AndroidTVConnected = isConnected;
    if (state.AndroidTVConnected) {
      SignalRInstance.queryForInitialAndroidTVStatuses();
    }
  }
  
  const handleHdmiInputQuery = (currentStatus:Number) => {
    console.log(`Hdmi switch input query response: ${currentStatus as number}`);
    state.selectedInput = currentStatus as number;
  }

  
  const handleAndroidTVQueryResponse = (queryType:Number, currentStatus:Number) => {
    switch (queryType) {
      case adbConstants.KeyCodes.VpnStatusQuery:
        console.log(`Vpn query response: ${adbConstants.KeyCodes[currentStatus as number]}`);
        state.VPNConnected = getVpnStatusAsToggleStatusGui(currentStatus as adbConstants.KeyCodes);
        break;
    }
  }
  
  const handleProjectorQueryResponse = (queryType:Number, currentStatus:Number) => {
    switch (queryType) {
      case projectorConstants.ProjectorCommands.ImageControlModeQuery:
        console.log(`Projector Image Mode query response: ${currentStatus}`);
        state.selectedImageMode = currentStatus as number;
        break;
      case projectorConstants.ProjectorCommands.SystemControlVolumeQuery:
        console.log(`Projector Volume query response: ${currentStatus}`);
        state.targetVolume = currentStatus as number;
        break;
      case projectorConstants.ProjectorCommands.SystemControlSourceQuery:
        console.log(`Projector Source query response: ${currentStatus}`);
        if (currentStatus as projectorConstants.ProjectorCommands == projectorConstants.ProjectorCommands.SystemControlSourceHDMI1) {
          SignalRInstance.queryForInitialHdmiStatuses();
        }
        else if (currentStatus as projectorConstants.ProjectorCommands == projectorConstants.ProjectorCommands.SystemControlSourceHDMI3) {
          state.selectedInput = 0;
        }
        break;
      case projectorConstants.ProjectorCommands.SystemControlPowerQuery:
        console.log(`Projector Power query response: ${currentStatus}`);
        state.ProjectorPoweredOn = getProjectPowerAsToggleStatusGui(currentStatus as projectorConstants.PowerStatusProjector);
        if (state.ProjectorPoweredOn){
          SignalRInstance.queryForInitialProjectorStatuses();
        }
        break;
      default:
        console.error("Invalid input selected");
        return;
    }
  }

  const getProjectPowerAsToggleStatusGui = (status:projectorConstants.PowerStatusProjector) => {
    switch (status) {
      case projectorConstants.PowerStatusProjector.StandbyNetworkOn:
        return projectorConstants.ToggleStatusGui.Off;
      case projectorConstants.PowerStatusProjector.LampOn:
      case projectorConstants.PowerStatusProjector.Warmup:
        return projectorConstants.ToggleStatusGui.On;
      default:
        return projectorConstants.ToggleStatusGui.Pending;
    }
  }

  const getVpnStatusAsToggleStatusGui = (status:adbConstants.KeyCodes) => {
    switch (status) {
      case adbConstants.KeyCodes.VpnOff:
        return projectorConstants.ToggleStatusGui.Off;
      case adbConstants.KeyCodes.VpnOn:
        return projectorConstants.ToggleStatusGui.On;
      default:
        return projectorConstants.ToggleStatusGui.Pending;
    }
  }

  const handleGUIConnectionStateChange = (isConnected: boolean) => {
    state.GUIConnected = isConnected;
    if (!state.GUIConnected) {
      state.selectedInput = -1;
      state.ProjectorPoweredOn = projectorConstants.ToggleStatusGui.Pending;
      state.VPNConnected = projectorConstants.ToggleStatusGui.Pending;
      state.ProjectorConnected = false;
      if (state.selectedTab == "settings")
      {
        state.selectedTab = "adb";
      }
    }
  }

  const handleClickTVCommand = async (command: tvConstants.IRCommands) => {
    SignalRInstance.sendTVCommand(command);
    console.log(`TV Command sent: ${command}`);
  }

  const handleClickAndroidCommand = async (command: adbConstants.KeyCodes, additionalParameter: string) => {
    SignalRInstance.sendAndroidCommand(command, additionalParameter);
    console.log(`Android Command sent: ${command}, AdditionalParameter: ${additionalParameter}`);
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
        while (state.ProjectorPoweredOn != projectorConstants.ToggleStatusGui.Off) {
          console.log("Waiting for power off...");
          SignalRInstance.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery)
          await new Promise(resolve => setTimeout(resolve, 4000));
        }
        if (state.selectedTab == "settings")
        {
          state.selectedTab = "adb";
        }
        console.log("Power off complete");
        break;
      case projectorConstants.ProjectorCommands.SystemControlPowerOn:
        while (state.ProjectorPoweredOn != projectorConstants.ToggleStatusGui.On) {
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
    handleAndroidTVQueryResponse,
    handleGUIConnectionStateChange,
    handlePowerToggle,
    handleVPNToggle
  };
}