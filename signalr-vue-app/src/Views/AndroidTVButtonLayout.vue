<template>
  <div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_BACK, isLongPress)"
      >
        Back
      </ControlButton>
      <Keyboard :adbConnection="SignalRInstance" :disable="buttonDisabled" />
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_HOME, isLongPress)"
      >
        Home
      </ControlButton>
    </div>
    <Dpad :buttonDisabled="buttonDisabled"
          :isTouchPadInverted="isTouchPadInverted"
          :handleClick="(keyCode, isLongPress) => handleClick(keyCode, isLongPress)"/>
  </div>
</template>

<script lang="ts" setup>
import Dpad from "@/components/DpadControl.vue";
import * as adbConstants from "@/Constants/AdbConstants";
import Keyboard from "@/components/OnScreenKeyboard.vue"
import ControlButton from "@/components/ControlButton.vue"
import { defineProps } from "vue";

defineProps<{
  buttonDisabled: Boolean; // Passed down from the parent component
  handleClick: Function;     // Function to handle button clicks
  SignalRInstance: any;
  isTouchPadInverted: Boolean;
}>();


</script>

<style scoped>

button,

.control-row {
  display: flex;
  justify-content: center;
  height: 10vh;
  width: 100%;
}

</style>