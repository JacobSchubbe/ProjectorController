<template>
  <div id="app">
    <h1>SignalR Vue App</h1>

    <!-- Dropdown Menu -->
    <label for="inputDropdown">Select Input:</label>
    <select id="inputDropdown" v-model="state.selectedInput" @change="handleDropdownChange">
      <option value="HDMI1">Input1</option>
      <option value="HDMI2">Input2</option>
      <option value="HDMI3">Input3</option>
      <option value="LAN">LAN</option>
    </select>

    <button @click="handleClick(tcpConsts.SystemControl.PowerOff)">Power Off</button>
    <button @click="handleClick(tcpConsts.SystemControl.PowerOn)">Power On</button>
    <button @click="handleClick(tcpConsts.SystemControl.PowerQuery)">Query Power State</button>

    <ul>
      <li v-for="(msg, index) in state.messages" :key="index">
        {{ msg.message }}
      </li>
    </ul>
  </div>
</template>

<script lang="ts">
import { reactive, onMounted } from "vue";
import {initializeSignalR, sendSystemCommand} from "./signalrService"; // Ensure correct import path
import * as tcpConsts from "./TcpConsts"; // Ensure correct import path

type Message = {
  message: string;
}

export default {
  name: 'App',
  setup() {
    const state = reactive({
      messages: [] as Message[],
      selectedInput: "HDMI3",
    });

    onMounted(() => {
      // Assuming initializeSignalR is set up to push messages to `state.messages`
      initializeSignalR((message: Message) => {
        state.messages.push(message);
      });
    });

    const handleClick = (command: tcpConsts.SystemControl) => {
      sendSystemCommand(command);
      console.log(`Command sent: ${command}`);
    };

    const handleDropdownChange = () => {
      console.log(`Selected Input: ${state.selectedInput}`);

      // Determine the command to send based on selected input
      let command: tcpConsts.SystemControl;
      switch (state.selectedInput) {
        case "HDMI1":
          command = tcpConsts.SystemControl.SourceHDMI1;
          break;
        case "HDMI2":
          command = tcpConsts.SystemControl.SourceHDMI2;
          break;
        case "HDMI3":
          command = tcpConsts.SystemControl.SourceHDMI3;
          break;
        case "LAN":
          command = tcpConsts.SystemControl.SourceHDMILAN;
          break;
        default:
          console.error("Invalid input selected");
          return;
      }

      sendSystemCommand(command);
      console.log(`Command sent: ${command}`);
    };

    return { state, handleDropdownChange, handleClick, tcpConsts };
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
