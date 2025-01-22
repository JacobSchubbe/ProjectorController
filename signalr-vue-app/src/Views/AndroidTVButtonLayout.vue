<template>
  <div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_BACK)"
      >
        Back
      </ControlButton>
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_UP)"
      >
        Up
      </ControlButton>
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_HOME)"
      >
        Home
      </ControlButton>
    </div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_LEFT)"
      >
        Left
      </ControlButton>
      <ControlButton
          styleClass="control-button enter-button"
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_ENTER)"
      >
        Enter
      </ControlButton>
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_RIGHT)"
      >
        Right
      </ControlButton>
    </div>
    <div class="control-row">
      <ControlButton
          :disabled="buttonDisabled"
          :onClick="() => handleClick(adbConstants.KeyCodes.KEYCODE_DPAD_DOWN)"
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

defineProps({
  buttonDisabled: Boolean, // Passed down from the parent component
  handleClick: Function     // Function to handle button clicks
});
</script>