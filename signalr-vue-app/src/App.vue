<template>
  <div id="app">
    <!-- Dropdown Menu -->
    <div >
      <label>Select Input:</label>
      <select id="inputDropdown" v-model="state.selectedInput" @change="handleDropdownChange">
        <option :value=projectorConstants.ProjectorCommands.SystemControlSourceHDMI1>Input1</option>
        <option :value=projectorConstants.ProjectorCommands.SystemControlSourceHDMI2>Input2</option>
        <option :value=projectorConstants.ProjectorCommands.SystemControlSourceHDMI3>Input3</option>
        <option :value=projectorConstants.ProjectorCommands.SystemControlSourceLAN>LAN</option>
      </select>
    </div>
    <label>GUI Connected: {{ state.GUIConnected }}</label>    

    <div class="projector-controls">
      <!-- Row 1 -->
      <div class="control-row">
        <button :disabled="buttonDisabledPowerButton" class="control-button" @click="handleClickProjectorCommands(
            state.ProjectorPoweredOn ? projectorConstants.ProjectorCommands.SystemControlPowerOff : projectorConstants.ProjectorCommands.SystemControlPowerOn)">
          {{ powerButtonText }}
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_UP)">
          Up
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button">
          -
        </button>
      </div>

      <!-- Row 2 -->
      <div class="control-row">
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_LEFT)">
          Left
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button enter-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_ENTER)">
          Enter
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_RIGHT)">
          Right
        </button>
      </div>

      <!-- Row 3 -->
      <div class="control-row">
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_HOME)">
          Home
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_DPAD_DOWN)">
          Down
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_BACK)">
          Back
        </button>
      </div>
      <!-- Row 4: Volume Buttons -->
      <div class="control-row">
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_VOLUME_UP)">
          Volume<br/><br/>-
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickAndroidCommand(adbConstants.KeyCodes.KEYCODE_VOLUME_DOWN)">
          Volume<br/><br/>+
        </button>
      </div>
      <!-- Row 5: Netflix, YouTube, and Amazon Prime -->
      <div class="control-row">
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button media-button" @click="handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.Netflix)">
          <img src="/assets/netflix-logo.png" alt="Netflix" class="media-icon" />
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button media-button" @click="handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.Youtube)">
          <img src="/assets/youtube-logo.png" alt="YouTube" class="media-icon" />
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button media-button" @click="handleClickAndroidOpenAppCommand(adbConstants.KeyCodes.AmazonPrimeVideo)">
          <img src="/assets/prime-video-logo.png" alt="Amazon Prime" class="media-icon" />
        </button>
      </div>

    </div>
  </div>
</template>

<script lang="ts">
import { reactive, onMounted, computed, onUnmounted } from "vue";
import { SignalRInstance } from "./SignalRServiceManager";
import * as signalr from "./SignalRServiceManager";
import * as projectorConstants from "./Constants/ProjectorConstants";
import * as adbConstants from "./Constants/AdbConstants";

