<template>
  <div class="onscreen-keyboard">
    <!-- The button to launch the keyboard -->
    <button @click="activateKeyboard" class="keyboard-button" :disabled="disable">
      <img src="../../public/assets/keyboard-icon.png" alt="Keyboard Icon" class="keyboard-icon"/>
    </button>

    <!-- Hidden input to trigger keyboard -->
    <input
      type="text"
      ref="keyboardInput"
      placeholder="Start typing..."
      @input="sendAdbInput"
      @keydown="handleKeydown"
      class="hidden-input"
    />
  </div>
</template>

<script>
import * as adbConstants from "@/Constants/AdbConstants";

export default {
  props: {
    adbConnection: {
      type: Object,
      required: true
    },
    disable: {
      type: Boolean,
      default: false
    }
  },
  methods: {
    activateKeyboard() {
      this.$refs.keyboardInput.focus();
    },
    sendAdbInput(event) {
      const text = event.target.value;
      if (text) {
        this.adbConnection.sendAndroidCommand(adbConstants.KeyCodes.TextInput, text);
        this.$refs.keyboardInput.value = "";
      }
    },
    handleKeydown(event) {
      if (event.which === 8) {
        event.preventDefault(); // Prevent inputText modification
        this.adbConnection.sendAndroidCommand(adbConstants.KeyCodes.KEYCODE_DEL, event.key);
        console.log("Backspace detected!");
      }
    },
  }
};
</script> 

<style>
.onscreen-keyboard {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.keyboard-button {
  background-color: transparent; /* Removes button's background */
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  height: min(9vw, 9vh);
  width: min(9vw, 9vh);
}

.hidden-input {
  position: absolute;
  opacity: 0;
  height: 0;
  width: 0;
  pointer-events: none;
}

.keyboard-icon {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.keyboard-button:disabled {
  cursor: not-allowed; /* Show not-allowed cursor */
  opacity: 0.5; /* Visually indicate the button is disabled */
}

</style>