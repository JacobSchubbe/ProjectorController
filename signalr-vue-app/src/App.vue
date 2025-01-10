<template>
  <div id="app">
    <h1>Projector Controller</h1>

    <!-- Dropdown Menu -->
    <div >
      <label for="inputDropdown">Select Input:</label>
      <select id="inputDropdown" v-model="state.selectedInput" @change="handleDropdownChange">
        <option :value=tcpConsts.ProjectorCommands.SourceHDMI1>Input1</option>
        <option :value=tcpConsts.ProjectorCommands.SourceHDMI2>Input2</option>
        <option :value=tcpConsts.ProjectorCommands.SourceHDMI3>Input3</option>
        <option :value=tcpConsts.ProjectorCommands.SourceLAN>LAN</option>
      </select>
      <label>
        Connected: {{state.connected}}
      </label>
    </div>

    <div>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.SystemControlPowerOff)">Power Off</button>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.SystemControlPowerOn)">Power On</button>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.SystemControlPowerQuery)">Query Power State</button>
    </div>
    <div class="">
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlUp)">Up</button>
    </div>
    <div>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlLeft)">Left</button>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlEnter)">Enter</button>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlRight)">Right</button>
    </div>
    <div>
      <button @click="handleClickProjectorCommands(tcpConsts.ProjectorCommands.KeyControlDown)">Down</button>
    </div>
    
    
    <ul>
      <li v-for="(msg, index) in state.messages" :key="index">
        {{ msg.message || 'No message content' }}
      </li>
    </ul>
  </div>
</template>

<script lang="ts">
import { reactive, onMounted } from "vue";
import * as signalr from "./signalrService";
import * as tcpConsts from "./TcpConsts";

export default {
  name: 'App',
  setup() {
    const state = reactive({
      messages: [] as signalr.Message[],
      selectedInput: -1,
      connected: false,
    });
    
    onMounted(() => {
      signalr.initializeSignalR(
          (message: signalr.Message) => { state.messages.push(message); },
          (response:signalr.QueryResponse) => { handleQueryResponse(response.queryType, response.currentStatus); },
          (connectionStatus:boolean) => { state.connected = connectionStatus; }
      );
    });

    const handleQueryResponse = (queryType:Number, currentStatus:Number) => {
      switch (queryType) {
        case tcpConsts.ProjectorCommands.SystemControlSourceQuery:
          state.selectedInput = currentStatus as tcpConsts.ProjectorCommands;
          break;
        default:
          console.error("Invalid input selected");
          return;
      }
    }
    
    const handleClickProjectorCommands = (command: tcpConsts.ProjectorCommands) => {
      signalr.sendProjectorCommands(command);
      console.log(`Command sent: ${command}`);
    };

    const handleDropdownChange = () => {
      console.log(`Selected Input: ${state.selectedInput}`);
      signalr.sendProjectorCommands(state.selectedInput);
      console.log(`Command sent: ${state.selectedInput}`);

      // // Determine the command to send based on selected input
      // let command: tcpConsts.SystemControl;
      // switch (state.selectedInput) {
      //   case "HDMI1":
      //     command = tcpConsts.SystemControl.SourceHDMI1;
      //     break;
      //   case "HDMI2":
      //     command = tcpConsts.SystemControl.SourceHDMI2;
      //     break;
      //   case "HDMI3":
      //     command = tcpConsts.SystemControl.SourceHDMI3;
      //     break;
      //   case "LAN":
      //     command = tcpConsts.SystemControl.SourceHDMILAN;
      //     break;
      //   default:
      //     console.error("Invalid input selected");
      //     return;
      // }

    };

    return { state, handleDropdownChange, handleClickProjectorCommands, tcpConsts };
  }
};
</script>

<style>
button {
  margin: 10px;
  padding: 10px;
}

select {
  margin: 10px;
  padding: 5px;
}
</style>
