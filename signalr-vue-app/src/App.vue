<template>
  <div id="app">
    <h1>Projector Controller</h1>

    <!-- Dropdown Menu -->
    <div >
      <label for="inputDropdown">Select Input:</label>
      <select id="inputDropdown" v-model="state.selectedInput" @change="handleDropdownChange">
        <option :value=tcpConsts.SystemControl.SourceHDMI1>Input1</option>
        <option :value=tcpConsts.SystemControl.SourceHDMI2>Input2</option>
        <option :value=tcpConsts.SystemControl.SourceHDMI3>Input3</option>
        <option :value=tcpConsts.SystemControl.SourceLAN>LAN</option>
      </select>
      <label>
        Connected: {{state.connected}}
      </label>
    </div>

    <div>
      <button @click="handleClickSystemControl(tcpConsts.SystemControl.PowerOff)">Power Off</button>
      <button @click="handleClickSystemControl(tcpConsts.SystemControl.PowerOn)">Power On</button>
      <button @click="handleClickSystemControl(tcpConsts.SystemControl.PowerQuery)">Query Power State</button>
    </div>
    <div class="">
      <button @click="handleClickKeyCommands(tcpConsts.KeyControl.KeyUp)">Up</button>
    </div>
    <div>
      <button @click="handleClickKeyCommands(tcpConsts.KeyControl.KeyLeft)">Left</button>
      <button @click="handleClickKeyCommands(tcpConsts.KeyControl.KeyEnter)">Enter</button>
      <button @click="handleClickKeyCommands(tcpConsts.KeyControl.KeyRight)">Right</button>
    </div>
    <div>
      <button @click="handleClickKeyCommands(tcpConsts.KeyControl.KeyDown)">Down</button>
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
        case tcpConsts.SystemControl.SourceQuery:
          state.selectedInput = currentStatus as tcpConsts.SystemControl;
          break;
        default:
          console.error("Invalid input selected");
          return;
      }
    }
    
    const handleClickSystemControl = (command: tcpConsts.SystemControl) => {
      signalr.sendSystemCommand(command);
      console.log(`Command sent: ${command}`);
    };
    const handleClickKeyCommands = (command: tcpConsts.KeyControl) => {
      signalr.sendKeyCommand(command);
      console.log(`Command sent: ${command}`);
    };

    const handleDropdownChange = () => {
      console.log(`Selected Input: ${state.selectedInput}`);
      signalr.sendSystemCommand(state.selectedInput);
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

    return { state, handleDropdownChange, handleClickSystemControl, handleClickKeyCommands, tcpConsts };
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
