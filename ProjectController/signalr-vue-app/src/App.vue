<template>
  <div id="app">
    <h1>SignalR Vue App</h1>
    <button @click="handleClick('Message 1')">Send Message 1</button>
    <button @click="handleClick('Message 2')">Send Message 2</button>
    <ul>
      <li v-for="(msg, index) in messages" :key="index">
        {{ msg.user }}: {{ msg.message }}
      </li>
    </ul>
  </div>
</template>

<script>
import { initializeSignalR, sendMessage } from './signalrService';

export default {
  name: 'App',
  data() {
    return {
      messages: [] // Store received messages
    };
  },
  mounted() {
    // Initialize SignalR and set up message handler
    initializeSignalR((user, message) => {
      this.messages.push({ user, message }); // Add received messages to the list
    });
  },
  methods: {
    handleClick(message) {
      sendMessage('VueUser', message);
    }
  }
};
</script>

<style>
button {
  margin: 10px;
  padding: 10px;
}
</style>
