<template>
  <div id="app">
    <h1>Projector Controller</h1>

    <!-- Dropdown Menu -->
    <div >
      <label>Select Input:</label>
      <select id="inputDropdown" v-model="state.selectedInput" @change="handleDropdownChange">
        <option :value=tcpConsts.ProjectorCommands.SystemControlSourceHDMI1>Input1</option>
        <option :value=tcpConsts.ProjectorCommands.SystemControlSourceHDMI2>Input2</option>
        <option :value=tcpConsts.ProjectorCommands.SystemControlSourceHDMI3>Input3</option>
        <option :value=tcpConsts.ProjectorCommands.SystemControlSourceLAN>LAN</option>
      </select>
    </div>
    <label>GUI Connected: {{ state.GUIConnected }}</label>    

    <div class="projector-controls">
      <!-- Row 1: Up Button -->
      <div class="control-row">
        <button :disabled="buttonDisabledPowerButton" class="control-button" @click="handleClickProjectorCommands(
            state.ProjectorPoweredOn ? 
            tcpConsts.ProjectorCommands.SystemControlPowerOff : tcpConsts.ProjectorCommands.SystemControlPowerOn)"
        >
          {{ powerButtonText }}
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlUp)">
          Up
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button">
          -
        </button>
      </div>

      <!-- Row 2: Left, Enter, and Right Buttons -->
      <div class="control-row">
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlLeft)">
          Left
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button enter-button" @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlEnter)">
          Enter
        </button>
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlRight)">
          Right
        </button>
      </div>

      <!-- Row 3: Down Button -->
      <div class="control-row">
        <button :disabled="buttonDisabledWhenPowerOff" class="control-button" @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlDown)">
          Down
        </button>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { reactive, onMounted, computed } from "vue";
import * as signalr from "./signalrService";
import * as tcpConsts from "./TcpConsts";

export default {
  name: 'App',
  setup() {
    const state = reactive({
      selectedInput: -1,
      GUIConnected: false,
      ProjectorConnected: false,
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
      await signalr.initializeSignalR(
          (isConnected: boolean) => { handleProjectConnectionStateChange(isConnected); },
          (response:signalr.QueryResponse) => { handleQueryResponse(response.queryType, response.currentStatus); },
          (connectionStatus:boolean) => { handleGUIConnectionStateChange(connectionStatus); }
      );
    });
    
    const handleProjectConnectionStateChange = (isConnected: boolean) => {
      console.log(`Projector Connected: ${isConnected}`);
      state.ProjectorConnected = isConnected;
      if (state.ProjectorConnected) {
        signalr.queryForInitialProjectorStatuses();
      }
      else
      {
        state.selectedInput = -1;
        state.ProjectorPoweredOn = false;
      }
    }
    
    const handleQueryResponse = (queryType:Number, currentStatus:Number) => {
      switch (queryType) {
        case tcpConsts.ProjectorCommands.SystemControlSourceQuery:
          state.selectedInput = currentStatus as tcpConsts.ProjectorCommands;
          break;
        case tcpConsts.ProjectorCommands.SystemControlPowerQuery:
          var powerStatus = currentStatus as tcpConsts.PowerStatus;
          var isPoweredOn = powerStatus === tcpConsts.PowerStatus.Warmup || powerStatus == tcpConsts.PowerStatus.LampOn;
          state.ProjectorPoweredOn = isPoweredOn;
          console.log("Projector Powered on: " + isPoweredOn);
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
    
    const handleClickProjectorCommands = async (command: tcpConsts.ProjectorCommands) => {
      signalr.sendProjectorCommands(command);
      console.log(`Command sent: ${command}`);

      switch (command) {
        case tcpConsts.ProjectorCommands.SystemControlPowerOff:
          while (state.ProjectorConnected) {
            console.log("Waiting for power off...");
            signalr.sendProjectorQuery(tcpConsts.ProjectorCommands.SystemControlPowerQuery)
            await new Promise(resolve => setTimeout(resolve, 2000));
          }
          console.log("Power off complete");
          break;
        case tcpConsts.ProjectorCommands.SystemControlPowerOn:
          while (!state.ProjectorConnected) {
            console.log("Waiting for power on...");
            signalr.sendProjectorQuery(tcpConsts.ProjectorCommands.SystemControlPowerQuery)
            await new Promise(resolve => setTimeout(resolve, 2000));
          }
          console.log("Power on complete");
          break;
      }
    };
    
    const handleDropdownChange = () => {
      console.log(`Selected Input: ${state.selectedInput}`);
      signalr.sendProjectorCommands(state.selectedInput);
      console.log(`Command sent: ${state.selectedInput}`);
    };

    return { state, handleDropdownChange, handleClickProjectorCommands, buttonDisabledPowerButton, buttonDisabledWhenPowerOff, tcpConsts, powerButtonText };
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
}</style>