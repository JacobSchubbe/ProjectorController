<template>
  <div>
    <div class="control-row">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_POWER)"
      >
        Power
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_UP)"
      >
        Up
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_EPG)"
      >
        Episode Guide
      </ControlButton>
    </div>
    <div class="control-row">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_LEFT)"
      >
        Left
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.KEY_OK)"
      >
        OK
      </ControlButton>
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_RIGHT)"
      >
        Right
      </ControlButton>
    </div>
    <div class="control-row">
      <ControlButton
        :onClick="() => handleClick(tvConstants.IRCommands.BTN_DPAD_DOWN)"
      >
        Down
      </ControlButton>
    </div>
  </div>
</template>

<script lang="ts" setup>
import * as tvConstants from "@/Constants/TVConstants";
import ControlButton from "@/components/ControlButton.vue";
import { defineProps } from "vue";

defineProps({
  handleClick: Function // Function to handle button clicks
});
</script>