export default {
  name: 'App',
  setup() {
    const state = reactive({
      selectedInput: -1,
      GUIConnected: false,
      ProjectorConnected: false,
      AndroidTVConnected: false,
      ProjectorPoweredOn: false,
    });
    
    const buttonDisabledWhenPowerOff = computed(() => {
      return !state.GUIConnected || !state.ProjectorConnected || !state.ProjectorPoweredOn
    })
    
    const buttonDisabledPowerButton = computed(() => {
      return !state.GUIConnected || !state.ProjectorConnected
    })
    
    const powerButtonText = computed(() => {
      if (!state.GUIConnected) {
        return "GUI Not Connected";
      }
      if (!state.ProjectorConnected) {
        return "Projector Not Connected";
      }
      return state.ProjectorPoweredOn ? "Turn Power Off" : "Turn Power On";
    });

    onMounted(async () => {
      await SignalRInstance.initialize(
          (isConnected: boolean) => { handleProjectorConnectionStateChange(isConnected); },
          (isConnected: boolean) => { handleAndroidTVConnectionStateChange(isConnected); },
          (response:signalr.QueryResponse) => { handleQueryResponse(response.queryType, response.currentStatus); },
          (connectionStatus:boolean) => { handleGUIConnectionStateChange(connectionStatus); }
      );
    });
    
    const handleProjectorConnectionStateChange = async (isConnected: boolean) => {
      state.ProjectorConnected = isConnected;
      if (state.ProjectorConnected) {
        SignalRInstance.queryForInitialProjectorStatuses();
      }
      else
      {
        state.selectedInput = -1;
        state.ProjectorPoweredOn = false;
        await new Promise((resolve) => setTimeout(resolve, 1000));
        SignalRInstance.queryForInitialBackendStatuses();
      }
    }
    
    const handleAndroidTVConnectionStateChange = async (isConnected: boolean) => {
      state.AndroidTVConnected = isConnected;
      if (state.AndroidTVConnected) {
        SignalRInstance.queryForInitialProjectorStatuses();
      }
      else
      {
        await new Promise((resolve) => setTimeout(resolve, 1000));
        SignalRInstance.queryForInitialBackendStatuses();
      }
    }
    
    const handleQueryResponse = (queryType:Number, currentStatus:Number) => {
      switch (queryType) {
        case projectorConstants.ProjectorCommands.SystemControlSourceQuery:
          state.selectedInput = currentStatus as projectorConstants.ProjectorCommands;
          break;
        case projectorConstants.ProjectorCommands.SystemControlPowerQuery:
          var powerStatus = currentStatus as projectorConstants.PowerStatus;
          var isPoweredOn = powerStatus === projectorConstants.PowerStatus.Warmup || powerStatus == projectorConstants.PowerStatus.LampOn;
          state.ProjectorPoweredOn = isPoweredOn;
          break;
        default:
          console.error("Invalid input selected");
          return;
      }
    }
    
    const handleGUIConnectionStateChange = (isConnected: boolean) => {
      state.GUIConnected = isConnected;
      if (!state.GUIConnected) {
        state.selectedInput = -1;
        state.ProjectorPoweredOn = false;
        state.ProjectorConnected = false;
      }
    }
    
    const handleClickAndroidCommand = async (command: adbConstants.KeyCodes) => {
      SignalRInstance.sendAndroidCommand(command);
      console.log(`Command sent: ${command}`);
    }
    
    const handleClickAndroidOpenAppCommand = async (command: adbConstants.KeyCodes) => {
      SignalRInstance.sendAndroidOpenAppCommand(command);
      console.log(`Command sent: ${command}`);
    }
    
    const handleClickProjectorCommands = async (command: projectorConstants.ProjectorCommands) => {
      SignalRInstance.sendProjectorCommand(command);
      console.log(`Command sent: ${command}`);

      switch (command) {
        case projectorConstants.ProjectorCommands.SystemControlPowerOff:
          while (state.ProjectorConnected) {
            console.log("Waiting for power off...");
            SignalRInstance.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery)
            await new Promise(resolve => setTimeout(resolve, 2000));
          }
          console.log("Power off complete");
          break;
        case projectorConstants.ProjectorCommands.SystemControlPowerOn:
          while (!state.ProjectorConnected) {
            console.log("Waiting for power on...");
            SignalRInstance.sendProjectorQuery(projectorConstants.ProjectorCommands.SystemControlPowerQuery)
            await new Promise(resolve => setTimeout(resolve, 2000));
          }
          console.log("Power on complete");
          break;
      }
    };
    
    const handleDropdownChange = () => {
      console.log(`Selected Input: ${state.selectedInput}`);
      SignalRInstance.sendProjectorCommand(state.selectedInput);
      console.log(`Command sent: ${state.selectedInput}`);
    };

    // Handle tab focus to refresh state
    const onTabFocused = () => {
      if (!SignalRInstance.isConnected()) {
        console.log("Tab regained focus. Reconnecting SignalR...");
        SignalRInstance.initialize(
            (isConnected) => handleProjectorConnectionStateChange(isConnected),
            (isConnected) => handleAndroidTVConnectionStateChange(isConnected),
            (response) => handleQueryResponse(response.queryType, response.currentStatus),
            (connectionStatus) => handleGUIConnectionStateChange(connectionStatus)
        );
      } else {
        console.log("Tab regained focus. Querying initial backend statuses...");
        SignalRInstance.queryForInitialBackendStatuses();
      }
    };

    // Add focus event listener
    onMounted(() => {
      console.log("App mounted. Setting up tab focus listener.");
      window.addEventListener("focus", onTabFocused);
    });

    // Clean up (remove listener on app unmount)
    onUnmounted(() => {
      console.log("App unmounted. Removing tab focus listener.");
      window.removeEventListener("focus", onTabFocused);
    });

    return { state, handleDropdownChange, handleClickAndroidCommand, handleClickAndroidOpenAppCommand, handleClickProjectorCommands, buttonDisabledPowerButton, buttonDisabledWhenPowerOff, adbConstants, projectorConstants, powerButtonText };
  }
};
</script>



<style scoped>
button {
  margin: 10px;
  padding: 10px;
}

select {
  margin: 10px;
  padding: 5px;
}

.projector-controls {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px; /* Spacing between rows */
}

.control-row {
  display: flex;
  justify-content: center;
  gap: 10px; /* Spacing between buttons */
}

.control-button {
  background-color: #007bff; /* Primary button color */
  color: white;
  border: none;
  border-radius: 5px;
  width: 80px; /* Fixed width */
  height: 80px; /* Fixed height */
  font-size: 14px; /* Reduce font size to prevent overflow if content is long */
  cursor: pointer;
  text-align: center;
  vertical-align: middle;
  display: flex; /* Flexbox for centering content */
  justify-content: center; /* Horizontally center content */
  align-items: center; /* Vertically center content */
  transition: background-color 0.3s ease, transform 0.2s;
  white-space: normal; /* Allow text to wrap inside the button */
  overflow: hidden; /* Hide any overflowing content */
  text-overflow: ellipsis; /* Add ellipsis if text overflows */
}

.control-button:disabled {
  background-color: #ccc; /* Grey background */
  color: #666;           /* Greyed-out text */
  cursor: not-allowed;   /* Change cursor to indicate it's not clickable */
  opacity: 0.6;          /* Dim the button */
}

.control-button:hover {
  background-color: #0056b3; /* Darker shade on hover */
}

.control-button:active {
  transform: scale(0.95); /* Button press effect */
}

.enter-button {
  background-color: #28a745; /* Distinct color for the Enter button */
}

.enter-button:hover {
  background-color: #1e7e34; /* Darker shade on hover for Enter button */
}

.media-button {
  background-color: transparent; /* Make the button background fully transparent */
  border: none; /* Remove borders for a cleaner visual */
  padding: 0; /* Remove extra spacing around the image */
  display: flex; /* Use flexbox for proper content alignment */
  justify-content: center;
  align-items: center;
  width: 80px; /* Button should remain square */
  height: 80px; /* Fixed button height (equal to width) */
  cursor: pointer;
  overflow: hidden; /* Clip any overflow from the image */
}

.media-button:disabled {
  opacity: 0.6;
  pointer-events: none; /* Disable pointer interactions when disabled */
}

.media-icon {
  width: 100%; /* Ensure image completely fills the button */
  height: 100%; /* Scale image to fill height */
  object-fit: cover; /* Make sure image covers the entire button evenly */
  pointer-events: none; /* Ensure clicking on the image still triggers the button */
}
</style>