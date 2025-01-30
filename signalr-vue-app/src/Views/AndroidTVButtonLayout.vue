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
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_UP, isLongPress)"
      >
        Up
      </ControlButton>
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_HOME, isLongPress)"
      >
        Home
      </ControlButton>
    </div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_LEFT, isLongPress)"
      >
        Left
      </ControlButton>
      <ControlButton
          styleClass="control-button enter-button"
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_ENTER, isLongPress)"
      >
        Enter
      </ControlButton>
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_RIGHT, isLongPress)"
      >
        Right
      </ControlButton>
    </div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="(isLongPress) => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_DOWN, isLongPress)"
      >
        Down
      </ControlButton>
    </div>
  </div>
</template>

<script lang="ts" setup>
import * as adbConstants from "@/Constants/AdbConstants";
import ControlButton from "@/components/ControlButton.vue";
import { defineProps } from "vue";

defineProps<{
  buttonDisabled: Boolean; // Passed down from the parent component
  handleClick: Function;     // Function to handle button clicks
  availableHeight: number;
}>();


</script>