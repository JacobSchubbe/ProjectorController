<template>
  <div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_BACK, isLongPress)"
      >
        Back
      </ControlButton>
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_HOME, isLongPress)"
      >
        Home
      </ControlButton>
    </div>
    <Dpad :buttonDisabled="buttonDisabled"
          :handleClick="(keyCode, isLongPress) => handleClick(keyCode, isLongPress)"/>
  </div>
</template>

<script lang="ts" setup>
import Dpad from "@/components/DpadControl.vue";
import * as adbConstants from "@/Constants/AdbConstants";
import ControlButton from "@/components/ControlButton.vue"
import { defineProps } from "vue";

defineProps<{
  buttonDisabled: Boolean; // Passed down from the parent component
  handleClick: Function;     // Function to handle button clicks
}>();


</script>

<style scoped>

button,
.ControlButton { /* If Buttons use a custom component */
  width: 100%; /* Buttons should fill their grid cell */
  height: 100%; /* Height matches the row height */
  max-height: 15vh;
  max-width: 15vh;
}

.control-row {
  display: flex;
  justify-content: center;
  width: 100vw;
  height: 18vh;
  gap: 10px; /* Spacing between buttons */
  margin-bottom: 10px;
}

.enter-button {
  background-color: #28a745; /* Distinct color for the Enter button */
}

.enter-button:hover {
  background-color: #1e7e34; /* Darker shade on hover for Enter button */
}


</style>